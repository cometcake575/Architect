using System.Collections.Generic;
using System.Linq;
using Architect.Content.Elements;
using Architect.Content.Elements.Internal;
using Architect.Content.Elements.Internal.Special;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content;

public static class ContentPacks
{
    internal static readonly List<ContentPack> Packs = new();
    private static readonly List<ContentPack> InternalPacks = new();
    
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
        List<(string, string)> preloadInfo = new();

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

    private static AbstractPackElement Create(string scene, string path, string name, string category, float offset = 0, int weight = 0)
    {
        return new GInternalPackElement(scene, path, name, category, offset, weight);
    }

    private static AbstractPackElement CreateEnemy(string scene, string path, string name, float offset = 0, int weight = 0)
    {
        return new GInternalPackElement(scene, path, name, "Enemies", offset, weight)
            .WithConfigGroup(ConfigGroup.Enemies)
            .WithBroadcasterGroup(BroadcasterGroup.Enemies);
    }

    internal static void PreloadInternalPacks()
    {
        RegisterInternalPack(new ContentPack("King's Pass", "Assets from King's Pass")
        {
            Create("Tutorial_01", "_Props/Stalactite Hazard", "Stalactite", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Tutorial_01", "_Scenery/plat_float_01", "Platform 1", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_07", "Platform 2", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_08", "Platform 3", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_10", "Platform 4", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_14", "Platform 5", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_17", "Platform 6", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_18", "Platform 7", "Solids"),
            Create("Tutorial_01", "_Scenery/plat_float_20", "Platform 8", "Solids"),
            Create("Tutorial_01", "_Props/Health Cocoon", "Lifeblood Cocoon", "Interactable")
                .WithConfigGroup(ConfigGroup.Cocoon)
        });
        RegisterInternalPack(new ContentPack("Forgotten Crossroads", "Assets from the regular Forgotten Crossroads")
        {
            CreateEnemy("Crossroads_07", "Uninfected Parent/Fly", "Gruzzer", 4.0117f),
            CreateEnemy("Crossroads_07", "_Enemies/Crawler", "Crawlid"),
            CreateEnemy("Crossroads_07", "_Enemies/Climber 3", "Tiktik")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Crossroads_16", "Uninfected Parent/Buzzer", "Vengefly", 34.375f),
            Create("Crossroads_12", "_Enemies/Worm", "Goam", "Enemies")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Crossroads_21", "non_infected_event/Zombie Runner", "Wandering Husk"),
            CreateEnemy("Crossroads_21", "Zombie Barger", "Husk Bully"),
            CreateEnemy("Crossroads_16", "_Enemies/Zombie Hornhead", "Husk Hornhead"),
            CreateEnemy("Crossroads_21", "non_infected_event/Zombie Leaper", "Leaping Husk"),
            CreateEnemy("Crossroads_15", "_Enemies/Zombie Shield", "Husk Warrior"),
            CreateEnemy("Crossroads_21", "non_infected_event/Zombie Guard", "Husk Guard"),
            CreateEnemy("Crossroads_19", "_Enemies/Spitter", "Aspid Hunter"),
            Create("Crossroads_18", "Soul Totem mini_horned", "Mini Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Crossroads_25", "Soul Totem mini_two_horned", "Horned Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Crossroads_36", "_Props/Soul Totem 4", "Angry Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Crossroads_ShamanTemple", "Soul Totem 2", "Ancestral Mound Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Crossroads_25", "Cave Spikes tile", "Cave Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight)
        });
        RegisterInternalPack(new ContentPack("Infected Crossroads", "Assets unique to the Infected Crossroads")
        {
            CreateEnemy("Crossroads_07", "Infected Parent/Bursting Bouncer", "Volatile Gruzzer", 11.4862f),
            CreateEnemy("Crossroads_16", "infected_event/Angry Buzzer", "Furious Vengefly"),
            CreateEnemy("Crossroads_21", "infected_event/Bursting Zombie", "Violent Husk"),
            CreateEnemy("Crossroads_15", "Infected Parent/Spitting Zombie", "Slobbering Husk")
        });
        RegisterInternalPack(new ContentPack("Greenpath", "Assets from Greenpath")
        {
            CreateEnemy("Fungus1_12", "Pigeon", "Maskfly"),
            CreateEnemy("Fungus1_22", "Plant Trap", "Fool Eater")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Fungus1_22", "Mosquito", "Squit"),
            CreateEnemy("Fungus1_12", "Acid Walker", "Durandoo"),
            CreateEnemy("Fungus1_09", "Acid Flyer", "Duranda"),
            CreateEnemy("Fungus1_31", "_Enemies/Mossman_Runner", "Mosskin"),
            CreateEnemy("Fungus1_22", "Mossman_Shaker", "Volatile Mosskin"),
            CreateEnemy("Fungus1_32", "Moss Knight C", "Moss Knight"),
            CreateEnemy("Fungus1_22", "Moss Walker", "Mosscreep"),
            CreateEnemy("Fungus1_12", "Plant Turret", "Gulka"),
            Create("Fungus1_20_v02", "Zote Death/Head", "Zote Head", "Interactable", weight:10)
                .WithRotationGroup(RotationGroup.Eight),
            CreateEnemy("Fungus1_31", "_Enemies/Fat Fly (1)", "Obble").FlipVertical()
        });
        RegisterInternalPack(new ContentPack("Fog Canyon", "Assets from Fog Canyon")
        {
            CreateEnemy("Fungus3_26", "Jellyfish", "Ooma"),
            CreateEnemy("Fungus3_26", "Jellyfish Baby", "Uoma"),
            Create("Fungus3_26", "Zap Cloud", "Charged Lumaflies", "Enemies"),
            Create("Fungus3_26", "Jelly Egg Bomb", "Jelly Egg Bomb", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Fungus3_26", "fung_plat_float_01", "Fog Canyon Platform 1", "Solids"),
            Create("Fungus3_26", "fung_plat_float_04", "Fog Canyon Platform 2", "Solids"),
            Create("Fungus3_26", "fung_plat_float_05", "Fog Canyon Platform 3", "Solids"),
            Create("Fungus3_26", "fung_plat_float_06", "Fog Canyon Platform 4", "Solids"),
            Create("Fungus3_26", "fung_plat_float_07", "Fog Canyon Platform 5", "Solids"),
            Create("Fungus3_26", "fung_plat_float_08", "Fog Canyon Platform 6", "Solids"),
            Create("Fungus3_archive_02", "fung temple_plat_float_small", "Archives Platform S", "Solids"),
            Create("Fungus3_archive_02", "fung_temple_plat_float (1)", "Archives Platform L", "Solids")
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
                .WithRotationGroup(RotationGroup.Eight),
            CreateEnemy("Fungus2_12", "Mantis", "Mantis Warrior"),
            CreateEnemy("Fungus2_12", "Mantis Flyer Child", "Mantis Youth"),
            Create("Fungus2_18", "_Props/Bounce Shrooms 1/Bounce Shroom B (1)", "Bouncy Mushroom", "Interactable", 15.5f, 5)
                .FlipVertical()
                .WithRotationGroup(RotationGroup.Eight),
            Create("Fungus2_04", "mush_plat_float_01", "Fungal Wastes Platform 1", "Solids"),
            Create("Fungus2_18", "_Scenery/mush_plat_float_03", "Fungal Wastes Platform 2", "Solids"),
            Create("Fungus2_18", "_Scenery/mush_plat_float_04", "Fungal Wastes Platform 3", "Solids"),
            Create("Fungus2_04", "mush_plat_float_05", "Fungal Wastes Platform 4", "Solids"),
            new LeverPackElement("Fungus2_04", "Mantis Lever", "Mantis Lever"),
            Create("Fungus2_04", "Mantis Gate", "Mantis Gate", "Interactable", weight:15)
                .WithReceiverGroup(ReceiverGroup.Gates)
                .WithRotationGroup(RotationGroup.Four)
        });
        RegisterInternalPack(new ContentPack("Queen's Gardens", "Assets from the Queen's Gardens")
        {
            CreateEnemy("Fungus3_34", "Garden Zombie", "Spiny Husk"),
            CreateEnemy("Fungus3_48", "Lazy Flyer Enemy", "Aluba"),
            CreateEnemy("Fungus3_34", "Moss Flyer", "Mossfly"),
            CreateEnemy("Fungus3_48", "Grass Hopper", "Loodle"),
            CreateEnemy("Fungus3_10", "Battle Scene/Completed/Mantis Heavy", "Mantis Traitor"),
            CreateEnemy("Fungus3_48", "Mantis Heavy Flyer", "Mantis Petra"),
            Create("Fungus3_34", "Royal Gardens Plat S", "Queen's Gardens Collapsing Platform S", "Solids"),
            Create("Fungus3_34", "Royal Gardens Plat L", "Queen's Gardens Collapsing Platform L", "Solids")
        });
        RegisterInternalPack(new ContentPack("City of Tears", "Assets from the City of Tears")
        {
            new LeverPackElement("Ruins2_01", "Ruins Lever", "City Lever")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Ruins2_01", "Battle Gate", "Battle Gate", "Interactable", weight:15)
                .WithRotationGroup(RotationGroup.Four)
                .WithReceiverGroup(ReceiverGroup.Gates),
            Create("Ruins2_01", "Ruins Gate 2", "City Gate", "Interactable", weight:15)
                .WithRotationGroup(RotationGroup.Four)
                .WithReceiverGroup(ReceiverGroup.Gates),
            CreateEnemy("Ruins2_01", "Ruins Sentry 1", "Husk Sentry"),
            CreateEnemy("Ruins2_01", "Ruins Flying Sentry", "Winged Sentry"),
            CreateEnemy("Ruins2_01", "Ruins Flying Sentry Javelin", "Lance Sentry"),
            CreateEnemy("Ruins2_01", "Ruins Sentry Fat", "Heavy Sentry"),
            CreateEnemy("Ruins1_30", "Mage Blob 1", "Mistake"),
            CreateEnemy("Ruins1_30", "Mage Balloon", "Folly"),
            CreateEnemy("Ruins2_01", "Royal Zombie Coward (1)", "Cowardly Husk"),
            CreateEnemy("Ruins2_01", "Royal Zombie 1 (1)", "Husk Dandy"),
            CreateEnemy("Ruins2_01", "Royal Zombie Fat (2)", "Gluttonous Husk"),
            CreateEnemy("Ruins2_01", "Battle Scene/Great Shield Zombie", "Great Husk Sentry"),
            CreateEnemy("Ruins_House_02", "Gorgeous Husk", "Gorgeous Husk"),
            Create("Ruins2_08", "ruind_bridge_roof_01 (1)/ruind_bridge_roof_04_spikes", "Roof Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Ruins1_03", "_Scenery/ruin_plat_float_01", "City Platform 1", "Solids"),
            Create("Ruins1_03", "_Scenery/ruin_plat_float_01_wide", "City Platform 2", "Solids"),
            Create("Ruins1_03", "_Scenery/ruin_plat_float_02", "City Platform 3", "Solids"),
            Create("Ruins1_03", "_Scenery/ruin_plat_float_05", "City Platform 4", "Solids"),
            Create("Ruins1_25", "Ruins Vial Empty", "Soul Vial", "Interactable", weight:1)
                .WithConfigGroup(ConfigGroup.Breakable),
            Create("Ruins1_24", "Soul Totem 1", "Thin Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Ruins1_32", "Soul Totem 3", "Round Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("Ruins1_32", "Soul Totem 5", "Leaning Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight)
        });
        RegisterInternalPack(new ContentPack("Royal Waterways", "Assets from the Royal Waterways")
        {
            CreateEnemy("GG_Pipeway", "Fluke Fly", "Flukefey"),
            CreateEnemy("GG_Pipeway", "Flukeman", "Flukemon"),
            CreateEnemy("GG_Pipeway", "Fat Fluke", "Flukemunga"),
            CreateEnemy("Waterways_01", "_Enemies/Inflater", "Hwurmp"),
            CreateEnemy("Waterways_01", "_Enemies/Flip Hopper", "Pilflip")
        });
        RegisterInternalPack(new ContentPack("Crystal Peak", "Assets from the Crystal Peak")
        {
            new ZombieMylaPackElement(),
            CreateEnemy("Mines_11", "Zombie Miner 1", "Husk Miner"),
            CreateEnemy("Mines_25", "Zombie Beam Miner", "Crystallised Husk"),
            CreateEnemy("Mines_11", "Crystal Crawler", "Blimback"),
            CreateEnemy("Mines_11", "Mines Crawler", "Shardmite"),
            CreateEnemy("Mines_25", "Crystal Flyer", "Crystal Hunter"),
            CreateEnemy("Mines_11", "Crystallised Lazer Bug", "Crystal Crawler")
                .WithRotationGroup(RotationGroup.Four),
            Create("Mines_11", "mines_metal_grate_02", "Metal Grate", "Solids")
                .WithRotationGroup(RotationGroup.Four),
            Create("Mines_31", "Mines Platform", "Crystal Peak Rotating Platform", "Solids")
                .WithRotationGroup(RotationGroup.Three),
            Create("Mines_37", "stomper_offset", "Crystal Peak Stomper (Slow)", "Hazards")
                .WithRotationGroup(RotationGroup.Four),
            Create("Mines_37", "stomper_fast", "Crystal Peak Stomper (Fast)", "Hazards")
                .WithRotationGroup(RotationGroup.Four)
        });
        RegisterInternalPack(new ContentPack("Kingdom's Edge", "Assets from Kingdom's Edge")
        {
            CreateEnemy("Deepnest_East_07", "Ceiling Dropper", "Belfly"),
            CreateEnemy("Deepnest_East_07", "Blow Fly", "Boofly"),
            CreateEnemy("Deepnest_East_07", "Super Spitter", "Primal Aspid"),
            new HopperPackElement("Deepnest_East_07", "Hopper", "Hopper"),
            new HopperPackElement("Deepnest_East_14b", "Giant Hopper", "Great Hopper"),
            Create("Ruins2_11_b", "jar_col_plat", "Tower of Love Platform", "Solids")
        });
        RegisterInternalPack(new ContentPack("The Hive", "Assets from the Hive")
        {
            CreateEnemy("Hive_04", "Bee Hatchling Ambient (31)", "Hiveling"),
            CreateEnemy("Hive_04", "Bee Stinger (9)", "Hive Soldier"),
            CreateEnemy("Hive_04", "Big Bee (3)", "Hive Guardian")
        });
        RegisterInternalPack(new ContentPack("Ancient Basin", "Assets from the Ancient Basin")
        {
            CreateEnemy("Abyss_20", "Parasite Balloon (6)", "Infected Balloon"),
            CreateEnemy("Abyss_17", "Lesser Mawlek", "Lesser Mawlek"),
            new MawlurkPackElement(),
            CreateEnemy("Abyss_04", "Abyss Crawler", "Shadow Creeper")
                .WithRotationGroup(RotationGroup.Four)
        });
        RegisterInternalPack(new ContentPack("Deepnest", "Assets from Deepnest")
        {
            CreateEnemy("Deepnest_33", "Zombie Runner Sp (1)", "Corpse Creeper (Wandering Husk)"),
            CreateEnemy("Deepnest_33", "Zombie Hornhead Sp (2)", "Corpse Creeper (Husk Hornhead)"),
            CreateEnemy("Deepnest_17", "Baby Centipede", "Dirtcarver"),
            CreateEnemy("Deepnest_41", "Spider Mini", "Deephunter")
                .WithRotationGroup(RotationGroup.Four),
            CreateEnemy("Deepnest_41", "Tiny Spider", "Deepling"),
            CreateEnemy("Deepnest_41", "Spider Flyer", "Little Weaver"),
            CreateEnemy("Deepnest_41", "Slash Spider", "Stalking Devout").FlipVertical()
        });
        RegisterInternalPack(new ContentPack("White Palace", "Assets from the White Palace")
        {
            Create("White_Palace_18", "White Palace Fly", "Wingsmould", "Enemies").FlipVertical(),
            Create("White_Palace_07", "wp_plat_float_01_wide (1)", "White Palace Platform 1", "Solids"),
            Create("White_Palace_07", "wp_plat_float_07", "White Palace Platform 2", "Solids"),
            Create("White_Palace_07", "wp_plat_float_03", "White Palace Platform 3", "Solids"),
            Create("White_Palace_07", "wp_plat_float_05 (1)", "White Palace Platform 4", "Solids"),
            Create("White_Palace_07", "wp_trap_spikes", "White Palace Moving Spikes", "Hazards")
                .WithRotationGroup(RotationGroup.Eight),
            Create("White_Palace_07", "wp_saw", "White Palace Saw", "Hazards"),
            Create("White_Palace_03_hub", "Soul Totem white", "Pale King Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("White_Palace_18", "Soul Totem white_Infinte", "Pure Vessel Soul Totem", "Interactable")
                .WithRotationGroup(RotationGroup.Eight),
            Create("White_Palace_03_hub", "White_Servant_01", "Royal Retainer 1", "Interactable"),
            Create("White_Palace_03_hub", "White_Servant_02", "Royal Retainer 2", "Interactable"),
            Create("White_Palace_03_hub", "White_Servant_03", "Royal Retainer 3", "Interactable")
        });
    }
}