using ItemChangerDataLoader;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using System.Collections.Generic;
using System.Linq;
using static RandomizerMod.Localization;

namespace Scatternest.Menu
{
    internal class StartSelectionPage
    {
        private static Dictionary<MenuPage, StartSelectionPage> Instances = new();

        private const string Random = "Random";

        internal BigButton JumpToSSButton;

        internal MenuPage SSMenuPage;

        internal RadioSwitch ssSwitch;
        internal VerticalItemPanel ssVIP;
        private string[] _starts;

        private static void OnExitMenu() => Instances.Clear();

        public static void Hook()
        {
            RandomizerMenuAPI.AddStartGameOverride(
                page => Instances[page] = new(page),
                (RandoController rc, MenuPage landingPage, out BaseButton button) =>
                {
                    button = null;
                    return Instances.TryGetValue(landingPage, out var instance) && instance.HandleButton(rc.ctx, landingPage, out button);
                });

            if (ModHooks.GetMod("ICDL Mod") is Mod) HookICDL();

            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        public static int ICDLHash { get; private set; }

        private static void HookICDL()
        {
            ICDLMenuAPI.AddStartGameOverride(
                page => Instances[page] = new(page),
                (ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button) =>
                {
                    ICDLHash = data.Hash();

                    button = null;
                    return Instances.TryGetValue(landingPage, out var instance) && instance.HandleButton(data.CTX, landingPage, out button);
                });
        }

        public void ResetRadioSwitch(RandoModContext ctx)
        {
            Scatternest.PrimaryStartName = null;

            if (ctx?.StartDef is not MultiRandoStart mrs)
            {
                _starts = null;
                return;
            }

            _starts = mrs.InnerDefs.Select(s => s.Name).ToArray();
            string[] _buttonNames = _starts.Prepend(Random).ToArray();

            ssSwitch = new(SSMenuPage, _buttonNames);
            ssSwitch.Changed += s => Scatternest.PrimaryStartName = s == Random ? null : s;
            
            foreach (IMenuElement e in ssVIP.Items)
            {
                e.Destroy();
            }
            ssVIP.Items.Clear();

            foreach (ToggleButton e in ssSwitch.Elements)
            {
                ssVIP.Add(e);
            }
            ssVIP.Reposition();
        }

        private bool HandleButton(RandoModContext ctx, MenuPage landingPage, out BaseButton button)
        {
            ResetRadioSwitch(ctx);

            button = JumpToSSButton;
            return _starts is not null;
        }

        public StartSelectionPage(MenuPage parent)
        {
            SSMenuPage = new MenuPage(Localize("Scatternest Start Selection"), parent);
            ssVIP = new(SSMenuPage, new(0, 300), 50f, true);

            JumpToSSButton = new(parent, Localize("Start Selection"));
            JumpToSSButton.AddHideAndShowEvent(SSMenuPage);
        }
    }
}
