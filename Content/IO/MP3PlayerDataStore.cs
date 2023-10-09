using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Core.IO;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace MP3Player.Content.IO
{
    public class MP3PlayerDataStore : PersistentDataStore
    {
        public const string LastManStandingKey = "LastManStanding";
        public const string OnDeathKey = "OnDeath";
        public const string BiomeConditionKey = "BiomeCondition";
        public const string BossConditionKey = "BossCondition";

        private const string PlayModeTag = "MP3Player:PlayMode";

        private const string ConditionDictionaryTag = "MP3Player:Conditions";

        public PlayMode PlayMode { get; set; }

        public Dictionary<string, string> Conditions { get; private set; } = new();

        public override string FileName => "playback_preferences.dat";

        public override void LoadGlobal(TagCompound tag)
        {
            if (tag.ContainsKey(PlayModeTag))
            {
                PlayMode = (PlayMode)tag.GetInt(PlayModeTag);
            }

            foreach (KeyValuePair<string, object> item in tag)
            {
                if (item.Key.StartsWith(ConditionDictionaryTag))
                {
                    string uuid = tag.GetString(item.Key);

                    string conditionKey = item.Key.Replace($"{ConditionDictionaryTag}:", "");

                    Conditions[conditionKey] = uuid;
                }
            }
        }

        public override void SaveGlobal(TagCompound tag)
        {
            tag[PlayModeTag] = (int)PlayMode;

            foreach (KeyValuePair<string, string> item in Conditions)
            {
                string key = $"{ConditionDictionaryTag}:{item.Key}";

                tag[key] = item.Value;
            }
        }

        public void AddCondition(string condition, string uuid)
        {
            Conditions[condition] = uuid;

            ForceSave();
        }

        // Condition data associated with a specific song needs to be removed when it's deleted.
        public void DeleteSongDataByUuid(string uuid)
        {
            List<string> removeKeys = new();

            foreach (KeyValuePair<string, string> item in Conditions)
            {
                if (item.Value == uuid)
                {
                    removeKeys.Add(item.Key);
                }
            }

            foreach (string key in removeKeys)
            {
                Conditions.Remove(key);
            }

            ForceSave();
        }

        public void DeleteSongDataByCondition(string condition)
        {
            Conditions.Remove(condition);

            ForceSave();
        }
    }
}
