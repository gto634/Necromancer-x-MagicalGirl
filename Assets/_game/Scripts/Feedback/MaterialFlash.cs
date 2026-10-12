using System.Collections.Generic;
using UnityEngine;

public class MaterialFlash : MonoBehaviour
{
    [SerializeField] GameObject _visualRoot;

    [SerializeField] Material _flashMaterial;

    [SerializeField, Min(0f)] float _flashDuration;
    [SerializeField, ReadOnly] float _flashTimer;

    List<Renderer> _renderers = new List<Renderer>();
    List<Material[]> _originalMaterials = new List<Material[]>();
    List<Material[]> _flashMaterials = new List<Material[]>();

    bool _isFlashing = false;
    bool _isInitialized;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (_isInitialized || _visualRoot == null) return;

        _renderers = new List<Renderer>(_visualRoot.GetComponentsInChildren<Renderer>());

        foreach (Renderer renderer in _renderers)
        {
            _originalMaterials.Add(renderer.sharedMaterials);

            Material[] materials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _flashMaterial;
            }

            _flashMaterials.Add(materials);
        }
        _isInitialized = true;
    }

    public void Flash()
    {
        Flash(_flashMaterial, _flashDuration);
    }

    void Flash(Material material, float duration)
    {
        if (material == null) return;
        Initialize();

        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i] == null) continue;
            for (int slot = 0; slot < _flashMaterials[i].Length; slot++)
                _flashMaterials[i][slot] = material;
            _renderers[i].sharedMaterials = _flashMaterials[i];
        }

        _isFlashing = true;
        _flashTimer = duration;
    }

    void Update()
    {
        if (_isFlashing)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
            {
                StopFlash();
            }
        }
    }

    void StopFlash()
    {
        if (!_isFlashing) return;
        _isFlashing = false;
        for (int i = 0; i < _renderers.Count; i++)
            if (_renderers[i] != null) _renderers[i].sharedMaterials = _originalMaterials[i];
    }

    void OnDisable()
    {
        StopFlash();
    }
}
