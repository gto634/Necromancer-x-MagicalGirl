using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

// Editor-only capture orchestration. The native Recorder writes every ProRes frame.
[InitializeOnLoad]
public static class TrailerRecorderRunner
{
    const string Key = "TrailerRecorder.";
    const int Idle = 0, AwaitPlay = 1, Warming = 2, Recording = 3, AwaitEdit = 4, Completed = 5, Cancelled = 6, Failed = 7;
    static RecorderController _controller;
    static RecorderControllerSettings _settings;
    static MovieRecorderSettings _movie;
    static TrailerShotDirector _director;
    static double _warmupUntil, _nextProgress;
    static int _firstFrame;
    static bool _finishing;
    static bool _reloadLocked;

    static int Phase { get => SessionState.GetInt(Key + "Phase", Idle); set => SessionState.SetInt(Key + "Phase", value); }
    static int Index { get => SessionState.GetInt(Key + "Index", 0); set => SessionState.SetInt(Key + "Index", value); }
    static bool Probe => SessionState.GetBool(Key + "Probe", false);
    static string Output => SessionState.GetString(Key + "Output", "");
    static string Work => TrailerCaptureEditor.Work;
    static bool Active => Phase >= AwaitPlay && Phase <= AwaitEdit;

    [Serializable]
    class Progress
    {
        public string state, shot, file, utc, error;
        public int shotIndex, shotCount, expectedFrames, renderedFrames;
        public float duration, elapsed;
        public int width = 1920, height = 1080, fps = 60;
        public string encoder = "Unity Recorder 5.1 / Apple ProRes 422 HQ";
        public bool audio = false;
        public long bytes;
    }

