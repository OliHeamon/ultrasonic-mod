using MP3Player.Content.UI.MP3PlayerUI;
using MP3Player.Core.UI;
using Terraria;
using Terraria.ModLoader;

namespace MP3Player.Content.Audio
{
    public class MP3SceneEffect : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, MP3Player.Silence);

        public override bool IsSceneEffectActive(Player player) => MP3PlayerUILoader.GetUIState<MP3PlayerState>().MainPanel.ActiveSong != null;

        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

        public override float GetWeight(Player player) => 1;
    }
}
