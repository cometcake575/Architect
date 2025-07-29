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
            new PreviewablePackElement(CreateHazardRespawnPoint(), "Hazard Respawn Point", "Room Edits",
                ResourceUtils.LoadInternal("hazard_respawn_point"))
                .WithConfigGroup(ConfigGroup.HazardRespawnPoint)
                .WithReceiverGroup(ReceiverGroup.HazardRespawnPoint),
            new SimplePackElement(CreateObject("Teleport Point"), "Teleport Point", "Room Edits",
                    ResourceUtils.LoadInternal("teleport_point"))
                .WithReceiverGroup(ReceiverGroup.TeleportPoint)
                .WithConfigGroup(ConfigGroup.Invisible),
            new PreviewablePackElement(CreateDarkness(), "Darkness", "Room Edits",
                    ResourceUtils.LoadInternal("darkness")),
            new SimplePackElement(CreateBinoculars(), "Binoculars", "Room Edits")
                .WithConfigGroup(ConfigGroup.Binoculars),
            CreateCameraBorder(),
            CreateSceneBorderRemover(),
            CreateObjectRemover("room_remover", "Clear Room", o =>
            {
                var clearer = o.GetOrAddComponent<RoomClearerConfig>();

                var objects = o.scene.GetRootGameObjects().Where(obj => 
                    !obj.name.StartsWith("[Architect]") 
                    && !obj.name.StartsWith("_SceneManager")
                );

                if (clearer.removeOther)
                {
                    if (!clearer.removeBenches) objects = objects.Where(obj => !obj.GetComponent<RestBench>());
                    if (!clearer.removeScenery)
                        objects =
                            objects.Where(obj => !obj.name.StartsWith("_Scenery") && obj.name != "Acid Control v2");
                    if (!clearer.removeTilemap) objects = objects.Where(obj => !obj.name.Contains("TileMap"));
                    if (!clearer.removeBlur) objects = objects.Where(obj => !obj.GetComponentInChildren<BlurPlane>());
                    if (!clearer.removeProps) objects = objects.Where(obj => !obj.name.StartsWith("_Props"));
                    if (!clearer.removeTransitions)
                        objects = objects.Where(obj => !obj.name.StartsWith("_Transition Gates"));
                    if (!clearer.removeCameraLocks) objects = objects.Where(obj => !obj.name.StartsWith("_CameraLock"));
                    if (!clearer.removeMusic) objects = objects.Where(obj => !obj.GetComponent<MusicRegion>());
                    if (!clearer.removeNpcs) objects = objects.Where(obj => !obj.name.StartsWith("NPC"));
                }
                else
                {
                    objects = objects.Where(obj =>
                        (obj.GetComponent<RestBench>() && clearer.removeBenches) ||
                        ((obj.name.StartsWith("_Scenery") || obj.name == "Acid Control v2") && clearer.removeScenery) ||
                        (obj.name.Contains("TileMap") && clearer.removeTilemap) ||
                        (obj.GetComponentInChildren<BlurPlane>() && clearer.removeBlur) ||
                        (obj.name.StartsWith("_Props") && clearer.removeProps) ||
                        (obj.name.StartsWith("_Transition Gates") && clearer.removeTransitions) ||
                        (obj.name.StartsWith("_CameraLock") && clearer.removeCameraLocks) ||
                        (obj.GetComponent<MusicRegion>() && clearer.removeMusic) ||
                        (obj.name.StartsWith("NPC") && clearer.removeNpcs)
                    );
                }

                return objects.Select(obj => obj.GetOrAddComponent<Disabler>()).ToArray();
            }).WithConfigGroup(ConfigGroup.RoomClearer),
            CreateObjectRemover("hrp_remover", "Remove Hazard Respawn Point", FindObjectsToDisable<HazardRespawnTrigger>)
                .WithConfigGroup(ConfigGroup.Invisible)
                .WithReceiverGroup(ReceiverGroup.Invisible),
            CreateObjectRemover("door_remover", "Remove Transition", FindObjectsToDisable<TransitionPoint>)
                .WithConfigGroup(ConfigGroup.Invisible)
                .WithReceiverGroup(ReceiverGroup.Invisible),
            CreateObjectRemover("enemy_remover", "Remove Enemy", FindObjectsToDisable<HealthManager>)
                .WithConfigGroup(ConfigGroup.Invisible)
                .WithReceiverGroup(ReceiverGroup.Invisible),
            CreateObjectRemover("collision_remover", "Remove Solid", FindObjectsToDisable<Collider2D>)
                .WithConfigGroup(ConfigGroup.Invisible)
                .WithReceiverGroup(ReceiverGroup.Invisible),
            CreateObjectRemover("object_remover", "Remove Object", o =>
            {
                var config = o.GetComponent<ObjectRemoverConfig>();
                GameObject point = null;
                
                if (config)
                {
                    try { point = o.scene.FindGameObject(config.objectPath); }
                    catch (ArgumentException) { }
                }

                return point is not null ? [point.GetOrAddComponent<Disabler>()] : [];
            })
                .WithConfigGroup(ConfigGroup.ObjectRemover)
                .WithReceiverGroup(ReceiverGroup.Invisible)
        };
        ContentPacks.RegisterPack(edits);
    }

    private static Disabler[] FindObjectsToDisable<T>(GameObject disabler) where T : UnityEngine.Behaviour
    {
        var objects = disabler.scene.GetRootGameObjects()
            .Where(obj => !obj.name.StartsWith("[Architect] "))
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .Select(obj => obj.gameObject);
        
        var lowest = float.MaxValue;
        GameObject point = null;
        foreach (var obj in objects)
        {
            var pos = obj.transform.position - disabler.transform.position;
            pos.z = 0;
            var dist = pos.sqrMagnitude;

            if (dist < lowest)
            {
                lowest = dist;
                point = obj;
            }
        }
        
        return point is not null && lowest <= 500 ? [point.gameObject.GetOrAddComponent<Disabler>()] : [];
    }

    private static readonly Dictionary<string, Func<GameObject, Disabler[]>> EditActions = new();

    private static AbstractPackElement CreateCameraBorder()
    {
        CameraBorder.Init();
        
        var obj = new GameObject("Camera Border");
        obj.AddComponent<CameraBorder>();
        
        var sprite = ResourceUtils.LoadInternal("camera");
        obj.layer = 10;
        obj.transform.position += new Vector3(0, 0, 0.1f);
        
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        return new PreviewablePackElement(obj, "Camera Border", "Room Edits", sprite)
            .WithConfigGroup(ConfigGroup.CameraBorder);
    }

    private static AbstractPackElement CreateSceneBorderRemover()
    {
        var obj = new GameObject("Scene Border Remover");
        obj.AddComponent<SceneBorderRemover>();
        
        var sprite = ResourceUtils.LoadInternal("scene_border_remover");
        obj.layer = 10;
        obj.transform.position += new Vector3(0, 0, 0.1f);
        
        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        return new PreviewablePackElement(obj, "Remove Top Right Border", "Room Edits", sprite);
    }

    private static SimplePackElement CreateObjectRemover(string id, string name, [CanBeNull] Func<GameObject, Disabler[]> action)
    {
        var obj = new GameObject { name = "Object Remover (" + id + ")" };
        
        EditActions[id] = action;
        obj.AddComponent<ObjectRemover>().triggerName = id;
        
        var sprite = ResourceUtils.LoadInternal(id, FilterMode.Point);
        obj.layer = 10;
        obj.transform.position += new Vector3(0, 0, 0.1f);

        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        return new PreviewablePackElement(obj, name, "Room Edits", sprite);
    }
    
    private static AbstractPackElement CreateTransitionPoint()
    {
        var obj = new GameObject { name = "Transition Point" };

        var point = obj.AddComponent<TransitionPoint>();
        point.nonHazardGate = true;
        point.transform.localScale *= 3;
        
        var col = obj.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1, 1);
        col.isTrigger = true;

        obj.layer = LayerMask.NameToLayer("TransitionGates");

        var sprite = ResourceUtils.LoadInternal("door", FilterMode.Point);

        Object.DontDestroyOnLoad(obj);
        obj.SetActive(false);
        return new SimplePackElement(obj, "Transition Point", "Room Edits", sprite);
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

    private static GameObject CreateDarkness()
    {
        Darkness.Init();
        
        var point = new GameObject("Darkness");

        point.AddComponent<Darkness>();
        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateBinoculars()
    {
        var point = new GameObject("Binoculars");

        var col = point.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.25f, 1.06f);
        
        point.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadInternal("binoculars");
        
        Binoculars.Init();
        var softTerrain = LayerMask.NameToLayer("Soft Terrain");
        point.layer = softTerrain;
        point.AddComponent<Binoculars>();
        
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