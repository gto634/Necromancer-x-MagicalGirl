using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public enum TrailerActionKind
{
    None,
    MGVolley,
    MGOrbit,
    NECBoomerang,
    NECTeleport,
    CooperativeVolley,
    ArmyMarch
}

// This component belongs to the dedicated trailer scenes, never to the gameplay prefabs.
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public class TrailerShotDirector : MonoBehaviour
{
    [SerializeField] string _shotName = "Trailer shot";
    [SerializeField, Min(2.1f)] float _duration = 12f;
    [SerializeField] CinemachineCamera _camera;
    [SerializeField] Transform _subject;
    [SerializeField] Transform _mg;
    [SerializeField] Transform _nec;
    [SerializeField] Transform _army;
    [SerializeField] TrailerActionKind _actionKind;

    [SerializeField] Vector3 _cameraStart;
    [SerializeField] Vector3 _cameraEnd;
    [SerializeField] Vector3 _targetStart;
    [SerializeField] Vector3 _targetEnd;
    [SerializeField, Range(1f, 179f)] float _fovStart = 45f;
    [SerializeField, Range(1f, 179f)] float _fovEnd = 45f;
    [SerializeField] float _cameraOrbitDegrees;
    [SerializeField] Vector3 _cameraOrbitCenter;
    [SerializeField] bool _cameraFollowSubject;

    [SerializeField] Vector3 _mgStart;
    [SerializeField] Vector3 _mgEnd;
    [SerializeField] Vector3 _necStart;
    [SerializeField] Vector3 _necEnd;
    [SerializeField] Vector3 _armyStart;
    [SerializeField] Vector3 _armyEnd;

    const float HandleDuration = 1f;
    const float FirstActionTime = 1.5f;
    const float VolleyInterval = 1.8f;
    const float OrbitInterval = 4.2f;
    const float BoomerangInterval = 3.6f;
    const float TeleportInterval = 4.2f;
    const float ChargeHoldDuration = 1.4f;
    const float AttackPoseDuration = 0.8f;
    const uint ParticleSeed = 31917;

    Attack1_MG _mgAttack1;
    Attack2_MG _mgAttack2;
    Attack1_NEC _necAttack1;
    Attack2_NEC _necAttack2;
    Brain_MG _mgBrain;
    Brain_NEC _necBrain;
    Feedback_MG _mgFeedback;
    Feedback_NEC _necFeedback;
    MoveSystem _mgMove;
    MoveSystem _necMove;
    LookSystem _mgLook;
    LookSystem _necLook;
    ShootSystem[] _mgShoots = new ShootSystem[0];
    ShootSystem[] _necShoots = new ShootSystem[0];

    readonly HashSet<Projectile> _existingProjectiles = new HashSet<Projectile>();
    readonly HashSet<int> _seededProjectiles = new HashSet<int>();
    bool _hasProjectileBaseline;
    bool _mgAttack1Pose;
    bool _mgAttack2Pose;
    bool _necAttack1Pose;
    bool _necAttack2Pose;
    float _nextMGAction;
    float _nextNECAction;
    float _mgAttack1StopTime;
    float _necAttack1StopTime;
    float _necReleaseTime;
    Transform _mgPoseOwner;
    Transform _necPoseOwner;
    Transform _armyPoseOwner;
    Quaternion _mgInitialRotation;
    Quaternion _necInitialRotation;
    Quaternion _armyInitialRotation;

    public string ShotName { get => _shotName; set => _shotName = value; }
    public float Duration { get => _duration; set => _duration = Mathf.Max(2.1f, value); }
    public CinemachineCamera Camera { get => _camera; set => _camera = value; }
    public Transform Subject { get => _subject; set => _subject = value; }
    public Transform MG { get => _mg; set => _mg = value; }
    public Transform NEC { get => _nec; set => _nec = value; }
    public Transform Army { get => _army; set => _army = value; }
    public TrailerActionKind ActionKind { get => _actionKind; set => _actionKind = value; }
    public Vector3 CameraStart { get => _cameraStart; set => _cameraStart = value; }
    public Vector3 CameraEnd { get => _cameraEnd; set => _cameraEnd = value; }
    public Vector3 TargetStart { get => _targetStart; set => _targetStart = value; }
    public Vector3 TargetEnd { get => _targetEnd; set => _targetEnd = value; }
    public float FovStart { get => _fovStart; set => _fovStart = Mathf.Clamp(value, 1f, 179f); }
    public float FovEnd { get => _fovEnd; set => _fovEnd = Mathf.Clamp(value, 1f, 179f); }
    public float CameraOrbitDegrees { get => _cameraOrbitDegrees; set => _cameraOrbitDegrees = value; }
    public Vector3 CameraOrbitCenter { get => _cameraOrbitCenter; set => _cameraOrbitCenter = value; }
    public bool CameraFollowSubject { get => _cameraFollowSubject; set => _cameraFollowSubject = value; }
    public Vector3 MGStart { get => _mgStart; set => _mgStart = value; }
    public Vector3 MGEnd { get => _mgEnd; set => _mgEnd = value; }
    public Vector3 NECStart { get => _necStart; set => _necStart = value; }
    public Vector3 NECEnd { get => _necEnd; set => _necEnd = value; }
    public Vector3 ArmyStart { get => _armyStart; set => _armyStart = value; }
    public Vector3 ArmyEnd { get => _armyEnd; set => _armyEnd = value; }
    public float Elapsed { get; private set; }
    public bool IsRunning { get; private set; }

    public void Begin()
    {
        ResetShot();
        if (!Application.isPlaying) return;
        if (!HasSceneReferences())
        {
            Debug.LogError("Trailer shot: camera and actors must belong to the director's dedicated scene.", this);
            return;
        }

        // Only instances created after this point can be cleaned up by this shot.
        foreach (Projectile projectile in Object.FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (projectile.gameObject.scene == gameObject.scene)
                _existingProjectiles.Add(projectile);
        }
        _hasProjectileBaseline = true;
        SeedParticles(_mg, ParticleSeed);
        SeedParticles(_nec, ParticleSeed + 100);
        SeedParticles(_army, ParticleSeed + 200);
        _nextMGAction = FirstActionTime;
        _nextNECAction = FirstActionTime;
        IsRunning = true;
    }

    public void ResetShot()
    {
        IsRunning = false;
        if (Application.isPlaying) StopActions();
        ReleaseReferences();
        CacheReferences();
        if (Application.isPlaying) CleanupProjectiles();
        _existingProjectiles.Clear();
        _seededProjectiles.Clear();
        _hasProjectileBaseline = false;
        Elapsed = 0f;
        SetMoveDirection(_mgMove, Vector3.zero);
        SetMoveDirection(_necMove, Vector3.zero);
        ResetActor(_mg, _mgStart, ref _mgPoseOwner, ref _mgInitialRotation);
        ResetActor(_nec, _necStart, ref _necPoseOwner, ref _necInitialRotation);
        ResetActor(_army, _armyStart, ref _armyPoseOwner, ref _armyInitialRotation);
        ClearLookDirection(_mgLook);
        ClearLookDirection(_necLook);
        ApplyCamera(0f);
    }

    void Update()
    {
        if (!IsRunning) return;

        Elapsed = Mathf.Min(_duration, Elapsed + Time.deltaTime);
        float progress = GetProgress(Elapsed);
        float velocity = GetProgressVelocity(Elapsed);
        SetMoveDirection(_mgMove, (_mgEnd - _mgStart) * velocity);
        SetMoveDirection(_necMove, _actionKind == TrailerActionKind.NECTeleport
            ? Vector3.zero : (_necEnd - _necStart) * velocity);
        AimActors(GetTarget(progress));

        if (_mgAttack1Pose && Elapsed >= _mgAttack1StopTime) StopMGAttack1Pose();
        if (_necAttack1Pose && Elapsed >= _necAttack1StopTime
            && (_necAttack1 == null || !_necAttack1.IsAttacking)) StopNECAttack1Pose();
        if (_necAttack2 != null && _necAttack2.IsCharging && Elapsed >= _necReleaseTime)
            _necAttack2.ReleaseAttack();

        if (Elapsed < _duration - HandleDuration) RunActions();
        SeedNewProjectiles(_mgShoots);
        SeedNewProjectiles(_necShoots);

        if (Elapsed < _duration) return;
        StopActions();
        SetMoveDirection(_mgMove, Vector3.zero);
        SetMoveDirection(_necMove, Vector3.zero);
        IsRunning = false;
    }

    void LateUpdate()
    {
        if (!IsRunning && Elapsed == 0f) return;

        float progress = GetProgress(Elapsed);
        if (BelongsToScene(_mg)) _mg.position = Vector3.Lerp(_mgStart, _mgEnd, progress);
        // The genuine teleport API owns NEC's position during this shot.
        if (BelongsToScene(_nec) && _actionKind != TrailerActionKind.NECTeleport)
            _nec.position = Vector3.Lerp(_necStart, _necEnd, progress);
        if (BelongsToScene(_army)) _army.position = Vector3.Lerp(_armyStart, _armyEnd, progress);
        ApplyCamera(progress);
    }

    void RunActions()
    {
        bool mgVolley = _actionKind == TrailerActionKind.MGVolley
            || _actionKind == TrailerActionKind.CooperativeVolley;
        bool necBoomerang = _actionKind == TrailerActionKind.NECBoomerang
            || _actionKind == TrailerActionKind.CooperativeVolley;

        if (mgVolley && Elapsed >= _nextMGAction)
        {
            _nextMGAction += VolleyInterval;
            if (_mgAttack1 != null && _mgAttack1.CanAttack)
            {
                StopMGAttack1Pose();
                _mgAttack1.StartAttack();
                _mgAttack1Pose = true;
                _mgAttack1StopTime = Elapsed + AttackPoseDuration;
                if (_mgBrain != null) _mgBrain.OnAttack1Started?.Invoke();
                else if (_mgFeedback != null) _mgFeedback.StartAttack1();
            }
        }

        if (_actionKind == TrailerActionKind.MGOrbit && Elapsed >= _nextMGAction
            && Elapsed <= _duration - HandleDuration - 2f)
        {
            _nextMGAction += 0.2f;
            if (_mgAttack2 != null && _mgAttack2.IsAttacking)
            {
                _mgAttack2.StartAttack();
            }
            else if (_mgAttack2 != null && _mgAttack2.CanAttack)
            {
                if (_mgAttack1 != null) _mgAttack1.CancelAttack();
                StopMGAttack1Pose();
                _mgAttack2.StartAttack();
                _mgAttack2Pose = _mgAttack2.IsAttacking;
                if (_mgAttack2Pose)
                {
                    if (_mgBrain != null) _mgBrain.OnAttack2Started?.Invoke();
                    else if (_mgFeedback != null) _mgFeedback.StartAttack2();
                }
            }
        }

        if (necBoomerang && Elapsed >= _nextNECAction)
        {
            _nextNECAction += BoomerangInterval;
            if (_necAttack1 != null && _necAttack1.CanAttack)
            {
                StopNECAttack1Pose();
                _necAttack1.StartAttack();
                _necAttack1Pose = true;
                _necAttack1StopTime = Elapsed + AttackPoseDuration;
                if (_necBrain != null) _necBrain.OnAttack1Started?.Invoke();
                else if (_necFeedback != null) _necFeedback.StartAttack1();
            }
        }

        if (_actionKind == TrailerActionKind.NECTeleport && Elapsed >= _nextNECAction
            && Elapsed <= _duration - HandleDuration - 2f)
        {
            _nextNECAction += TeleportInterval;
            if (_necAttack2 != null && _necAttack2.CanAttack)
            {
                if (_necAttack1 != null) _necAttack1.StopAttack();
                StopNECAttack1Pose();
                _necAttack2.StartAttack();
                _necAttack2Pose = _necAttack2.IsAttacking;
                _necReleaseTime = Elapsed + ChargeHoldDuration;
                if (_necAttack2Pose)
                {
                    if (_necBrain != null) _necBrain.OnAttack2Started?.Invoke();
                    else if (_necFeedback != null) _necFeedback.StartAttack2();
                }
            }
        }
    }

    void AimActors(Vector3 target)
    {
        if (_actionKind == TrailerActionKind.None) return;
        ClearLookDirection(_mgLook);
        ClearLookDirection(_necLook);

        // Attack2_MG rotates its owner through OrbitSystem.Rotated; preserve that motion.
        if (_mgAttack2 == null || !_mgAttack2.IsAttacking)
            FaceDirection(_mg, _actionKind == TrailerActionKind.ArmyMarch
                ? _mgEnd - _mgStart : target - (_mg != null ? _mg.position : Vector3.zero));
        if (_necAttack2 == null || !_necAttack2.IsTeleporting)
            FaceDirection(_nec, _actionKind == TrailerActionKind.ArmyMarch
                || (_actionKind == TrailerActionKind.NECTeleport && (_necEnd - _necStart).sqrMagnitude > 0.001f)
                ? _necEnd - _necStart : target - (_nec != null ? _nec.position : Vector3.zero));
        if (_actionKind == TrailerActionKind.ArmyMarch) FaceDirection(_army, _armyEnd - _armyStart);
    }

    void FaceDirection(Transform actor, Vector3 direction)
    {
        if (!BelongsToScene(actor)) return;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f) actor.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    static void SetMoveDirection(MoveSystem move, Vector3 velocity)
    {
        if (move == null) return;
        move.MoveDirection = move.Speed > 0f
            ? Vector2.ClampMagnitude(new Vector2(velocity.x, velocity.z) / move.Speed, 1f) : Vector2.zero;
    }

    static void ClearLookDirection(LookSystem look)
    {
        if (look == null) return;
        look.UseMouseAim = false;
        look.LookDirection = Vector2.zero;
    }

    void ResetActor(Transform actor, Vector3 position, ref Transform poseOwner, ref Quaternion rotation)
    {
        if (!BelongsToScene(actor)) return;
        if (poseOwner != actor)
        {
            poseOwner = actor;
            rotation = actor.rotation;
        }
        actor.SetPositionAndRotation(position, rotation);
    }

    float GetProgress(float time)
    {
        float ratio = Mathf.Clamp01((time - HandleDuration) / Mathf.Max(0.1f, _duration - 2f * HandleDuration));
        return ratio * ratio * (3f - 2f * ratio);
    }

    float GetProgressVelocity(float time)
    {
        float travelDuration = Mathf.Max(0.1f, _duration - 2f * HandleDuration);
        float ratio = Mathf.Clamp01((time - HandleDuration) / travelDuration);
        return 6f * ratio * (1f - ratio) / travelDuration;
    }

    Vector3 GetSubjectOffset()
    {
        return _cameraFollowSubject && BelongsToScene(_subject) ? _subject.position : Vector3.zero;
    }

    Vector3 GetTarget(float progress)
    {
        return GetSubjectOffset() + Vector3.Lerp(_targetStart, _targetEnd, progress);
    }

    void ApplyCamera(float progress)
    {
        if (_camera == null || _camera.gameObject.scene != gameObject.scene) return;
        Vector3 position = Vector3.Lerp(_cameraStart, _cameraEnd, progress);
        if (_cameraOrbitDegrees != 0f)
            position = _cameraOrbitCenter + Quaternion.AngleAxis(_cameraOrbitDegrees * progress, Vector3.up)
                * (position - _cameraOrbitCenter);
        position += GetSubjectOffset();
        Vector3 direction = GetTarget(progress) - position;
        Quaternion rotation = direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction, Vector3.up) : _camera.transform.rotation;
        _camera.transform.SetPositionAndRotation(position, rotation);
        _camera.Lens.FieldOfView = Mathf.Lerp(_fovStart, _fovEnd, progress);
    }

    bool BelongsToScene(Transform actor)
    {
        return actor != null && actor.gameObject.scene == gameObject.scene;
    }

    bool HasSceneReferences()
    {
        return _camera != null && _camera.gameObject.scene == gameObject.scene
            && (_subject == null || BelongsToScene(_subject))
            && (_mg == null || BelongsToScene(_mg))
            && (_nec == null || BelongsToScene(_nec))
            && (_army == null || BelongsToScene(_army));
    }

    void CacheReferences()
    {
        if (BelongsToScene(_mg))
        {
            _mgAttack1 = _mg.GetComponentInChildren<Attack1_MG>(true);
            _mgAttack2 = _mg.GetComponentInChildren<Attack2_MG>(true);
            _mgBrain = _mg.GetComponentInChildren<Brain_MG>(true);
            _mgFeedback = _mg.GetComponentInChildren<Feedback_MG>(true);
            _mgMove = _mg.GetComponentInChildren<MoveSystem>(true);
            _mgLook = _mg.GetComponentInChildren<LookSystem>(true);
            _mgShoots = _mg.GetComponentsInChildren<ShootSystem>(true);
            if (_mgAttack2 != null) _mgAttack2.Finished += OnMGAttack2Finished;
        }
        if (BelongsToScene(_nec))
        {
            _necAttack1 = _nec.GetComponentInChildren<Attack1_NEC>(true);
            _necAttack2 = _nec.GetComponentInChildren<Attack2_NEC>(true);
            _necBrain = _nec.GetComponentInChildren<Brain_NEC>(true);
            _necFeedback = _nec.GetComponentInChildren<Feedback_NEC>(true);
            _necMove = _nec.GetComponentInChildren<MoveSystem>(true);
            _necLook = _nec.GetComponentInChildren<LookSystem>(true);
            _necShoots = _nec.GetComponentsInChildren<ShootSystem>(true);
            if (_necAttack2 != null) _necAttack2.Finished += OnNECAttack2Finished;
        }
    }

    void ReleaseReferences()
    {
        if (_mgAttack2 != null) _mgAttack2.Finished -= OnMGAttack2Finished;
        if (_necAttack2 != null) _necAttack2.Finished -= OnNECAttack2Finished;
        _mgAttack1 = null; _mgAttack2 = null; _mgBrain = null; _mgFeedback = null;
        _mgMove = null; _mgLook = null; _mgShoots = new ShootSystem[0];
        _necAttack1 = null; _necAttack2 = null; _necBrain = null; _necFeedback = null;
        _necMove = null; _necLook = null; _necShoots = new ShootSystem[0];
    }

    void OnMGAttack2Finished()
    {
        if (!_mgAttack2Pose) return;
        _mgAttack2Pose = false;
        if (_mgBrain != null) _mgBrain.OnAttack2Stoped?.Invoke();
        else if (_mgFeedback != null) _mgFeedback.StopAttack2();
    }

    void OnNECAttack2Finished()
    {
        if (!_necAttack2Pose) return;
        _necAttack2Pose = false;
        if (_necBrain != null) _necBrain.OnAttack2Stoped?.Invoke();
        else if (_necFeedback != null) _necFeedback.StopAttack2();
    }

    void StopMGAttack1Pose()
    {
        if (!_mgAttack1Pose) return;
        _mgAttack1Pose = false;
        if (_mgBrain != null) _mgBrain.OnAttack1Stoped?.Invoke();
        else if (_mgFeedback != null) _mgFeedback.StopAttack1();
    }

    void StopNECAttack1Pose()
    {
        if (!_necAttack1Pose) return;
        _necAttack1Pose = false;
        if (_necBrain != null) _necBrain.OnAttack1Stoped?.Invoke();
        else if (_necFeedback != null) _necFeedback.StopAttack1();
    }

    void StopActions()
    {
        if (_mgAttack1 != null) _mgAttack1.CancelAttack();
        if (_mgAttack2 != null) _mgAttack2.StopAttack();
        if (_necAttack1 != null) _necAttack1.StopAttack();
        if (_necAttack2 != null) _necAttack2.StopAttack();
        StopMGAttack1Pose();
        StopNECAttack1Pose();
        OnMGAttack2Finished();
        OnNECAttack2Finished();
    }

    static void SeedParticles(Transform root, uint seed)
    {
        if (root == null) return;
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].useAutoRandomSeed = false;
            particles[i].randomSeed = seed + (uint)i;
        }
    }

    void SeedNewProjectiles(ShootSystem[] shoots)
    {
        foreach (ShootSystem shoot in shoots)
        {
            if (shoot == null || shoot.LastProjectile == null
                || shoot.LastProjectile.scene != gameObject.scene) continue;
            if (_seededProjectiles.Add(shoot.LastProjectile.GetInstanceID()))
                SeedParticles(shoot.LastProjectile.transform, ParticleSeed + 300);
        }
    }

    void CleanupProjectiles()
    {
        if (!_hasProjectileBaseline) return;
        foreach (Projectile projectile in Object.FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (projectile.gameObject.scene != gameObject.scene || _existingProjectiles.Contains(projectile)) continue;
            Transform owner = projectile.Owner;
            if (owner == null || (owner != _mg && owner != _nec
                && !owner.IsChildOf(_mg != null ? _mg : transform)
                && !owner.IsChildOf(_nec != null ? _nec : transform))) continue;
            Destroy(projectile.gameObject);
        }
    }

    void OnDisable()
    {
        IsRunning = false;
        if (Application.isPlaying) StopActions();
        SetMoveDirection(_mgMove, Vector3.zero);
        SetMoveDirection(_necMove, Vector3.zero);
        ReleaseReferences();
    }
}
