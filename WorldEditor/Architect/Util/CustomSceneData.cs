using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Objects;
using Newtonsoft.Json;
using UnityEngine;

namespace Architect.Util;

public class CustomSceneData
{
    private readonly Dictionary<string, List<ObjectPlacement>> _scenes = new();
    
    public List<ObjectPlacement> GetPlacements(string scene)
    {
        if (!_scenes.ContainsKey(scene)) _scenes[scene] = new List<ObjectPlacement>();
        return _scenes[scene];
    }

    public void SetPlacements(string scene, List<ObjectPlacement> placements)
    {
        _scenes[scene] = placements;
    }

    public class CustomSceneDataPartConverter : JsonConverter<List<ObjectPlacement>>
    {
        public override void WriteJson(JsonWriter writer, List<ObjectPlacement> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
                
            writer.WritePropertyName("elements");
            
            writer.WriteStartArray();
                
            foreach (var placement in value)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("placement_info");
                WritePlacementInfo(writer, placement, serializer);

                if (placement.HasBroadcasters())
                {
                    writer.WritePropertyName("broadcasters");
                    WriteBroadcasters(writer, placement, serializer);
                }

                if (placement.HasReceivers())
                {
                    writer.WritePropertyName("listeners");
                    WriteListeners(writer, placement, serializer);
                }

                if (placement.HasConfig())
                {
                    writer.WritePropertyName("config");
                    WriteConfig(writer, placement, serializer);
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
                
            writer.WriteEndObject();
        }

        private static void WritePlacementInfo(JsonWriter writer, ObjectPlacement placement, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(placement.GetName());
            writer.WritePropertyName("guid");
            writer.WriteValue(placement.GetGuid());
            writer.WritePropertyName("pos");
            serializer.Serialize(writer, placement.GetPos());
            writer.WritePropertyName("rotation");
            writer.WriteValue(placement.GetRotation());
            writer.WritePropertyName("scale");
            writer.WriteValue(placement.GetScale());
            writer.WritePropertyName("flipped");
            writer.WriteValue(placement.IsFlipped());
            writer.WriteEndObject();
        }

        private static void WriteBroadcasters(JsonWriter writer, ObjectPlacement placement, JsonSerializer serializer)
        {
            serializer.Serialize(writer, placement.SerializeBroadcasters());
        }

        private static void WriteListeners(JsonWriter writer, ObjectPlacement placement, JsonSerializer serializer)
        {
            serializer.Serialize(writer, placement.SerializeReceivers());
        }

        private static void WriteConfig(JsonWriter writer, ObjectPlacement placement, JsonSerializer serializer)
        {
            serializer.Serialize(writer, placement.SerializeConfig());
        }

        public override List<ObjectPlacement> ReadJson(JsonReader reader, Type objectType, List<ObjectPlacement> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var scene = new List<ObjectPlacement>();

            reader.Read();
            if (reader.Value as string == "elements")
            {
                reader.Read();
                reader.Read();
                while (reader.TokenType == JsonToken.StartObject)
                {
                    var name = "";
                    string guid = null;
                    var pos = Vector3.zero;
                    var flipped = true;
                    var rotation = 0;
                    var scale = 1f;
                    List<Dictionary<string, string>> broadcasters = new();
                    List<Dictionary<string, string>> receivers = new();
                    Dictionary<string, string> config = new();

                    reader.Read();
                    while (reader.TokenType == JsonToken.PropertyName)
                    {
                        switch (reader.Value as string)
                        {
                            case "placement_info":
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
                                        case "guid":
                                            reader.ReadAsString();
                                            guid = reader.Value as string;
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
                            case "broadcasters":
                                reader.Read();
                                broadcasters = serializer.Deserialize<List<Dictionary<string, string>>>(reader);
                                break;
                            case "listeners":
                                reader.Read();
                                receivers = serializer.Deserialize<List<Dictionary<string, string>>>(reader);
                                break;
                            case "config":
                                reader.Read();
                                config = serializer.Deserialize<Dictionary<string, string>>(reader);
                                break;
                        }

                        reader.Read();
                    }

                    guid ??= Guid.NewGuid().ToString();
                    var placement = new ObjectPlacement(name, pos, flipped, rotation, scale, guid);

                    foreach (var broadcaster in broadcasters)
                    {
                        placement.AddBroadcaster(EventManager.DeserializeBroadcaster(broadcaster));
                    }

                    foreach (var receiver in receivers)
                    {
                        placement.AddReceiver(EventManager.DeserializeReceiver(receiver));
                    }

                    foreach (var cvalue in config)
                    {
                        placement.AddConfig(Attributes.ConfigManager.DeserializeConfigValue(cvalue.Key, cvalue.Value));
                    }

                    scene.Add(placement);
                    reader.Read();
                }
            }

            reader.Read();
            return scene;
        }
    }

    public static readonly CustomSceneDataPartConverter PartConverter = new();
    
    public class CustomSceneDataConverter : JsonConverter<CustomSceneData>
    {

        public override void WriteJson(JsonWriter writer, CustomSceneData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var scene in value._scenes.Keys.Where(scene => value._scenes[scene].Count != 0))
            {
                writer.WritePropertyName(scene);
                
                PartConverter.WriteJson(writer, value._scenes[scene], serializer);
            }
            writer.WriteEndObject();
        }

        public override CustomSceneData ReadJson(JsonReader reader, Type objectType, CustomSceneData existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            EventManager.InitializeReceivers();
            Attributes.ConfigManager.Initialize();
            
            if (!hasExistingValue) existingValue = new CustomSceneData();

            reader.Read();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                if (reader.Value is not string scene) return existingValue;
                reader.Read();
                existingValue._scenes[scene] =
                    PartConverter.ReadJson(reader, objectType, null, false, serializer);
                reader.Read();
            }
            
            return existingValue;
        }
    }
}