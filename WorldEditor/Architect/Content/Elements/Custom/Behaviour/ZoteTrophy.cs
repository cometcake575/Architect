using Architect.MultiplayerHook;
using SFCore.Utils;
using UnityEngine;
using FsmUtil = Satchel.FsmUtil;

namespace Architect.Content.Elements.Custom.Behaviour;

public class ZoteTrophy : MonoBehaviour
{
    private bool _collected;
    private float _collectedTime;
    private bool _particlesStopped;
    private SpriteRenderer _renderer;
    private ParticleSystem _system;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();

        _system = GetComponent<ParticleSystem>();
        _system.Stop();

        _system.textureSheetAnimation.AddSprite(_renderer.sprite);

        var particleRenderer = _system.GetComponent<ParticleSystemRenderer>();

        var particleMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            mainTexture = _renderer.sprite.texture
        };

        particleRenderer.material = particleMaterial;
        var rot = _system.rotationOverLifetime;
        rot.enabled = true;

        var curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.1f);
        curve.AddKey(0.75f, 0.6f);

        var curve2 = new AnimationCurve();
        curve2.AddKey(0.0f, 0.2f);
        curve2.AddKey(0.5f, 0.9f);

        rot.z = new ParticleSystem.MinMaxCurve(2.0f, curve, curve2);
    }

    private void Update()
    {
        if (!_collected) return;
        if (_particlesStopped) return;
        _collectedTime += Time.deltaTime;
        if (!(_collectedTime > 5)) return;
        _particlesStopped = true;
        _system.Stop();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;

        if (_collected) return;

        if (Architect.UsingMultiplayer)
        {
            WinScreen("You Win!");
            HkmpHook.BroadcastWin();
        }

        _system.Play();
        _system.time = 4;

        _renderer.enabled = false;
        _collected = true;
    }

    public static void Init()
    {
        On.PlayMakerFSM.Awake += (orig, fsm) =>
        {
            if (fsm.FsmName != "Area Title Control") return;

            var header = fsm.FsmVariables.FindFsmString("Title Sup");
            var footer = fsm.FsmVariables.FindFsmString("Title Sub");
            var body = fsm.FsmVariables.FindFsmString("Title Main");

            FsmUtil.AddCustomAction(fsm.GetState("Init all"), () =>
            {
                if (!_overrideAreaText) return;

                header.Value = _areaHeader;
                footer.Value = _areaFooter;
                body.Value = _areaBody;

                _overrideAreaText = false;
            });
            orig(fsm);
        };
    }
    
    private static bool _overrideAreaText;
    private static string _areaHeader;
    private static string _areaBody;
    private static string _areaFooter;

    public static void WinScreen(string name)
    {
        _overrideAreaText = true;
        _areaHeader = "";
        _areaBody = name;
        _areaFooter = "Wins";
        
        AreaTitle.instance.gameObject.SetActive(true);
    }
}