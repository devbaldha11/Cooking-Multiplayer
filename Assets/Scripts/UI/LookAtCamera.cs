using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineVirtualCamera Camera;

    private void Start()
    {
        Camera = MultiplayerManager.Instance.Cinemachine;
    }

    private void LateUpdate()
    {
        if (Camera != null)
            transform.forward = Camera.transform.forward;
    }
}
