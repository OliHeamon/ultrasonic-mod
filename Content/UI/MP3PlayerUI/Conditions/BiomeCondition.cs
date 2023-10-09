using MP3Player.Content.IO;
using MP3Player.Core.IO;
using MP3Player.Localization;
using System.Collections.Generic;
using Terraria;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public class BiomeCondition : ListCondition
    {
        public override string Info => "MP3PlayerMenu.BiomeCondition";

        public BiomeCondition(string uuid) : base(uuid)
        {
        }

        public override void ApplyChoice()
        {
            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            string condition = $"{MP3PlayerDataStore.BiomeConditionKey}:{Choice}";

            store.Conditions[condition] = Uuid;

            store.ForceSave();

            Main.NewText(LocalizationHelper.GetGUIText("MP3PlayerMenu.SetNewBiomeCondition", GetDataDisplay(Choice)), MP3Player.Cyan);
        }

        public override void RemoveCondition()
        {
            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            string condition = $"{MP3PlayerDataStore.BiomeConditionKey}:{Choice}";

            store.DeleteSongDataByCondition(condition);

            Main.NewText(LocalizationHelper.GetGUIText("MP3PlayerMenu.RemovedBiomeCondition", GetDataDisplay(Choice)), MP3Player.Pink);
        }

        public override List<string> BackingList() => BiomeChecker.Biomes;

        public override string GetDataDisplay(string data)
        {
            string biomeName = BiomeChecker.GetDisplayName(data);

            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            // [!] is added to indicate if something has existing presets.
            if (store.Conditions.ContainsKey($"BiomeCondition:{data}"))
            {
                biomeName += " [!]";
            }

            return biomeName;
        }
    }
}
