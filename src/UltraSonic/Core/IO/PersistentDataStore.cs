// Credit to Scalie for PersistentDataStore - https://github.com/ProjectStarlight/StarlightRiver/blob/master/Core/Systems/PersistentDataSystem/PersistentDataStore.cs

using System.IO;
using Terraria.ModLoader.IO;
using Terraria;
using Terraria.ModLoader;

namespace MP3Player.Core.IO
{
    public abstract class PersistentDataStore : ILoadable
    {
        public abstract string FileName { get; }

        public void Load(Mod mod)
        {
            PersistentDataStoreSystem.PutDataStore(this);

            string path = MP3Player.CachePath;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string currentPath = Path.Combine(path, FileName);

            LoadFromFile(currentPath);
        }

        public void Unload() { }

        public void ForceSave()
        {
            if (Main.dedServ)
            {
                return;
            }

            string currentPath = Path.Combine(MP3Player.CachePath, FileName);

            ExportToFile(currentPath);
        }

        public void ExportToFile(string path)
        {
            var tag = new TagCompound();

            SaveGlobal(tag);

            if (!File.Exists(path))
            {
                FileStream stream = File.Create(path);

                stream.Close();
            }

            TagIO.ToFile(tag, path);
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            TagCompound tag = TagIO.FromFile(path);

            if (tag != null)
            {
                LoadGlobal(tag);
            }
            else
            {
                MP3Player.Instance.Logger.Error($"Failed to load persistent data: {FileName}");
            }
        }

        public abstract void SaveGlobal(TagCompound tag);

        public abstract void LoadGlobal(TagCompound tag);
    }
}
