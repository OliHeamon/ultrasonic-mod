using MP3Player.Content.UI.MP3PlayerUI;
using MP3Player.Core.UI;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MP3Player.Common.Players
{
    public class MP3ModPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (MP3Player.MP3PlayerBind.JustPressed)
            {
                MP3PlayerUILoader.GetUIState<MP3PlayerState>().Visible = !MP3PlayerUILoader.GetUIState<MP3PlayerState>().Visible;

                SoundEngine.PlaySound(MP3PlayerUILoader.GetUIState<MP3PlayerState>().Visible ? SoundID.MenuOpen : SoundID.MenuClose);
            }
        }
    }
}
