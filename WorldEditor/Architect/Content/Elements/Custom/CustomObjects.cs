using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Architect.Attributes;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Groups;
using Architect.Util;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MagicUI.Core;
using MagicUI.Elements;
using Modding;
using MonoMod.RuntimeDetour;
using Satchel;
using UnityEngine;
using GridLayout = MagicUI.Elements.GridLayout;
using Object = UnityEngine.Object;

namespace Architect.Content.Elements.Custom;

public static class CustomObjects
{
    internal static readonly HashSet<string> TemporaryAbilities = [];
    internal static readonly List<PlayerHook> PlayerListeners = [];
    internal static readonly Dictionary<string, List<CustomBinder>> Bindings = new();
    internal static readonly List<string> ExtraVisualsBindings = [];

    private static GridLayout _bindingsLayout; 
        
    private const int ShapeWeight = 1;

    public static void Initialize()
    {
        var customs = new ContentPack("Custom", "Custom assets that don't exist in the base game")
        {
            new SimplePackElement(CreateTriggerZone(), "Trigger Zone", "Custom",
                    ResourceUtils.Load("trigger_zone", FilterMode.Point))
                .WithBroadcasterGroup(BroadcasterGroup.TriggerZones)
                .WithConfigGroup(ConfigGroup.TriggerZones),
            new SimplePackElement(CreateTimer(), "Timer", "Custom",
                    ResourceUtils.Load("timer", FilterMode.Point))
                .WithBroadcasterGroup(BroadcasterGroup.Callable)
                .WithConfigGroup(ConfigGroup.Timers),
            new SimplePackElement(CreateKeyListener(), "Key Listener", "Custom",
                    ResourceUtils.Load("key_listener", FilterMode.Point))
                .WithBroadcasterGroup(BroadcasterGroup.KeyListeners)
                .WithConfigGroup(ConfigGroup.KeyListeners),
            new SimplePackElement(CreateRelay(), "Relay", "Custom",
                    ResourceUtils.Load("event_relay", FilterMode.Point))
                .WithBroadcasterGroup(BroadcasterGroup.Callable)
                .WithReceiverGroup(ReceiverGroup.Relays)
                .WithConfigGroup(ConfigGroup.Relays),
            new SimplePackElement(CreateTextDisplay(), "Text Display", "Custom",
                    ResourceUtils.Load("text_display", FilterMode.Point))
                .WithReceiverGroup(ReceiverGroup.TextDisplay)
                .WithConfigGroup(ConfigGroup.TextDisplay),
            new SimplePackElement(CreatePlayerListener(), "Player Hook", "Custom",
                    ResourceUtils.Load("player_listener"))
                .WithBroadcasterGroup(BroadcasterGroup.PlayerHook)
                .WithReceiverGroup(ReceiverGroup.PlayerHook)
                .WithConfigGroup(ConfigGroup.Invisible),
            new SimplePackElement(CreateEnemyBarrier(), "Enemy Barrier", "Custom",
                    ResourceUtils.Load("enemy_barrier"))
                .WithConfigGroup(ConfigGroup.Stretchable),
            new SimplePackElement(CreateObjectMover(), "Object Mover", "Custom",
                    ResourceUtils.Load("object_mover"))
                .WithReceiverGroup(ReceiverGroup.ObjectMover)
                .WithConfigGroup(ConfigGroup.ObjectMover),
            new SimplePackElement(CreateObjectDuplicator(), "Object Duplicator", "Custom",
                    ResourceUtils.Load("object_duplicator"))
                .WithReceiverGroup(ReceiverGroup.ObjectDuplicator)
                .WithConfigGroup(ConfigGroup.ObjectDuplicator),
            new SimplePackElement(CreateWidePlatform(), "Wide Platform", "Solids")
                .WithConfigGroup(ConfigGroup.MovingPlatforms),
            CreateSquare(),
            CreateCircle(),
            CreateTriangle(),
            new SimplePackElement(CreateZoteTrophy(), "Winner's Trophy", "Custom"),
            CreateTemporaryAbilityGranter("dash_crystal", "Dash", false, "Dash Crystal"),
            CreateTemporaryAbilityGranter("single_dash_crystal", "Dash", true, "Single Use Dash Crystal"),
            CreateTemporaryAbilityGranter("wings_crystal", "Wings", false, "Wings Crystal"),
            CreateTemporaryAbilityGranter("single_wings_crystal", "Wings", true, "Single Use Wings Crystal"),
            new SimplePackElement(CreateDamagingOrb("energy_orb", "Energy Orb", 1), "Energy Orb", "Custom")
                .WithConfigGroup(ConfigGroup.MovingObjects),
            new SimplePackElement(CreateDamagingOrb("radiant_orb", "Radiant Orb", 999), "Radiant Orb", "Custom")
                .WithConfigGroup(ConfigGroup.MovingObjects),
            CreateBinding("nail", "Nail Binding", false),
            CreateBinding("shell", "Shell Binding", false),
            CreateBinding("charms", "Charm Binding", false),
            CreateBinding("soul", "Soul Binding", false),
            CreateBinding("focus", "Focus Binding"),
            CreateBinding("spirit", "Vengeful Spirit Binding"),
            CreateBinding("dive", "Desolate Dive Binding"),
            CreateBinding("wraiths", "Howling Wraiths Binding"),
            CreateBinding("pogo", "Pogo Binding"),
            CreateBinding("lantern", "Lantern Binding"),
            CreateBinding("dash", "Dash Binding"),
            CreateBinding("shadow_dash", "Shadow Dash Binding"),
            CreateBinding("claw", "Mantis Claw Binding"),
            CreateBinding("cdash", "Crystal Heart Binding"),
            CreateBinding("tear", "Isma's Tear Binding"),
            CreateBinding("wings", "Monarch Wings Binding"),
            CreateBinding("dnail", "Dream Nail Binding"),
            CreateBinding("gate", "Dreamgate Binding"),
            CreateBinding("gslash", "Great Slash Binding"),
            CreateBinding("cslash", "Cyclone Slash Binding"),
            CreateBinding("dslash", "Dash Slash Binding")
        };
        ContentPacks.RegisterPack(customs);

        InitializeBindingsUI();
        InitializeHooks();
    }

