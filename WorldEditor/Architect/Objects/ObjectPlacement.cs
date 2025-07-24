using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Config;
using Architect.Attributes.Receivers;
using Architect.Content.Groups;
using Architect.Util;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Objects;

[JsonConverter(typeof(ObjectPlacementConverter))]
public class ObjectPlacement
{
    internal bool Touching(Vector2 pos)
    {
        if (!_previewObject) return false;

        var size = _previewObject.GetComponent<SpriteRenderer>().size;
        var width = size.x / 2 * Mathf.Abs(_previewObject.transform.GetScaleX());
        var height = size.y / 2 * Mathf.Abs(_previewObject.transform.GetScaleY());

        var objPos = _previewObject.transform.position;
        var objRotation = _previewObject.transform.rotation;

        var localPos = Quaternion.Inverse(objRotation) * (pos - (Vector2) objPos);

        return Mathf.Abs(localPos.x) <= width && Mathf.Abs(localPos.y) <= height;
    }

    internal void Destroy()
    {
        if (_previewObject) Object.Destroy(_previewObject);
        PlacementManager.GetCurrentPlacements().Remove(this);
    }

    public PlaceableObject GetPlaceableObject()
    {
        return _placeableObject ??= PlaceableObject.AllObjects.TryGetValue(_name, out var obj)
            ? _placeableObject = obj as PlaceableObject
            : null;
    }

    public void Move(Vector3 pos)
    {
        pos.z = _pos.z;
        _pos = pos;
        if (_previewObject) _previewObject.transform.position = pos + ResourceUtils.FixOffset(GetPlaceableObject().Offset, Flipped, Rotation, Scale);
    }

    public Vector3 GetPos()
    {
        return _pos;
    }

    private Vector3 _pos;
    private PlaceableObject _placeableObject;
    private readonly string _name;
    private readonly string _id;
    
    public readonly float Rotation;
    public readonly float Scale;
    public readonly bool Flipped;

    public string GetId()
    {
        return _id;
    }

    private GameObject _previewObject;

    internal void PlaceGhost()
    {
        var selected = GetPlaceableObject();
        
        _previewObject = new GameObject
        {
            transform = { position = _pos + ResourceUtils.FixOffset(selected.Offset, Flipped, Rotation, Scale) },
            name = "[Architect] Object Preview"
        };

        var scaleX = Scale;
        var scaleY = Scale;

        var r = 1f;
        var g = 1f;
        var b = 1f;
        var a = 0.5f;
        
        var renderer = _previewObject.AddComponent<SpriteRenderer>();
        
        foreach (var config in Config)
        {
            if (config.GetTypeId() == "width" && config is FloatConfigValue width) scaleX *= width.GetValue();
            else if (config.GetTypeId() == "height" && config is FloatConfigValue height) scaleY *= height.GetValue();
            else if (config.GetTypeId() == "r" && config is FloatConfigValue red) r = red.GetValue();
            else if (config.GetTypeId() == "g" && config is FloatConfigValue green) g = green.GetValue();
            else if (config.GetTypeId() == "b" && config is FloatConfigValue blue) b = blue.GetValue();
            else if (config.GetTypeId() == "a" && config is FloatConfigValue alpha) a *= Mathf.Max(0.15f, alpha.GetValue());
            else if (config.GetTypeId() == "layer" && config is IntConfigValue layer) renderer.sortingOrder = layer.GetValue(); 
            
            else if (config.GetTypeId() == "decoration_z_offset" && config is FloatConfigValue zOffset)
            {
                var pos = _previewObject.transform.position;
                pos.z += zOffset.GetValue();
                _previewObject.transform.position = pos;
            }
        }

        renderer.color = new Color(r, g, b, a);
        
        selected.PackElement.PostPlace(this, _previewObject);
        
        GhostPlacementUtils.SetupForPlacement(_previewObject, renderer, selected, Flipped, Rotation, scaleX, scaleY);
    }

