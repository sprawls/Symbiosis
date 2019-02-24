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
    [SerializeField] private float _unzoomDistancePerHeightUnit = 0.5f;
    [SerializeField] private float _unzoomSmoothTime = 1.5f;


    private Vector3 _curVelocity;
    private CameraBehavior _currentBehavior;

    private void OnEnable() {
        SeedController.OnSeedLifetimeExpired += Callback_OnSeedLifetimeExpired;
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
        Vector3 targetPosition = new Vector3(0f, _target.position.y / 2f, -_zoomDistance - (_target.position.y * _unzoomDistancePerHeightUnit));
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _curVelocity, _unzoomSmoothTime);
    }

    #region Callback

    private void Callback_OnSeedLifetimeExpired(SeedController seedController) {
        if(seedController != null && seedController.IsControlled) {
            SetBehavior(CameraBehavior.Unzoom);
        }
    }

    #endregion
}
