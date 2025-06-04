using System.Collections.Generic;

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

        foreach (ContentPack pack in InternalPacks)
        {
            if (!pack.IsEnabled()) continue;
            foreach (ContentPack.AbstractPackElement element in pack)
            {
                if (element is not ContentPack.InternalPackElement internalPackElement) continue;
                preloadInfo.Add((internalPackElement.Scene, internalPackElement.Path));
            }
        }

        return preloadInfo;
    }

    private static ContentPack.InternalPackElement Create(string scene, string path, string name, string category, bool flipX = false)
    {
        return new ContentPack.InternalPackElement(scene, path, name, category, flipX);
    }

    internal static void PreloadInternalPacks()
    {
        RegisterInternalPack(new ContentPack("Forgotten Crossroads", "Assets from the regular Forgotten Crossroads")
        {
            Create("Crossroads_01", "_Scenery/plat_float_05", "Crossroads Platform 1", "Platforms"),
            Create("Crossroads_07", "Uninfected Parent/Fly", "Gruzzer", "Enemies"),
            Create("Crossroads_07", "_Enemies/Crawler", "Crawlid", "Enemies"),
            Create("Crossroads_07", "_Enemies/Climber 3", "Tiktik", "Enemies"),
            Create("Crossroads_16", "Uninfected Parent/Buzzer", "Vengefly", "Enemies"),
            Create("Crossroads_21", "non_infected_event/Zombie Runner", "Wandering Husk", "Enemies"),
            Create("Crossroads_21", "Zombie Barger", "Husk Bully", "Enemies"),
            Create("Crossroads_16", "_Enemies/Zombie Hornhead", "Husk Hornhead", "Enemies"),
            Create("Crossroads_21", "non_infected_event/Zombie Leaper", "Leaping Husk", "Enemies"),
            Create("Crossroads_15", "_Enemies/Zombie Shield", "Husk Warrior", "Enemies"),
            Create("Crossroads_21", "non_infected_event/Zombie Guard", "Husk Guard", "Enemies"),
            Create("Crossroads_19", "_Enemies/Spitter", "Aspid Hunter", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Infected Crossroads", "Assets unique to the Infected Crossroads")
        {
            Create("Crossroads_07", "Infected Parent/Bursting Bouncer", "Volatile Gruzzer", "Enemies"),
            Create("Crossroads_16", "infected_event/Angry Buzzer", "Furious Vengefly", "Enemies"),
            Create("Crossroads_21", "infected_event/Bursting Zombie", "Violent Husk", "Enemies"),
            Create("Crossroads_15", "Infected Parent/Spitting Zombie", "Slobbering Husk", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Greenpath", "Assets from Greenpath")
        {
            Create("Fungus1_12", "Pigeon", "Maskfly", "Enemies"),
            Create("Fungus1_22", "Mosquito", "Squit", "Enemies"),
            Create("Fungus1_12", "Acid Walker", "Durandoo", "Enemies"),
            Create("Fungus1_09", "Acid Flyer", "Duranda", "Enemies"),
            Create("Fungus1_31", "_Enemies/Mossman_Runner", "Mosskin", "Enemies"),
            Create("Fungus1_22", "Mossman_Shaker", "Volatile Mosskin", "Enemies"),
            Create("Fungus1_32", "Moss Knight C", "Moss Knight", "Enemies"),
            Create("Fungus1_22", "Moss Walker", "Mosscreep", "Enemies"),
            Create("Fungus1_12", "Plant Turret", "Gulka", "Enemies"),
            Create("Fungus1_31", "_Enemies/Fat Fly (1)", "Obble", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Fog Canyon", "Assets from Fog Canyon")
        {
            Create("Fungus3_26", "Jellyfish", "Ooma", "Enemies"),
            Create("Fungus3_26", "Jellyfish Baby", "Uoma", "Enemies"),
            Create("Fungus3_26", "Zap Cloud", "Charged Lumaflies", "Enemies"),
            Create("Fungus3_26", "Jelly Egg Bomb", "Jelly Egg Bomb", "Hazards")
        });
        RegisterInternalPack(new ContentPack("Fungal Wastes", "Assets from the Fungal Wastes")
        {
            Create("Fungus2_18", "Zombie Fungus A", "Fungified Husk", "Enemies"),
            Create("Fungus2_05", "Battle Scene v2/Completed/Mushroom Brawler", "Shrumal Ogre", "Enemies"),
            Create("Fungus2_05", "Battle Scene v2/Completed/Mushroom Baby", "Shrumeling", "Enemies"),
            Create("Fungus2_18", "_Scenery/Fungoon Baby", "Fungling", "Enemies"),
            Create("Fungus2_18", "_Scenery/Fungus Flyer", "Fungoon", "Enemies"),
            Create("Fungus2_28", "Mushroom Roller", "Shrumal Warrior", "Enemies"),
            Create("Fungus2_18", "Fung Crawler", "Ambloom", "Enemies"),
            Create("Fungus2_04", "Mushroom Turret", "Sporg", "Enemies"),
            Create("Fungus2_12", "Mantis", "Mantis Warrior", "Enemies"),
            Create("Fungus2_12", "Mantis Flyer Child", "Mantis Youth", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Queen's Gardens", "Assets from the Queen's Gardens")
        {
            Create("Fungus3_34", "Garden Zombie", "Spiny Husk", "Enemies"),
            Create("Fungus3_48", "Lazy Flyer Enemy", "Aluba", "Enemies"),
            Create("Fungus3_34", "Moss Flyer", "Mossfly", "Enemies"),
            Create("Fungus3_48", "Grass Hopper", "Loodle", "Enemies"),
            Create("Fungus3_10", "Battle Scene/Completed/Mantis Heavy", "Mantis Traitor", "Enemies"),
            Create("Fungus3_48", "Mantis Heavy Flyer", "Mantis Petra", "Enemies")
        });
        RegisterInternalPack(new ContentPack("City of Tears", "Assets from the City of Tears")
        {
            Create("Ruins2_01", "Ruins Sentry 1", "Husk Sentry", "Enemies"),
            Create("Ruins2_01", "Ruins Flying Sentry", "Winged Sentry", "Enemies"),
            Create("Ruins2_01", "Ruins Flying Sentry Javelin", "Lance Sentry", "Enemies"),
            Create("Ruins2_01", "Ruins Sentry Fat", "Heavy Sentry", "Enemies"),
            Create("Ruins2_01", "Royal Zombie Coward (1)", "Cowardly Husk", "Enemies"),
            Create("Ruins2_01", "Royal Zombie 1 (1)", "Husk Dandy", "Enemies"),
            Create("Ruins2_01", "Royal Zombie Fat (2)", "Gluttonous Husk", "Enemies"),
            Create("Ruins2_01", "Battle Scene/Great Shield Zombie", "Great Husk Sentry", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Royal Waterways", "Assets from the Royal Waterways")
        {
            Create("GG_Pipeway", "Fluke Fly", "Flukefey", "Enemies"),
            Create("GG_Pipeway", "Flukeman", "Flukemon", "Enemies"),
            Create("GG_Pipeway", "Fat Fluke", "Flukemunga", "Enemies"),
            Create("Waterways_01", "_Enemies/Inflater", "Hwurmp", "Enemies"),
            Create("Waterways_01", "_Enemies/Flip Hopper", "Pilflip", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Crystal Peak", "Assets from the Crystal Peak")
        {
            Create("Mines_11", "Zombie Miner 1", "Husk Miner", "Enemies"),
            Create("Mines_25", "Zombie Beam Miner", "Crystallised Husk", "Enemies"),
            Create("Mines_11", "Crystal Crawler", "Blimback", "Enemies"),
            Create("Mines_11", "Mines Crawler", "Shardmite", "Enemies"),
            Create("Mines_25", "Crystal Flyer", "Crystal Hunter", "Enemies"),
            Create("Mines_11", "Crystallised Lazer Bug", "Crystal Crawler", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Kingdom's Edge", "Assets from Kingdom's Edge")
        {
            Create("Deepnest_East_07", "Ceiling Dropper", "Belfly", "Enemies"),
            Create("Deepnest_East_07", "Blow Fly", "Boofly", "Enemies"),
            Create("Deepnest_East_07", "Super Spitter", "Primal Aspid", "Enemies"),
            Create("Deepnest_East_07", "Hopper", "Hopper", "Enemies"),
            Create("Deepnest_East_14b", "Giant Hopper", "Great Hopper", "Enemies")
        });
        RegisterInternalPack(new ContentPack("The Hive", "Assets from the Hive")
        {
            Create("Hive_04", "Bee Hatchling Ambient (31)", "Hiveling", "Enemies"),
            Create("Hive_04", "Bee Stinger (9)", "Hive Soldier", "Enemies"),
            Create("Hive_04", "Big Bee (3)", "Hive Guardian", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Ancient Basin", "Assets from the Ancient Basin")
        {
            Create("Abyss_20", "Parasite Balloon (6)", "Infected Balloon", "Enemies"),
            Create("Abyss_17", "Lesser Mawlek", "Lesser Mawlek", "Enemies"),
            Create("Abyss_20", "Mawlek Turret", "Mawlurk (Floor)", "Enemies"),
            Create("Abyss_20", "Mawlek Turret Ceiling", "Mawlurk (Ceiling)", "Enemies"),
            Create("Abyss_04", "Abyss Crawler", "Shadow Creeper", "Enemies")
        });
        RegisterInternalPack(new ContentPack("Deepnest", "Assets from Deepnest")
        {
            Create("Deepnest_33", "Zombie Runner Sp (1)", "Corpse Creeper (Wandering Husk)", "Enemies"),
            Create("Deepnest_33", "Zombie Hornhead Sp (2)", "Corpse Creeper (Husk Hornhead)", "Enemies"),
            Create("Deepnest_17", "Baby Centipede", "Dirtcarver", "Enemies"),
            Create("Deepnest_41", "Spider Mini", "Deephunter", "Enemies"),
            Create("Deepnest_41", "Tiny Spider", "Deepling", "Enemies"),
            Create("Deepnest_41", "Spider Flyer", "Little Weaver", "Enemies"),
            Create("Deepnest_41", "Slash Spider", "Stalking Devout", "Enemies", true)
        });
        RegisterInternalPack(new ContentPack("White Palace", "Assets from White Palace")
        {
            Create("White_Palace_06", "White Palace Fly", "Wingsmould", "Enemies"),
            Create("White_Palace_07", "wp_plat_float_01_wide (1)", "White Palace Platform 1", "Platforms"),
            Create("White_Palace_07", "wp_plat_float_07", "White Palace Platform 2", "Platforms"),
            Create("White_Palace_07", "wp_plat_float_03", "White Palace Platform 3", "Platforms"),
            Create("White_Palace_07", "wp_plat_top_wide", "White Palace Platform 4", "Platforms"),
            Create("White_Palace_07", "wp_plat_float_05 (1)", "White Palace Platform 5", "Platforms"),
            Create("White_Palace_07", "wp_trap_spikes", "White Palace Moving Spikes", "Hazards"),
            Create("White_Palace_07", "wp_saw", "White Palace Saw", "Hazards"),
        });
    }
}