    private static void InitializeBindingsUI()
    {
        var root = new LayoutRoot(true, "Bindings Layout");
        _bindingsLayout = new GridLayout(root, "Bindings")
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        for (var i = 0; i < 17; i++)
        {
            _bindingsLayout.ColumnDefinitions.Add(new GridDimension(1, GridUnit.Proportional));
            _bindingsLayout.Children.Add(new Image(root, Architect.BlankSprite)
                .WithProp(GridLayout.Column, i));
        }
    }

    private static AbstractPackElement CreateSquare()
    {
        var square = CreateShape("square");
        
        var collider = square.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(10, 10);
        
        return new SimplePackElement(square, "Coloured Square", "Decorations", weight: ShapeWeight)
            .WithConfigGroup(ConfigGroup.Shapes)
            .WithRotationGroup(RotationGroup.All);
    }

    private static AbstractPackElement CreateCircle()
    {
        var circle = CreateShape("circle");
        
        var collider = circle.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;

        var points = new Vector2[24];
        for (var i = 0; i < 24; i++)
        {
            var angle = 2 * Mathf.PI * i / 24;
            var x = Mathf.Cos(angle) * 5;
            var y = Mathf.Sin(angle) * 5;
            points[i] = new Vector2(x, y);
        }

        collider.pathCount = 1;
        collider.SetPath(0, points);
        
        return new SimplePackElement(circle, "Coloured Circle", "Decorations", weight: ShapeWeight)
            .WithConfigGroup(ConfigGroup.Shapes)
            .WithRotationGroup(RotationGroup.All);
    }

