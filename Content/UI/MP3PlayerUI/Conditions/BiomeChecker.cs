using MP3Player.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public static class BiomeChecker
    {
        private static Dictionary<string, string> biomeIdToName;

        private static Dictionary<string, BiomeConditionEntry> biomeConditions;

        private static List<string> biomeNames;

        private static bool registered;

        public static List<string> Biomes
        {
            get
            {
                if (!registered)
                {
                    Initialise();
                }

                return biomeNames;
            }
        }

        public static string GetDisplayName(string condition)
        {
            if (!registered)
            {
                Initialise();
            }

            return biomeIdToName[condition];
        }

        public static bool ConditionMet(Player player, string condition, out float priority)
        {
            if (!registered)
            {
                Initialise();
            }

            BiomeConditionEntry entry = biomeConditions[condition];

            priority = entry.Priority;

            return entry.Condition.Invoke(player);
        }

        private static void Initialise()
        {
            biomeIdToName = new();
            biomeConditions = new();
            biomeNames = new();

            RegisterBiomes();

            biomeNames = biomeIdToName.Keys.ToList();

            registered = true;
        }

        private static void RegisterBiomes()
        {
            Register("Forest", p => p.ZoneForest, 0);
            Register("Night", p => p.ZoneForest && !Main.dayTime, 0.1f);

            Register("TownDay", p => Main.dayTime && p.townNPCs > 2, 0.2f);
            Register("TownNight", p => !Main.dayTime && p.townNPCs > 2, 0.2f);

            Register("Underground", p => p.ZoneNormalCaverns || p.ZoneNormalUnderground, 0.3f);

            Register("SurfaceHallow", p => p.ZoneHallow && p.ZoneOverworldHeight, 0.5f);
            Register("UndergroundHallow", p => p.ZoneHallow && (p.ZoneRockLayerHeight || p.ZoneDirtLayerHeight), 0.7f);

            Register("SurfaceCorruption", p => p.ZoneCorrupt && p.ZoneOverworldHeight, 0.5f);
            Register("UndergroundCorruption", p => p.ZoneCorrupt && (p.ZoneRockLayerHeight || p.ZoneDirtLayerHeight), 0.6f);

            Register("SurfaceCrimson", p => p.ZoneCrimson && p.ZoneOverworldHeight, 0.5f);
            Register("UndergroundCrimson", p => p.ZoneCrimson && (p.ZoneRockLayerHeight || p.ZoneDirtLayerHeight), 0.6f);

            Register("SurfaceJungle", p => p.ZoneJungle && p.ZoneOverworldHeight, 0.2f);
            Register("UndergroundJungle", p => p.ZoneJungle && (p.ZoneRockLayerHeight || p.ZoneDirtLayerHeight), 0.4f);

            Register("SurfaceDesert", p => p.ZoneDesert, 0.2f);
            Register("UndergroundDesert", p => p.ZoneDesert, 0.4f);

            Register("SurfaceSnow", p => p.ZoneSnow, 0.2f);
            Register("UndergroundSnow", p => p.ZoneSnow, 0.4f);

            Register("Ocean", p => p.ZoneBeach, 0.2f);
            Register("Space", p => p.ZoneNormalSpace, 0.6f);
            Register("Dungeon", p => p.ZoneDungeon, 0.8f);
            Register("Hell", p => p.ZoneUnderworldHeight, 2);
            Register("Mushroom", p => p.ZoneGlowshroom, 0.4f);
            Register("Temple", p => p.ZoneLihzhardTemple, 0.7f);
            Register("Meteor", p => p.ZoneMeteor, 0.3f);
            Register("Graveyard", p => p.ZoneGraveyard, 0.2f);

            Register("WindyDay", p => p.ZoneOverworldHeight && Main.IsItAHappyWindyDay, 0.1f);
            Register("Rain", p => p.ZoneOverworldHeight && p.ZoneRain, 0.1f);
            Register("Thunderstorm", p => p.ZoneOverworldHeight && Main.IsItAHappyWindyDay && Main.cloudAlpha > 2 / 3f, 0.2f);
            Register("Sandstorm", p => p.ZoneSandstorm, 0.8f);

            Register("LunarEvent", p => p.ZoneTowerNebula || p.ZoneTowerSolar || p.ZoneTowerStardust || p.ZoneTowerVortex, 1);
            Register("OOA", p => p.ZoneOldOneArmy, 1);
            Register("GoblinArmy", p => Main.invasionType == InvasionID.GoblinArmy, 1);
            Register("FrostLegion", p => Main.invasionType == InvasionID.SnowLegion, 1);
            Register("Pirates", p => Main.invasionType == InvasionID.PirateInvasion, 1);
            Register("PumpkinMoon", p => Main.invasionType == InvasionID.CachedPumpkinMoon, 1.5f);
            Register("FrostMoon", p => Main.invasionType == InvasionID.CachedFrostMoon, 1.5f);
            Register("Martians", p => Main.invasionType == InvasionID.MartianMadness, 1);

            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Register("Calamity.Crags", p => (bool)calamity.Call("GetInZone", p, "crags"), 0.9f);
                Register("Calamity.Astral", p => (bool)calamity.Call("GetInZone", p, "astral"), 0.9f);
                Register("Calamity.SunkenSea", p => (bool)calamity.Call("GetInZone", p, "sunkensea"), 0.9f);
                Register("Calamity.SulphurSea", p => (bool)calamity.Call("GetInZone", p, "sulphursea"), 0.9f);
                Register("Calamity.Abyss", p => (bool)calamity.Call("GetInZone", p, "abyss"), 0.9f);
            }
        }

        private static void Register(string biome, Func<Player, bool> condition, float priority)
        {
            string localisedName = LocalizationHelper.GetText($"Biomes.{biome}");

            biomeIdToName[biome] = localisedName;
            biomeConditions[biome] = new(condition, priority);
        }

        private record BiomeConditionEntry(Func<Player, bool> Condition, float Priority);
    }
}
