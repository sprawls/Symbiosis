using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSpawner : MonoBehaviour {

    [SerializeField] private GameObject _seedPrefab;

    [Header("Seed Spawning Data")]
    [SerializeField] private int            _amountSeeds = 100;
    [SerializeField] private AnimationCurve _spawnCurvePosition;
    [SerializeField] private AnimationCurve _spawnCurveWidth;
    [SerializeField] private Vector2        _heightBetweenSeedsMinMax;

    private List<EnergySeed> _spawnedSeeds;

    private void OnEnable() {
        EnergySeed.OnEnergySeedGathered += Callback_OnEnergySeedGathered;
    }

    private void OnDisable() {
        EnergySeed.OnEnergySeedGathered -= Callback_OnEnergySeedGathered;
    }


    public void SpawnAllSeeds(SeedController seed1, SeedController seed2) {
        RemoveSpawnedSeeds();

        SpawnSeedsForPlayer(seed1, true);
        SpawnSeedsForPlayer(seed2, false);
    }

    private void RemoveSpawnedSeeds() {
        if(_spawnedSeeds != null) {
            for (int i = _spawnedSeeds.Count - 1; i >= 0; --i) {
                Destroy(_spawnedSeeds[i]);
            }
            _spawnedSeeds.Clear();

        }
        _spawnedSeeds = new List<EnergySeed>();

    }

    private void SpawnSeedsForPlayer(SeedController seedController, bool left) {
        //todo : un algo cool pour spawner de seeds
        Vector3 startPos = seedController.transform.position;
        Vector3 side = left ? Vector3.left : Vector3.right;
        Vector3 up = Vector3.up;
        for (int i = 0; i < _amountSeeds; ++i) {
            up += Vector3.up * Random.Range(_heightBetweenSeedsMinMax.x, _heightBetweenSeedsMinMax.y);
            float ratio = (float)i / (float)_amountSeeds;
            float randomX = _spawnCurveWidth.Evaluate(ratio);
            Vector3 pos = startPos + up + side * (_spawnCurvePosition.Evaluate(ratio) + Random.Range(-randomX, randomX/2f));

            SpawnSeed(seedController, pos);
        }
    }

    private void SpawnSeed(SeedController seedController, Vector3 pos) {
        EnergySeed spawnedSeed = Instantiate(_seedPrefab, transform).GetComponent<EnergySeed>();
        spawnedSeed.transform.position = pos;

        spawnedSeed.Setup(seedController.PlayerType, seedController.SeedColor);
    }


    #region Callback

    private void Callback_OnEnergySeedGathered(EnergySeed seed) {
        if (seed != null && _spawnedSeeds.Contains(seed)) {
            _spawnedSeeds.Remove(seed);
        }
    }

    #endregion

}
