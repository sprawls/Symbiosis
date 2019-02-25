using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraBehavior {
        Follow = 0,
        Unzoom, 
        SeedZoom
    }

    [Header("Default Behavior")]
    [SerializeField] private Transform _target;
    [SerializeField] private float _zoomDistance = 15f;
    [SerializeField] private float _smoothTime = 0.75f;

    [Header("Unzoom Behavior")]
    [SerializeField] private float _unzoomDistancePerHeightUnit = 0.75f;
    [SerializeField] private float _unzoomSmoothTime = 1.5f;


    private Vector3 _curVelocity;
    private float _curSizeVelocity;
    private Camera _camera;
    private CameraBehavior _currentBehavior;

    private void OnEnable() {
        SeedController.OnSeedLifetimeExpired += Callback_OnSeedLifetimeExpired;
        _camera = GetComponent<Camera>();
    }

    private void OnDisable() {
        SeedController.OnSeedLifetimeExpired -= Callback_OnSeedLifetimeExpired;
    }

    private void Update()
    {
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

    public void SetBehavior(CameraBehavior behavior) {
        _currentBehavior = behavior;
    }

    private void FollowBehavior() {
        Vector3 targetPosition = _target.position + (-Vector3.forward * _zoomDistance);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _curVelocity, _smoothTime);
    }

    private void UnzoomBehavior() {
        //Vector3 targetPosition = new Vector3(0f, _target.position.y / 2f, -_zoomDistance - (_target.position.y * _unzoomDistancePerHeightUnit));
        Vector3 targetPosition = new Vector3(0f, _target.position.y / 2f, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _curVelocity, _unzoomSmoothTime);
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, 5 + (_target.position.y * _unzoomDistancePerHeightUnit), ref _curSizeVelocity, _unzoomSmoothTime);
    }

    #region Callback

    private void Callback_OnSeedLifetimeExpired(SeedController seedController) {
        if(seedController != null && seedController.IsControlledByMainPlayer) {
            SetBehavior(CameraBehavior.Unzoom);
        }
    }

    #endregion
}
