using System.Collections.Generic;
using System.Linq;
using Architect.Content.Elements;
using Architect.Content.Elements.Internal;
using Architect.Content.Elements.Internal.Fixers;
using Architect.Content.Groups;
using Architect.Util;
using UnityEngine;

namespace Architect.Content;

public static class ContentPacks
{
    public const int SoulSourceWeight = 1;
    public const int BreakableWallsWeight = 2;
    public const int LoreTabletWeight = 3;
    public const int SpecialBenchWeight = 4;
    public const int NailmasterBenchWeight = 5;
    public const int BenchWeight = 6;
    public const int NpcWeight = 7;
    public const int MiscInteractableWeight = 8;
    public const int LeverWeight = 9;
    public const int GateWeight = 10;
    //public const int ShinyWeight = 11;

    internal static readonly List<ContentPack> Packs = [];
    private static readonly List<ContentPack> InternalPacks = [];
    
    /**
     * Registers a new content pack.
     */
    public static void RegisterPack(ContentPack pack)
    {
        Packs.Add(pack);
    }
    
    private static void RegisterInternalPack(ContentPack pack)
    {
        InternalPacks.Add(pack);
        RegisterPack(pack);
    }

    internal static List<(string, string)> GetPreloadValues()
    {
        List<(string, string)> preloadInfo = [];

        foreach (var element in InternalPacks.Where(pack => pack.IsEnabled()).SelectMany(pack => pack))
        {
            if (element is not InternalPackElement internalPackElement) continue;
            internalPackElement.AddPreloads(preloadInfo);
        }

        return preloadInfo;
    }

