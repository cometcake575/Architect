using Architect.Storage;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class WavObject : MonoBehaviour
{
    public string url;
    public AudioClip sound;
    private AudioSource _source;
    
    public float volume = 1;

    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetLoader.DoLoadSound(gameObject, url);
        _source = HeroController.instance.GetComponent<AudioSource>();
    }

    public void PlaySound()
    {
        if (!sound) return;
        _source.PlayOneShot(sound, volume);
    }
}