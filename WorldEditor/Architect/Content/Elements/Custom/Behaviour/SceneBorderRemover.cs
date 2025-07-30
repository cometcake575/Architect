using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class SceneBorderRemover : MonoBehaviour
{
    private static int _count;
    private static CameraController _camera;

    private void Update()
    {
        Refresh();
    }

    private void OnEnable()
    {
        _count++;
        Refresh();
    }
    
    public static void Refresh()
    {
        if (!_camera) _camera = GameCameras.instance.cameraController;
        if (_count == 0)
        {
            _camera.xLimit = _camera.sceneWidth - 14.6f;
            _camera.yLimit = _camera.sceneHeight - 8.3f;
        }
        else
        {
            _camera.yLimit = float.MaxValue;
            _camera.xLimit = float.MaxValue;
        }
    }

    private void OnDisable()
    {
        _count--;
        Refresh();
    }
}