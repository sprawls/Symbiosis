using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameStarted;
    public static event Action OnGameStopped;

    public SeedController _seedController1;
    public SeedController _seedController2;
    public SeedSpawner _spawner;

    private bool _playing = false;

    public void OnEnable() {
        SeedController.OnSeedLifetimeExpired += Callback_OnSeedLifetimeExpired;
    }
    public void OnDisable() {
        SeedController.OnSeedLifetimeExpired -= Callback_OnSeedLifetimeExpired;
    }

    private void Start() {
        StartGame();
    }

    private void StartGame() {
        _seedController1.StartSeed(_seedController1.transform.position);
        _seedController2.StartSeed(_seedController2.transform.position);
        _spawner.SpawnAllSeeds(_seedController1, _seedController2);
        _playing = true;

        if(OnGameStarted != null) OnGameStarted();
    }

    private void StopGame() {
        _playing = false;

        if (OnGameStopped != null) OnGameStopped();
    }

    public void Update() {
        _seedController1.UpdateSeed();
        _seedController2.UpdateSeed();
        
    }


    #region Callback

    private void Callback_OnSeedLifetimeExpired(SeedController seedController) {
        if (!_seedController1.CanMove && !_seedController2.CanMove) {
            StopGame();
        }
    }

    #endregion
}
