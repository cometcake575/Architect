using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Config;
using Architect.Attributes.Receivers;
using Architect.Content.Groups;
using Architect.Util;
using Modding.Converters;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Objects;

[JsonConverter(typeof(ObjectPlacementConverter))]
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
        PlacementManager.GetCurrentPlacements().Remove(this);
    }

    public PlaceableObject GetPlaceableObject()
    {
        return _placeableObject ??= PlaceableObject.AllObjects.TryGetValue(_name, out var obj)
            ? _placeableObject = obj as PlaceableObject
            : null;
    }

    private PlaceableObject _placeableObject;
    private readonly string _name;
    private readonly Vector3 _pos;
    private readonly bool _flipped;
    private readonly int _rotation;
    private readonly float _scale;
    private readonly string _id;

    private GameObject _obj;

    internal void PlaceGhost()
    {
        if (PlaceableObject.AllObjects[_name] is not PlaceableObject selected) return;
        
        _obj = new GameObject { transform = { position = _pos + ResourceUtils.FixOffset(selected.Offset, _flipped, _rotation, _scale) } };

        var renderer = _obj.AddComponent<SpriteRenderer>();
        renderer.color = new Color(1, 1, 1, 0.5f);
        
        GhostPlacementUtils.SetupForPlacement(_obj, renderer, selected, _flipped, _rotation, _scale);
    }

    internal void SpawnObject()
    {
        var packElement = GetPlaceableObject().PackElement;
        var prefab = packElement.GetPrefab(_flipped, _rotation);
        
        var obj = Object.Instantiate(prefab, _pos, prefab.transform.rotation);
        obj.name = _name + " (" + _id + ")";
        
        if (!Mathf.Approximately(_scale, 1))
        {
            if (obj.GetComponent<HealthManager>() && !packElement.DisableScaleParent())
            {
                var par = new GameObject("Scale Parent") { transform = { position = _pos } };
                obj.transform.parent = par.transform;
                par.transform.localScale *= _scale;
            }
            else obj.transform.localScale *= _scale;
        }

        foreach (var c in _config)
        {
            if (!c.PreAwake()) continue;
            c.Setup(obj);
        }
        
        obj.SetActive(true);
        
        GetPlaceableObject().PackElement.PostSpawn(obj, _flipped, _rotation, _scale);
        
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
            EventManager.RegisterEventReceiver(receiver.Name, instance);
        }

        foreach (var c in _config)
        {
            if (c.PreAwake()) continue;
            c.Setup(obj);
        }
    }

    private readonly EventBroadcaster[] _broadcasters;
    private readonly EventReceiver[] _receivers;
    private readonly ConfigValue[] _config;
    
    public ObjectPlacement(
        string name, 
        Vector3 pos, 
        bool flipped, 
        int rotation, 
        float scale, 
        string id, 
        EventBroadcaster[] broadcasters, 
        EventReceiver[] receivers, 
        ConfigValue[] config)
    {
        _name = name;
        _pos = pos;
        _flipped = flipped;
        _scale = scale;
        _id = id;
        _rotation = rotation;

        _broadcasters = broadcasters;
        _receivers = receivers;
        _config = config;
    }

    public class ObjectPlacementConverter : Newtonsoft.Json.JsonConverter<ObjectPlacement>
    {
        public static readonly ObjectPlacementConverter Instance = new();
        public static readonly Vector3Converter Vector3Converter = new();
        
        public override void WriteJson(JsonWriter writer, ObjectPlacement value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            WritePlacementInfo(writer, value, serializer);
            
            if (value._broadcasters.Length > 0)
            {
                writer.WritePropertyName("events");
                serializer.Serialize(writer, value._broadcasters.Select(broadcaster => broadcaster.Serialize()).ToList());
            }

            if (value._receivers.Length > 0)
            {
                writer.WritePropertyName("listeners");
                serializer.Serialize(writer, value._receivers.Select(receiver => receiver.Serialize()).ToList());
            }

            if (value._config.Length > 0)
            {
                writer.WritePropertyName("config");
                serializer.Serialize(writer, value._config.ToDictionary(c => c.GetName(), c => c.SerializeValue()));
            }
            
            writer.WriteEndObject();
        }

        private static void WritePlacementInfo(JsonWriter writer, ObjectPlacement placement, JsonSerializer serializer)
        {
            writer.WritePropertyName("placement");
            
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(placement._name);
            writer.WritePropertyName("id");
            writer.WriteValue(placement._id);
            writer.WritePropertyName("pos");
            serializer.Serialize(writer, placement._pos);

            writer.WritePropertyName("flipped");
            writer.WriteValue(placement._flipped);
            
            if (placement._rotation != 0)
            {
                writer.WritePropertyName("rotation");
                writer.WriteValue(placement._rotation);
            }
            
            if (!Mathf.Approximately(placement._scale, 1))
            {
                writer.WritePropertyName("scale");
                writer.WriteValue(placement._scale);
            }
            
            writer.WriteEndObject();
        }

        public override ObjectPlacement ReadJson(JsonReader reader, Type objectType, ObjectPlacement existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            // Required for deserialization
            ConfigGroup.Initialize();
            ReceiverGroup.Initialize();
            
            var name = "";
            var id = "";
            var pos = Vector3.zero;
            var flipped = true;
            var rotation = 0;
            var scale = 1f;
            
            var broadcasters = Array.Empty<EventBroadcaster>();
            var receivers = Array.Empty<EventReceiver>();
            var config = Array.Empty<ConfigValue>();

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                switch (reader.Value as string)
                {
                    case "placement":
                    {
                        reader.Read();
                        reader.Read();
                        while (reader.TokenType == JsonToken.PropertyName)
                        {
                            var key = reader.Value as string;
                            switch (key)
                            {
                                case "name":
                                    reader.ReadAsString();
                                    name = reader.Value as string;
                                    break;
                                case "id":
                                    reader.ReadAsString();
                                    id = reader.Value as string;
                                    break;
                                case "pos":
                                    reader.Read();
                                    pos = serializer.Deserialize<Vector3>(reader);
                                    break;
                                case "flipped":
                                    reader.ReadAsBoolean();
                                    flipped = (bool)reader.Value;
                                    break;
                                case "rotation":
                                    reader.ReadAsInt32();
                                    rotation = (int)reader.Value;
                                    break;
                                case "scale":
                                    reader.ReadAsDouble();
                                    scale = (float)(double)reader.Value;
                                    break;
                            }

                            reader.Read();
                        }

                        break;
                    }
                    case "events":
                        reader.Read();
                        broadcasters = DeserializeBroadcasters(serializer.Deserialize<List<Dictionary<string, string>>>(reader));
                        break;
                    case "listeners":
                        reader.Read();
                        receivers = DeserializeReceivers(serializer.Deserialize<List<Dictionary<string, string>>>(reader));
                        break;
                    case "config":
                        reader.Read();
                        config = DeserializeConfig(serializer.Deserialize<Dictionary<string, string>>(reader));
                        break;
                }
                reader.Read();
            }
            
            var placement = new ObjectPlacement(name, pos, flipped, rotation, scale, id, broadcasters, receivers, config);

            return placement;
        }

        private static EventBroadcaster[] DeserializeBroadcasters(List<Dictionary<string, string>> data)
        {
            var broadcasters = new EventBroadcaster[data.Count];
            for (var i = 0; i < data.Count; i++)
            {
                broadcasters[i] = EventManager.DeserializeBroadcaster(data[i]);
            }
            return broadcasters;
        }

        private static EventReceiver[] DeserializeReceivers(List<Dictionary<string, string>> data)
        {
            var receivers = new EventReceiver[data.Count];
            for (var i = 0; i < data.Count; i++)
            {
                receivers[i] = EventManager.DeserializeReceiver(data[i]);
            }
            return receivers;
        }

        private static ConfigValue[] DeserializeConfig(Dictionary<string, string> data)
        {
            var config = new ConfigValue[data.Count];
            var i = 0;
            foreach (var cvalue in data)
            {
                config[i] = Attributes.ConfigManager.DeserializeConfigValue(cvalue.Key, cvalue.Value);
                i++;
            }
            return config;
        }
    }
}