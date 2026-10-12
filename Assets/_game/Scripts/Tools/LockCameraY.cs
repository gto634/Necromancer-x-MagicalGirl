using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public class LockCameraY : CinemachineExtension
{
    [SerializeField]
    [Tooltip("Hauteur fixe de la caméra dans le monde.")]
    private float _height = 8f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize)
            return;

        float heightCorrection = _height - state.GetFinalPosition().y;
        state.PositionCorrection += Vector3.up * heightCorrection;
    }
}