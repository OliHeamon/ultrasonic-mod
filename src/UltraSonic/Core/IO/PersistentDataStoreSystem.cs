using System;
using System.Collections.Generic;

namespace MP3Player.Core.IO
{
    public class PersistentDataStoreSystem
    {
        private static readonly Dictionary<Type, PersistentDataStore> stores = new();

        public static T GetDataStore<T>() where T : PersistentDataStore => stores[typeof(T)] as T;

        public static void PutDataStore(PersistentDataStore store)
        {
            Type key = store.GetType();

            if (stores.ContainsKey(key))
            {
                stores[key] = store;
            }
            else
            {
                stores.Add(key, store);
            }
        }
    }
}
