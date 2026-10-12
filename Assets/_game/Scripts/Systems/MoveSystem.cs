using UnityEngine;
using UnityEngine.InputSystem;

public class MoveSystem : MonoBehaviour
{
    [SerializeField] Transform _rootTransform;
    [SerializeField] float _speed = 5f;
    public float Speed { get => _speed; set => _speed = value; }
    public Vector2 MoveDirection { get; set; }

    private void Reset()
    {
        _rootTransform = transform.root;
    }

    void Update()
    {
        _rootTransform.Translate(new Vector3(MoveDirection.x, 0f, MoveDirection.y) * _speed * Time.deltaTime, Space.World);
    }
}