    internal GameObject SpawnObject()
    {
        var packElement = GetPlaceableObject().PackElement;
        var prefab = packElement.GetPrefab(Flipped, Rotation);
        
        var obj = Object.Instantiate(prefab, _pos, prefab.transform.rotation);
        obj.name = "[Architect] " + _name + " (" + _id + ")";
        
        if (!Mathf.Approximately(Scale, 1))
        {
            if (obj.GetComponent<HealthManager>() && !packElement.DisableScaleParent())
            {
                var par = new GameObject("[Architect] Scale Parent") { transform = { position = _pos } };
                obj.transform.parent = par.transform;
                par.transform.localScale *= Scale;
            }
            else obj.transform.localScale *= Scale;
        }

        foreach (var c in Config)
        {
            if (!c.PreAwake()) continue;
            c.Setup(obj);
        }
        
        obj.SetActive(true);
        
        GetPlaceableObject().PackElement.PostSpawn(obj, Flipped, Rotation, Scale);
        
        if (!GetPlaceableObject().PackElement.OverrideFlip() && Flipped)
        {
            var scale = obj.transform.localScale;
            scale.x = -scale.x;
            obj.transform.localScale = scale;
        }

        if (!GetPlaceableObject().PackElement.OverrideRotation())
        {
            var rotation = obj.transform.rotation.eulerAngles;
            rotation.z += Rotation;
            obj.transform.rotation = Quaternion.Euler(rotation);
        }

        foreach (var broadcaster in Broadcasters)
        {
            var instance = obj.AddComponent<EventBroadcasterInstance>();
            instance.eventBroadcasterType = broadcaster.EventBroadcasterType;
            instance.eventName = broadcaster.EventName;
        }

        foreach (var receiver in Receivers)
        {
            var instance = obj.AddComponent<EventReceiverInstance>();
            instance.Receiver = receiver;
            EventManager.RegisterEventReceiver(receiver.Name, instance);
        }

        foreach (var c in Config)
        {
            if (c.PreAwake()) continue;
            c.Setup(obj);
        }

        return obj;
    }

    public readonly EventBroadcaster[] Broadcasters;
    public readonly EventReceiver[] Receivers;
    public readonly ConfigValue[] Config;
    
    public ObjectPlacement(
        string name, 
        Vector3 pos, 
        bool flipped, 
        float rotation, 
        float scale, 
        string id, 
        EventBroadcaster[] broadcasters, 
        EventReceiver[] receivers, 
        ConfigValue[] config)
    {
        _name = name;
        _pos = pos;
        Flipped = flipped;
        Scale = scale;
        _id = id;
        Rotation = rotation;

        Broadcasters = broadcasters;
        Receivers = receivers;
        Config = config;
    }

    public class ObjectPlacementConverter : JsonConverter<ObjectPlacement>
    {
        public override void WriteJson(JsonWriter writer, ObjectPlacement value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            
            WritePlacementInfo(writer, value, serializer);
            
            if (value.Broadcasters.Length > 0)
            {
                writer.WritePropertyName("events");
                serializer.Serialize(writer, value.Broadcasters.Select(broadcaster => broadcaster.Serialize()).ToList());
            }

            if (value.Receivers.Length > 0)
            {
                writer.WritePropertyName("listeners");
                serializer.Serialize(writer, value.Receivers.Select(receiver => receiver.Serialize()).ToList());
            }

            if (value.Config.Length > 0)
            {
                writer.WritePropertyName("config");
                serializer.Serialize(writer, value.Config.ToDictionary(c => c.GetTypeId(), c => c.SerializeValue()));
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
            writer.WriteValue(placement.Flipped);
            
            if (placement.Rotation != 0)
            {
                writer.WritePropertyName("rotation");
                writer.WriteValue(placement.Rotation);
            }
            
            if (!Mathf.Approximately(placement.Scale, 1))
            {
                writer.WritePropertyName("scale");
                writer.WriteValue(placement.Scale);
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
            string id = null;
            var pos = Vector3.zero;
            var flipped = true;
            var rotation = 0f;
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
                                    reader.ReadAsDouble();
                                    rotation = (float)(double)reader.Value;
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

            name = Updater.UpdateObject(name);
            id ??= Guid.NewGuid().ToString().Substring(0, 8);
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