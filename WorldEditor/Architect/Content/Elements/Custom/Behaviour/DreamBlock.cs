using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Architect.Util;
using GlobalEnums;
using Modding;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class DreamBlock : MonoBehaviour
{
    private const string DreamBlockSource = "DreamBlock";
    
    private static readonly List<DreamBlock> ActiveBlocks = [];
    private static readonly List<DreamBlock> TouchingBlocks = [];
    private static bool _damaging;

    private ParticleSystem.EmissionModule _emission;
    private ParticleSystem.ShapeModule _shape;

    private static readonly FieldInfo DashTimer = typeof(HeroController).GetField("dash_timer", 
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static int _walljumpCt;
    
    public static void Init()
    {
        On.HeroController.SceneInit += (orig, self) =>
        {
            orig(self);
            ActiveBlocks.Clear();
            TouchingBlocks.Clear();
        };

        On.HeroController.FixedUpdate += (orig, self) =>
        {
            orig(self);
            if (_walljumpCt > 0) _walljumpCt--;
        };

        On.HeroController.CanJump += (orig, self) => orig(self) || 
                                                     (self.playerData.GetBool("hasWalljump") && !self.cState.touchingNonSlider && _walljumpCt > 0);

        ModHooks.HeroUpdateHook += () =>
        {
            var hc = HeroController.instance;
            if (hc.cState.shadowDashing && ActiveBlocks.Count > 0)
            {
                DashTimer.SetValue(hc, 0);
            }
        };

        On.HeroController.TakeDamage += (orig, self, go, side, amount, type) =>
        {
            if (ActiveBlocks.Count > 0)
            {
                _damaging = true;
                ActiveBlocks.Clear();
                MoveOut();
            }

            orig(self, go, side, amount, type);
        };
        
        var blockDir = false;
        On.HeroController.HeroDash += (orig, self) =>
        {
            if (TouchingBlocks.Count > 0 && self.cState.wallSliding)
            {
                var actions = InputHandler.Instance.inputActions;
                if (self.touchingWallR && !actions.left.IsPressed) self.FaceRight();
                else if (self.touchingWallL && !actions.right.IsPressed) self.FaceLeft();
                self.cState.touchingWall = false;
                self.cState.wallSliding = false;
                blockDir = true;
            }

            orig(self);
            blockDir = false;
        };

        On.HeroController.FaceLeft += (orig, self) =>
        {
            if (blockDir) return;
            orig(self);
        };

        On.HeroController.FaceRight += (orig, self) =>
        {
            if (blockDir) return;
            orig(self);
        };

        On.HeroController.OnCollisionEnter2D += (orig, self, collision) =>
        {
            if (ActiveBlocks.Count > 0 && !collision.gameObject.GetComponent<DreamBlock>())
            {
                self.TakeDamage(collision.gameObject, CollisionSide.other, 1, 2);
            }
            orig(self, collision);
        };
    }

    private bool _setup;

    private BoxCollider2D _collider;
    private BoxCollider2D _trigger;
    private static AudioSource _source;

    private void Awake()
    {
        _collider = GetComponents<BoxCollider2D>().First(obj => !obj.isTrigger);
        _trigger = GetComponents<BoxCollider2D>().First(obj => obj.isTrigger);
    }

    private void Update()
    {
        _collider.enabled = !HeroController.instance.cState.shadowDashing;

        if (!_setup)
        {
            _setup = true;
            
            _emission.rateOverTimeMultiplier *= transform.localScale.x * transform.localScale.y;
            _shape.scale = new Vector3(10 - 3 / transform.localScale.x, 10 - 3 / transform.localScale.y, 10);

            _trigger.size = new Vector2(_collider.size.x - 0.2f / transform.localScale.x,
                _collider.size.y - 0.2f / transform.localScale.y);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer != 9) return;
        if (ActiveBlocks.Contains(this)) return;

        if (!_trigger.bounds.Contains(new Vector3(other.bounds.center.x, other.bounds.center.y, 0.01f))) return;
        
        var hc = HeroController.instance;
        if (hc.cState.shadowDashing)
        {
            ActiveBlocks.Add(this);
            hc.GetComponent<MeshRenderer>().enabled = false;
            if (ActiveBlocks.Count == 1) StartCoroutine(Play());
        }
    }

    private static IEnumerator Play()
    {
        _source.volume = GameManager.instance.GetImplicitCinematicVolume(); 
        
        _source.loop = false;
        _source.clip = Enter;
        _source.Play();
        var paused = false;

        while (ActiveBlocks.Count > 0)
        {
            if (GameManager.instance.isPaused)
            {
                if (!paused) _source.Pause();
                paused = true;
                yield return null;
            } else if (paused)
            {
                paused = false;
                _source.Play();
            }
            
            if (!_source.isPlaying && !paused)
            {
                _source.loop = true;
                _source.clip = Loop;
                _source.Play();
            }
            
            yield return null;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != 9) return;
        
        if (!ActiveBlocks.Contains(this)) return;
        ActiveBlocks.Remove(this);

        if (ActiveBlocks.Count == 0) MoveOut();
        
        _walljumpCt = 1;
    }

    private static void MoveOut()
    {
        var hc = HeroController.instance;
        
        if (!_damaging) hc.GetComponent<MeshRenderer>().enabled = true;
        _damaging = false;
        hc.ResetAirMoves();
        CustomObjects.RefreshShadowDash();

        _source.Stop();
        _source.PlayOneShot(Exit);
        
        DashTimer.SetValue(hc, hc.dashingDown ? hc.DOWN_DASH_TIME : hc.DASH_TIME);
    }

    private void Start()
    {
        var sounds = HeroController.instance.transform.Find("Sounds");
        _source = sounds.Find(DreamBlockSource)?.GetComponent<AudioSource>();
        if (!_source)
        {
            _source = new GameObject(DreamBlockSource)
            {
                transform = { parent = sounds }
            }.AddComponent<AudioSource>();
        }
    }

    private static readonly AudioClip Enter = ResourceUtils.LoadInternalClip("ScatteredAndLost.dream_block_enter");
    private static readonly AudioClip Loop = ResourceUtils.LoadInternalClip("ScatteredAndLost.dream_block_loop");
    private static readonly AudioClip Exit = ResourceUtils.LoadInternalClip("ScatteredAndLost.dream_block_exit");

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer != 9) return;
        TouchingBlocks.Add(this);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.layer != 9) return;
        TouchingBlocks.Remove(this);
    }

    public void SetupParticles()
    {
        var ps = gameObject.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetimeMultiplier /= 2;
        main.startSpeedMultiplier = 0;
        
        var mmg = new ParticleSystem.MinMaxGradient
        {
            mode = ParticleSystemGradientMode.RandomColor,
            gradient = new Gradient
            {
                colorKeys =
                [
                    new GradientColorKey(new Color(1, 0, 0), 0),
                    new GradientColorKey(new Color(0, 1, 0), 0.33f),
                    new GradientColorKey(new Color(0, 0, 1), 0.66f),
                    new GradientColorKey(new Color(1, 1, 0), 1)
                ]
            }
        };
        main.startColor = mmg;
        main.scalingMode = ParticleSystemScalingMode.Shape;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(new Gradient
        {
            alphaKeys = 
            [
                new GradientAlphaKey(0, 0),
                new GradientAlphaKey(1, 0.5f),
                new GradientAlphaKey(0, 1)
            ]
        });

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"))
        {
            mainTexture = ResourceUtils.LoadInternal("ScatteredAndLost.star", FilterMode.Point).texture
        };
        rend.sortingOrder = 1;

        _shape = ps.shape;
        _shape.shapeType = ParticleSystemShapeType.Box;

        var sol = ps.sizeOverLifetime;
        sol.size = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
        sol.enabled = true;
        sol.sizeMultiplier = 0.1f;

        _emission = ps.emission;
    }
}