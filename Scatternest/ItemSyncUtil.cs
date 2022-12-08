using Modding;

namespace Scatternest
{
    /// <summary>
    /// Class to safely get information about Itemsync.
    /// </summary>
    internal static class ItemSyncUtil
    {
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
    }
}