    private static AbstractPackElement CreateTriangle()
    {
        var triangle = CreateShape("triangle");
        
        var collider = triangle.AddComponent<EdgeCollider2D>();
        collider.isTrigger = true;
        collider.points =
        [
            new Vector2(-5, -4.17f),
            new Vector2(0, 4.45f),
            new Vector2(5, -4.17f),
            new Vector2(-5, -4.17f)
        ];
        
        return new SimplePackElement(triangle, "Coloured Triangle", "Decorations", weight: ShapeWeight)
            .WithConfigGroup(ConfigGroup.Shapes)
            .WithRotationGroup(RotationGroup.All);
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

    private static GameObject CreateKeyListener()
    {
        var point = new GameObject("Key Listener");
        point.transform.localScale *= 10;

        point.AddComponent<KeyListener>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateRelay()
    {
        var point = new GameObject("Relay");
        point.transform.localScale *= 10;

        point.SetActive(false);
        point.AddComponent<Relay>();
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateTextDisplay()
    {
        var point = new GameObject("Text Display");
        point.transform.localScale *= 10;

        point.SetActive(false);
        point.AddComponent<TextDisplay>();
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreatePlayerListener()
    {
        var point = new GameObject("Player Hook");
        point.transform.localScale *= 2;

        point.SetActive(false);
        point.AddComponent<PlayerHook>();
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateObjectMover()
    {
        var point = new GameObject("Object Mover");

        point.SetActive(false);
        point.AddComponent<ObjectMover>();
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateObjectDuplicator()
    {
        var point = new GameObject("Object Duplicator");

        point.SetActive(false);
        point.AddComponent<ObjectDuplicator>();
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateWidePlatform()
    {
        var point = new GameObject("Wide Platform");

        point.AddComponent<SpriteRenderer>().sprite = ResourceUtils.Load("wide_platform");
        var col = point.AddComponent<BoxCollider2D>();
        col.size = new Vector2(11.30954f, 0.8839264f);
        col.offset = new Vector2(0.007995605f, 0.1063919f);
        point.layer = 8;

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateEnemyBarrier()
    {
        var point = new GameObject("Enemy Barrier");
        var heroOnly = LayerMask.NameToLayer("Hero Only");
        
        point.AddComponent<BoxCollider2D>().size = new Vector2(2, 2);
        point.transform.localScale *= 5f / 3;
        point.layer = heroOnly;
        
        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateTimer()
    {
        var point = new GameObject("Timer");
        point.transform.localScale *= 10;

        point.AddComponent<Timer>();

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return point;
    }

    private static GameObject CreateShape(string name)
    {
        var sprite = ResourceUtils.Load(name);
        
        var point = new GameObject("Shape (" + name + ")");
        point.transform.localScale /= 3;

        point.AddComponent<SpriteRenderer>().sprite = sprite;

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

    private static AbstractPackElement CreateBinding(string id, string name, bool extraVisuals = true)
    {
        if (extraVisuals) ExtraVisualsBindings.Add(id);
        
        var disabledSprite = ResourceUtils.Load("Bindings." + id + "_disabled");
        var enabledSprite = ResourceUtils.Load("Bindings." + id + "_enabled");

        var bindingObj = new GameObject("Binding (" + id + ")");

        var renderer = bindingObj.AddComponent<SpriteRenderer>();
        renderer.sprite = enabledSprite;

        var binder = bindingObj.AddComponent<CustomBinder>();
        binder.bindingType = id;
        binder.disabledSprite = disabledSprite;
        binder.enabledSprite = enabledSprite;

        binder.extraVisuals = extraVisuals;

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
        if (!orig) return false;
        
        if (!Bindings.TryGetValue(type, out var list) || list.Count == 0) return true;
        foreach (var binding in list.ToList())
        {
            if (!binding) list.Remove(binding);
            else if (binding.active && binding.gameObject.activeInHierarchy) return false;
        }

        return true;
    }

    private static void InitializeHooks()
    {
        SetupPlayerListeners();
        
        On.PersistentBoolItem.Awake += (orig, self) =>
        {
            if (self.persistentBoolData == null) return;
            orig(self);
        };

        CustomBinder.Init();
        On.HeroController.SceneInit += (orig, self) =>
        {
            orig(self);
            if (BossSequenceController.IsInSequence) return;
            
            RefreshCharmsBinding();
            RefreshNailBinding();
            RefreshShellBinding();
            RefreshSoulBinding();
        };
        
        InitializePantheonBindings();
        
        // Bindings
        On.HeroController.CanDash += (orig, self) => BindingCheck(orig(self), "dash");
        On.HeroController.CanFocus += (orig, self) => BindingCheck(orig(self), "focus");
        On.HeroController.CanSuperDash += (orig, self) => BindingCheck(orig(self), "cdash");
        On.HeroController.CanWallJump += (orig, self) => BindingCheck(orig(self), "claw");
        On.HeroController.CanWallSlide += (orig, self) => BindingCheck(orig(self), "claw");
        On.HeroController.CanDoubleJump += (orig, self) => BindingCheck(orig(self), "wings");
        On.HeroController.CanDreamNail += (orig, self) => BindingCheck(orig(self), "dnail");
        
        ModHooks.GetPlayerBoolHook += (name, orig) =>
        {
            return name switch
            {
                "hasShadowDash" => BindingCheck(orig, "shadow_dash"),
                "hasLantern" => BindingCheck(orig, "lantern"),
                _ => orig
            };
        };

        On.ShadowGateColliderControl.FixedUpdate += (orig, self) =>
        {
            var unlocked = self.GetType().GetField("unlocked", BindingFlags.NonPublic | BindingFlags.Instance);
            if (unlocked != null) unlocked.SetValue(self, GameManager.instance.playerData.GetBool("hasShadowDash"));
            orig(self);
        };

        On.HeroController.Bounce += (orig, self) =>
        {
            if (BindingCheck(true, "pogo")) orig(self);
        };

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
                    InitSpellHooks(self);
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

    private static void InitSpellHooks(PlayMakerFSM fsm)
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
        
        fsm.InsertCustomAction("Level Check", () =>
        {
            PlayerEvent("Spirit");
        }, 0);
        fsm.InsertCustomAction("Level Check 2", () =>
        {
            PlayerEvent("Dive");
        }, 0);
        fsm.InsertCustomAction("Level Check 3", () =>
        {
            PlayerEvent("Wraiths");
        }, 0);
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
            case "lantern":
                RefreshLanternBinding();
                break;
        }
/*
        var i = 0;
        foreach (var bind in ExtraVisualsBindings.Where(bind => Bindings[bind].Count > 0))
        {
            ((Image) _bindingsLayout.Children[i]).Sprite = Bindings[bind][0].enabledSprite;
            i++;
        }*/ // Incomplete
    }

    private static void RefreshTearBinding()
    {
        PlayMakerFSM.BroadcastEvent("REFRESH ACID ARMOUR");
    }

    internal static void RefreshLanternBinding()
    {
        PlayMakerFSM.BroadcastEvent("RESET");
        if (GameManager.instance.sm.GetDarknessLevel() > 0 && 
            BindingCheck(PlayerData.instance.GetBool("hasLantern"), "lantern"))
        {
            HeroController.instance.SetWieldingLantern(true);
        } else HeroController.instance.SetWieldingLantern(false);
    }

    private static void RefreshNailBinding()
    {
        if (BossSequenceController.IsInSequence) return;
        if (_nailBound == ShouldBindNail()) return;
        _nailBound = !_nailBound;
        
        EventRegister.SendEvent(_nailBound ? "SHOW BOUND NAIL" : "HIDE BOUND NAIL");
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    private static void RefreshShellBinding()
    {
        if (BossSequenceController.IsInSequence) return;
        if (_shellBound == ShouldBindShell()) return;
        _shellBound = !_shellBound;

        var health = GameManager.instance.playerData.health;
        GameManager.instance.playerData.MaxHealth();
        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
        PlayMakerFSM.BroadcastEvent("HUD IN");
        GameManager.instance.playerData.health = health;
    }

    private static int[] _previousEquippedCharms;
    private static bool _wasOvercharmed;
    
    private static bool _charmsBound;
    private static bool _shellBound;
    private static bool _soulBound;
    private static bool _nailBound;

    private static void RefreshCharmsBinding()
    {
        if (BossSequenceController.IsInSequence) return;
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
        if (BossSequenceController.IsInSequence) return;
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
            if (BossSequenceController.IsInSequence)
            {
                orig(self);
                return;
            }
            if (ShouldBindSoul())
            {
                self.Fsm.Event(self.boundEvent);
                self.Finish();
            }
            else orig(self);
        };
    }

    private static void PlayerEvent(string name)
    {
        foreach (var listener in PlayerListeners)
        {
            EventManager.BroadcastEvent(listener.gameObject, name);
        }
    }
    
    private static void SetupPlayerListeners()
    {
        On.HeroController.TakeDamage += (orig, self, go, side, amount, type) =>
        {
            PlayerEvent("OnDamage");
            if (type != 1) PlayerEvent("OnHazardRespawn");
            orig(self, go, side, amount, type);
        };

        On.HeroController.AddHealth += (orig, self, amount) =>
        {
            PlayerEvent("OnHeal");
            orig(self, amount);
        };

        ModHooks.BeforePlayerDeadHook += () =>
        {
            PlayerEvent("OnDeath");
        };

        On.HeroController.HeroJump += (orig, self) =>
        {
            PlayerEvent("Jump");
            orig(self);
        };

        On.HeroController.DoWallJump += (orig, self) =>
        {
            PlayerEvent("WallJump");
            orig(self);
        };

        On.HeroController.DoDoubleJump += (orig, self) =>
        {
            PlayerEvent("DoubleJump");
            orig(self);
        };

        On.HeroController.DoHardLanding += (orig, self) =>
        {
            PlayerEvent("HardLand");
            orig(self);
        };

        On.HeroController.BackOnGround += (orig, self) =>
        {
            PlayerEvent("Land");
            orig(self);
        };

        On.HeroController.FlipSprite += (orig, self) =>
        {
            orig(self);
            PlayerEvent(self.cState.facingRight ? "FaceRight" : "FaceLeft");
        };

        On.HeroController.DoAttack += (orig, self) =>
        {
            PlayerEvent("Attack");
            orig(self);
        };

        On.HeroController.Dash += (orig, self) =>
        {
            PlayerEvent("Dash");
            orig(self);
        };

        On.PlayMakerFSM.Awake += (orig, self) =>
        {
            orig(self);
            switch (self.FsmName)
            {
                case "Superdash":
                    self.InsertCustomAction("Dash Start", _ => PlayerEvent("CrystalDash"), 0);
                    break;
            }
        };
    }
}