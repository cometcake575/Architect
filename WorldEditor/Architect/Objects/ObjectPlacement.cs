using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Config;
using Architect.Attributes.Receivers;
using Architect.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Objects;

public class ObjectPlacement
{
    internal bool Touching(Vector2 pos)
    {
        if (!_obj) return false;

        var size = _obj.GetComponent<SpriteRenderer>().size;
        var width = size.x / 2 * Mathf.Abs(_obj.transform.GetScaleX());
        var height = size.y / 2 * Mathf.Abs(_obj.transform.GetScaleY());

        var objPos = _obj.transform.position;
        var objRotation = _obj.transform.rotation;

        var localPos = Quaternion.Inverse(objRotation) * (pos - (Vector2) objPos);

        return Mathf.Abs(localPos.x) <= width && Mathf.Abs(localPos.y) <= height;
    }

    internal void Destroy()
    {
        if (_obj) Object.Destroy(_obj);
        PlacementManager.Placements.Remove(this);
    }

    private PlaceableObject GetPlaceableObject()
    {
        return _placeableObject ??= PlaceableObject.AllObjects[_name] as PlaceableObject;
    }

    private PlaceableObject _placeableObject;
    private readonly string _name;
    private readonly Vector3 _pos;
    private readonly bool _flipped;
    private readonly int _rotation;
    private readonly float _scale;
    private readonly string _guid;

    private GameObject _obj;

    internal void PlaceGhost()
    {
        if (PlaceableObject.AllObjects[_name] is not PlaceableObject selected) return;
        
        _obj = new GameObject { transform = { position = _pos + WeSpriteUtils.FixOffset(selected.Offset, _flipped, _rotation, _scale) } };

        var renderer = _obj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(1, 1, 1, 0.5f);
        
        GhostPlacementUtils.SetupForPlacement(_obj, renderer, selected, _flipped, _rotation, _scale);
    }

    internal void SpawnObject()
    {
        var prefab = GetPlaceableObject().PackElement.GetPrefab(_flipped, _rotation);
        
        var obj = Object.Instantiate(prefab, _pos, prefab.transform.rotation);
        obj.name = _name + " (" + _guid + ")";
        
        if (!Mathf.Approximately(_scale, 1))
        {
            if (obj.GetComponent<PlayMakerFSM>())
            {
                var par = new GameObject("Scale Parent") { transform = { position = _pos } };
                obj.transform.parent = par.transform;
                par.transform.localScale *= _scale;
            }
            else obj.transform.localScale *= _scale;
        }
        
        obj.SetActive(true);
        
        GetPlaceableObject().PackElement.PostSpawn(obj);
        
        if (!GetPlaceableObject().PackElement.OverrideFlip() && _flipped)
        {
            var scale = obj.transform.localScale;
            scale.x = -scale.x;
            obj.transform.localScale = scale;
        }

        if (!GetPlaceableObject().PackElement.OverrideRotation())
        {
            var rotation = obj.transform.rotation.eulerAngles;
            rotation.z += _rotation;
            obj.transform.rotation = Quaternion.Euler(rotation);
        }

        foreach (var broadcaster in _broadcasters)
        {
            var instance = obj.AddComponent<EventBroadcasterInstance>();
            instance.eventBroadcasterType = broadcaster.EventBroadcasterType;
            instance.eventName = broadcaster.EventName;
        }

        foreach (var receiver in _receivers)
        {
            var instance = obj.AddComponent<EventReceiverInstance>();
            instance.Receiver = receiver;
            EventManager.RegisterEventReceiver(receiver.EventName, instance);
        }

        foreach (var c in _config)
        {
            c.Setup(obj);
        }
    }

    private readonly List<EventBroadcaster> _broadcasters = new();
    private readonly List<EventReceiver> _receivers = new();
    private readonly List<ConfigValue> _config = new();

    public void AddBroadcaster(EventBroadcaster broadcaster)
    {
        _broadcasters.Add(broadcaster);
    }

    public void AddReceiver(EventReceiver receiver)
    {
        _receivers.Add(receiver);
    }

    public void AddConfig(ConfigValue value)
    {
        _config.Add(value);
    }

    public bool HasBroadcasters()
    {
        return _broadcasters.Count > 0;
    }

    public bool HasReceivers()
    {
        return _receivers.Count > 0;
    }

    public bool HasConfig()
    {
        return _config.Count > 0;
    }

    public Dictionary<string, string> SerializeConfig()
    {
        return _config.ToDictionary(c => c.GetName(), c => c.SerializeValue());
    }

    public List<Dictionary<string, string>> SerializeBroadcasters()
    {
        return _broadcasters.Select(broadcaster => broadcaster.Serialize()).ToList();
    }

    public List<Dictionary<string, string>> SerializeReceivers()
    {
        return _receivers.Select(receiver => receiver.Serialize()).ToList();
    }
    
    internal ObjectPlacement(string name, Vector3 pos, bool flipped, int rotation, float scale, string guid)
    {
        _name = name;
        _pos = pos;
        _flipped = flipped;
        _scale = scale;
        _guid = guid;
        _rotation = rotation;
    }
    
    public string GetName()
    {
        return _name;
    }

    public string GetGuid()
    {
        return _guid;
    }

    public int GetRotation()
    {
        return _rotation;
    }

    public float GetScale()
    {
        return _scale;
    }

    public Vector3 GetPos()
    {
        return _pos;
    }

    public bool IsFlipped()
    {
        return _flipped;
    }
}