using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;

namespace Scatternest
{
    public class RandoMenuPage
    {
        internal MenuPage SnPage;
        internal MenuElementFactory<GlobalSettings> snMEF;
        internal VerticalItemPanel snVIP;

        internal SmallButton JumpToSnButton;
        internal static RandoMenuPage Instance { get; private set; }

        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance.JumpToSnButton;
            return true;
        }

        private void SetTopLevelButtonColor()
        {
            if (JumpToSnButton != null)
            {
                JumpToSnButton.Text.color = Scatternest.GS.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private RandoMenuPage(MenuPage landingPage)
        {
            SnPage = new MenuPage(Localize("Scatternest"), landingPage);
            snMEF = new(SnPage, Scatternest.GS);
            snVIP = new(SnPage, new(0, 300), 75f, true, snMEF.Elements);
            Localize(snMEF);

            foreach (IValueElement e in snMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            JumpToSnButton = new(landingPage, Localize("Scatternest"));
            JumpToSnButton.AddHideAndShowEvent(landingPage, SnPage);
            SetTopLevelButtonColor();
        }
    }
}
