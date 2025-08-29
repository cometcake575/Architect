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
    private static float _verticalWindForce;
    private static readonly int EnemyLayer = LayerMask.NameToLayer("Enemies");
    private static readonly int ProjectileLayer = LayerMask.NameToLayer("Projectiles");
    private static readonly int AttackLayer = LayerMask.NameToLayer("Enemy Attack");
    
    public float speed = 30;
    private Vector3 _force;
    private Vector3 _wallForce;
    private bool _setForce;

    public float r = 1;
    public float g = 1;
    public float b = 1;
    public float a = 1;

    public bool affectsPlayer = true;
    public bool affectsEnemies = true;
    public bool affectsProjectiles = true;

    private ParticleSystem.MainModule? _main;
    private ParticleSystem.EmissionModule? _emission;

    public static void Init()
    {
        ModHooks.HeroUpdateHook += () =>
        {
            if (HeroController.instance.cState.jumping
                || HeroController.instance.cState.doubleJumping
                || HeroController.instance.cState.wallJumping) _actuallyJumping = true;
            else if (HeroController.instance.GetComponent<Rigidbody2D>().velocity.y < 0) _actuallyJumping = false;
        };

        On.HeroController.SceneInit += (orig, self) =>
        {
            _verticalWindForce = 0;
            orig(self);
        };

        On.HeroController.BackOnGround += (orig, self) =>
        {
            _actuallyJumping = false;
            orig(self);
        };

        var jumpSteps = typeof(HeroController).GetField("jump_steps", BindingFlags.NonPublic | BindingFlags.Instance);
        On.HeroController.JumpReleased += (orig, self) =>
        {
            if (!_actuallyJumping) return;
            if ((int) jumpSteps!.GetValue(self) >= self.JUMP_STEPS_MIN) _actuallyJumping = false;
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
        if (!affectsEnemies && other.gameObject.layer == EnemyLayer) return;
        if (!affectsProjectiles && (other.gameObject.layer == ProjectileLayer || other.gameObject.layer == AttackLayer)) return;
        
        var rb2d = other.GetComponent<Rigidbody2D>();
        if (!rb2d) return;
        
        var hc = other.GetComponent<HeroController>();
        if (!affectsPlayer && hc) return;

        if (hc && hc.controlReqlinquished && !hc.cState.superDashing && !EditorManager.LostControlToCustomObject)
        {
            if (_windPlayer) rb2d.velocity = Vector2.zero;
            _windPlayer = false;
            return;
        }
        
        if (hc && hc.cState.touchingWall) rb2d.AddForce(_wallForce);
        else rb2d.AddForce(_force);
        
        if (!hc) return;
        _windPlayer = true;
        hc.ResetHardLandingTimer();
        
        if (!hc.cState.superDashing && hc.cState.onGround && !hc.CheckTouchingGround())
        {
            hc.cState.onGround = false;
            hc.cState.falling = true;
            hc.proxyFSM.SendEvent("HeroCtrl-LeftGround");
            _setState.Invoke(hc, [ActorStates.airborne]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HeroController>()) _verticalWindForce += _force.y;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<HeroController>())
        {
            _verticalWindForce -= _force.y;
            _windPlayer = false;
        }
    }

    private void Update()
    {
        if (!_setForce)
        { 
            _setForce = true;
            if (transform.localScale.x < 0) speed = -speed;
            _force = transform.localRotation * new Vector3(speed, 0, 0);
            _wallForce = new Vector3(0, _force.y, 0);
            
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