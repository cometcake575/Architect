using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Feather : MonoBehaviour
{
    private static readonly List<Sprite> Sprites = [];
    private static readonly List<Sprite> BreakAnim = [];
    private static readonly Sprite Outline = ResourceUtils.LoadInternal("ScatteredAndLost.feather.outline",
        filterMode:FilterMode.Point, ppu:10);
    
    private SpriteRenderer _spriteRenderer;
    private float _dt; 
    private int _spriteIndex = 2;
    private bool _enabled = true;

    private static float _remainingTime;
    private static GameObject _comet;
    private static Rigidbody2D _cometBody;
    private static CircleCollider2D _cometCollider;
    private static bool _cometValid;
    
    private static bool _show;

    private static readonly FieldInfo AirDashedField = typeof(HeroController).GetField("airDashed", 
        BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly FieldInfo DoubleJumpedField = typeof(HeroController).GetField("doubleJumped",
        BindingFlags.NonPublic | BindingFlags.Instance);
    
    public float featherTime = 5; 
    public float respawnTime = 6;

    public static void Init()
    {
        SetupSprites("ScatteredAndLost.feather.flash.f");
        SetupSprites("ScatteredAndLost.feather.loop.f");
        SetupBreakAnim();

        _comet = new GameObject("Comet");
        _comet.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadInternal(
            "ScatteredAndLost.feather.comet", filterMode: FilterMode.Point, ppu: 10);
        _comet.SetActive(false);

        _cometCollider = _comet.AddComponent<CircleCollider2D>();
        _cometCollider.isTrigger = true;
        _cometCollider.radius = 0.25f;
        _cometCollider.offset = new Vector2(0, -1);

        _cometBody = _comet.AddComponent<Rigidbody2D>();
        _cometBody.gravityScale = 0;

        _comet.AddComponent<CometMovement>();

        _comet.layer = LayerMask.NameToLayer("Player");

        var child = new GameObject("Particle System") { transform =
            {
                parent = _comet.transform,
                position = new Vector3(0, -1, -0.1f),
                localScale = new Vector2(0.2f, 0.2f)
            }
        };
        
        var ps = child.AddComponent<ParticleSystem>();
        ps.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"))
        {
            mainTexture = ResourceUtils.LoadInternal("ScatteredAndLost.feather.comet_trail", FilterMode.Point).texture
        };
        
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeedMultiplier = 40;
        main.startLifetimeMultiplier /= 10;

        var emission = ps.emission;
        emission.rateOverTimeMultiplier *= 20;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.EaseInOut(0, 1, 0.5f, 0));
        
        DontDestroyOnLoad(_comet);

        On.HeroController.TakeDamage += (orig, self, go, side, amount, type) =>
        {
            if (_cometValid)
            {
                _cometValid = false;
                _show = false;
            }

            orig(self, go, side, amount, type);
        };
    }

    private static void SetupSprites(string name)
    {
        for (var i = 0; i <= 20; i++)
        {
            Sprites.Add(ResourceUtils.LoadInternal(name + i, filterMode:FilterMode.Point, ppu:10));
        }
    }

    private static void SetupBreakAnim()
    {
        for (var i = 0; i <= 5; i++)
        {
            BreakAnim.Add(ResourceUtils.LoadInternal("ScatteredAndLost.feather.break.f" + i, filterMode:FilterMode.Point, ppu:10));
        }
    }

    public void Setup()
    {
        gameObject.AddComponent<SpriteRenderer>().sprite = Sprites[0];
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!_enabled) return;
        _dt += Time.deltaTime * 15;
        while (_dt > 1)
        {
            _spriteIndex = (_spriteIndex + 1) % Sprites.Count;
            _dt--;
        }
        _spriteRenderer.sprite = Sprites[_spriteIndex];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_enabled) return;
        if (!other.GetComponent<HeroController>()) return;
        
        _enabled = false;
        _spriteIndex = 2;
        _dt = 0;
        
        StartCoroutine(Fly());
        StartCoroutine(Respawn());
    }

    private IEnumerator Fly()
    {
        var hero = HeroController.instance;
        
        _remainingTime = Mathf.Max(_remainingTime, featherTime);
        
        if (hero.controlReqlinquished) yield break;

        _comet.SetActive(true);

        var rb2d = hero.GetComponent<Rigidbody2D>();

        _show = true;
        
        _comet.transform.position = transform.position + new Vector3(0, 1);
        _cometBody.velocity = rb2d.velocity / 3;
        _cometValid = true;

        hero.GetComponent<MeshRenderer>().enabled = false;
        hero.RelinquishControl();

        var up = InputHandler.Instance.inputActions.up;
        var down = InputHandler.Instance.inputActions.down;
        var left = InputHandler.Instance.inputActions.left;
        var right = InputHandler.Instance.inputActions.right;

        var dashed = false;
        
        while (_remainingTime > 0)
        {
            if (rb2d.gravityScale == 0)
            {
                _show = false;
                _cometValid = false;
            }
            
            if (!_cometValid) break;
            
            _remainingTime -= Time.deltaTime;

            var target = new Vector2();
            if (up.IsPressed) target.y += 25;
            if (down.IsPressed) target.y -= 25;
            if (right.IsPressed) target.x += 25;
            if (left.IsPressed) target.x -= 25;
            
            rb2d.velocity = Vector2.zero;
            hero.transform.position = _comet.transform.position;
            _cometBody.velocity = Vector2.Lerp(_cometBody.velocity, target, Time.deltaTime * 1.5f);
            
            if (InputHandler.Instance.inputActions.dash.WasPressed)
            {
                dashed = true;
                hero.SetStartWithDash();
                break;
            }
            
            yield return null;
        }
        
        if (_show)
        {
            hero.RegainControl();
            hero.GetComponent<MeshRenderer>().enabled = true;
        }
        
        if (!dashed) AirDashedField.SetValue(hero, false);
        DoubleJumpedField.SetValue(hero, false);
        
        _comet.SetActive(false);
        _cometValid = false;
    }

    private IEnumerator Respawn()
    {
        for (var i = 0; i <= 5; i++)
        {
            _spriteRenderer.sprite = BreakAnim[i];
            yield return new WaitForSeconds(0.066f);
        }
        
        yield return new WaitForSeconds(respawnTime);
        _enabled = true;
    }

    private class CometMovement : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.isTrigger) return;
            Architect.Instance.Log(other.gameObject.name);
            _cometValid = false;
        }
    }
}