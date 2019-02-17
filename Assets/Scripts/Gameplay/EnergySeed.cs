using System;
using System.Collections.Generic;
using UnityEngine;

public class EnergySeed : MonoBehaviour
{
    public static event Action<EnergySeed> OnEnergySeedGathered;

    [SerializeField] private float _energyToGive = 5f;

    #region ACCESSORS

    public float AmountEnergy
    {
        get { return _energyToGive; }
    }

    #endregion


    private void OnTriggerEnter2D(Collider2D coll)
    {
        if(OnEnergySeedGathered != null)
        {
            OnEnergySeedGathered(this);
        }

        Destroy(gameObject);
    }
}
