using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Architect.Content.Groups;
using Architect.Util;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using MonoMod.RuntimeDetour;
using Satchel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Content.Elements.Custom;

public static class CustomObjects
{
    internal static readonly HashSet<string> TemporaryAbilities = new();
    internal static readonly Dictionary<string, List<CustomBinder>> Bindings = new();

    public static void Initialize()
    {
        var clip = ResourceUtils.LoadClip("Bindings.chain_cut");

        var customs = new ContentPack("Custom", "Custom assets that don't exist in the base game")
        {
            new SimplePackElement(CreateHazardRespawnPoint(), "Hazard Respawn Point", "Custom",
                ResourceUtils.Load("hazard_respawn_point")),
            new SimplePackElement(CreateObject("Teleport Point"), "Teleport Point", "Custom",
                ResourceUtils.Load("teleport_point"))
                .WithReceiverGroup(ReceiverGroup.TeleportPoint),
            new SimplePackElement(CreateTriggerZone(), "Trigger Zone", "Custom",
                    ResourceUtils.Load("trigger_zone", FilterMode.Point))
                .WithBroadcasterGroup(BroadcasterGroup.TriggerZones),
            new SimplePackElement(CreateZoteTrophy(), "Winner's Trophy", "Custom"),
            CreateTemporaryAbilityGranter("dash_crystal", "Dash", false, "Dash Crystal"),
            CreateTemporaryAbilityGranter("single_dash_crystal", "Dash", true, "Single Use Dash Crystal"),
            CreateTemporaryAbilityGranter("wings_crystal", "Wings", false, "Wings Crystal"),
            CreateTemporaryAbilityGranter("single_wings_crystal", "Wings", true, "Single Use Wings Crystal"),
            new SimplePackElement(CreateDamagingOrb("energy_orb", "Energy Orb", 1), "Energy Orb", "Custom")
                .WithConfigGroup(ConfigGroup.MovingObjects),
            new SimplePackElement(CreateDamagingOrb("radiant_orb", "Radiant Orb", 999), "Radiant Orb", "Custom")
                .WithConfigGroup(ConfigGroup.MovingObjects),
            CreateBinding("nail", "Nail Binding", clip),
            CreateBinding("shell", "Shell Binding", clip),
            CreateBinding("charms", "Charm Binding", clip),
            CreateBinding("soul", "Soul Binding", clip),
            CreateBinding("focus", "Focus Binding", clip),
            CreateBinding("spirit", "Vengeful Spirit Binding", clip),
            CreateBinding("dive", "Desolate Dive Binding", clip),
            CreateBinding("wraiths", "Howling Wraiths Binding", clip),
            CreateBinding("dash", "Dash Binding", clip),
            CreateBinding("claw", "Mantis Claw Binding", clip),
            CreateBinding("cdash", "Crystal Heart Binding", clip),
            CreateBinding("tear", "Isma's Tear Binding", clip),
            CreateBinding("wings", "Monarch Wings Binding", clip),
            CreateBinding("dnail", "Dream Nail Binding", clip),
            CreateBinding("gate", "Dreamgate Binding", clip),
            CreateBinding("gslash", "Great Slash Binding", clip),
            CreateBinding("cslash", "Cyclone Slash Binding", clip),
            CreateBinding("dslash", "Dash Slash Binding", clip)
        };
        ContentPacks.RegisterPack(customs);

        InitializeHooks();
    }

