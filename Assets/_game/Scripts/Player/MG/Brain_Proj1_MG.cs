using UnityEngine;

public class Brain_Proj1_MG : MonoBehaviour
{
    [SerializeField] MoveSystem _moveSystem;
    [SerializeField] AttackSystem _attackSystem;

    Transform _rootTransform;

    [SerializeField] float _lifeDuration;

    private void Reset()
    {
        _lifeDuration = 5f;
    }

    void Start()
    {
        _rootTransform = transform.root;
        _attackSystem.Attack();

        _attackSystem.OnHit += OnHit;
    }

    void OnDestroy()
    {
        _attackSystem.OnHit -= OnHit;
    }

    void Update()
    {
        _lifeDuration -= Time.deltaTime;
        if (_lifeDuration <= 0f)
        {
            Destroy(_rootTransform.gameObject);
            return;
        }

        _moveSystem.MoveDirection = new Vector2(_rootTransform.forward.x, _rootTransform.forward.z);
    }

    void OnHit()
    {
        Destroy(_rootTransform.gameObject);
    }
}
