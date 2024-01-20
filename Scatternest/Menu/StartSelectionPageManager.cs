using ItemChangerDataLoader;
using MenuChanger.MenuElements;
using MenuChanger;
using System.Collections.Generic;
using Modding;
using RandomizerMod.Menu;
using RandomizerMod.RC;

namespace Scatternest.Menu
{
    internal static class StartSelectionPageManager
    {
        public static int ICDLHash { get; private set; }

        internal static Dictionary<MenuPage, StartSelectionPage> Instances { get; private set; } = new();

        public static void Hook()
        {
            RandomizerMenuAPI.AddStartGameOverride(ConstructMenu, RandoHandleButton);
            MenuChangerMod.OnExitMainMenu += Instances.Clear;

            if (ModHooks.GetMod("ICDL Mod") is Mod) HookICDL();
        }

        internal static void HookICDL()
        {
            ICDLMenuAPI.AddStartGameOverride(ConstructMenu, ICDLHandleButton);
        }

        private static void ConstructMenu(MenuPage landingPage) => Instances[landingPage] = new(landingPage);

        private static bool RandoHandleButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = null;
            return Instances.TryGetValue(landingPage, out StartSelectionPage instance) && instance.HandleButton(rc.ctx, out button);
        }

        // This needs to be a function rather than a delegate so that the compiler doesn't generate
        // a field to hold a reference to the delegate
        private static bool ICDLHandleButton(ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button)
        {
            ICDLHash = data.Hash();

            button = null;
            return Instances.TryGetValue(landingPage, out StartSelectionPage instance) && instance.HandleButton(data.CTX, out button);
        }
    }
}
