using System.Collections.Generic;

namespace MP3Player.Content.UI.MP3PlayerUI.Conditions
{
    public abstract class ListCondition
    {
        public string Uuid { get; private set; }

        public string Choice { get; set; }

        public abstract string Info { get; }

        public ListCondition(string uuid)
        {
            Uuid = uuid;
        }

        public abstract void ApplyChoice();

        public abstract void RemoveCondition();

        public abstract List<string> BackingList();

        public abstract string GetDataDisplay(string data);
    }
}
