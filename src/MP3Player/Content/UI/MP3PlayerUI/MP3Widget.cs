using MP3Player.Common.UI.Abstract;
using MP3Player.Content.UI.MP3PlayerUI.Enums;

namespace MP3Player.Content.UI.MP3PlayerUI
{
    public abstract class MP3Widget : SmartUIElement
    {
        protected MP3PlayerMainPanel Panel { get; private set; }

        public MP3Widget(MP3PlayerMainPanel panel)
        {
            Panel = panel;
        }

        public abstract void AcceptInput(ButtonType type);

        protected record Song(string Title, string Uuid) { }
    }
}
