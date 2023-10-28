using MP3Player.Content.IO;
using MP3Player.Content.UI.MP3PlayerUI;
using MP3Player.Content.UI.MP3PlayerUI.Conditions;
using MP3Player.Content.UI.MP3PlayerUI.Enums;
using MP3Player.Core.IO;
using MP3Player.Core.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MP3Player.Content.Audio
{
    public class MP3TrackUpdaterSystem : ModSystem
    {
        public bool CurrentlyForcingSong { get; private set; }

        public bool CurrentlyFadingOut { get; private set; }

        public override void PostUpdateInput()
        {
            if (!Main.gameMenu)
            {
                CurrentlyForcingSong = false;

                MP3PlayerMainPanel panel = MP3PlayerUILoader.GetUIState<MP3PlayerState>().MainPanel;

                MP3PlayerDataStore store = PersistentDataStoreSystem.GetDataStore<MP3PlayerDataStore>();

                // Only play custom presets if the playmode is "once", so it doesn't interrupt someone's playlist.
                if (store.PlayMode == PlayMode.Once)
                {
                    bool lastManStanding = CheckLastManStanding(panel, store);

                    // Last man standing song takes higher priority over boss/biome songs.
                    if (!lastManStanding)
                    {
                        bool onDeath = CheckOnDeath(panel, store);

                        if (!onDeath)
                        {
                            string uuid = CheckListConditions(store);

                            if (uuid != null)
                            {
                                if (panel.ActiveSong?.Uuid != uuid)
                                {
                                    panel.BeginPlayingSong(uuid, true);
                                }

                                CurrentlyForcingSong = true;
                            }
                        }
                    }
                }

                HandleFade(panel);

                panel.UpdateActiveSong();
            }
        }

        private bool CheckLastManStanding(MP3PlayerMainPanel panel, MP3PlayerDataStore store)
        {
            // This method will return true if it changed the song, so we can avoid running any more checks that might caused forced songs to conflict with each other.
            bool found = store.Conditions.TryGetValue(MP3PlayerDataStore.LastManStandingKey, out string value);

            if (!found)
            {
                return false;
            }

            // In singleplayer the last man standing song plays if a boss is alive and the player is below 20% HP.
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                bool bossAlive = Main.CurrentFrameFlags.AnyActiveBossNPC;

                if (bossAlive && (float)Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2 <= 0.2f)
                {
                    if (panel.ActiveSong?.Uuid != value)
                    {
                        panel.BeginPlayingSong(value, true);
                    }

                    CurrentlyForcingSong = true;

                    return true;
                }
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                int livingCount = 0;

                for (int i = 0; i < Main.CurrentFrameFlags.ActivePlayersCount; i++)
                {
                    if (!Main.player[i].dead)
                    {
                        livingCount++;
                    }
                }

                // If the amount of living players is 1 and the client is alive then the client must be the last man alive.
                if (livingCount == 1 && !Main.LocalPlayer.dead)
                {
                    if (panel.ActiveSong?.Uuid != value)
                    {
                        panel.BeginPlayingSong(value, true);
                    }

                    CurrentlyForcingSong = true;

                    return true;
                }
            }

            return false;
        }

        private bool CheckOnDeath(MP3PlayerMainPanel panel, MP3PlayerDataStore store)
        {
            bool found = store.Conditions.TryGetValue(MP3PlayerDataStore.OnDeathKey, out string value);

            if (!found)
            {
                return false;
            }

            if (Main.LocalPlayer.dead)
            {
                if (panel.ActiveSong?.Uuid != value)
                {
                    panel.BeginPlayingSong(value, true);
                }

                CurrentlyForcingSong = true;

                return true;
            }

            return false;
        }

        private string CheckListConditions(MP3PlayerDataStore store)
        {
            if (store.Conditions == null)
            {
                return null;
            }

            foreach (string condition in store.Conditions.Keys)
            {
                if (condition.StartsWith(MP3PlayerDataStore.BossConditionKey))
                {
                    string conditionInfo = condition.Replace($"{MP3PlayerDataStore.BossConditionKey}:", "");

                    if (BossConditionMet(conditionInfo))
                    {
                        return store.Conditions[condition];
                    }
                }
            }

            string biomeUuid = null;

            float highestPriority = float.NegativeInfinity;

            foreach (string condition in store.Conditions.Keys)
            {
                if (condition.StartsWith(MP3PlayerDataStore.BiomeConditionKey))
                {
                    string conditionInfo = condition.Replace($"{MP3PlayerDataStore.BiomeConditionKey}:", "");

                    if (BiomeChecker.ConditionMet(Main.LocalPlayer, conditionInfo, out float priority))
                    {
                        if (priority > highestPriority)
                        {
                            biomeUuid = store.Conditions[condition];
                            highestPriority = priority;
                        }
                    }
                }
            }

            return biomeUuid;
        }

        private bool BossConditionMet(string condition)
        {
            int id;

            // If this parse is successful then the data is a numerical ID representing a vanilla boss.
            if (int.TryParse(condition, out int result))
            {
                id = result;
            }
            // If not, must be a modded boss.
            else
            {
                string modName = condition.Split('.')[0];
                string npcName = condition.Split('.')[^1];

                if (!ModLoader.TryGetMod(modName, out Mod mod))
                {
                    return false;
                }

                if (!mod.TryFind(npcName, out ModNPC value))
                {
                    return false;
                }

                id = value.Type;
            }

            return NPC.AnyNPCs(id);
        }
        
        private void HandleFade(MP3PlayerMainPanel panel)
        {
            int silentSlot = MusicLoader.GetMusicSlot(Mod, MP3Player.Silence);

            // Allows the playing song to fade in while the current music fades out.
            if (panel.ActiveSong != null && CurrentlyForcingSong)
            {
                panel.ActiveSong.VolumeFadeMultiplier = Main.musicFade[silentSlot];
            }

            // Allows the playing song to fade out slowly when it's no longer being forced.
            if (!CurrentlyForcingSong && panel.ActiveSong != null && panel.ActiveSong.Forced)
            {
                CurrentlyFadingOut = true;
            }

            if (panel.ActiveSong != null && CurrentlyFadingOut)
            {
                panel.ActiveSong.VolumeFadeMultiplier -= 1 / 180f;

                if (panel.ActiveSong.VolumeFadeMultiplier <= 0)
                {
                    panel.StopCurrentSong();

                    CurrentlyFadingOut = false;
                }
            }

            if (panel.ActiveSong == null)
            {
                CurrentlyForcingSong = CurrentlyFadingOut = false;
            }
        }
    }
}
