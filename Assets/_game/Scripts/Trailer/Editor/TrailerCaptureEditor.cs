using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class TrailerCaptureEditor
{
    public static readonly string Work = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Trailer-20261010"));
    static double _nextPoll;

    static TrailerCaptureEditor() { EditorApplication.update += Tick; }

    public static void InspectAndWait()
    {
        Directory.CreateDirectory(Work);
        SessionState.SetBool("TrailerCapture.Owned", true);
        Inspect();
        File.WriteAllText(Work + "/editor-ready.txt", DateTime.Now.ToString("O"));
    }

    public static void BuildAndWait()
    {
        Directory.CreateDirectory(Work);
        SessionState.SetBool("TrailerCapture.Owned", true);
        TrailerSceneBuilder.Build();
        File.WriteAllText(Work + "/editor-ready.txt", DateTime.Now.ToString("O"));
    }

    [MenuItem("Tools/Trailer/2 - Record all ProRes takes (1080p60)")]
    public static void RecordAll() => TrailerRecorderRunner.StartAll();

    [MenuItem("Tools/Trailer/Stop recording")]
    public static void StopRecording() => TrailerRecorderRunner.Stop();

    static void Tick()
    {
        if (!SessionState.GetBool("TrailerCapture.Owned", false) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.timeSinceStartup < _nextPoll) return;
        _nextPoll = EditorApplication.timeSinceStartup + 1;
        string path = Work + "/command.txt";
        if (!File.Exists(path)) return;
        string command = File.ReadAllText(path).Trim();
        if (command == SessionState.GetString("TrailerCapture.Command", "")) return;
        SessionState.SetString("TrailerCapture.Command", command);
        try
        {
            if (command.StartsWith("inspect")) Inspect();
            else if (command.StartsWith("build")) TrailerSceneBuilder.Build();
            else if (command.StartsWith("probe:")) TrailerRecorderRunner.StartProbe(int.Parse(command.Split(':')[1]));
            else if (command.StartsWith("take:")) TrailerRecorderRunner.StartTake(int.Parse(command.Split(':')[1]));
            else if (command.StartsWith("resume:")) TrailerRecorderRunner.StartFromIndex(int.Parse(command.Split(':')[1]));
            else if (command.StartsWith("record")) TrailerRecorderRunner.StartAll();
            else if (command.StartsWith("stop")) TrailerRecorderRunner.Stop();
            else if (command.StartsWith("refresh")) AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            else if (command.StartsWith("restore"))
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play before returning to the level.");
                if (SceneManager.GetActiveScene().path.StartsWith("Assets/_game/Scenes/Trailer/"))
                    EditorSceneManager.OpenScene("Assets/_game/Scenes/Final/Levels/Level2/Level2.unity", OpenSceneMode.Single);
                File.WriteAllText(Work + "/editor-restored.txt", DateTime.Now.ToString("O"));
            }
            else if (command.StartsWith("quit")) EditorApplication.Exit(0);
        }
        catch (Exception ex) { Debug.LogException(ex); File.WriteAllText(Work + "/error.txt", ex.ToString()); }
    }

    [Serializable] class Node
    {
        public string path;
        public Vector3 center, size, position;
        public string[] scripts;
    }
    [Serializable] class SceneInfo { public string scene; public List<Node> nodes = new List<Node>(); }
    [Serializable] class Report { public List<SceneInfo> scenes = new List<SceneInfo>(); }

    static IEnumerable<T> Components<T>(Scene scene) where T : Component
        => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));

    static void Inspect()
    {
        var report = new Report();
        foreach (string path in new[] {
            "Assets/_game/Scenes/Final/HomeScreen.unity",
            "Assets/_game/Scenes/Final/Levels/Level1/Level1.unity",
            "Assets/_game/Scenes/Final/Levels/Level2/Level2.unity" })
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var info = new SceneInfo { scene = path };
            foreach (Transform transform in Components<Transform>(scene))
            {
                if (transform.parent != null && transform.parent.parent != null && transform.parent.parent.parent != null) continue;
                Renderer[] renderers = transform.GetComponentsInChildren<Renderer>(true);
                Bounds bounds = new Bounds(transform.position, Vector3.zero);
                if (renderers.Length > 0)
                {
                    bounds = renderers[0].bounds;
                    foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
                }
                info.nodes.Add(new Node { path = FullPath(transform), center = bounds.center, size = bounds.size,
                    position = transform.position, scripts = transform.GetComponents<MonoBehaviour>().Where(x => x != null).Select(x => x.GetType().Name).ToArray() });
            }
            report.scenes.Add(info);
        }
        File.WriteAllText(Work + "/scene-inspection.json", JsonUtility.ToJson(report, true));
    }

    static string FullPath(Transform transform) => transform.parent == null ? transform.name : FullPath(transform.parent) + "/" + transform.name;

    public static void Capture(Camera camera, string filename)
    {
        RenderTexture oldActive = RenderTexture.active;
        RenderTexture oldTarget = camera.targetTexture;
        var target = RenderTexture.GetTemporary(960, 540, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Texture2D image = null;
        try
        {
            camera.targetTexture = target;
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture.active = target;
            image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Work + "/" + filename, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = oldTarget;
            RenderTexture.active = oldActive;
            RenderTexture.ReleaseTemporary(target);
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
