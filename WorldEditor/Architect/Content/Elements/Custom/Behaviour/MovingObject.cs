using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class MovingObject : MonoBehaviour
{
    public float trackDistance = 10;
    public float offset;
    public float speed = 5;
    public float pauseTime;
    public float rotation;
    public float rotationOverTime;
    public float smoothing = 0.5f;

    private Vector3 _startPos;
    private float _pauseRemaining;
    private Transform _movingPart;
    private bool _platform;
    private float _targetSpeed;
    private float _currentSpeed;
    private bool _flipped;

    private void Awake()
    {
        _targetSpeed = speed;
        _currentSpeed = speed;
        
        if (gameObject.layer == 8)
        {
            if (!transform.parent) transform.SetParent(new GameObject("[Architect] Motion Parent").transform);
            _movingPart = transform.parent;
            _platform = true;
        }
        else _movingPart = transform;
        
        _startPos = _movingPart.position;
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
                offset += speed * -_pauseRemaining;
                _pauseRemaining = 0;
            } else return;
        }

        rotation += rotationOverTime * Time.deltaTime;
        
        var radians = rotation * Mathf.Deg2Rad;

        var newPos = _startPos + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * offset;
        _movingPart.position = newPos;

        _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, smoothing <= 0 ? 1 : Mathf.Min(Time.deltaTime / smoothing, 1));
        offset += _currentSpeed * Time.deltaTime;
        
        if (offset >= trackDistance && !_flipped)
        {
            _flipped = true;
            offset = 2 * trackDistance - offset;
            _targetSpeed = -speed;
            _pauseRemaining = pauseTime;
        } else if (offset <= 0 && _flipped)
        {
            _flipped = false;
            offset = -offset;
            _targetSpeed = speed;
            _pauseRemaining = pauseTime;
        }
    }
}