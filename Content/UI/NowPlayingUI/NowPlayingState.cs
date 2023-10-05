using Microsoft.Xna.Framework;
using MP3Player.Common.UI.Abstract;
using MP3Player.Content.UI.MP3PlayerUI.Playback;
using System.Collections.Generic;
using Terraria.UI;

namespace MP3Player.Content.UI.NowPlayingUI
{
    public class NowPlayingState : SmartUIState
    {
        private NowPlayingPopup currentPopup;

        // Above everything else layer-wise.
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.Count - 1;

        public override void OnInitialize() => Visible = true;

        public override void SafeUpdate(GameTime gameTime)
        {
            if (currentPopup != null && currentPopup.IsCompleted)
            {
                RemoveChild(currentPopup);
                currentPopup = null;
            }

            base.SafeUpdate(gameTime);
        }

        public void NotifyActiveSong(PlaybackWidget widget)
        {
            // If there's already an active popup then don't bother with the initial animation.
            if (currentPopup != null)
            {
                RemoveChild(currentPopup);
                currentPopup = new(widget.Name, widget.Author, true);
            }
            else
            {
                currentPopup = new(widget.Name, widget.Author);
            }

            Append(currentPopup);
        }
    }
}
