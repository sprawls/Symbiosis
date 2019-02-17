using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _zoomDistance = 15f;
    [SerializeField] private float _smoothTime = 0.75f;

    private Vector3 _curVelocity;

    private void Update()
    {
        Vector3 targetPosition = _target.position + (new Vector3(0, 0, -1) * _zoomDistance);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _curVelocity, _smoothTime);
    }
}
