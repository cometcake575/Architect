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
                    if (!clearer.removeBlur) objects = objects.Where(obj => !obj.GetComponentInChildren<BlurPlane>());
                    if (!clearer.removeProps) objects = objects.Where(obj => !obj.name.StartsWith("_Props"));
                    if (!clearer.removeTransitions) objects = objects.Where(obj => !obj.name.StartsWith("_Transition Gates"));
                    if (!clearer.removeCameraLocks) objects = objects.Where(obj => !obj.name.StartsWith("_CameraLock"));
                    if (!clearer.removeMusic) objects = objects.Where(obj => !obj.GetComponent<MusicRegion>());
                    if (!clearer.removeNpcs) objects = objects.Where(obj => !obj.name.StartsWith("NPC"));
                }
                
                return objects.Select(obj => obj.GetOrAddComponent<Disabler>()).ToArray();
            }).WithConfigGroup(ConfigGroup.RoomClearer),
            CreateRoomEditor("hrp_remover", "Remove Hazard Respawn Point", FindObjectsToDisable<HazardRespawnTrigger>)
                .WithConfigGroup(ConfigGroup.Invisible),
            CreateRoomEditor("door_remover", "Remove Transition", FindObjectsToDisable<TransitionPoint>)
                .WithConfigGroup(ConfigGroup.Invisible),
            CreateRoomEditor("enemy_remover", "Remove Enemy", FindObjectsToDisable<HealthManager>)
                .WithConfigGroup(ConfigGroup.Invisible),
            CreateRoomEditor("object_remover", "Remove Object", o =>
            {
                var config = o.GetComponent<ObjectRemoverConfig>();
                GameObject point = null;
                
                if (config)
                {
                    try { point = o.scene.FindGameObject(config.objectPath); }
                    catch (ArgumentException) { }
                }

                return point is not null ? new[] { point.GetOrAddComponent<Disabler>() } : new Disabler[] { };
            }).WithConfigGroup(ConfigGroup.ObjectRemover)
        };
        ContentPacks.RegisterPack(edits);
    }

    private static Disabler[] FindObjectsToDisable<T>(GameObject disabler) where T : MonoBehaviour
    {
        var objects = disabler.scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .Select(obj => obj.gameObject);
        
        var lowest = float.MaxValue;
        GameObject point = null;
        foreach (var obj in objects)
        {
            var dist = (obj.transform.position - disabler.transform.position).sqrMagnitude;

            if (dist < lowest)
            {
                lowest = dist;
                point = obj;
            }
        }
        
        return point is not null && lowest <= 25 ? new[] { point.gameObject.GetOrAddComponent<Disabler>() } : new Disabler[] { };
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