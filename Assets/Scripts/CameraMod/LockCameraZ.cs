using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's Y co-ordinate
/// </summary>
[ExecuteAlways]
[AddComponentMenu("")] // Hide in menu
public class LockCameraZ : CinemachineExtension
{
    [Tooltip("Lock the camera's Y position to this value")]
    public float zPosition = 10;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            var pos = state.RawPosition;
            pos.z = zPosition;
            state.RawPosition = pos;
        }
    }
}
