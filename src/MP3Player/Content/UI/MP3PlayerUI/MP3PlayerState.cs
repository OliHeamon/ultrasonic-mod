using MP3Player.Common.UI.Abstract;
using System.Collections.Generic;
using Terraria.UI;

namespace MP3Player.Content.UI.MP3PlayerUI
{
    public class MP3PlayerState : SmartUIState
    {
        public MP3PlayerMainPanel MainPanel { get; private set; }

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override void OnInitialize()
        {
            MainPanel = new();
            MainPanel.Width.Set(288, 0);
            MainPanel.Height.Set(384, 0);
            MainPanel.Left.Set(0, 0.65f);
            MainPanel.Top.Set(-MainPanel.Width.Pixels / 2, 0.5f);

            Append(MainPanel);
        }
    }
}
