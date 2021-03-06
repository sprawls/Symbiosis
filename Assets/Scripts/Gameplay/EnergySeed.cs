﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class EnergySeed : MonoBehaviour
{
    public static event Action<EnergySeed> OnEnergySeedGathered;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private float _energyToGive = 5f;
    [SerializeField] private SeedController.PlayerTypeEnum _seedType;

    #region ACCESSORS

    public float AmountEnergy
    {
        get { return _energyToGive; }
    }

    public SeedController.PlayerTypeEnum Type {
        get { return _seedType; }
    }

    #endregion


    public void Setup(SeedController.PlayerTypeEnum type, Color col) {
        _renderer.color = col;
        _seedType = type;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        SeedController controller = coll.GetComponent<SeedController>();

        if (controller.PlayerType == _seedType) {
            if (OnEnergySeedGathered != null) {
                OnEnergySeedGathered(this);
            }

            Destroy(gameObject);
        }
 
    }
}
