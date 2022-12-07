namespace Scatternest
{
    internal static class DebugInterop
    {
        internal static void Hook()
        {
            DebugMod.AddActionToKeyBindList(CycleStart, "Cycle Start", "Scatternest");
            DebugMod.AddActionToKeyBindList(ResetStart, "Reset Start", "Scatternest");
        }

        public static void CycleStart()
        {
            if (MultiItemchangerStart.Instance is MultiItemchangerStart start)
            {
                start.CycleIndex();
                DebugMod.LogToConsole($"Set Start to {start.Name}");
            }
        }
        public static void ResetStart()
        {
            if (MultiItemchangerStart.Instance is MultiItemchangerStart start)
            {
                start.ResetIndex();
                DebugMod.LogToConsole($"Reset Start to {start.Name}");
            }
        }
    }
}
