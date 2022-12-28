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
using Scatternest.ExclusionPresets;

namespace Scatternest
{
    public class RandoMenuPage
    {
        private readonly System.Random rng = new();

        internal MenuPage SnPage;
        internal MenuElementFactory<ScatternestSettings> snMEF;
        internal VerticalItemPanel snVIP;

        internal SmallButton JumpToSnButton;

        internal MenuPage StartLocationExclusionPage;
        internal ToggleButton[] StartLocationButtons;
        internal IMenuPanel StartLocationPanel;
        internal MenuItem<string> ApplyPresetNow;
        internal MenuItem<string> ApplyPresetLater;
        internal SmallButton SelectAll;
        internal SmallButton DeselectAll;
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
                JumpToSleButton.Text.color = Scatternest.SET.AnyStartsDisabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private ToggleButton MakeStartToggle(string startName, MenuPage owner)
        {
            ToggleButton button = new(owner, startName);
            
            {
                bool val = Scatternest.SET.DisabledStarts.Contains(startName);
                button.SetValue(val);
                if (val) button.SetColor(Color.Lerp(Color.white, Color.red, 0.5f));
            }
            
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
            ApplyPresetLater.SetValue(settings.DelayedPreset.DisplayName);

            UpdateStartLocationExclusionSelector();
        }

        private RandoMenuPage(MenuPage landingPage)
        {
            // Clear settings when constructing the menu
            Scatternest.SET = new();

            SnPage = new MenuPage(Localize("Scatternest"), landingPage);

            StartLocationExclusionPage = new MenuPage(Localize("Excluded Starts"), SnPage);
            
            {
                Dictionary<string, StartPresetGenerator> generators = StartPresetGenerator.CreateDict();

                ApplyPresetNow = new(StartLocationExclusionPage, "Apply Preset Now", generators.Select(kvp => kvp.Key).ToArray());
                ApplyPresetNow.ValueChanged += s =>
                {
                    Scatternest.SET.DisabledStarts.Clear();
                    StartPresetGenerator gen = generators[s];

                    HashSet<string> newlyDisabled = gen is null ? new() : gen.CreateExclusionList(
                        RandomizerMenuAPI.GenerateStartLocationDict(),
                        RandomizerMod.RandomizerMod.GS.DefaultMenuSettings,
                        null,
                        rng);
                    Scatternest.SET.DisabledStarts.UnionWith(newlyDisabled);
                    UpdateStartLocationExclusionSelector();
                };

                ApplyPresetLater = new(StartLocationExclusionPage, "Apply Preset Later", generators.Select(kvp => kvp.Key).ToArray());
                ApplyPresetLater.SetValue(Scatternest.SET.DelayedPreset.DisplayName);
                ApplyPresetLater.ValueChanged += s =>
                {
                    Scatternest.SET.DelayedPreset = generators[s];
                };

                SelectAll = new(StartLocationExclusionPage, "Disable All");
                SelectAll.OnClick += () =>
                {
                    Scatternest.SET.DisabledStarts.UnionWith(RandomizerMenuAPI.GenerateStartLocationDict().Keys);
                    UpdateStartLocationExclusionSelector();
                };

                DeselectAll = new(StartLocationExclusionPage, "Allow All");
                DeselectAll.OnClick += () =>
                {
                    Scatternest.SET.DisabledStarts.Clear();
                    UpdateStartLocationExclusionSelector();
                };

                ApplyPresetNow.MoveTo(new(-300, 400));
                ApplyPresetLater.MoveTo(new(-300, 350));
                SelectAll.MoveTo(new(400, 400));
                DeselectAll.MoveTo(new(400, 350));
                
            }
            JumpToSleButton = new(SnPage, Localize("Excluded Starts"));
            JumpToSleButton.AddHideAndShowEvent(SnPage, StartLocationExclusionPage);
            UpdateStartLocationExclusionSelector();


            snMEF = new(SnPage, Scatternest.SET);
            snVIP = new(SnPage, new(0, 300), 75f, true, snMEF.Elements);
            snVIP.Add(JumpToSleButton);
            SnPage.ResetNavigation();
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