    internal static void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        foreach (var element in InternalPacks.Where(pack => pack.IsEnabled()).SelectMany(pack => pack))
        {
            if (element is not InternalPackElement internalPackElement) continue;
            internalPackElement.AfterPreload(preloadedObjects);
        }
    }

    private static AbstractPackElement Create(string scene, string path, string name, string category, int weight = 0, float offset = 0)
    {
        return new GInternalPackElement(scene, path, name, category, weight, offset);
    }

    private static AbstractPackElement CreateEnemy(string scene, string path, string name, int weight = 0, string category = "Enemies")
    {
        return new GInternalPackElement(scene, path, name, category, weight)
            .WithConfigGroup(ConfigGroup.KillableEnemies)
            .WithReceiverGroup(ReceiverGroup.Enemies)
            .WithBroadcasterGroup(BroadcasterGroup.Enemies);
    }

    private static AbstractPackElement CreateSolid(string scene, string path, string name, int weight = 0)
    {
        return new GInternalPackElement(scene, path, name, "Solids", weight)
            .WithRotationGroup(RotationGroup.Four)
            .WithConfigGroup(ConfigGroup.Platforms);
    }

    private static AbstractPackElement CreateDecoration(string scene, string path, string name, float offset = 0)
    {
        return new GInternalPackElement(scene, path, name, "Decorations", 0, offset:offset)
            .WithRotationGroup(RotationGroup.All)
            .WithConfigGroup(ConfigGroup.Decorations);
    }

    internal static void PreloadInternalPacks()
    {
        ConfigGroup.Initialize();
        ReceiverGroup.Initialize();
        
        RegisterInternalPack(new ContentPack("Howling Cliffs", "Assets from the Howling Cliffs")
        {
            Create("Tutorial_01", "_Props/Stalactite Hazard", "Stalactite", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_01", "Platform 1"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_07", "Platform 2"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_08", "Platform 3"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_10", "Platform 4"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_14", "Platform 5"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_17", "Platform 6"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_18", "Platform 7"),
            CreateSolid("Tutorial_01", "_Scenery/plat_float_20", "Platform 8"),
            Create("Tutorial_01", "_Props/Tut_tablet_top (2)", "King's Pass Lore Tablet", "Interactable", weight:LoreTabletWeight)
                .WithConfigGroup(ConfigGroup.Tablets),
            Create("Tutorial_01", "_Props/Health Cocoon", "Lifeblood Cocoon", "Interactable", weight:MiscInteractableWeight)
                .WithConfigGroup(ConfigGroup.Cocoon),
            new BenchElement("Room_nailmaster", "RestBench", "Nailmaster Mato's Bench", "Interactable", weight:NailmasterBenchWeight)
        });
        RegisterInternalPack(new ContentPack("Forgotten Crossroads", "Assets from the regular Forgotten Crossroads")
        {
            //new CharmElement(weight:ShinyWeight),
            CreateEnemy("Crossroads_07", "Uninfected Parent/Fly", "Gruzzer"),
            new GruzMotherElement(),
            CreateEnemy("Crossroads_07", "_Enemies/Crawler", "Crawlid"),
            CreateEnemy("Crossroads_07", "_Enemies/Climber 3", "Tiktik")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Crossroads_16", "Uninfected Parent/Buzzer", "Vengefly"),
            new VengeflyKingElement(),
            Create("Crossroads_12", "_Enemies/Worm", "Goam", "Enemies")
                .WithRotationGroup(RotationGroup.Eight)
                .WithConfigGroup(ConfigGroup.Goams),
            CreateEnemy("Crossroads_21", "non_infected_event/Zombie Runner", "Wandering Husk"),
            CreateEnemy("Crossroads_21", "Zombie Barger", "Husk Bully"),
            CreateEnemy("Crossroads_16", "_Enemies/Zombie Hornhead", "Husk Hornhead"),
            CreateEnemy("Crossroads_21", "non_infected_event/Zombie Leaper", "Leaping Husk"),
            CreateEnemy("Crossroads_15", "_Enemies/Zombie Shield", "Husk Warrior"),
            CreateEnemy("Crossroads_21", "non_infected_event/Zombie Guard", "Husk Guard"),
            CreateEnemy("Crossroads_10_boss_defeated", "Prayer Room/Prayer Slug", "Maggot").FlipHorizontal(),
            CreateEnemy("Crossroads_ShamanTemple", "_Enemies/Roller", "Baldur"),
            new ElderBaldurElement(),
            new MenderbugElement(),
            CreateEnemy("Crossroads_19", "_Enemies/Spitter", "Aspid Hunter"),
            new HatcherPackElement("Crossroads_19", "_Enemies/Hatcher", "Hatcher Cage (1)", "Aspid Mother", "Hatcher"),
            Create("Crossroads_36", "_Props/Soul Totem 4", "Angry Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("Crossroads_36", "_Props/Collapser Small 1", "Collapsing Floor", "Interactable", weight:BreakableWallsWeight)
                .WithRotationGroup(RotationGroup.Four),
            Create("Crossroads_ShamanTemple", "Soul Totem 2", "Ancestral Mound Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("Crossroads_ShamanTemple", "Bone Gate", "Ancestral Mound Gate", "Interactable", weight:GateWeight)
                .WithReceiverGroup(ReceiverGroup.Gates)
                .WithRotationGroup(RotationGroup.Four)
                .FlipHorizontal(),
            new BenchElement("Crossroads_47", "RestBench", "Common Bench", "Interactable", weight:BenchWeight),
            new BenchElement("Crossroads_ShamanTemple", "BoneBench", "Ancestral Mound Bench", "Interactable", weight:BenchWeight),
            new BenchElement("Crossroads_04", "RestBench", "Salubra's Bench", "Interactable", weight:BenchWeight),
            new BenchElement("Room_Final_Boss_Atrium", "RestBench", "Black Egg Bench", "Interactable", weight:SpecialBenchWeight),
            new GrubfatherElement(weight:NpcWeight),
            Create("Crossroads_07", "_Scenery/plat_lift_06", "Lift Platform S", "Solids")
                .WithRotationGroup(RotationGroup.Four),
            Create("Crossroads_07", "_Scenery/plat_lift_05", "Lift Platform L", "Solids")
                .WithRotationGroup(RotationGroup.Four),
            Create("Crossroads_31", "Grub Bottle", "Grub Bottle", "Interactable", weight:MiscInteractableWeight)
                .WithConfigGroup(ConfigGroup.Grub),
            CreateDecoration("Crossroads_07", "_Scenery/brk_barrel_05", "Crossroads Barrel")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            CreateDecoration("Crossroads_07", "_Scenery/brk_cart_05", "Crossroads Cart")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            CreateDecoration("Crossroads_47", "_Props/Crossroads Statue Horned", "Horned Statue")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            CreateDecoration("Crossroads_47", "_Props/Crossroads Statue Stone", "Stone Statue")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            new DiveGroundElement(weight:BreakableWallsWeight),
            new LeverPackElement("Room_Town_Stag_Station", "Gate Switch", "Small Lever", weight:LeverWeight)
                .FlipHorizontal()
        });
        RegisterInternalPack(new ContentPack("Infected Crossroads", "Assets unique to the Infected Crossroads")
        {
            CreateEnemy("Crossroads_07", "Infected Parent/Bursting Bouncer", "Volatile Gruzzer"),
            CreateEnemy("Crossroads_16", "infected_event/Angry Buzzer", "Furious Vengefly"),
            CreateEnemy("Crossroads_21", "infected_event/Bursting Zombie", "Violent Husk"),
            CreateEnemy("Crossroads_15", "Infected Parent/Spitting Zombie", "Slobbering Husk"),
            CreateDecoration("Crossroads_07", "Infected Parent/infected_large_blob_010000", "Infected Blob S", offset:11.4922f),
            CreateDecoration("Crossroads_07", "Infected Parent/infected_large_blob_030000 (1)", "Infected Blob L", offset:11.4922f),
            CreateDecoration("Crossroads_07", "Infected Parent/infected_orange_drip_020000 (3)", "Infected Goop", offset:11.4922f),
            CreateDecoration("Crossroads_07", "Infected Parent/infected_vine_01", "Infected Vine", offset:11.4922f)
        });
        RegisterInternalPack(new ContentPack("Greenpath", "Assets from Greenpath")
        {
            CreateEnemy("Fungus1_12", "Pigeon", "Maskfly"),
            CreateEnemy("Fungus1_22", "Plant Trap", "Fool Eater")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Fungus1_17", "Moss Charger", "Moss Charger"),
            CreateEnemy("Fungus1_22", "Mosquito", "Squit"),
            CreateEnemy("Fungus1_12", "Acid Walker", "Durandoo"),
            CreateEnemy("Fungus1_09", "Acid Flyer", "Duranda"),
            CreateEnemy("Fungus1_31", "_Enemies/Mossman_Runner", "Mosskin"),
            CreateEnemy("Fungus1_22", "Mossman_Shaker", "Volatile Mosskin"),
            CreateEnemy("Fungus1_32", "Moss Knight C", "Moss Knight"),
            CreateEnemy("Fungus1_22", "Moss Walker", "Mosscreep")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Fungus1_12", "Plant Turret", "Gulka")
                .WithRotationGroup(RotationGroup.All),
            new ZoteHeadElement(weight:MiscInteractableWeight),
            CreateEnemy("Fungus1_31", "_Enemies/Fat Fly (1)", "Obble").FlipVertical(),
            CreateDecoration("Fungus1_31", "_Scenery/fung_lamp2", "Standing Lantern")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            Create("Fungus1_31", "Toll Gate", "Toll Gate", "Interactable", weight:GateWeight)
                .WithReceiverGroup(ReceiverGroup.Gates)
                .WithRotationGroup(RotationGroup.Four),
            new TollMachineElement(weight:LeverWeight),
            
            new BenchElement("Fungus1_37", "RestBench", "Stone Sanctuary Bench", "Interactable", weight:BenchWeight),
            new BenchElement("Room_Slug_Shrine", "RestBench", "Unn Shrine Bench", "Interactable", weight:BenchWeight),
            new BenchElement("Fungus1_15", "RestBench", "Nailmaster Sheo's Bench", "Interactable", weight:NailmasterBenchWeight),
            
            new MassiveMossChargerElement()
        });
        RegisterInternalPack(new ContentPack("Fog Canyon", "Assets from Fog Canyon")
        {
            CreateEnemy("Fungus3_26", "Jellyfish", "Ooma"),
            CreateEnemy("Fungus3_26", "Jellyfish Baby", "Uoma"),
            Create("Fungus3_26", "Zap Cloud", "Charged Lumaflies", "Enemies"),
            Create("Fungus3_26", "Jelly Egg Bomb", "Jelly Egg Bomb", "Hazards")
                .WithRotationGroup(RotationGroup.Eight)
                .WithConfigGroup(ConfigGroup.MovingObjects)
                .WithReceiverGroup(ReceiverGroup.JellyEgg),
            CreateSolid("Fungus3_26", "fung_plat_float_01", "Fog Canyon Platform 1"),
            CreateSolid("Fungus3_26", "fung_plat_float_04", "Fog Canyon Platform 2"),
            CreateSolid("Fungus3_26", "fung_plat_float_05", "Fog Canyon Platform 3"),
            CreateSolid("Fungus3_26", "fung_plat_float_06", "Fog Canyon Platform 4"),
            CreateSolid("Fungus3_26", "fung_plat_float_07", "Fog Canyon Platform 5"),
            CreateSolid("Fungus3_26", "fung_plat_float_08", "Fog Canyon Platform 6"),
            CreateSolid("Fungus3_archive_02", "fung temple_plat_float_small", "Archives Platform S"),
            CreateSolid("Fungus3_archive_02", "fung_temple_plat_float (1)", "Archives Platform L"),
            new BenchElement("Fungus3_archive", "RestBench", "Archive Bench", "Interactable", weight:BenchWeight)
        });
        RegisterInternalPack(new ContentPack("Fungal Wastes", "Assets from the Fungal Wastes")
        {
            CreateEnemy("Fungus2_18", "Zombie Fungus A", "Fungified Husk"),
            CreateEnemy("Fungus2_05", "Battle Scene v2/Completed/Mushroom Brawler", "Shrumal Ogre"),
            CreateEnemy("Fungus2_05", "Battle Scene v2/Completed/Mushroom Baby", "Shrumeling"),
            CreateEnemy("Fungus2_18", "_Scenery/Fungoon Baby", "Fungling"),
            CreateEnemy("Fungus2_18", "_Scenery/Fungus Flyer", "Fungoon"),
            CreateEnemy("Fungus2_28", "Mushroom Roller", "Shrumal Warrior"),
            CreateEnemy("Fungus2_18", "Fung Crawler", "Ambloom"),
            CreateEnemy("Fungus2_04", "Mushroom Turret", "Sporg")
                .WithRotationGroup(RotationGroup.All),
            CreateEnemy("Fungus2_12", "Mantis", "Mantis Warrior")
                .WithConfigGroup(ConfigGroup.Mantis),
            CreateEnemy("Fungus2_12", "Mantis Flyer Child", "Mantis Youth").FlipHorizontal()
                .WithConfigGroup(ConfigGroup.Mantis)
                .WithRotationGroup(RotationGroup.Four),
            Create("Fungus2_18", "_Props/Bounce Shrooms 1/Bounce Shroom B (1)", "Bouncy Mushroom", "Interactable", weight:MiscInteractableWeight, offset:15.5f)
                .FlipVertical()
                .WithConfigGroup(ConfigGroup.BouncyMushrooms)
                .WithRotationGroup(RotationGroup.All),
            CreateSolid("Fungus2_04", "mush_plat_float_01", "Fungal Wastes Platform 1"),
            CreateSolid("Fungus2_18", "_Scenery/mush_plat_float_03", "Fungal Wastes Platform 2"),
            CreateSolid("Fungus2_18", "_Scenery/mush_plat_float_04", "Fungal Wastes Platform 3"),
            CreateSolid("Fungus2_04", "mush_plat_float_05", "Fungal Wastes Platform 4"),
            new LeverPackElement("Fungus2_04", "Mantis Lever", "Mantis Lever", weight:LeverWeight),
            Create("Fungus2_04", "Mantis Gate", "Mantis Gate", "Interactable", weight:GateWeight)
                .WithReceiverGroup(ReceiverGroup.Gates)
                .WithRotationGroup(RotationGroup.Four),
            new BenchElement("Fungus2_26", "RestBench", "Leg Eater's Bench", "Interactable", weight:BenchWeight),
            new BenchElement("Fungus2_31", "RestBench", "Mantis Bench", "Interactable", weight:BenchWeight)
        });
        RegisterInternalPack(new ContentPack("Crystal Peak", "Assets from the Crystal Peak")
        {
            Create("Mines_37", "Chest", "Geo Chest", "Interactable", weight:MiscInteractableWeight)
                .WithConfigGroup(ConfigGroup.GeoChest).FlipHorizontal(),
            Create("Mines_31", "Soul Totem mini_horned", "Mini Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("Mines_31", "Soul Totem mini_two_horned", "Horned Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("Mines_31", "Cave Spikes (4)", "Cave Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Mines_31", "Laser Turret", "Laser Crystal", "Hazards")
                .WithRotationGroup(RotationGroup.All)
                .WithConfigGroup(ConfigGroup.LaserCrystal)
                .FlipHorizontal(),
            new ForceActivatedElement("Crossroads_45", "Zombie Myla", "Husk Myla", "Enemies")
                .WithConfigGroup(ConfigGroup.KillableEnemies)
                .WithBroadcasterGroup(BroadcasterGroup.Enemies)
                .WithReceiverGroup(ReceiverGroup.Enemies),
            CreateEnemy("Mines_20", "Zombie Miner 1", "Husk Miner")
                .FlipVertical(),
            CreateEnemy("Mines_25", "Zombie Beam Miner", "Crystallised Husk"),
            CreateEnemy("Mines_20", "Crystal Crawler", "Blimback"),
            new ShardmiteElement(),
            CreateEnemy("Mines_25", "Crystal Flyer", "Crystal Hunter"),
            Create("Mines_20", "Metal Gate v2", "Mines Gate", "Interactable", weight:GateWeight)
                .WithReceiverGroup(ReceiverGroup.Gates)
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Mines_20", "Crystallised Lazer Bug (3)", "Crystal Crawler").FlipHorizontal()
                .WithRotationGroup(RotationGroup.Four),
            CreateSolid("Mines_20", "mines_metal_grate_06", "Metal Grate"),
            CreateSolid("Mines_20", "plat_float_06", "Crystal Peak Platform"),
            Create("Mines_31", "Mines Platform", "Crystal Peak Rotating Platform", "Interactable", weight:MiscInteractableWeight)
                .WithRotationGroup(RotationGroup.Four),
            new ConveyorBeltElement(weight:MiscInteractableWeight),
            new FallingCrystalsElement(),
            Create("Mines_37", "stomper_offset", "Crystal Peak Stomper (Slow)", "Hazards")
                .WithRotationGroup(RotationGroup.Four)
                .WithReceiverGroup(ReceiverGroup.Stompers)
                .WithConfigGroup(ConfigGroup.Animated),
            Create("Mines_37", "stomper_fast", "Crystal Peak Stomper (Fast)", "Hazards")
                .WithRotationGroup(RotationGroup.Four)
                .WithReceiverGroup(ReceiverGroup.Stompers)
                .WithConfigGroup(ConfigGroup.Animated),
            new BreakableWallElement("Mines_25", "Breakable Wall", "Breakable Wall", weight:BreakableWallsWeight),
            CreateDecoration("Mines_20", "brk_Crystal3", "Crystal")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            CreateDecoration("Mines_20", "crystal_barrel", "Crystal Barrel")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable)
        });
        RegisterInternalPack(new ContentPack("Queen's Gardens", "Assets from the Queen's Gardens")
        {
            new MossyVagabondElement(),
            CreateEnemy("Fungus3_34", "Garden Zombie", "Spiny Husk"),
            CreateEnemy("Fungus3_48", "Lazy Flyer Enemy", "Aluba"),
            CreateEnemy("Fungus3_34", "Moss Flyer", "Mossfly"),
            CreateEnemy("Fungus3_48", "Grass Hopper", "Loodle"),
            CreateEnemy("Fungus3_10", "Battle Scene/Completed/Mantis Heavy", "Mantis Traitor"),
            CreateEnemy("Fungus3_48", "Mantis Heavy Flyer", "Mantis Petra"),
            Create("Fungus3_34", "Royal Gardens Plat S", "Queen's Gardens Collapsing Platform S", "Interactable", weight:MiscInteractableWeight),
            Create("Fungus3_34", "Royal Gardens Plat L", "Queen's Gardens Collapsing Platform L", "Interactable", weight:MiscInteractableWeight),
            new ShadeGateElement(weight:MiscInteractableWeight),
            CreateSolid("Fungus3_44", "Royal_garden_plat_float_08", "Queen's Gardens Platform S"),
            CreateSolid("Fungus3_44", "Royal_garden_plat_float_06", "Queen's Gardens Platform L"),
            new TollBenchElement(weight:BenchWeight)
        });
        RegisterInternalPack(new ContentPack("City of Tears", "Assets from the City of Tears")
        {
            new LeverPackElement("Ruins2_01", "Ruins Lever", "City Lever", weight:LeverWeight)
                .WithRotationGroup(RotationGroup.All),
            Create("Ruins2_01", "Battle Gate", "Battle Gate", "Interactable", weight:GateWeight)
                .WithRotationGroup(RotationGroup.Four)
                .WithReceiverGroup(ReceiverGroup.BattleGate)
                .WithConfigGroup(ConfigGroup.BattleGate),
            Create("Ruins2_01", "Ruins Gate 2", "City Gate", "Interactable", weight:GateWeight)
                .WithRotationGroup(RotationGroup.Four)
                .WithReceiverGroup(ReceiverGroup.Gates),
            CreateEnemy("Ruins2_01", "Ruins Sentry 1", "Husk Sentry"),
            CreateEnemy("Ruins2_01", "Ruins Flying Sentry", "Winged Sentry"),
            CreateEnemy("Ruins2_01", "Ruins Flying Sentry Javelin", "Lance Sentry")
                .FlipHorizontal(),
            CreateEnemy("Ruins2_01", "Ruins Sentry Fat", "Heavy Sentry"),
            new TwisterPackElement("Ruins1_30", "Mage", "Soul Twister", "Mage"),
            CreateEnemy("Ruins1_30", "Mage Blob 1", "Mistake"),
            CreateEnemy("Ruins1_30", "Mage Balloon", "Folly"),
            new SoulWarriorElement(),
            CreateEnemy("Ruins2_01", "Royal Zombie Coward (1)", "Cowardly Husk"),
            CreateEnemy("Ruins2_01", "Royal Zombie 1 (1)", "Husk Dandy"),
            CreateEnemy("Ruins2_01", "Royal Zombie Fat (2)", "Gluttonous Husk"),
            CreateEnemy("Ruins2_01", "Battle Scene/Great Shield Zombie", "Great Husk Sentry"),
            CreateEnemy("Ruins_House_02", "Gorgeous Husk", "Gorgeous Husk"),
            CreateDecoration("Ruins_House_02", "ruind_dressing_light_03", "Ceiling Lantern")
                .WithBroadcasterGroup(BroadcasterGroup.Breakable),
            Create("Ruins2_08", "ruind_bridge_roof_01 (1)/ruind_bridge_roof_04_spikes", "Roof Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            CreateSolid("Ruins1_03", "_Scenery/ruin_plat_float_01", "City Platform 1"),
            CreateSolid("Ruins1_03", "_Scenery/ruin_plat_float_01_wide", "City Platform 2"),
            CreateSolid("Ruins1_03", "_Scenery/ruin_plat_float_02", "City Platform 3"),
            CreateSolid("Ruins1_03", "_Scenery/ruin_plat_float_05", "City Platform 4"),
            CreateCustomPlatform("wide_platform", "Wide Platform", new Vector2(11.30954f, 0.8839264f), new Vector2(0.007995605f, 0.1063919f), 100),
            Create("Ruins1_25", "Ruins Vial Empty", "Soul Vial", "Interactable", weight:SoulSourceWeight)
                .WithConfigGroup(ConfigGroup.Breakable),
            Create("Ruins1_24", "Soul Totem 1", "Thin Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("Ruins1_32", "Soul Totem 3", "Round Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("Ruins1_32", "Soul Totem 5", "Leaning Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            CreateSolid("Ruins2_01", "ruins_mage_building_0011_a_royal_plat", "Pleasure House Platform"),
            CreateSolid("Ruins2_01", "ruins_plat_royal_02", "Royal Platform"),
            new BenchElement("Ruins_Bathhouse", "RestBench", "Pleasure House Bench", "Interactable", weight:BenchWeight),
            new MillibellePackElement(weight:NpcWeight),
            new ReusableLeverElement(weight:LeverWeight),
            new WatcherKnightElement()
        });
        RegisterInternalPack(new ContentPack("Royal Waterways", "Assets from the Royal Waterways")
        {
            CreateEnemy("Waterways_01", "_Enemies/Inflater", "Hwurmp"),
            CreateEnemy("Waterways_01", "_Enemies/Flip Hopper", "Pilflip"),
            CreateEnemy("GG_Pipeway", "Fluke Fly", "Flukefey"),
            CreateEnemy("GG_Pipeway", "Flukeman", "Flukemon"),
            CreateEnemy("GG_Pipeway", "Fat Fluke", "Flukemunga"),
            new HatcherPackElement("GG_Flukemarm", "Fluke Mother", "Hatcher Cage (2)", "Flukemarm", "Fluke Mother", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Kingdom's Edge", "Assets from Kingdom's Edge")
        {
            CreateEnemy("Deepnest_East_07", "Ceiling Dropper", "Belfly"),
            CreateEnemy("Deepnest_East_07", "Blow Fly", "Boofly"),
            CreateEnemy("Deepnest_East_07", "Super Spitter", "Primal Aspid"),
            new HopperPackElement("Deepnest_East_06", "Hopper", "Hopper", true),
            new HopperPackElement("Deepnest_East_06", "Giant Hopper (1)", "Great Hopper", false),
            CreateSolid("Ruins2_11_b", "jar_col_plat", "Tower of Love Platform"),
            new BenchElement("Deepnest_East_06", "RestBench", "Nailmaster Oro's Bench", "Interactable", weight:NailmasterBenchWeight),
            
            // Colosseum of Fools
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 1/Colosseum Cage Large", "Heavy Fool", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 1/Colosseum Cage Large (1)", "Sturdy Fool", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 2/Colosseum Cage Small", "Armoured Squit", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 3/Colosseum Cage Large", "Shielded Fool", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 4/Colosseum Cage Large", "Winged Fool", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 4/Colosseum Cage Small", "Sharp Baldur", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 6/Colosseum Cage Small", "Battle Obble", "Enemies"),
            new ColosseumPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 9/Colosseum Cage Small (1)", "Death Loodle", "Enemies"),
            new TwisterPackElement("Room_Colosseum_Gold", "Colosseum Manager/Waves/Wave 25/Electric Mage New", "Volt Twister", "Electric Mage"),
            new TamerBeastElement(),
            new BenchElement("Room_Colosseum_02", "RestBench", "Colosseum Bench", "Interactable", weight:BenchWeight),
            
            new ColosseumWallElement(weight:MiscInteractableWeight),
            Create("Room_Colosseum_Gold", "Colosseum Manager/Waves/Arena 1/Colosseum Platform", "Colosseum Platform", "Interactable", weight:MiscInteractableWeight)
                .WithConfigGroup(ConfigGroup.ColosseumPlatform)
                .WithReceiverGroup(ReceiverGroup.ColosseumPlatform)
        });
        RegisterInternalPack(new ContentPack("Zoteboat", "Zotelings from the Eternal Ordeal")
        {
            new ZotelingElement("Dormant Warriors/Zote Crew Normal (1)", "Zoteling The Mighty").FlipHorizontal(),
            new BallZotelingElement("Winged Zoteling", "BUZZER").FlipHorizontal(),
            new BallZotelingElement("Hopping Zoteling", "HOPPER").FlipHorizontal(),
            new FatZotelingElement().FlipHorizontal(),
            new TallZotelingElement().FlipHorizontal(),
            new VolatileZotelingElement(),
            new FlukeZotelingElement()
        });
        RegisterInternalPack(new ContentPack("The Hive", "Assets from the Hive")
        {
            CreateEnemy("Hive_03_c", "Bee Hatchling Ambient (11)", "Hiveling"),
            new HatcherPackElement("Hive_01", "Zombie Hive", "Hatcher Cage (1)", "Husk Hive", "Hive Zombie"),
            CreateEnemy("Hive_03_c", "Bee Stinger (4)", "Hive Soldier"),
            CreateEnemy("Hive_03_c", "Big Bee", "Hive Guardian"),
            CreateSolid("Hive_03_c", "hive_plat_01 (4)", "Hive Platform 1"),
            CreateSolid("Hive_03_c", "hive_plat_02 (2)", "Hive Platform 2"),
            CreateSolid("Hive_03_c", "hive_plat_03 (3)", "Hive Platform 3"),
            CreateSolid("Hive_03_c", "hive_plat_04 (4)", "Hive Platform 4"),
            CreateSolid("Hive_03_c", "hive_plat_brk_02", "Breakable Hive Platform 1"),
            CreateSolid("Hive_03_c", "hive_plat_brk_03 (1)", "Breakable Hive Platform 2"),
            CreateSolid("Hive_03_c", "hive_plat_brk_04", "Breakable Hive Platform 3"),
            Create("Hive_03_c", "Hive Breakable Pillar (5)", "Hive Breakable Wall", "Interactable", weight:BreakableWallsWeight)
                .WithRotationGroup(RotationGroup.Four)
                .WithConfigGroup(ConfigGroup.Breakable)
        });
        RegisterInternalPack(new ContentPack("Resting Grounds", "Assets from the Resting Grounds")
        {
            CreateEnemy("RestingGrounds_10", "Grave Zombie", "Entombed Husk")
        });
        RegisterInternalPack(new ContentPack("Ancient Basin", "Assets from the Ancient Basin")
        {
            CreateEnemy("Abyss_20", "Parasite Balloon (6)", "Infected Balloon"),
            CreateEnemy("Abyss_17", "Lesser Mawlek", "Lesser Mawlek"),
            new MawlurkPackElement(),
            CreateEnemy("Abyss_04", "Abyss Crawler", "Shadow Creeper")
                .WithRotationGroup(RotationGroup.Four),
            new BrokenVesselElement("GG_Broken_Vessel", "Infected Knight", "Broken Vessel"),
            new BrokenVesselElement("Dream_03_Infected_Knight", "Lost Kin", "Lost Kin"),
            CreateSolid("Dream_03_Infected_Knight", "dream_plat_small (1)", "Dream Platform S"),
            CreateSolid("Dream_03_Infected_Knight", "dream_plat_med (7)", "Dream Platform M"),
            CreateSolid("Dream_03_Infected_Knight", "dream_plat_large (3)", "Dream Platform L")
        });
        RegisterInternalPack(new ContentPack("Deepnest", "Assets from Deepnest")
        {
            CreateEnemy("Deepnest_33", "Zombie Runner Sp (1)", "Corpse Creeper (Wandering Husk)"),
            CreateEnemy("Deepnest_33", "Zombie Hornhead Sp (2)", "Corpse Creeper (Husk Hornhead)"),
            CreateEnemy("Deepnest_17", "Baby Centipede", "Dirtcarver"),
            new HatcherPackElement("Deepnest_26b", "Centipede Hatcher (4)", "Centipede Cage", "Carver Hatcher", "Centipede Hatcher"),
            new ForceActivatedElement("Deepnest_Spider_Town", "Spider Mini", "Deephunter", "Enemies")
                .WithRotationGroup(RotationGroup.Four)
                .WithConfigGroup(ConfigGroup.KillableEnemies)
                .WithBroadcasterGroup(BroadcasterGroup.Enemies)
                .WithReceiverGroup(ReceiverGroup.Enemies),
            new ForceActivatedElement("Deepnest_Spider_Town", "Tiny Spider", "Deepling", "Enemies")
                .WithConfigGroup(ConfigGroup.KillableEnemies)
                .WithBroadcasterGroup(BroadcasterGroup.Enemies)
                .WithReceiverGroup(ReceiverGroup.Enemies),
            new ForceActivatedElement("Deepnest_41", "Spider Flyer", "Little Weaver", "Enemies")
                .WithConfigGroup(ConfigGroup.KillableEnemies)
                .WithBroadcasterGroup(BroadcasterGroup.Enemies)
                .WithReceiverGroup(ReceiverGroup.Enemies),
            new ForceActivatedElement("Deepnest_Spider_Town", "Slash Spider", "Stalking Devout", "Enemies")
                .WithConfigGroup(ConfigGroup.KillableEnemies)
                .WithBroadcasterGroup(BroadcasterGroup.Enemies)
                .WithReceiverGroup(ReceiverGroup.Enemies)
                .FlipVertical(),
            CreateEnemy("Deepnest_Spider_Town", "Egg Sac", "Bluggsac"),
            new BenchElement("Deepnest_Spider_Town", "RestBench Return", "Beast's Den Bench", "Interactable", weight:BenchWeight),
            new MidwifeElement(weight:NpcWeight)
        });
        RegisterInternalPack(new ContentPack("The Abyss", "Assets from The Abyss")
        {
            CreateSolid("Abyss_06_Core", "_Scenery/abyss_plat_float_01", "Abyss Platform 1"),
            CreateSolid("Abyss_06_Core", "_Scenery/abyss_plat_float_02", "Abyss Platform 2"),
            CreateSolid("Abyss_06_Core", "_Scenery/abyss_plat_float_03", "Abyss Platform 3"),
            CreateSolid("Abyss_06_Core", "_Scenery/abyss_plat_float_04", "Abyss Platform 4"),
            new VoidTendrilsElement(),
            new ShadeSiblingElement(),
            new HornetElement(weight:NpcWeight)
        });
        RegisterInternalPack(new ContentPack("White Palace", "Assets from the White Palace")
        {
            Create("White_Palace_18", "White Palace Fly", "Wingsmould", "Enemies")
                .FlipVertical()
                .WithConfigGroup(ConfigGroup.Wingsmould),
            CreateEnemy("White_Palace_20", "Battle Scene/Wave 1/Royal Gaurd (1)", "Kingsmould"),
            CreateSolid("White_Palace_07", "wp_plat_float_01_wide (1)", "White Palace Platform 1"),
            CreateSolid("White_Palace_07", "wp_plat_float_07", "White Palace Platform 2"),
            CreateSolid("White_Palace_07", "wp_plat_float_03", "White Palace Platform 3"),
            CreateSolid("White_Palace_07", "wp_plat_float_05 (1)", "White Palace Platform 4"),
            new WhitePalaceSawElement(),
            new WhiteThornsElement(),
            Create("White_Palace_07", "wp_trap_spikes", "White Palace Moving Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight)
                .WithConfigGroup(ConfigGroup.Animated),
            Create("White_Palace_03_hub", "White_ Spikes", "White Palace Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            Create("White_Palace_03_hub", "Soul Totem white", "Pale King Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            Create("White_Palace_18", "Soul Totem white_Infinte", "Pure Vessel Soul Totem", "Interactable", weight:SoulSourceWeight)
                .WithRotationGroup(RotationGroup.Eight),
            new WhitePalaceShieldGateElement(weight:GateWeight),
            Create("White_Palace_03_hub", "White_Servant_01", "Royal Retainer 1", "Interactable", weight:SoulSourceWeight),
            Create("White_Palace_03_hub", "White_Servant_02", "Royal Retainer 2", "Interactable", weight:SoulSourceWeight).FlipHorizontal(),
            Create("White_Palace_03_hub", "White_Servant_03", "Royal Retainer 3", "Interactable", weight:SoulSourceWeight),
            new BenchElement("White_Palace_03_hub", "WhiteBench", "White Palace Bench", "Interactable", weight:BenchWeight)
        });
        RegisterInternalPack(new ContentPack("Godhome", "Assets from Godhome")
        {
            CreateCustomPlatform("godhome_tiles", "Godhome Tiles", new Vector2(3.95f, 0.75f), new Vector2(0.15f, 0.22f), 64),
            CreateSolid("GG_Atrium_Roof", "gg_plat_float_small", "Godhome Platform S"),
            CreateSolid("GG_Workshop", "gg_plat_float_wide", "Godhome Platform L"),
            CreateCustomPlatform("godhome_wide_platform", "Godhome Platform XL", new Vector2(5.6f, 1), new Vector2(0, 0.25f), 64),
            new BenchElement("GG_Workshop", "RestBench (1)", "Godhome Bench", "Interactable", weight:SpecialBenchWeight),
            new MultiPartBenchElement("GG_Atrium_Roof", "RestBench (1)", "GG_bench_metal_0001_1", "Godhome Roof Bench", "Interactable", weight:SpecialBenchWeight),
            new GodseekerElement(weight:NpcWeight),
            CreateCustomDecoration("godhome_cloud_1", "Godhome Cloud 1"),
            CreateCustomDecoration("godhome_cloud_2", "Godhome Cloud 2"),
            new CustomSpriteElement("GG_Radiance", "Boss Control/Plat Sets/P2 SetA/Radiant Plat Small (2)", "Small Radiance Platform", "radiance_small", "Solids")
                .WithConfigGroup(ConfigGroup.RadiancePlatforms)
                .WithReceiverGroup(ReceiverGroup.UpDownPlatforms),
            new CustomSpriteElement("GG_Radiance", "Boss Control/Plat Sets/P2 SetA/Radiant Plat Wide (2)", "Wide Radiance Platform", "radiance_wide", "Solids")
                .WithConfigGroup(ConfigGroup.RadiancePlatforms)
                .WithReceiverGroup(ReceiverGroup.UpDownPlatforms),
            new CustomSpriteElement("GG_Radiance", "Boss Control/Plat Sets/P2 SetA/Radiant Plat Thick (2)", "Thick Radiance Platform", "radiance_thick", "Solids")
                .WithConfigGroup(ConfigGroup.RadiancePlatforms)
                .WithReceiverGroup(ReceiverGroup.UpDownPlatforms)
        });
        /*RegisterInternalPack(new ContentPack("Experimental", "Experimental Features - These will be moved to a regular pack when confirmed to work")
        {,
           CreateEnemy("GG_Nailmasters", "Brothers/Oro", "Oro"),
           CreateEnemy("GG_Nailmasters", "Brothers/Mato", "Mato"),
           CreateEnemy("GG_Nailmasters", "Brothers", "Brothers"),
           CreateEnemy("GG_Painter", "Battle Scene/Sheo Boss", "Sheo"),
           CreateEnemy("GG_Painter", "Battle Scene", "Sheo 2"),
           CreateEnemy("GG_God_Tamer", "Entry Object/Lancer", "God Tamer"),
           CreateEnemy("GG_God_Tamer", "Entry Object", "God Tamer Full"),
            new OblobbleElement(),

           CreateEnemy("GG_Crystal_Guardian", "Mega Zombie Beam Miner (1)", "Crystal Guardian"),
           CreateEnemy("GG_Crystal_Guardian_2", "Battle Scene/Zombie Beam Miner Rematch", "Enraged Guardian"),
        });*/
    }

    private static AbstractPackElement CreateCustomDecoration(string path, string name)
    {
        var obj = new GameObject(name);

        obj.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadInternal(path, ppu: 64);

        obj.SetActive(false);
        Object.DontDestroyOnLoad(obj);
        
        return new SimplePackElement(obj, name, "Decorations")
            .WithConfigGroup(ConfigGroup.Decorations);
    }

    private static AbstractPackElement CreateCustomPlatform(string path, string name, Vector2 size, Vector2 offset, int ppu)
    {
        var point = new GameObject(name);

        point.AddComponent<SpriteRenderer>().sprite = ResourceUtils.LoadInternal(path, ppu:ppu);
        var col = point.AddComponent<BoxCollider2D>();
        col.size = size;
        col.offset = offset;
        point.layer = 8;

        point.SetActive(false);
        Object.DontDestroyOnLoad(point);

        return new SimplePackElement(point, name, "Solids")
            .WithConfigGroup(ConfigGroup.Platforms)
            .WithRotationGroup(RotationGroup.Four);
    }
}