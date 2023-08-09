using Modding;
using System.Collections.Generic;

namespace Scatternest
{
    /// <summary>
    /// Class to safely get information about Itemsync.
    /// </summary>
    internal static class ItemSyncUtil
    {
        private const string PRIMARY_START_LABEL = "SCATTERNEST_PRIMARY_START";

        public static bool ItemSyncInstalled() => ModHooks.GetMod("ItemSyncMod") is not null;

        public static bool IsItemSync()
        {
            if (!ItemSyncInstalled()) return false;
            return IsItemSyncInternal();
        }

        private static bool IsItemSyncInternal()
        {
            return ItemSyncMod.ItemSyncMod.ISSettings.IsItemSync;
        }

        public static int PlayerCount()
        {
            if (!IsItemSync()) return -1;
            return PlayerCountInternal();
        }

        private static int PlayerCountInternal()
        {
            return ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames().Length;
        }

        public static int PlayerID()
        {
            if (!IsItemSync()) return -1;
            return PlayerIDInternal();
        }

        private static int PlayerIDInternal()
        {
            return ItemSyncMod.ItemSyncMod.ISSettings.MWPlayerId;
        }

        public static void AddPrimaryStart(Dictionary<string, string> metadata)
        {
            if (Scatternest.PrimaryStartName == null) return;
            metadata[PRIMARY_START_LABEL] = Scatternest.PrimaryStartName;
        }

        public static Dictionary<int, string> GetPrimaryStarts()
        {
            Dictionary<int, string> dict = new();
            var metadataList = ItemSyncMod.ItemSyncMod.ISSettings.readyMetadata;
            for (int i = 0; i < metadataList.Count; i++) if (metadataList[i].TryGetValue(PRIMARY_START_LABEL, out var value)) dict[i] = value;

            return dict;
        }
    }
}
