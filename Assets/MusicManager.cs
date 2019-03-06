using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource _happySource;
    public AudioSource _sadSource;

    public AnimationCurve _volumeHappyOverTime;
    public AnimationCurve _volumeSadOverTime;

    public SeedController _seedController;

    // Start is called before the first frame update
    void Start()
    {
        _happySource.volume = 1f;
        _sadSource.volume = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        float ratio = 1f - _seedController.LifetimeRatio;
        SetAudio(ratio);
    }

    private void SetAudio(float ratio) {
        _happySource.volume = _volumeHappyOverTime.Evaluate(ratio);
        _sadSource.volume = _volumeSadOverTime.Evaluate(ratio);
    }
}
