﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public enum CameraBehavior {
        Follow = 0,
        Unzoom, 
        SeedZoom
    }

    [Header("Default Behavior")]
    [SerializeField] private Transform _target1;
    [SerializeField] private Transform _target2;
    [SerializeField] private float _zoomDistance = 15f;
    [SerializeField] private float _smoothTime = 0.75f;

    [Header("Zoom Behavior")]
    [SerializeField] private float _zoomDistancePerHeightUnit = 0.25f;
    [SerializeField] private float _zoomSmoothTime = 1.5f;

    [Header("Unzoom Behavior")]
    [SerializeField] private float _unzoomDistancePerHeightUnit = 0.75f;
    [SerializeField] private float _unzoomSmoothTime = 1.5f;

    private Vector3 _targetPosition;
    private float _distanceBetweenTargets;
    private Vector3 _curVelocity;
    private float _curSizeVelocity;
    private CinemachineVirtualCamera _camera;
    private CameraBehavior _currentBehavior;

    public Vector3 TargetPosition {
        get { return _targetPosition; }
    }

    private void OnEnable() {
        _camera = GetComponent<CinemachineVirtualCamera>();

        GameManager.OnGameStopped += Callback_OnGameStopped;
    }

    private void OnDisable() {
        GameManager.OnGameStopped -= Callback_OnGameStopped;

    }

    private void Update()
    {
        UpdateTargetPosition();

        switch(_currentBehavior) {
            case CameraBehavior.Follow:
                FollowBehavior();
                break;
            case CameraBehavior.Unzoom:
                UnzoomBehavior();
                break;
            case CameraBehavior.SeedZoom:
                break;


        }
    }

    private void UpdateTargetPosition() {
        Vector3 fromTo = (_target2.position - _target1.position);
        _distanceBetweenTargets = fromTo.magnitude;
        _targetPosition = _target1.position + fromTo / 2f;
    }

    public void SetBehavior(CameraBehavior behavior) {
        _currentBehavior = behavior;
    }

    private void FollowBehavior() {
        Vector3 targetPosition = _targetPosition + (-Vector3.forward * _zoomDistance);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _curVelocity, _smoothTime);

        _camera.m_Lens.OrthographicSize = Mathf.SmoothDamp(_camera.m_Lens.OrthographicSize, 5 + (_distanceBetweenTargets * _zoomDistancePerHeightUnit), ref _curSizeVelocity, _zoomSmoothTime);
    }

    private void UnzoomBehavior() {
        float highestPosition = Mathf.Max(_target1.position.y, _target2.position.y);

        //Vector3 targetPosition = new Vector3(0f, _target.position.y / 2f, -_zoomDistance - (_target.position.y * _unzoomDistancePerHeightUnit));
        Vector3 targetPosition = new Vector3(0f, highestPosition / 2f, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _curVelocity, _unzoomSmoothTime);
        _camera.m_Lens.OrthographicSize = Mathf.SmoothDamp(_camera.m_Lens.OrthographicSize, 5 + (highestPosition * _unzoomDistancePerHeightUnit), ref _curSizeVelocity, _unzoomSmoothTime);
    }

    #region Callback

    private void Callback_OnGameStopped() {
        SetBehavior(CameraBehavior.Unzoom);
    }

    #endregion
}
