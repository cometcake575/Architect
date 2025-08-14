using Architect.Attributes;
using GlobalEnums;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Binoculars : MonoBehaviour
{
    private static bool _frozen;

    private bool _active;
    public float speed = 40;

    public float maxZoom = 2.5f;
    public float minZoom = 0.25f;

    private Vector3 _targetPos;
    
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
            GameCameras.instance.tk2dCam.ZoomFactor = 1;
            _frozen = true;
            _active = true;
            HeroController.instance.damageMode = DamageMode.NO_DAMAGE;
            HeroController.instance.vignette.gameObject.SetActive(false);
            HeroController.instance.RelinquishControl();
            _targetPos = GameCameras.instance.cameraController.transform.position;
            
            EventManager.BroadcastEvent(gameObject, "StartUse");
        }
    }

    private void Update()
    {
        if (!_active) return;

        var actions = InputHandler.Instance.inputActions;
        if (actions.jump.WasPressed)
        {
            GameCameras.instance.tk2dCam.ZoomFactor = 1;
            _frozen = false;
            _active = false;
            HeroController.instance.damageMode = DamageMode.FULL_DAMAGE;
            HeroController.instance.vignette.gameObject.SetActive(true);
            HeroController.instance.RegainControl();
            EventManager.BroadcastEvent(gameObject, "StopUse");
            return;
        }

        float vertical = 0;
        if (actions.up.IsPressed) vertical++;
        if (actions.down.IsPressed) vertical--;

        float horizontal = 0;
        if (actions.right.IsPressed) horizontal++;
        if (actions.left.IsPressed) horizontal--;

        var zf = GameCameras.instance.tk2dCam.ZoomFactor;
        
        _targetPos += new Vector3(horizontal * Time.deltaTime * speed / zf, vertical * Time.deltaTime * speed / zf, 0);
        var cameraTransform = GameCameras.instance.cameraController.transform;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, _targetPos, 10 * Time.deltaTime);

        GameCameras.instance.tk2dCam.ZoomFactor = Mathf.Clamp(zf + Input.mouseScrollDelta.y / 20, minZoom, maxZoom);
    }
}