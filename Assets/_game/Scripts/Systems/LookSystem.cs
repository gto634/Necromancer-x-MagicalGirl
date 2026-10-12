using UnityEngine;

public class LookSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField] Camera _camera;
    [SerializeField] float _rotationSpeed = 720f;
    [SerializeField] float _minAimDistance = 0.5f;
    public Vector2 LookDirection { get; set; }
    public Vector2 AimScreenPosition { get; set; }
    public bool UseMouseAim { get; set; }

    public void SetCamera(Camera camera)
    {
        _camera = camera;
    }

    void Reset()
    {
        _rootTransform = transform.root;
    }

    void Update()
    {
        Vector3 targetDir = GetTargetDirection();
        if (targetDir == Vector3.zero) return;

        Quaternion RotatioTarget = Quaternion.LookRotation(targetDir);

        float maxDegrés = _rotationSpeed * Time.deltaTime;
        _rootTransform.rotation = Quaternion.RotateTowards(_rootTransform.rotation, RotatioTarget, maxDegrés);
        
    }

    Vector3 GetTargetDirection()
    {
        if (UseMouseAim) return GetMouseDirection();
        if(LookDirection.sqrMagnitude > 0.01f)
        {
            return new Vector3(LookDirection.x, 0f, LookDirection.y);
        }


        return Vector3.zero;
    }
    Vector3 GetMouseDirection()                
    {
        Ray ray = _camera.ScreenPointToRay(AimScreenPosition);
        Plane plane = new Plane(Vector3.up, _rootTransform.position);
        if (!plane.Raycast(ray, out float distance)) return Vector3.zero;
        Vector3 point = ray.GetPoint(distance);
        Vector3 direction = point - _rootTransform.position;
        direction.y = 0f;
        if (direction.magnitude < _minAimDistance) { return Vector3.zero; }
        return direction;
    }


    public void Rotate(float angle)
    {
        _rootTransform.Rotate(Vector3.up, angle, Space.World);
    }
}
