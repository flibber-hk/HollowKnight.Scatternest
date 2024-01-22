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
        public static int ICDLHash { get; internal set; }

        internal static Dictionary<MenuPage, StartSelectionPage> Instances { get; private set; } = new();

        public static void Hook()
        {
            RandomizerMenuAPI.AddStartGameOverride(ConstructMenu, RandoHandleButton);
            MenuChangerMod.OnExitMainMenu += Instances.Clear;

            if (ModHooks.GetMod("ICDL Mod") is Mod) StartSelectionPageManager_ICDL.HookICDL();
        }

        internal static void ConstructMenu(MenuPage landingPage) => Instances[landingPage] = new(landingPage);

        private static bool RandoHandleButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = null;
            return Instances.TryGetValue(landingPage, out StartSelectionPage instance) && instance.HandleButton(rc.ctx, out button);
        }

    }

    /* This needs to be a separate class because in some builds (e.g. on Github), the compiler
     * generates a nested class to hold references to the delegates and in some builds it does not.
     * Rather than forcing a particular toolchain, we'll just nip the problem in the bud
     * and separate out the ICDL-dependent functions into a new class.
     */
    internal static class StartSelectionPageManager_ICDL
    {
        internal static void HookICDL()
        {
            ICDLMenuAPI.AddStartGameOverride(StartSelectionPageManager.ConstructMenu, ICDLHandleButton);
        }

        private static bool ICDLHandleButton(ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button)
        {
            StartSelectionPageManager.ICDLHash = data.Hash();

            button = null;
            return StartSelectionPageManager.Instances.TryGetValue(landingPage, out StartSelectionPage instance) && instance.HandleButton(data.CTX, out button);
        }
    }
}
