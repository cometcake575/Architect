using Architect.Storage;
using UnityEngine;
using UnityEngine.Video;

namespace Architect.Content.Elements.Custom.Behaviour;

public class PngObject : MonoBehaviour
{
    public string url;
    public bool point;
    public float ppu = 100;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetLoader.DoLoadSprite(gameObject, url, point, ppu);
    }
}

public abstract class Playable : MonoBehaviour
{
    public abstract void Play();
}

public class MovObject : Playable
{
    public string url;
    public bool playOnStart = true;
    private VideoPlayer _player;

    private bool _started;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetLoader.DoLoadVideo(gameObject, transform.GetScaleX(), url);
        _player = GetComponent<VideoPlayer>();
    }

    // This is used instead of playOnAwake so the first frame is displayed
    private void Update()
    {
        if (!_started)
        {
            _started = true;
            if (!playOnStart) Pause();
        }
    }

    public override void Play()
    {
        if (!_player) return;
        _player.Play();
    }

    public void Pause()
    {
        if (!_player) return;
        _player.Pause();
    }
}

public class WavObject : Playable
{
    public string url;
    public AudioClip sound;

    public float volume = 1;
    private AudioSource _source;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetLoader.DoLoadSound(gameObject, url);
        _source = HeroController.instance.GetComponent<AudioSource>();
    }

    public override void Play()
    {
        if (!sound) return;
        _source.pitch = 1;
        _source.PlayOneShot(sound, volume);
    }
}