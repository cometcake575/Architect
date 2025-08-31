using System;
using System.Collections;
using Architect.Attributes;
using GlobalEnums;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class Binoculars : MonoBehaviour
{
    private static bool _frozen;
    public float speed = 40;

    public float maxZoom = 2.5f;
    public float minZoom = 0.25f;
    public float startZoom = 1;
    public Vector3 startOffset;

    private bool _active;

    private Vector3 _targetPos;
    private float _zoom;

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
            StartCoroutine(ReturnZoom());
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
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, _targetPos, 9 * Time.deltaTime);

        _zoom = Mathf.Clamp(_zoom + Input.mouseScrollDelta.y / 20, minZoom, maxZoom);

        GameCameras.instance.tk2dCam.ZoomFactor = Mathf.Lerp(zf, _zoom, 15 * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<NailSlash>()) StartUsing();
    }

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

    public void StartUsing()
    {
        _zoom = startZoom;
        _frozen = true;
        _active = true;
        HeroController.instance.damageMode = DamageMode.NO_DAMAGE;
        HeroController.instance.vignette.gameObject.SetActive(false);
        HeroController.instance.RelinquishControl();

        _targetPos = GameCameras.instance.cameraController.transform.position + startOffset;

        EventManager.BroadcastEvent(gameObject, "StartUse");
    }

    private static IEnumerator ReturnZoom()
    {
        while (Math.Abs(GameCameras.instance.tk2dCam.ZoomFactor - 1) > 0.001f)
        {
            if (_frozen) yield break;
            GameCameras.instance.tk2dCam.ZoomFactor =
                Mathf.Lerp(GameCameras.instance.tk2dCam.ZoomFactor, 1, 10 * Time.deltaTime);
            yield return null;
        }

        GameCameras.instance.tk2dCam.ZoomFactor = 1;
    }
}