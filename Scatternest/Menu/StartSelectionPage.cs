using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using System.Linq;
using static RandomizerMod.Localization;

namespace Scatternest.Menu
{
    internal class StartSelectionPage
    {
        private const string Random = "Random";

        internal static StartSelectionPage Instance { get; private set; }
        internal BigButton JumpToSSButton;

        internal MenuPage SSMenuPage;

        internal RadioSwitch ssSwitch;
        internal VerticalItemPanel ssVIP;
        private string[] _starts;


        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddStartGameOverride(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        public void ResetRadioSwitch(RandoController rc)
        {
            Scatternest.PrimaryStartName = null;

            if (rc.ctx.StartDef is not MultiRandoStart mrs)
            {
                _starts = null;
                return;
            }

            _starts = mrs.InnerDefs.Select(s => s.Name).ToArray();
            string[]  _buttonNames = _starts.Prepend(Random).ToArray();

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

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private static bool HandleButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            Instance?.ResetRadioSwitch(rc);
            button = Instance?.JumpToSSButton;

            bool shouldShowButton = Instance?._starts is not null;

            if (!shouldShowButton) button?.Hide();
            else button?.Show();

            return shouldShowButton;
        }

        public StartSelectionPage(MenuPage parent)
        {
            SSMenuPage = new MenuPage(Localize("Scatternest Start Selection"), parent);

            ssVIP = new(SSMenuPage, new(0, 300), 50f, true);

            JumpToSSButton = new(parent, Localize("Start Selection"));
            JumpToSSButton.AddHideAndShowEvent(parent, SSMenuPage);
        }
    }
}
