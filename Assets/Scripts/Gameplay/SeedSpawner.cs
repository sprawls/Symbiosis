using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSpawner : MonoBehaviour {

    private List<EnergySeed> _spawnedSeeds;

    private void OnEnable() {
        EnergySeed.OnEnergySeedGathered += Callback_OnEnergySeedGathered;
    }

    private void OnDisable() {
        EnergySeed.OnEnergySeedGathered -= Callback_OnEnergySeedGathered;
    }


    public void SpawnAllSeeds(SeedController seed1, SeedController seed2) {
        RemoveSpawnedSeeds();

        SpawnSeedForPlayer(seed1, true);
        SpawnSeedForPlayer(seed2, false);
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

    private void SpawnSeedForPlayer(SeedController seedController, bool left) {

    }


    #region Callback

    private void Callback_OnEnergySeedGathered(EnergySeed seed) {
        if (seed != null && _spawnedSeeds.Contains(seed)) {
            _spawnedSeeds.Remove(seed);
        }
    }

    #endregion

}