    private static GameObject CreateDamagingOrb(string path, string name, int damage)
    {
        var sprite = ResourceUtils.Load(path);
        var point = new GameObject(name);

        point.AddComponent<SpriteRenderer>().sprite = sprite;
        var collider = point.AddComponent<CircleCollider2D>();
        collider.radius = 0.35f;
        collider.isTrigger = true;

        point.AddComponent<CustomDamager>().damageAmount = damage;

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
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

    private static GameObject CreateTriggerZone()
    {
        var point = new GameObject("Trigger Zone");
        point.transform.localScale *= 10;

        var collider = point.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.32f, 0.32f);

        point.AddComponent<TriggerZone>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateZoteTrophy()
    {
        var sprite = ResourceUtils.Load("zote_trophy");
        var obj = new GameObject("Winner's Trophy");

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        obj.AddComponent<ParticleSystem>();

        obj.AddComponent<ZoteTrophy>();

        obj.SetActive(false);
        Object.DontDestroyOnLoad(obj);

        return obj;
    }

    private static SimplePackElement CreateTemporaryAbilityGranter(string path, string ability, bool singleUse,
        string name)
    {
        var sprite = ResourceUtils.Load(path, FilterMode.Point);

        var granterObj = new GameObject(ability + " Granter");

        var renderer = granterObj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        var granter = granterObj.AddComponent<TemporaryAbilityGranter>();
        granter.abilityType = ability;
        granter.singleUse = singleUse;

        var collider = granterObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        granterObj.SetActive(false);
        Object.DontDestroyOnLoad(granterObj);

        granterObj.transform.localScale *= 7;

        return new SimplePackElement(granterObj, name, "Custom");
    }

    private static AbstractPackElement CreateBinding(string id, string name, AudioClip clip)
    {
        var disabledSprite = ResourceUtils.Load("Bindings." + id + "_disabled");
        var enabledSprite = ResourceUtils.Load("Bindings." + id + "_enabled");

        var bindingObj = new GameObject("Binding (" + id + ")");

        var renderer = bindingObj.AddComponent<SpriteRenderer>();
        renderer.sprite = enabledSprite;

        var binder = bindingObj.AddComponent<CustomBinder>();
        binder.bindingType = id;
        binder.disabledSprite = disabledSprite;
        binder.enabledSprite = enabledSprite;
        binder.clip = clip;

        var collider = bindingObj.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.65f;

        bindingObj.SetActive(false);
        Object.DontDestroyOnLoad(bindingObj);

        return new SimplePackElement(bindingObj, name, "Custom")
            .WithConfigGroup(ConfigGroup.Bindings)
            .WithBroadcasterGroup(BroadcasterGroup.Bindings);
    }

    private static bool BindingCheck(bool orig, string type)
    {
        if (!Bindings.TryGetValue(type, out var list) || list.Count == 0) return orig;
        foreach (var binding in list.ToList())
        {
            if (!binding) list.Remove(binding);
            else if (binding.active && binding.gameObject.activeInHierarchy) return false;
        }

        return orig;
    }

    private static void InitializeHooks()
    {
        InitializePantheonBindings();


        // Bindings
        On.HeroController.CanDash += (orig, self) => BindingCheck(orig(self), "dash");
        On.HeroController.CanFocus += (orig, self) => BindingCheck(orig(self), "focus");
        On.HeroController.CanSuperDash += (orig, self) => BindingCheck(orig(self), "cdash");
        On.HeroController.CanWallJump += (orig, self) => BindingCheck(orig(self), "claw");
        On.HeroController.CanWallSlide += (orig, self) => BindingCheck(orig(self), "claw");
        On.HeroController.CanDoubleJump += (orig, self) => BindingCheck(orig(self), "wings");
        On.HeroController.CanDreamNail += (orig, self) => BindingCheck(orig(self), "dnail");

        On.HeroController.CanNailCharge += (orig, self) =>
        {
            if (!orig(self)) return false;
            return BindingCheck(PlayerData.instance.hasUpwardSlash, "dslash") ||
                   BindingCheck(PlayerData.instance.hasDashSlash, "gslash") ||
                   BindingCheck(PlayerData.instance.hasCyclone, "cslash");
        };
        On.HeroController.CanNailArt += (orig, self) =>
        {
            if (!orig(self)) return false;
            return BindingCheck(PlayerData.instance.hasUpwardSlash, "dslash") ||
                   BindingCheck(PlayerData.instance.hasDashSlash, "gslash") ||
                   BindingCheck(PlayerData.instance.hasCyclone, "cslash");
        };

        On.PlayMakerFSM.Awake += (orig, self) =>
        {
            orig(self);
            switch (self.FsmName)
            {
                case "Spell Control":
                    InitSpellBindings(self);
                    break;
                case "Nail Arts":
                    InitNailArtBindings(self);
                    break;
                case "Dream Nail":
                    InitDreamNailBindings(self);
                    break;
                case "Acid Armour Check":
                    InitTearBinding(self);
                    break;
            }
        };

        // Crystals
        On.HeroController.CanDash += (orig, self) => TemporaryAbilities.Contains("Dash") || orig(self);
        On.HeroController.HeroDash += (orig, self) =>
        {
            orig(self);
            TemporaryAbilities.Remove("Dash");
        };
        On.HeroController.CanDoubleJump += (orig, self) => TemporaryAbilities.Contains("Wings") || orig(self);
        On.HeroController.DoDoubleJump += (orig, self) =>
        {
            orig(self);
            TemporaryAbilities.Remove("Wings");
        };
    }

    private static void InitSpellBindings(PlayMakerFSM fsm)
    {
        fsm.InsertCustomAction("Has Fireball?", playMakerFsm =>
        {
            if (!BindingCheck(true, "spirit")) playMakerFsm.FsmVariables.FindFsmInt("Spell Level").Value = 0;
        }, 1);
        fsm.InsertCustomAction("Has Quake?", playMakerFsm =>
        {
            if (!BindingCheck(true, "dive")) playMakerFsm.FsmVariables.FindFsmInt("Spell Level").Value = 0;
        }, 1);
        fsm.InsertCustomAction("Has Scream?", playMakerFsm =>
        {
            if (!BindingCheck(true, "wraiths")) playMakerFsm.FsmVariables.FindFsmInt("Spell Level").Value = 0;
        }, 1);
    }

    private static void InitNailArtBindings(PlayMakerFSM fsm)
    {
        fsm.AddCustomAction("Dash Slash Ready", playMakerFsm =>
        {
            if (!BindingCheck(true, "dslash")) playMakerFsm.SetState("Inactive");
        });

        fsm.InsertCustomAction("Has Cyclone?", playMakerFsm =>
        {
            if (!BindingCheck(true, "cslash")) playMakerFsm.FsmVariables.FindFsmBool("Has Cyclone").Value = false;
        }, 2);
        fsm.InsertCustomAction("Has Cyclone?", playMakerFsm =>
        {
            if (!BindingCheck(true, "gslash")) playMakerFsm.FsmVariables.FindFsmBool("Has G Slash").Value = false;
        }, 2);

        fsm.InsertCustomAction("Has G Slash?", playMakerFsm =>
        {
            if (!BindingCheck(true, "cslash")) playMakerFsm.FsmVariables.FindFsmBool("Has Cyclone").Value = false;
        }, 2);
        fsm.InsertCustomAction("Has G Slash?", playMakerFsm =>
        {
            if (!BindingCheck(true, "gslash")) playMakerFsm.FsmVariables.FindFsmBool("Has G Slash").Value = false;
        }, 2);
    }

    private static readonly SetCollider DisableSwim = new()
    {
        active = false,
        gameObject = new FsmOwnerDefault
        {
            OwnerOption = OwnerDefaultOption.UseOwner
        }
    };
    
    private static readonly SetDamageHeroAmount Damage = new()
    {
        damageDealt = 1,
        target = new FsmOwnerDefault
        {
            OwnerOption = OwnerDefaultOption.UseOwner
        }
    };

    private static void InitTearBinding(PlayMakerFSM fsm)
    {
        fsm.DisableAction("Check", 0);
        fsm.AddCustomAction("Check", CustomTearAction);
        fsm.AddGlobalTransition("REFRESH ACID ARMOUR", "Check");
        var damager = fsm.TryGetState("Disable", out _);
        fsm.AddState("Lost Tear").AddAction(damager ? Damage : DisableSwim);
        
        fsm.AddTransition("Check", "LOST", "Lost Tear");
    }

    private static void CustomTearAction(PlayMakerFSM makerFsm)
    {
        if (!BindingCheck(PlayerData.instance.hasAcidArmour, "tear"))
        {
            makerFsm.SendEvent("LOST");
            return;
        }
            
        makerFsm.SendEvent("DISABLE");
        makerFsm.SendEvent("ENABLE");
    }

    private static void InitDreamNailBindings(PlayMakerFSM fsm)
    {
        fsm.InsertCustomAction("Dream Gate?", makerFsm =>
        {
            if (!BindingCheck(true, "gate")) makerFsm.SendEvent("FINISHED");
        }, 1);
    }

    public static void RefreshBinding(string binding)
    {
        if (BossSequenceController.IsInSequence) return;
        switch (binding)
        {
            case "charms":
                RefreshCharmsBinding();
                break;
            case "shell":
                RefreshShellBinding();
                break;
            case "soul":
                RefreshSoulBinding();
                break;
            case "nail":
                RefreshNailBinding();
                break;
            case "tear":
                RefreshTearBinding();
                break;
        }
    }

    private static void RefreshTearBinding()
    {
        PlayMakerFSM.BroadcastEvent("REFRESH ACID ARMOUR");
    }

    private static void RefreshNailBinding()
    {
        if (_nailBound == ShouldBindNail()) return;
        _nailBound = !_nailBound;
        
        EventRegister.SendEvent(_nailBound ? "SHOW BOUND NAIL" : "HIDE BOUND NAIL");
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    private static void RefreshShellBinding()
    {
        if (_shellBound == ShouldBindShell()) return;
        _shellBound = !_shellBound;

        var health = GameManager.instance.playerData.health;
        GameManager.instance.playerData.MaxHealth();
        GameManager.instance.playerData.health = health;
        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
        PlayMakerFSM.BroadcastEvent("HUD IN");
    }

    private static int[] _previousEquippedCharms;
    private static bool _wasOvercharmed;
    
    private static bool _charmsBound;
    private static bool _shellBound;
    private static bool _soulBound;
    private static bool _nailBound;

    private static void RefreshCharmsBinding()
    {
        if (ShouldBindCharms())
        {
            if (_charmsBound) return;
            
            _previousEquippedCharms = GameManager.instance.playerData.GetVariable<List<int>>("equippedCharms").ToArray();
            GameManager.instance.playerData.GetVariable<List<int>>("equippedCharms").Clear();
            _wasOvercharmed = GameManager.instance.playerData.GetBool("overcharmed");
            GameManager.instance.playerData.SetBool("overcharmed", false);
            
            foreach (var previousEquippedCharm in _previousEquippedCharms)
                GameManager.instance.SetPlayerDataBool("equippedCharm_" + previousEquippedCharm, false);
            
            EventRegister.SendEvent("SHOW BOUND CHARMS");

            _charmsBound = true;
        }
        else if (_charmsBound)
        {
            GameManager.instance.playerData.SetVariable(
                "equippedCharms",
                new List<int>(_previousEquippedCharms));
            
            foreach (var previousEquippedCharm in _previousEquippedCharms)
                GameManager.instance.SetPlayerDataBool("equippedCharm_" + previousEquippedCharm, true);
            
            GameManager.instance.playerData.SetBool("overcharmed", _wasOvercharmed);
            EventRegister.SendEvent("HIDE BOUND CHARMS");

            _charmsBound = false;
        }
    }

    private static void RefreshSoulBinding()
    {
        if (ShouldBindSoul() == _soulBound) return;
        _soulBound = !_soulBound;
        
        if (_soulBound)
        {
            var soul = PlayerData.instance.MPCharge;
            PlayerData.instance.ClearMP();
            PlayerData.instance.AddMPCharge(Math.Min(soul, 33));
        }
        
        GameManager.instance.StartCoroutine(RefreshSoulBindingRoutine());
    }

    private static IEnumerator RefreshSoulBindingRoutine()
    {
        while (GameManager.instance.soulOrb_fsm.ActiveStateName != "Idle")
        {
            yield return Task.Yield();
        }
        if (_soulBound)
        {
            EventRegister.SendEvent("BIND VESSEL ORB");
            GameManager.instance.soulOrb_fsm.SendEvent("MP LOSE");
            GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
        }
        else
        {
            GameManager.instance.soulOrb_fsm.SendEvent("MP LOSE");
            EventRegister.SendEvent("UNBIND VESSEL ORB");
        }
    }

    public static bool ShouldBindSoul()
    {
        if (BossSequenceController.IsInSequence) return ShouldBind(BossSequenceController.ChallengeBindings.Soul);
        return !BindingCheck(true, "soul");

    }

    public static bool ShouldBindShell()
    {
        if (BossSequenceController.IsInSequence) return ShouldBind(BossSequenceController.ChallengeBindings.Shell);
        return !BindingCheck(true, "shell");
    }

    private static bool ShouldBind(BossSequenceController.ChallengeBindings binding)
    {
        if (!BossSequenceController.IsInSequence) return false;
        var cd = ReflectionHelper.GetField<BossSequenceController.BossSequenceData>(typeof(BossSequenceController),
            "currentData");
        if (cd == null) return false;
        return (cd.bindings & binding) == binding;
    }

    public static int BoundShell()
    {
        var num = !GameManager.instance.playerData.GetBool("equippedCharm_23") || GameManager.instance.playerData.GetBool("brokenCharm_23") ? 0 : 2;
        return 4 + num;
    }

    public static int BoundNailDamage()
    {
        var num = GameManager.instance.playerData.GetInt("nailDamage");
        if (!ShouldBindNail()) return num;
        return num >= 13 ? 13 : Mathf.RoundToInt(num * 0.8f);
    }

    public static bool ShouldBindNail()
    {
        if (BossSequenceController.IsInSequence) return ShouldBind(BossSequenceController.ChallengeBindings.Nail);
        return !BindingCheck(true, "nail");
    }

    public static bool ShouldBindCharms()
    {
        if (BossSequenceController.IsInSequence) return ShouldBind(BossSequenceController.ChallengeBindings.Charms);
        return !BindingCheck(true, "charms");
    }

    public static void InitializePantheonBindings()
    {
        _ = new Detour(
            typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundSoul))?.GetGetMethod(),
            typeof(CustomObjects).GetMethod(nameof(ShouldBindSoul))
        );
        
        _ = new Detour(
            typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundShell))?.GetGetMethod(),
            typeof(CustomObjects).GetMethod(nameof(ShouldBindShell))
        );
        _ = new Detour(
            typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundMaxHealth))?.GetGetMethod(),
            typeof(CustomObjects).GetMethod(nameof(BoundShell))
        );
        
        _ = new Detour(
            typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundNail))?.GetGetMethod(),
            typeof(CustomObjects).GetMethod(nameof(ShouldBindNail))
        );
        _ = new Detour(
            typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundNailDamage))?.GetGetMethod(),
            typeof(CustomObjects).GetMethod(nameof(BoundNailDamage))
        );
        
        _ = new Detour(
            typeof(BossSequenceController).GetProperty(nameof(BossSequenceController.BoundCharms))?.GetGetMethod(),
            typeof(CustomObjects).GetMethod(nameof(ShouldBindCharms))
        );

        On.GGCheckBoundSoul.OnEnter += (orig, self) =>
        {
            if (BossSequenceController.IsInSequence) return;
            if (ShouldBindSoul())
            {
                self.Fsm.Event(self.boundEvent);
                self.Finish();
            }
            else orig(self);
        };

        On.HeroController.SceneInit += (orig, self) =>
        {
            orig(self);
            if (BossSequenceController.IsInSequence) return;
            
            RefreshCharmsBinding();
            RefreshNailBinding();
            RefreshShellBinding();
            RefreshSoulBinding();
        };
    }
}