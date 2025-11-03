using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class AnimatorDelay : MonoBehaviour
{
    public float delay;
    private Animator _animator;
    private bool _done;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        if (delay > 0) _animator.StartPlayback();
        else _done = true;
    }

    private void Update()
    {
        if (_done) return;
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            _animator.StopPlayback();
            _done = true;
        }
    }
}