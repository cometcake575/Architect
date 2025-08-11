using GlobalEnums;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Binoculars : MonoBehaviour
{
    private static bool _frozen;

    private bool _active;
    public float speed = 40;
    
    public static void Init()
    {
        On.CameraController.LateUpdate += (orig, self) =>
        {
            if (_frozen) return;
            orig(self);
        };

        On.HeroController.Move += (orig, self, direction) =>
        {
            if (_frozen) return;
            orig(self, direction);
        };
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<NailSlash>())
        {
            _frozen = true;
            _active = true;
            HeroController.instance.damageMode = DamageMode.NO_DAMAGE;
            HeroController.instance.vignette.gameObject.SetActive(false);
            HeroController.instance.RelinquishControl();
        }
    }

    private void Update()
    {
        if (!_active) return;
        
        var actions = InputHandler.Instance.inputActions;
        if (actions.jump.WasPressed)
        {
            _frozen = false;
            _active = false;
            HeroController.instance.damageMode = DamageMode.FULL_DAMAGE;
            HeroController.instance.vignette.gameObject.SetActive(true);
            HeroController.instance.RegainControl();
            return;
        }

        float vertical = 0;
        if (actions.up.IsPressed) vertical++;
        if (actions.down.IsPressed) vertical--;
        
        float horizontal = 0;
        if (actions.right.IsPressed) horizontal++;
        if (actions.left.IsPressed) horizontal--;
        
        GameCameras.instance.cameraController.transform.Translate(horizontal * Time.deltaTime * speed, vertical * Time.deltaTime * speed, 0);
    }
}