﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameStarted;
    public static event Action OnGameStopped;

    [SerializeField] private SeedController _seedController1;
    [SerializeField] private SeedController _seedController2;
    [SerializeField] private SeedSpawner _spawner;

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
        _seedController1.UpdateProximityDecay();
        _seedController2.UpdateProximityDecay();

        _seedController1.UpdateSeed();
        _seedController2.UpdateSeed();
        
        if(Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(0);
        }
    }


    #region Callback

    private void Callback_OnSeedLifetimeExpired(SeedController seedController) {
        if (!_seedController1.CanMove && !_seedController2.CanMove) {
            StopGame();
        }
    }

    #endregion
}
