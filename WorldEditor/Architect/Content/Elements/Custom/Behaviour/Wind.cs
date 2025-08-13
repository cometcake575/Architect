using System.Reflection;
using Architect.Util;
using GlobalEnums;
using Modding;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Wind : MonoBehaviour
{
    private static bool _actuallyJumping;
    private static MethodInfo _setState;
    private static Material _windMaterial;
    
    public float speed = 30;
    private Vector3 _force;
    private bool _setForce;

    public float r = 1;
    public float g = 1;
    public float b = 1;
    public float a = 1;

    private ParticleSystem.MainModule? _main;
    private ParticleSystem.EmissionModule? _emission;

    public static void Init()
    {
        ModHooks.HeroUpdateHook += () =>
        {
            if (HeroController.instance.cState.jumping
                || HeroController.instance.cState.doubleJumping
                || HeroController.instance.cState.wallJumping)
            {
                _actuallyJumping = true;
            }
            else if (HeroController.instance.GetComponent<Rigidbody2D>().velocity.y < 0) _actuallyJumping = false;
        };
        
        On.HeroController.JumpReleased += (orig, self) =>
        {
            if (!_actuallyJumping) return;
            _actuallyJumping = false;
            orig(self);
        };
        
        _setState = typeof(HeroController).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic);
        
        _windMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            mainTexture = ResourceUtils.LoadInternal("wind_particle", FilterMode.Point).texture
        };
    }

    private static bool _windPlayer;
    
    private void OnTriggerStay2D(Collider2D other)
    {
        var rb2d = other.GetComponent<Rigidbody2D>();
        if (!rb2d) return;
        
        var hc = other.GetComponent<HeroController>();

        if (hc && hc.controlReqlinquished && !hc.cState.superDashing && !EditorManager.LostControlToCustomObject)
        {
            if (_windPlayer) rb2d.velocity = Vector2.zero;
            _windPlayer = false;
            return;
        }
        
        rb2d.AddForce(_force);
        
        if (!hc) return;
        _windPlayer = true;
        
        if (hc.cState.onGround && !hc.CheckTouchingGround())
        {
            hc.cState.onGround = false;
            hc.cState.falling = true;
            hc.proxyFSM.SendEvent("HeroCtrl-LeftGround");
            _setState.Invoke(hc, [ActorStates.airborne]);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<HeroController>()) _windPlayer = false;
    }

    private void Update()
    {
        if (!_setForce)
        { 
            _setForce = true;
            if (transform.localScale.x < 0) speed = -speed;
            _force = transform.localRotation * new Vector3(speed, 0, 0);
            
            if (_main.HasValue)
            {
                var main = _main.Value;
                main.startRotationMultiplier = Mathf.Deg2Rad * -transform.localRotation.eulerAngles.z;
            }

            if (_emission.HasValue)
            {
                var emission = _emission.Value;
                emission.rateOverTimeMultiplier *= transform.localScale.y;
            }
        }
    }

    public void SetupParticles()
    {
        var particles = new GameObject("Particles");
        particles.transform.SetParent(transform, false);
        particles.transform.localPosition -= new Vector3(5f, 0, 0);
        particles.layer = LayerMask.NameToLayer("Damage All");
        
        var ps = particles.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.startLifetimeMultiplier = 100000;
        main.startSpeedMultiplier = Mathf.Sqrt(speed) * 2;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(r, g, b, a));

        var emission = ps.emission;
        emission.rateOverTimeMultiplier = Mathf.Sqrt(speed);

        _emission = emission;
        _main = main;
        
        var shape = ps.shape;

        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(10, 1, 10);
        shape.rotation = new Vector3(0, 90, 270);

        var triggers = ps.trigger;
        triggers.enabled = true;
        var trigger = particles.AddComponent<BoxCollider2D>();
        trigger.offset = new Vector2(5, 0);
        trigger.size = new Vector2(10, 10);
        
        triggers.AddCollider(trigger);

        triggers.inside = ParticleSystemOverlapAction.Ignore;
        triggers.outside = ParticleSystemOverlapAction.Kill;
        
        ps.GetComponent<ParticleSystemRenderer>().material = _windMaterial;
    }
}