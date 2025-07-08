using System.Reflection;
using Architect.Util;
using HK8YPlando;
using HK8YPlando.Scripts.SharedLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Elements.Custom.SaL;

public static class SaLObjects
{
    public static void Initialize()
    {
        var pack = new ContentPack("Architect S&L", "Architect's expansions for the Celeste themed Scattered and Lost mod")
        {
            MakeZipper(),
            MakeArchitectObject(ScatteredAndLostSceneManagerAPI.LoadPrefab<GameObject>("Switch"), "Coin Switch", "switch", 168.6f)
                .WithConfigGroup(SaLGroups.CoinConfig),
            MakeArchitectObject(ScatteredAndLostSceneManagerAPI.LoadPrefab<GameObject>("SSwitchDoor"), "Coin Switch Door", "switchdoor", 64)
                .WithConfigGroup(SaLGroups.CoinDoorConfig),
            MakeArchitectObject(ScatteredAndLostSceneManagerAPI.LoadPrefab<GameObject>("Bumper"), "Bumper", "bumper")
                .WithConfigGroup(SaLGroups.BumperConfig),
            MakeArchitectObject(ScatteredAndLostSceneManagerAPI.LoadPrefab<GameObject>("BubbleController"), "Bubble", "bubble")
                .WithConfigGroup(SaLGroups.BubbleConfig),
            CustomSaL.MakeCrystalObject()
        };
        
        ContentPacks.RegisterPack(pack);
    }

    private static AbstractPackElement MakeArchitectObject(GameObject obj, string name, string path, float ppu = 100)
    {
        Object.DontDestroyOnLoad(obj);
        return new SimplePackElement(obj, name, "Scattered & Lost", 
            ResourceUtils.Load($"HK8YPlando.Resources.Sprites.{path}.png", FilterMode.Point, typeof(ScatteredAndLostMod), ppu));
    }

    public static void SetField(GameObject o, string classPath, float value, string name)
    {
        var comp = o.GetComponent(classPath);
        if (comp)
        {
            var info = comp.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (info != null) info.SetValue(comp, value);
        }
    }

    public static float GetField(GameObject o, string classPath, string name)
    {
        var comp = o.GetComponent(classPath);
        if (comp)
        {
            var info = comp.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (info != null) return (float) info.GetValue(comp);
        }

        return 1;
    }

    public static void SetProperty(GameObject o, string classPath, int value, string name)
    {
        var comp = o.GetComponent(classPath);
        if (comp)
        {
            var info = comp.GetType().GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null) info.SetValue(comp, value);
        }
    }

    private static AbstractPackElement MakeZipper()
    {
        var gameObject = ScatteredAndLostSceneManagerAPI.LoadPrefab<GameObject>("Zipper");
        
        SetTrackX(gameObject, "HK8YPlando.Scripts.Platforming.Zipper", 5);
        
        return MakeArchitectObject(gameObject, "Zipper", "zipper", 52.5f)
            .WithConfigGroup(SaLGroups.ZipperConfig);
    }

    internal static void SetTopSpikes(GameObject gameObject, bool value)
    {
        var (_, right, bot, left) = ZipperLib.LoadSpikes(gameObject);
        ZipperLib.UpdateZipperAssets(gameObject, value, right, bot, left, _ => { });
    }

    internal static void SetBottomSpikes(GameObject gameObject, bool value)
    {
        var (top, right, _, left) = ZipperLib.LoadSpikes(gameObject);
        ZipperLib.UpdateZipperAssets(gameObject, top, right, value, left, _ => { });
    }

    internal static void SetLeftSpikes(GameObject gameObject, bool value)
    {
        var (top, right, bot, _) = ZipperLib.LoadSpikes(gameObject);
        ZipperLib.UpdateZipperAssets(gameObject, top, right, bot, value, _ => { });
    }

    internal static void SetRightSpikes(GameObject gameObject, bool value)
    {
        var (top, _, bot, left) = ZipperLib.LoadSpikes(gameObject);
        ZipperLib.UpdateZipperAssets(gameObject, top, value, bot, left, _ => { });
    }

    internal static void SetTrackY(GameObject gameObject, string classPath, float y)
    {
        var comp = gameObject.GetComponent(classPath);

        if (!comp) return;
        
        var info = comp.GetType().GetField("TargetPosition", BindingFlags.Public | BindingFlags.Instance);
        if (info == null) return;

        var target = (Transform) info.GetValue(comp);
        target.localPosition = new Vector3(target.localPosition.x, y, target.localPosition.z);
        
        UpdateZipline(gameObject);
    }

    internal static void SetTrackX(GameObject gameObject, string classPath, float x)
    {
        var comp = gameObject.GetComponent(classPath);

        if (!comp) return;
        
        var info = comp.GetType().GetField("TargetPosition", BindingFlags.Public | BindingFlags.Instance);
        if (info == null) return;

        var target = (Transform) info.GetValue(comp);
        target.localPosition = new Vector3(x, target.localPosition.y, target.localPosition.z);
        
        UpdateZipline(gameObject);
    }

    internal static void SetXMove(GameObject gameObject, string classPath, float x)
    {
        var comp = gameObject.GetComponent(classPath);

        if (!comp) return;
        
        var info = comp.GetType().GetField("MoveOffset", BindingFlags.Public | BindingFlags.Instance);
        if (info == null) return;

        var offset = (Vector3) info.GetValue(comp);
        offset.x = x;

        object[] parametersArray = [offset];

        var method = comp.GetType()
            .GetMethod("DecoMasterSetMoveOffset", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) return;

        var pos = gameObject.transform.position;
        
        method.Invoke(comp, parametersArray);

        gameObject.transform.position = pos;
    }

    internal static void SetYMove(GameObject gameObject, string classPath, float y)
    {
        var comp = gameObject.GetComponent(classPath);

        if (!comp) return;
        
        var info = comp.GetType().GetField("MoveOffset", BindingFlags.Public | BindingFlags.Instance);
        if (info == null) return;

        var offset = (Vector3) info.GetValue(comp);
        offset.y = y;

        object[] parametersArray = [offset];

        var method = comp.GetType()
            .GetMethod("DecoMasterSetMoveOffset", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) return;
        
        var pos = gameObject.transform.position;

        method.Invoke(comp, parametersArray);
        
        gameObject.transform.position = pos;
    }

    private static void UpdateZipline(GameObject gameObject)
    {
        var (top, right, bot, left) = ZipperLib.LoadSpikes(gameObject);
        ZipperLib.UpdateZipperAssets(gameObject, top, right, bot, left, _ => { });
    }
}