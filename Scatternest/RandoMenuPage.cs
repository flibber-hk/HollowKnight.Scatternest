using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using static RandomizerMod.Localization;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Scatternest
{
    public class RandoMenuPage
    {
        internal MenuPage SnPage;
        internal MenuElementFactory<ScatternestSettings> snMEF;
        internal VerticalItemPanel snVIP;

        internal SmallButton JumpToSnButton;

        internal MenuPage StartLocationExclusionPage;
        internal ToggleButton[] StartLocationButtons;
        internal IMenuPanel StartLocationPanel;
        internal SmallButton JumpToSleButton;

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

        private void SetTopLevelButtonColors()
        {
            if (JumpToSnButton != null)
            {
                JumpToSnButton.Text.color = Scatternest.SET.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
            if (JumpToSleButton != null)
            {
                JumpToSleButton.Text.color = Scatternest.SET.DisabledStarts.Count > 0 ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private ToggleButton MakeStartToggle(string startName, MenuPage owner)
        {
            ToggleButton button = new(owner, startName);
            button.SetValue(Scatternest.SET.DisabledStarts.Contains(startName));
            button.ValueChanged += value =>
            {
                if (value) Scatternest.SET.DisabledStarts.Add(startName);
                else Scatternest.SET.DisabledStarts.Remove(startName);

                if (value) button.SetColor(Color.Lerp(Color.white, Color.red, 0.5f));

                SetTopLevelButtonColors();
            };

            return button;
        }

        internal void UpdateStartLocationExclusionSelector()
        {
            StartLocationPanel?.Destroy();
            StartLocationPanel = null;


            Dictionary<string, StartDef> startDefs = RandomizerMenuAPI.GenerateStartLocationDict();

            StartLocationButtons = startDefs.Select(kvp => MakeStartToggle(kvp.Key, StartLocationExclusionPage)).ToArray();

            // Panel info lifted from RandomizerMenu
            if (startDefs.Count <= 33)
            {
                StartLocationPanel = new GridItemPanel(StartLocationExclusionPage, new Vector2(0, 150), 3, 50f, 600f, true, StartLocationButtons);
            }
            else
            {
                StartLocationPanel = new MultiGridItemPanel(
                    StartLocationExclusionPage, 3, 10, 50f, 600f, 
                    new Vector2(0, 150),
                    new(-600f, -350f),
                    new(0f, 350f),
                    new(600f, 350f), 
                    StartLocationButtons);
            }
        }

        internal void Apply(ScatternestSettings settings)
        {
            snMEF.SetMenuValues(settings);
            Scatternest.SET.DisabledStarts.Clear();
            Scatternest.SET.DisabledStarts.UnionWith(settings.DisabledStarts);

            UpdateStartLocationExclusionSelector();
        }

        private RandoMenuPage(MenuPage landingPage)
        {
            // Clear settings when constructing the menu
            Scatternest.SET = new();

            SnPage = new MenuPage(Localize("Scatternest"), landingPage);

            StartLocationExclusionPage = new MenuPage(Localize("Excluded Starts"), SnPage);
            JumpToSleButton = new(SnPage, Localize("Excluded Starts"));
            JumpToSleButton.AddHideAndShowEvent(SnPage, StartLocationExclusionPage);
            UpdateStartLocationExclusionSelector();


            snMEF = new(SnPage, Scatternest.SET);
            snVIP = new(SnPage, new(0, 300), 75f, true, snMEF.Elements);
            snVIP.Add(JumpToSleButton);
            snVIP.ResetNavigation();
            Localize(snMEF);

            foreach (IValueElement e in snMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColors();
            }


            JumpToSnButton = new(landingPage, Localize("Scatternest"));
            JumpToSnButton.AddHideAndShowEvent(landingPage, SnPage);
            SetTopLevelButtonColors();
        }
    }
}
