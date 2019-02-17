using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private AnimationCurve _visualWidthCurve;

    [Header("Movement")]
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
    [SerializeField] private float _boostSpeedMultipler = 2f;
    [SerializeField] private float _boostAngularSpeedMultipler = 2f;
    private float _boostTime;
    private float _boostSlowTime = 1f;



    //Line Renderer
    private List<Vector3> _positions;
    private Vector3 _lastPosition;
    private float _minDistanceFowNewPoint = 0.1f;

    //Movement
    private Vector2 _direction = Vector2.up;
    private float _currentSpeed;
    private float _angleSpeedRegular;
    private float _angleSpeedOpposite;

    #region ACCESSORS

    public bool CanMove
    {
        get { return _currentLifetime > 0; }
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
        _positions = new List<Vector3>(5000);
        _lastPosition = transform.position;
        _currentLifetime = _startLifetime;
        AddPointToRenderer(transform.position);
    }

    void Update()
    {
        UpdateLifetime();
        UpdatePlayer();
        UpdateLineRendererPoint();
    }

    private void UpdateLifetime()
    {
        _currentLifetime = Mathf.Max(0, _currentLifetime - Time.deltaTime);
        if(_currentLifetime > 0)
        {
            SetTipWidth(_visualWidthCurve.Evaluate(_currentLifetime / _startLifetime));
        }
        else
        {
            SetTipWidth(_visualWidthCurve.Evaluate(0f));
        }
    }

    private void UpdatePlayer()
    {
        if (!CanMove) return;

        CalculateCurrentSpeed();
        CalculateCurrentAngularSpeed();

        bool goingLeft = Input.GetKey(KeyCode.A);
        bool goingRight = Input.GetKey(KeyCode.D);
 
        if (goingLeft && !goingRight)
        {
            _direction = _direction.Rotate(_direction.x > 0 ? _angleSpeedOpposite : _angleSpeedRegular);
        }
        else if (!goingLeft && goingRight)
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
        transform.position += (Vector3) (_direction * _currentSpeed * Time.deltaTime);
    }

    private void CalculateCurrentSpeed()
    {
        float speed = _speed;

        if(_boostTime > 0)
        {
            speed *= (1 + _boostTimeCurve.Evaluate(_boostTime / _boostSlowTime)) * _boostSpeedMultipler;
        }

        _currentSpeed = (Input.GetKey(KeyCode.W)) ? speed * 2f : speed;
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
        frames[1].value = wantedWidth;
        _lineRenderer.widthCurve = new AnimationCurve(frames);
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
