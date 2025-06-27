using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Groups;
using Architect.Util;
using JetBrains.Annotations;
using Modding.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Elements.Custom;

public static class RoomObjects
{
    public static void Initialize()
    {
        var edits = new ContentPack("Room Edits", "Tools used to edit rooms")
        {
            new SimplePackElement(CreateHazardRespawnPoint(), "Hazard Respawn Point", "Room Edits",
                ResourceUtils.Load("hazard_respawn_point"))
                .WithConfigGroup(ConfigGroup.HazardRespawnPoint)
                .WithReceiverGroup(ReceiverGroup.HazardRespawnPoint),
            new SimplePackElement(CreateObject("Teleport Point"), "Teleport Point", "Room Edits",
                    ResourceUtils.Load("teleport_point"))
                .WithReceiverGroup(ReceiverGroup.TeleportPoint)
                .WithConfigGroup(ConfigGroup.Invisible),
            CreateRoomEditor("room_remover", "Clear Room", o =>
            {
                var objects = o.scene.GetRootGameObjects()
                    .Where(obj => !obj.name.StartsWith("[Architect]"));

                var clearer = o.GetComponent<RoomClearerConfig>();
                if (clearer)
                {
                    if (!clearer.removeBenches) objects = objects.Where(obj => !obj.GetComponent<RestBench>());
                    if (!clearer.removeScenery) objects = objects.Where(obj => !obj.name.StartsWith("_Scenery") && obj.name != "Acid Control v2");
                    if (!clearer.removeTilemap) objects = objects.Where(obj => !obj.name.Contains("TileMap"));
                    if (!clearer.removeBlur) objects = objects.Where(obj => !obj.GetComponent<BlurPlane>());
                    if (!clearer.removeProps) objects = objects.Where(obj => !obj.name.StartsWith("_Props"));
                    if (!clearer.removeTransitions) objects = objects.Where(obj => !obj.name.StartsWith("_Transition Gates"));
                    if (!clearer.removeCameraLocks) objects = objects.Where(obj => !obj.name.StartsWith("_CameraLocks"));
                    if (!clearer.removeMusic) objects = objects.Where(obj => !obj.GetComponent<MusicRegion>());
                    if (!clearer.removeNpcs) objects = objects.Where(obj => !obj.name.StartsWith("_NPCs"));
                }
                
                return objects.Select(obj => obj.GetOrAddComponent<Disabler>()).ToArray();
            }).WithConfigGroup(ConfigGroup.RoomClearer),
            CreateRoomEditor("hrp_remover", "Remove Hazard Respawn Point", o =>
            {
                var objects = Object.FindObjectsOfType<HazardRespawnTrigger>();
                var lowest = float.MaxValue;
                HazardRespawnTrigger point = null;
                foreach (var obj in objects)
                {
                    var dist = (obj.transform.position - o.transform.position).magnitude;

                    if (dist < lowest)
                    {
                        lowest = dist;
                        point = obj;
                    }
                }

                return point is not null && lowest <= 5 ? new[] { point.gameObject.GetOrAddComponent<Disabler>() } : new Disabler[] { };
            }).WithConfigGroup(ConfigGroup.Invisible),
            CreateRoomEditor("door_remover", "Remove Transition", o =>
            {
                var objects = Object.FindObjectsOfType<TransitionPoint>();
                var lowest = float.MaxValue;
                TransitionPoint point = null;
                foreach (var obj in objects)
                {
                    var dist = (obj.transform.position - o.transform.position).magnitude;

                    if (dist < lowest)
                    {
                        lowest = dist;
                        point = obj;
                    }
                }
                
                return point is not null && lowest <= 5 ? new[] { point.gameObject.GetOrAddComponent<Disabler>() } : new Disabler[] { };
            }).WithConfigGroup(ConfigGroup.Invisible)
        };
        ContentPacks.RegisterPack(edits);
    }

    private static readonly Dictionary<string, Func<GameObject, Disabler[]>> EditActions = new();

    private static SimplePackElement CreateRoomEditor(string id, string name, [CanBeNull] Func<GameObject, Disabler[]> action)
    {
        var obj = new GameObject { name = "Room Editor (" + id + ")" };
        
        EditActions[id] = action;
        obj.AddComponent<ObjectRemover>().triggerName = id;
        
        var sprite = ResourceUtils.Load(id, FilterMode.Point);

        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        return new SimplePackElement(obj, name, "Room Edits", sprite);
    }

    public static Disabler[] GetObjects(ObjectRemover editor)
    {
        return EditActions[editor.triggerName].Invoke(editor.gameObject);
    }

    private static GameObject CreateObject(string name)
    {
        var point = new GameObject(name);

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateHazardRespawnPoint()
    {
        var point = new GameObject("Hazard Respawn Point");

        point.AddComponent<HazardRespawnTrigger>().respawnMarker = point.AddComponent<CustomHazardRespawnMarker>();

        var collider = point.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }
}