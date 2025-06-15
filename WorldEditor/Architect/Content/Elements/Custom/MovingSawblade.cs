using UnityEngine;

namespace Architect.Content.Elements.Custom;

public class MovingSawblade : MonoBehaviour
{
    public float trackDistance = 10;
    public float offset;
    public float speed = 5;
    public float pauseTime = 1;
    public int rotation;

    private Vector3 _startPos;
    private float _pauseRemaining;

    private void Awake()
    {
        _startPos = transform.position;
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
        
        var radians = rotation * Mathf.Deg2Rad;
        transform.position = _startPos + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * offset;
        offset += speed * Time.deltaTime;
        
        if (offset >= trackDistance)
        {
            offset = 2 * trackDistance - offset;
            speed = -speed;
            _pauseRemaining = pauseTime;
        } else if (offset <= 0)
        {
            offset = -offset;
            speed = -speed;
            _pauseRemaining = pauseTime;
        }
    }
}