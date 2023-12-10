using ItemChangerDataLoader;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using System.Linq;
using static RandomizerMod.Localization;

namespace Scatternest.Menu
{
    internal class StartSelectionPage
    {
        private static StartSelectionPage RandoInstance;
        private static StartSelectionPage IcdlInstance;

        private const string Random = "Random";

        internal BigButton JumpToSSButton;

        internal MenuPage SSMenuPage;

        internal RadioSwitch ssSwitch;
        internal VerticalItemPanel ssVIP;
        private string[] _starts;

        private static void OnExitMenu()
        {
            RandoInstance = null;
            IcdlInstance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddStartGameOverride(
                page => RandoInstance = new(page),
                (RandoController rc, MenuPage landingPage, out BaseButton button) =>
                {
                    button = null;
                    return RandoInstance?.HandleButton(rc.ctx, landingPage, out button) ?? false;
                });

            if (ModHooks.GetMod("ICDL Mod") is Mod) HookICDL();

            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static void HookICDL()
        {
            ICDLMenuAPI.AddStartGameOverride(
                page => IcdlInstance = new(page),
                (ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button) =>
                {
                    button = null;
                    return IcdlInstance?.HandleButton(data.CTX, landingPage, out button) ?? false;
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

            if (_starts is not null)
            {
                JumpToSSButton = new(landingPage, Localize("Start Selection"));
                JumpToSSButton.AddHideAndShowEvent(landingPage, SSMenuPage);
                JumpToSSButton.Show();

                button = JumpToSSButton;
                return true;
            }
            else
            {
                button = null;
                return false;
            }
        }

        public StartSelectionPage(MenuPage parent)
        {
            SSMenuPage = new MenuPage(Localize("Scatternest Start Selection"), parent);
            ssVIP = new(SSMenuPage, new(0, 300), 50f, true);
        }
    }
}
