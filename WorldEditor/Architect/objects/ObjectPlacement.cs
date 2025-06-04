using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.objects;

public class ObjectPlacement
{
    internal static List<ObjectPlacement> Placements = new();

    internal static void ClearPlacements()
    {
        foreach (ObjectPlacement placement in Placements)
        {
            Object.Destroy(placement._obj);
        }
        Placements.Clear();
    }

    internal bool Touching(Vector2 pos)
    {
        Vector2 size = _obj.GetComponent<SpriteRenderer>().size;
        float width = size.x / 2;
        float height = size.y / 2;
        if (_pos.x - width < pos.x && pos.x < _pos.x + width)
        {
            if (_pos.y - height < pos.y && pos.y < _pos.y + height)
            {
                return true;
            }
        }

        return false;
    }

    internal void Destroy()
    {
        if (_obj) Object.Destroy(_obj);
        Placements.Remove(this);
    }

    internal static void PlaceObjects()
    {
        foreach (ObjectPlacement placement in Placements)
        {
            placement.Place();
        }
    }

    internal static Dictionary<string, List<Dictionary<string, string>>> SavePlacements()
    {
        Dictionary<string, List<Dictionary<string, string>>> values = new();

        List<Dictionary<string, string>> placementValues = new();

        foreach (ObjectPlacement placement in Placements)
        {
            placementValues.Add(placement.Serialize());
        }
        
        values["elements"] = placementValues;
        
        return values;
    }

    internal static bool HasPlacements()
    {
        return Placements.Count > 0;
    }

    internal static void LoadPlacements(Dictionary<string, List<Dictionary<string, string>>> data)
    {
        Dictionary<string, List<Dictionary<string, string>>> values = data;

        foreach (Dictionary<string, string> s in values["elements"])
        {
            ObjectPlacement placement = Deserialize(s);
            placement?.Initialize();
        }
    }
    
    private GameObject GetPrefab()
    {
        return (PlaceableObject.AllObjects[_name] as PlaceableObject)?.GetPrefab();
    }

    private readonly string _name;
    private readonly Vector3 _pos;
    private readonly Dictionary<string, string> _data;
    private readonly bool _flipped;
    private readonly int _rotation;
    
    internal ObjectPlacement(string name, Vector3 pos, bool flipped, int rotation)
    {
        _name = name;
        _pos = pos;
        _data = new Dictionary<string, string>
        {
            ["pos"] = pos.x + "," + pos.y + "," + pos.z,
            ["name"] = name,
            ["guid"] = Guid.NewGuid().ToString(),
            ["flipped"] = flipped.ToString(),
            ["rotation"] = rotation.ToString()
        };
        _flipped = flipped;
        _rotation = rotation;
    }

    private ObjectPlacement(Dictionary<string, string> data)
    {
        _data = data;
        string[] pos = data["pos"].Split(',');
        float x = Convert.ToSingle(pos[0]);
        float y = Convert.ToSingle(pos[1]);
        float z = Convert.ToSingle(pos[2]);
        _pos = new Vector3(x, y, z);
        _name = data["name"];
        _flipped = data["flipped"] == "True";
        _rotation = Convert.ToInt32(data["rotation"]);
    }

    private GameObject _obj;

    internal void Initialize()
    {
        Placements.Add(this);

        if (Architect.IsEditing)
        {
            _obj = new GameObject();
            SelectableObject selected = PlaceableObject.AllObjects[_name];
            
            SpriteRenderer renderer = _obj.AddComponent<SpriteRenderer>();
            renderer.sprite = selected.GetSprite();
            renderer.color = new Color(1, 1, 1, 0.5f);
            _obj.transform.position = _pos;
            _obj.transform.localScale = GetPrefab().transform.localScale;
            int rotation = selected.GetRotation() + _rotation;
            _obj.transform.rotation = Quaternion.Euler(0, 0, rotation);
            if (rotation % 180 != 0)
            {
                renderer.flipY = _flipped;
                renderer.flipX = selected.FlipX();
            }
            else
            {
                renderer.flipX = _flipped;
                renderer.flipY = selected.FlipX();
            }
        }
    }

    private static ObjectPlacement Deserialize(Dictionary<string, string> data)
    {
        try
        {
            return new ObjectPlacement(data);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    private void Place()
    {
        GameObject obj = Object.Instantiate(GetPrefab(), _pos, new Quaternion());
        obj.SetActive(true);
        if (!_flipped) return;
        Vector3 scale = obj.transform.localScale;
        scale.x = -scale.x;
        obj.transform.localScale = scale;

        Vector3 rotation = obj.transform.rotation.eulerAngles;
        rotation.z = _rotation;
        obj.transform.rotation = Quaternion.Euler(rotation);
    }

    private Dictionary<string, string> Serialize()
    {
        return _data;
    }
}