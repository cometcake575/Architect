using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class MovingObject : MonoBehaviour
{
    public float trackDistance = 10;
    
    public float offset;
    public float rotation;
    
    public float pauseTime;
    public float smoothing = 0.5f;

    public bool stickPlayer = true;

    private float _speed = 5;
    private float _currentSpeed;
    
    private float _rotationSpeed;
    private float _currentRotationSpeed;
    
    private Vector3 _startPos;
    private float _pauseRemaining;
    private Transform _movingPart;
    private bool _platform;
    private bool _flipped;

    public void SetSpeed(float value)
    {
        _speed = value;
        _currentSpeed = value;
    }

    public void SetRotationSpeed(float value)
    {
        _rotationSpeed = value;
        _currentRotationSpeed = value;
    }

    public void PreviewReset()
    {
        _movingPart.localPosition = Vector3.zero;
        trackDistance = 10;
        offset = 0;
        
        _speed = 5;
        _currentSpeed = 0;
        
        pauseTime = 0;
        rotation = 0;
        
        _rotationSpeed = 0;
        _currentRotationSpeed = 0;
        
        smoothing = 0.5f;
        _flipped = false;
    }

    private void Awake()
    {
        _currentSpeed = _speed;
        
        if (gameObject.layer == 8 && stickPlayer)
        {
            if (!transform.parent) transform.SetParent(new GameObject("[Architect] Motion Parent").transform);
            _movingPart = transform.parent;
            _platform = true;
        }
        else _movingPart = transform;
        
        _startPos = _movingPart.localPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_platform) return;
        
        var collisionObject = collision.gameObject;
        if (collisionObject.layer != 9) return;
        var component1 = collisionObject.GetComponent<HeroController>();
        if (component1) component1.SetHeroParent(transform.parent);
        else collisionObject.transform.SetParent(transform.parent);
        var component2 = collisionObject.GetComponent<Rigidbody2D>();
        if (!component2) return;
        component2.interpolation = RigidbodyInterpolation2D.None;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!_platform) return;
        
        var collisionObject = collision.gameObject;
        if (collisionObject.layer != 9) return;
        var component1 = collisionObject.GetComponent<HeroController>();
        if (component1) component1.SetHeroParent(null);
        else collisionObject.transform.SetParent(null);
        var component2 = collisionObject.GetComponent<Rigidbody2D>();
        if (!component2) return;
        component2.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (_pauseRemaining > 0)
        {
            _pauseRemaining -= Time.deltaTime;
            if (_pauseRemaining < 0)
            {
                offset += _speed * -_pauseRemaining;
                _pauseRemaining = 0;
            } else return;
        }

        var radians = rotation * Mathf.Deg2Rad;

        var newPos = _startPos + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * offset;
        _movingPart.localPosition = newPos;

        _currentSpeed = Mathf.Lerp(_currentSpeed, _speed, smoothing <= 0 ? 1 : Mathf.Min(Time.deltaTime / smoothing, 1));
        offset += _currentSpeed * Time.deltaTime;

        _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, _rotationSpeed, smoothing <= 0 ? 1 : Mathf.Min(Time.deltaTime / smoothing, 1));
        rotation += _currentRotationSpeed * Time.deltaTime;
        
        if (offset >= trackDistance && !_flipped)
        {
            _flipped = true;
            
            _speed = -_speed;
            _rotationSpeed = -_rotationSpeed;
            
            _pauseRemaining = pauseTime;
        } else if (offset <= 0 && _flipped)
        {
            _flipped = false;
            
            _speed = -_speed;
            _rotationSpeed = -_rotationSpeed;
            
            _pauseRemaining = pauseTime;
        }
    }
}