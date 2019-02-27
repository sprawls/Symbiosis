using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float _boostTimePerBoost = 2f;
    [SerializeField] private AnimationCurve _boostTimeCurve;
    [SerializeField] private float _boostSpeedMultipler = 1.5f;
    [SerializeField] private float _boostAngularSpeedMultipler = 2f;
    private float _boostTime;
    private float _boostSlowTime = 1f;

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

    public bool CanMove
    {
        get { return _currentLifetime > 0; }
    }

    public bool IsControlledByMainPlayer {
        get { return _playerType == PlayerTypeEnum.Player1; }
    }

    public PlayerTypeEnum PlayerType {
        get { return _playerType; }
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

    void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _positions = new List<Vector3>(5000);
        _lastPosition = transform.position;
        _currentLifetime = _startLifetime;
        AddPointToRenderer(transform.position);
    }

    void Update()
    {
        UpdateInputs();
        UpdateLifetime();
        UpdatePlayer();
        UpdateLineRendererPoint();

        if(Input.GetKeyDown(KeyCode.E)) {
            SpawnBranch();
        }
    }

    private void UpdateLifetime()
    {
        float prevLifetime = _currentLifetime;
        _currentLifetime = Mathf.Max(0, _currentLifetime - Time.deltaTime);
        if(_currentLifetime > 0)
        {
            SetTipWidth(_visualWidthCurve.Evaluate(_currentLifetime / _startLifetime));
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
        if(IsControlledByMainPlayer) {
            _goingLeft = Input.GetKey(KeyCode.A);
            _goingRight = Input.GetKey(KeyCode.D);
            _goingForward = Input.GetKey(KeyCode.W);
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
        _tipTransform.LookAt(transform);
    }

    private void CalculateCurrentSpeed()
    {
        float speed = _speed;

        if(_boostTime > 0)
        {
            speed *= (1 + _boostTimeCurve.Evaluate(_boostTime / _boostSlowTime)) * _boostSpeedMultipler;
            _boostTime -= Time.deltaTime;
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

    private void AddBoost()
    {
        _boostTime += _boostTimePerBoost;
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
    }

    #endregion

    #region Branches

    private void SpawnBranch() {
        GameObject spawnedBranch = Instantiate(_branchPrefab, transform);
    }

    #endregion

    #region Callback

    private void Callback_OnEnergySeedGathered(EnergySeed seed)
    {
        if(seed != null)
        {
            AddLifetime(seed.AmountEnergy);
            AddBoost();
        }
    }

    #endregion
}
