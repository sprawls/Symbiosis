using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class SeedController : MonoBehaviour
{
    public enum PlayerTypeEnum {
        Player1,
        Player2,
        AI
    }

    public static event Action<SeedController> OnSeedLifetimeExpired;

    [Header("Visuals")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private AnimationCurve _visualWidthCurve;
    [SerializeField] private Transform _tipTransform;
    [SerializeField] private Color _seedColor;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer[] _rendererToColor;

    [Header("Movement")]
    [SerializeField] private PlayerTypeEnum _playerType;
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private float _angleSpeed = 90f;
    [SerializeField] private float _angleSpeedMultiplerNoInputs = 0.25f;
    [SerializeField] private float _maxRotationAngle = 120f;
    [SerializeField] private AnimationCurve _rotationSpeedCurve;
    [SerializeField] private AnimationCurve _rotationSpeedCurveOppositeDirection;

    [Header("Life")]
    [SerializeField] private float _startLifetime = 6f;
    private float _currentLifetime;

    [Header("Boost")]
    [SerializeField] private AnimationCurve _sharedBoostEffiencyBasedOnRange;
    [SerializeField] private float _boostTimePerBoost = 2f;
    [SerializeField] private AnimationCurve _boostTimeCurve;
    [SerializeField] private float _boostSpeedMultipler = 1.5f;
    [SerializeField] private float _boostAngularSpeedMultipler = 2f;
    private float _boostTime;
    private float _boostSlowTime = 1f;

    [Header("Proximity Decay")]
    [SerializeField] private float _proximityRangeForDecay = 5f;
    [SerializeField] private SeedController _linkedSeedController;
    [SerializeField] private float _decayMultiplierDefault = 1f;
    [SerializeField] private float _decayMultiplierWhenNearby = 0.5f;
    private float _decayMultiplier;

    [Header("Branches")]
    [SerializeField] private GameObject _branchPrefab;

    //Line Renderer
    private List<Vector3> _positions;
    private Vector3 _lastPosition;
    private float _minDistanceFowNewPoint = 0.02f;

    //Movement
    private Rigidbody2D _rigidBody2D;
    private Vector2 _direction = Vector2.up;
    private float _currentSpeed;
    private float _angleSpeedRegular;
    private float _angleSpeedOpposite;

    //Inputs
    private bool _goingLeft = false;
    private bool _goingRight = false;
    private bool _goingForward = false;


    #region ACCESSORS

    public Color SeedColor {
        get { return _seedColor; }
    }

    public bool CanMove
    {
        get { return _currentLifetime > 0; }
    }

    public bool IsControlledByPlayer {
        get { return _playerType != PlayerTypeEnum.AI; }
    }

    public PlayerTypeEnum PlayerType {
        get { return _playerType; }
    }

    public float LifetimeRatio {
        get {
            return _currentLifetime / _startLifetime;
        }
    }

    public Vector3 LastPosition {
        get { return _lastPosition; }
    }

    public float GetDistanceToLinkedController() {
        return Vector3.Distance(_linkedSeedController.LastPosition, LastPosition);
    }

    #endregion

    #region LIFECYCLE

    private void OnEnable()
    {
        EnergySeed.OnEnergySeedGathered += Callback_OnEnergySeedGathered;
    }

    private void OnDisable()
    {
        EnergySeed.OnEnergySeedGathered -= Callback_OnEnergySeedGathered;
    }

    void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _positions = new List<Vector3>(5000);

        foreach(SpriteRenderer spr in _rendererToColor) {
            spr.color = _seedColor;
        }
        _lineRenderer.material.color = _seedColor;

    }

    public void StartSeed(Vector3 startPos) {
        transform.position = startPos;
        _positions.Clear();
        _lastPosition = transform.position;
        _currentLifetime = _startLifetime;
        AddPointToRenderer(transform.position);

        _direction = _direction.Rotate(Random.Range(-15,15));
    }

    public void UpdateSeed()
    {
        UpdateInputs();
        UpdateProximityDecay();
        UpdateLifetime();
        UpdatePlayer();
        UpdateLineRendererPoint();

        if(Input.GetKeyDown(KeyCode.E)) {
            SpawnBranch();
        }
    }

    private void UpdateProximityDecay() {
        _decayMultiplier = _decayMultiplierDefault;

        if(_linkedSeedController != null) {
            float dist = GetDistanceToLinkedController();
            if(dist < _proximityRangeForDecay) {
                _animator.SetBool("Nearby", true);
                if (LifetimeRatio < _linkedSeedController.LifetimeRatio) {
                    _decayMultiplier = _decayMultiplierWhenNearby;
                }
            } else {
                _animator.SetBool("Nearby", false);
            }
        }

    }

    private void UpdateLifetime()
    {
        float prevLifetime = _currentLifetime;
        float speedMultiplier = _goingForward ? 1f : 0.75f;
        _currentLifetime = Mathf.Max(0, _currentLifetime - (Time.deltaTime * _decayMultiplier * speedMultiplier));

        if(_currentLifetime > 0)
        {
            SetTipWidth(_visualWidthCurve.Evaluate(LifetimeRatio));
        }
        else
        {
            if(prevLifetime > 0) {
                if (OnSeedLifetimeExpired != null) OnSeedLifetimeExpired(this);
            }
            SetTipWidth(_visualWidthCurve.Evaluate(0f));
        }
    }

    private void UpdateInputs() {
        if(IsControlledByPlayer) {
            if(PlayerType == PlayerTypeEnum.Player1) {
                _goingLeft = Input.GetKey(KeyCode.A);
                _goingRight = Input.GetKey(KeyCode.D);
                _goingForward = Input.GetKey(KeyCode.W);
            } else {
                _goingLeft = Input.GetKey(KeyCode.LeftArrow);
                _goingRight = Input.GetKey(KeyCode.RightArrow);
                _goingForward = Input.GetKey(KeyCode.UpArrow);
            }

        }
    }

    private void UpdatePlayer()
    {
        if (!CanMove) return;

        CalculateCurrentSpeed();
        CalculateCurrentAngularSpeed();
 
        if (_goingLeft && !_goingRight)
        {
            _direction = _direction.Rotate(_direction.x > 0 ? _angleSpeedOpposite : _angleSpeedRegular);
        }
        else if (!_goingLeft && _goingRight)
        {
            _direction = _direction.Rotate(_direction.x < 0 ? -_angleSpeedOpposite : -_angleSpeedRegular);
        }
        else if(Mathf.Abs(_direction.x) > 0.05f)
        {
            float scaledSpeed = _direction.x > 0 ? _angleSpeedRegular : -_angleSpeedRegular;
            scaledSpeed = scaledSpeed * _angleSpeedMultiplerNoInputs;
            _direction = _direction.Rotate(scaledSpeed);
        }

        //Debug.Log("Direction :" + _direction + "     angle speed : " + angleSpeedRegular + "      Opposite : " + angleSpeedOpposite);
        Vector3 newPos = transform.position + (Vector3)(_direction * _currentSpeed * Time.deltaTime);
        _rigidBody2D.MovePosition(newPos);

        _tipTransform.position = _lastPosition;
        _tipTransform.LookAt(transform, _tipTransform.up);
    }

    private void CalculateCurrentSpeed()
    {
        float speed = _speed;

        if(_boostTime > 0)
        {
            speed *= (1 + _boostTimeCurve.Evaluate(_boostTime / _boostSlowTime)) * _boostSpeedMultipler;

            _boostTime -= Time.deltaTime;
            if (_boostTime > _boostTimePerBoost)
                _boostTime -= Time.deltaTime * 2f;
        }

        _currentSpeed = (_goingForward) ? speed * 2f : speed;
    }

    private void CalculateCurrentAngularSpeed()
    {
        float angleFromUp = Vector2.Angle(Vector2.up, _direction);
        _angleSpeedRegular = _rotationSpeedCurve.Evaluate(angleFromUp / _maxRotationAngle) * _angleSpeed * Time.deltaTime;
        _angleSpeedOpposite = _rotationSpeedCurveOppositeDirection.Evaluate(angleFromUp / _maxRotationAngle) * _angleSpeed * Time.deltaTime;

        _angleSpeedRegular *= (1 + _boostTimeCurve.Evaluate(_boostTime / _boostSlowTime)) * _boostAngularSpeedMultipler;
        _angleSpeedOpposite *= (1 + _boostTimeCurve.Evaluate(_boostTime / _boostSlowTime)) * _boostAngularSpeedMultipler;
    }

    #endregion 

    private void AddLifetime(float time)
    {
        _currentLifetime += time;
    }

    private void AddBoost(float efficiency)
    {
        _boostTime += _boostTimePerBoost * efficiency;
    }

    #region Visuals

    private void UpdateLineRendererPoint()
    {
        float distToLastPoint = Vector3.Distance(_lastPosition, transform.position);
        if (distToLastPoint > _minDistanceFowNewPoint)
        {
            _lastPosition = transform.position;
            AddPointToRenderer(transform.position);
        }
    }

    private void AddPointToRenderer(Vector3 newPos)
    {
        _positions.Add(newPos);
        _lineRenderer.positionCount = (_positions.Count);
        _lineRenderer.SetPositions(_positions.ToArray());
    }

    private void SetTipWidth(float wantedWidth)
    {
        Keyframe[] frames = _lineRenderer.widthCurve.keys;
        frames[2].value = wantedWidth;
        _lineRenderer.widthCurve = new AnimationCurve(frames);

        _tipTransform.localScale = Vector3.one * wantedWidth;
    }

    #endregion

    #region Branches

    private void SpawnBranch() {
        GameObject spawnedBranch = Instantiate(_branchPrefab, transform);
        spawnedBranch.transform.position += new Vector3(0f, 0f, 3f);
        ParticleToLineRenderer p2lr = spawnedBranch.GetComponent<ParticleToLineRenderer>();
        if (p2lr != null) {
            float testValue = 1f - ((1f - Mathf.Clamp(_currentLifetime / _startLifetime, 0, 1)) * 0.75f);
            p2lr.SetSizeMultiplier(testValue);
        }
    }

    private IEnumerator SpawnBranchesFromSeed(float efficiency) {
        SpawnBranch();
        yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

        if (Random.Range(0f, 1f) > 1-efficiency) {
            SpawnBranch();
        }

        if (Random.Range(0f, 1f) > 1.5f - efficiency) {
            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
            SpawnBranch();
        }

        if (Random.Range(0f, 1f) > 1.5f - efficiency) {
            yield return new WaitForSeconds(Random.Range(0.35f, 1f));
            SpawnBranch();
        }
    }

    #endregion

    #region Callback

    private void Callback_OnEnergySeedGathered(EnergySeed seed)
    {
        if(seed != null)
        {
            float efficiency = 1f;
            if(seed.Type != _playerType) {
                efficiency = Mathf.Clamp(_sharedBoostEffiencyBasedOnRange.Evaluate(GetDistanceToLinkedController()), 0f, 1f);
            }
            AddLifetime(seed.AmountEnergy * efficiency);
            AddBoost(efficiency);
            StartCoroutine(SpawnBranchesFromSeed(efficiency));
        }
    }

    #endregion
}
