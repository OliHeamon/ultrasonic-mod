using MP3Player.Content.IO;
using MP3Player.Core.IO;
using MP3Player.Localization;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public class BossCondition : ListCondition
    {
        public override string Info => "MP3PlayerMenu.BossCondition";

        public BossCondition(string uuid) : base(uuid)
        {
        }

        public override void ApplyChoice()
        {
            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            string condition = $"{MP3PlayerDataStore.BossConditionKey}:{Choice}";

            store.Conditions.Add(condition, Uuid);

            store.ForceSave();

            Main.NewText(LocalizationHelper.GetGUIText("MP3PlayerMenu.SetNewBossCondition", GetDataDisplay(Choice)), MP3Player.Cyan);
        }

        public override void RemoveCondition()
        {
            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            string condition = $"{MP3PlayerDataStore.BossConditionKey}:{Choice}";

            store.DeleteSongDataByCondition(condition);

            Main.NewText(LocalizationHelper.GetGUIText("MP3PlayerMenu.RemovedBossCondition", GetDataDisplay(Choice)), MP3Player.Pink);
        }

        public override List<string> BackingList() => MP3Player.Bosses;

        public override string GetDataDisplay(string data)
        {
            string bossName;

            // If this parse is successful then the data is a numerical ID representing a vanilla boss.
            if (int.TryParse(data, out int result))
            {
                NPC npc = new();
                npc.SetDefaults(result);

                bossName = npc.FullName;
            }
            // If not, must be a modded boss.
            else
            {
                string modName = data.Split('.')[0];
                string npcName = data.Split('.')[^1];

                Mod mod = ModLoader.GetMod(modName);

                bool found = mod.TryFind(npcName, out ModNPC value);

                if (!found)
                {
                    bossName = $"{modName}.{npcName}";
                }
                else
                {
                    bossName = value.DisplayName.Value;
                }
            }

            MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

            // [!] is added to indicate if something has existing presets.
            if (store.Conditions.ContainsKey($"BossCondition:{data}"))
            {
                bossName += " [!]";
            }

            return bossName;
        }
    }
}
