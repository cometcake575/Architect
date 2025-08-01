using Architect.MultiplayerHook;
using HutongGames.PlayMaker;
using Modding;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class ZoteTrophy : MonoBehaviour
{
    private ParticleSystem _system;
    private SpriteRenderer _renderer;
    private bool _collected;
    private float _collectedTime;
    private bool _particlesStopped;
    
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

    private void Update()
    {
        if (!_collected) return;
        if (_particlesStopped) return;
        _collectedTime += Time.deltaTime;
        if (!(_collectedTime > 5)) return;
        _particlesStopped = true;
        _system.Stop();
    }

    private static GameObject _titleCard;
    private static FsmString _titleCardData;

    public static void Init()
    {
        _titleCard = GameCameras.instance.hudCamera.transform.GetChild(10).GetChild(0).gameObject;
        _titleCardData = _titleCard.LocateMyFSM("Area Title Control").FsmVariables.FindFsmString("Area Event");
        
        ModHooks.LanguageGetHook += (key, _, orig) =>
        {
            if (key.EndsWith("_RawText_SUPER") || key.EndsWith("_RawText_SUB")) return "";
            return key.EndsWith("_RawText_MAIN") ? key.Replace("_RawText_MAIN", "") : orig;
        };
    }

    public static void WinScreen(string name)
    {
        _titleCardData.Value = name + "_RawText";
        _titleCard.SetActive(true);
    }
}