    static TrailerRecorderRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += Tick;
        Application.logMessageReceived += CaptureError;
        EditorApplication.delayCall += RecoverAfterReload;
    }

    public static void StartAll() => Start(false, 0, false);
    public static void StartFromIndex(int index) => Start(false, index, false);
    public static void StartProbe(int index) => Start(true, index, true);
    public static void StartTake(int index) => Start(false, index, true);

    static void Start(bool probe, int index, bool single)
    {
        if (Active || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop the current capture and leave Play mode before starting a trailer capture.");
        if (index < 0 || index >= TrailerSceneBuilder.Shots.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        Directory.CreateDirectory(Work);
        Directory.CreateDirectory(Path.Combine(Work, "clips"));
        SessionState.SetBool(Key + "Probe", probe);
        SessionState.SetBool(Key + "Single", single);
        SessionState.SetBool(Key + "Quit", false);
        SessionState.SetString(Key + "Error", "");
        Index = index;
        Phase = Idle;
        File.WriteAllText(Path.Combine(Work, "recording-errors.log"), "", Encoding.UTF8);
        foreach (string marker in new[] { "complete.log", "probe-complete.log", "recording-error.txt" })
        {
            string path = Path.Combine(Work, marker);
            if (File.Exists(path)) File.Delete(path);
        }
        try
        {
            foreach (TrailerShotDefinition shot in TrailerSceneBuilder.Shots)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(shot.ScenePath) == null) { TrailerSceneBuilder.Build(); break; }
            ConfigureAndPlay();
        }
        catch (Exception exception) { Fail(exception); }
    }

    static void ConfigureAndPlay()
    {
        TrailerSceneBuilder.ConfigureShot(Index);
        string name = SafeName(TrailerSceneBuilder.Shots[Index].Name);
        string output = Probe ? Path.Combine(Work, "Probe_" + name + ".mov")
            : Path.Combine(Work, "clips", (Index + 1).ToString("00") + "_" + name + ".mov");
        SessionState.SetString(Key + "Output", Path.GetFullPath(output));
        // These exact names are dedicated outputs of this runner.
        if (File.Exists(Output)) File.Delete(Output);
        Phase = AwaitPlay;
        WriteProgress("entering-play");
        EditorApplication.isPlaying = true;
    }

    static void RecoverAfterReload()
    {
        if (!Active) return;
        if ((Phase == AwaitPlay || Phase == Warming) && EditorApplication.isPlaying) BeginWarmup();
        else if (Phase == AwaitEdit && !EditorApplication.isPlayingOrWillChangePlaymode) FinishEditMode();
        else if (Phase == Recording)
            Fail(new InvalidOperationException("A domain reload interrupted the Recorder capture. Restart this shot after compilation finishes."));
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!Active) return;
        if (state == PlayModeStateChange.EnteredPlayMode && (Phase == AwaitPlay || Phase == Warming)) BeginWarmup();
        else if (state == PlayModeStateChange.ExitingPlayMode && Phase != AwaitEdit)
            Fail(new InvalidOperationException("Play mode was interrupted before this trailer shot completed."));
        else if (state == PlayModeStateChange.EnteredEditMode && Phase == AwaitEdit)
            EditorApplication.delayCall += FinishEditMode;
    }

    static void BeginWarmup()
    {
        Phase = Warming;
        Application.runInBackground = true;
        _warmupUntil = EditorApplication.timeSinceStartup + 0.8;
        WriteProgress("warming-up");
    }

    static void Tick()
    {
        if (!Active || _finishing || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        try
        {
            string error = SessionState.GetString(Key + "Error", "");
            if (!string.IsNullOrEmpty(error)) { Fail(new InvalidOperationException(error)); return; }
            if (Phase == Warming)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if (EditorApplication.timeSinceStartup >= _warmupUntil) BeginRecording();
            }
            else if (Phase == Recording)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if (_controller == null) throw new InvalidOperationException("The Recorder controller was lost during capture.");
                if (!_controller.IsRecording()) { FinishShot(); return; }
                if (EditorApplication.timeSinceStartup >= _nextProgress)
                {
                    _nextProgress = EditorApplication.timeSinceStartup + 1;
                    WriteProgress("recording");
                }
            }
            else if(Phase==AwaitPlay || Phase==AwaitEdit)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                if(Phase==AwaitEdit && !EditorApplication.isPlayingOrWillChangePlaymode)FinishEditMode();
            }
        }
        catch (Exception exception) { Fail(exception); }
    }

    static float Duration => Probe ? 3f : TrailerSceneBuilder.Shots[Index].Duration;
    static int FrameCount => Mathf.Max(1, Mathf.RoundToInt(Duration * 60f));

    static void BeginRecording()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Recorder can only start in Play mode.");
        _director = UnityEngine.Object.FindFirstObjectByType<TrailerShotDirector>();
        if (_director == null || Camera.main == null)
            throw new InvalidOperationException("The trailer scene must contain its director and a MainCamera.");
        EditorApplication.LockReloadAssemblies();
        _reloadLocked=true;
        _settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        _settings.FrameRate = 60f;
        _settings.FrameRatePlayback = FrameRatePlayback.Constant;
        _settings.CapFrameRate = false;
        _settings.ExitPlayMode = false;
        _settings.SetRecordModeToFrameInterval(0, FrameCount - 1);
        _movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        _movie.name = "Trailer ProRes 422 HQ 1080p60";
        _movie.Enabled = true;
        _movie.EncoderSettings = new ProResEncoderSettings { Format = ProResEncoderSettings.OutputFormat.ProRes422HQ };
        _movie.ImageInputSettings = new CameraInputSettings
        {
            Source = ImageSource.MainCamera, OutputWidth = 1920, OutputHeight = 1080, CaptureUI = false
        };
        _movie.CaptureAlpha = false;
        _movie.CaptureAudio = false;
        _movie.OutputFile = Path.ChangeExtension(Output, null);
        _settings.AddRecorderSettings(_movie);
        _controller = new RecorderController(_settings);
        _controller.PrepareRecording();
        // Reset actors and frame the first image in the same update that starts Recorder.
        _director.Begin();
        if (!_director.IsRunning) throw new InvalidOperationException("The trailer director refused to start this shot.");
        if (!_controller.StartRecording()) throw new InvalidOperationException("Unity Recorder failed to start the native ProRes encoder.");
        _firstFrame = Time.renderedFrameCount;
        Phase = Recording;
        _nextProgress = EditorApplication.timeSinceStartup + 1;
        WriteProgress("recording");
        Debug.Log("Trailer Recorder started: " + Output);
    }

    static void FinishShot()
    {
        _finishing = true;
        try
        {
            ReleaseRecorder();
            string error = SessionState.GetString(Key + "Error", "");
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            if (!File.Exists(Output) || new FileInfo(Output).Length == 0)
                throw new IOException("Recorder stopped without producing a non-empty MOV: " + Output);
            WriteProgress("shot-complete");
            File.AppendAllText(Path.Combine(Work, "recording-shots.log"), DateTime.UtcNow.ToString("O") + " " + Output + Environment.NewLine);
            Phase = AwaitEdit;
            EditorApplication.isPlaying = false;
        }
        finally { _finishing = false; }
    }

    static void FinishEditMode()
    {
        if (Phase != AwaitEdit || EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            if (Probe || SessionState.GetBool(Key+"Single",false) || Index + 1 >= TrailerSceneBuilder.Shots.Length)
            {
                Phase = Completed;
                WriteProgress("complete");
                File.WriteAllText(Path.Combine(Work, Probe ? "probe-complete.log" : "complete.log"), DateTime.UtcNow.ToString("O") + Environment.NewLine + Output);
                if (SessionState.GetBool(Key + "Quit", false)) QuitOwnedEditor();
            }
            else { Index++; ConfigureAndPlay(); }
        }
        catch (Exception exception) { Fail(exception); }
    }

    public static void Stop()
    {
        if (!Active) return;
        Phase = Cancelled;
        try { ReleaseRecorder(); WriteProgress("cancelled"); }
        catch (Exception exception) { Fail(exception); }
        if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.isPlaying = false;
    }

    public static void QuitOwnedEditor()
    {
        if (!SessionState.GetBool("TrailerCapture.Owned", false))
            throw new InvalidOperationException("This runner cannot close an Editor it did not open for trailer capture.");
        if (Active || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Stop();
            EditorApplication.delayCall += QuitOwnedEditor;
            return;
        }
        EditorApplication.Exit(0);
    }

    static void ReleaseRecorder()
    {
        if (_controller != null) _controller.StopRecording();
        _controller = null;
        if (_movie != null) UnityEngine.Object.DestroyImmediate(_movie);
        if (_settings != null) UnityEngine.Object.DestroyImmediate(_settings);
        _movie = null;
        _settings = null;
        if(_reloadLocked){_reloadLocked=false;EditorApplication.UnlockReloadAssemblies();}
    }

    static void CaptureError(string message, string stackTrace, LogType type)
    {
        if (!Active || (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)) return;
        string error = message + Environment.NewLine + stackTrace;
        if (string.IsNullOrEmpty(SessionState.GetString(Key + "Error", ""))) SessionState.SetString(Key + "Error", error);
        try { File.AppendAllText(Path.Combine(Work, "recording-errors.log"), DateTime.UtcNow.ToString("O") + " " + error + Environment.NewLine); }
        catch (IOException) { }
    }

    static void Fail(Exception exception)
    {
        Phase = Failed;
        SessionState.SetString(Key + "Error", exception.ToString());
        try { ReleaseRecorder(); }
        catch (Exception flushError) { Debug.LogException(flushError); }
        Directory.CreateDirectory(Work);
        File.WriteAllText(Path.Combine(Work, "recording-error.txt"), exception.ToString(), Encoding.UTF8);
        WriteProgress("error");
        Debug.LogException(exception);
        if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.isPlaying = false;
    }

    static void WriteProgress(string state)
    {
        TrailerShotDefinition shot = TrailerSceneBuilder.Shots[Index];
        var progress = new Progress
        {
            state = state, shot = shot.Name, shotIndex = Index, shotCount = TrailerSceneBuilder.Shots.Length,
            file = Output, utc = DateTime.UtcNow.ToString("O"), duration = Duration, expectedFrames = FrameCount,
            elapsed = _director != null ? _director.Elapsed : 0f,
            renderedFrames = Phase == Recording ? Mathf.Max(0, Time.renderedFrameCount - _firstFrame) : 0,
            error = SessionState.GetString(Key + "Error", ""), bytes = File.Exists(Output) ? new FileInfo(Output).Length : 0
        };
        File.WriteAllText(Path.Combine(Work, "recording-state.json"), JsonUtility.ToJson(progress, true), Encoding.UTF8);
    }

    static string SafeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Shot";
        foreach (char character in Path.GetInvalidFileNameChars()) name = name.Replace(character, '_');
        return name.Trim().Replace(' ', '_');
    }
}
