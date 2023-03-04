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

namespace Scatternest.Menu
{
    public class RandoMenuPage
    {
        private readonly System.Random rng = new();

        internal MenuPage SnPage;
        internal MenuElementFactory<ScatternestSettings> snMEF;
        internal VerticalItemPanel snVIP;

        internal SmallButton JumpToSnButton;

        internal MenuPage StartLocationExclusionPage;
        internal SmallButton[] StartLocationButtons;
        internal IMenuPanel StartLocationPanel;
        internal MenuItem<string> ApplyPresetNow;
        internal MenuItem<string> ApplyPresetOnStart;
        internal SmallButton SelectAll;
        internal SmallButton DeselectAll;
        internal ToggleButton AllowExplicitStarts;
        internal SmallButton JumpToSleButton;

        internal ToggleButton GlobalSettingsToggle;

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

        private void SetButtonColors()
        {
            if (JumpToSnButton != null)
            {
                JumpToSnButton.Text.color = Scatternest.SET.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
            if (JumpToSleButton != null)
            {
                JumpToSleButton.Text.color = Scatternest.SET.AnyStartsModified ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
            if (ApplyPresetOnStart != null)
            {
                ApplyPresetOnStart.Text.color = Scatternest.SET.DelayedPreset is not EmptyPreset ? Colors.TRUE_COLOR : Colors.FALSE_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private readonly Dictionary<StartState, Color> ColorMap = new()
        {
            [StartState.Normal] = Colors.FALSE_COLOR,
            [StartState.Disabled] = Color.Lerp(Color.white, Color.red, 0.5f),
            [StartState.Enabled] = Color.Lerp(Color.white, Color.green, 0.5f),
        };

        private SmallButton MakeStartToggle(string startName, MenuPage owner)
        {
            ColouredMenuButton<StartState> button = new(owner, startName, ColorMap);

            {
                StartState val =
                    Scatternest.SET.DisabledStarts.Contains(startName)
                    ? StartState.Disabled
                    : Scatternest.SET.ExplicitlyEnabledStarts.Contains(startName)
                        ? StartState.Enabled
                        : StartState.Normal;
                button.SetValue(val);
            }

            button.ValueChanged += value =>
            {
                Scatternest.SET.DisabledStarts.Remove(startName);
                Scatternest.SET.ExplicitlyEnabledStarts.Remove(startName);

                switch (value)
                {
                    case StartState.Disabled:
                        Scatternest.SET.DisabledStarts.Add(startName);
                        break;
                    case StartState.Enabled when Scatternest.SET.AllowExplictlyEnablingStarts:
                        Scatternest.SET.ExplicitlyEnabledStarts.Add(startName);
                        break;
                    case StartState.Enabled when !Scatternest.SET.AllowExplictlyEnablingStarts:
                        button.SetValue(StartState.Normal);
                        break;
                }

                SetButtonColors();
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

            SetButtonColors();
        }

        internal void Apply(ScatternestSettings settings)
        {
            if (settings is null)
            {
                snMEF.ElementLookup[nameof(ScatternestSettings.Enabled)].SetValue(false);
                return;
            }

            snMEF.SetMenuValues(settings);
            Scatternest.SET.DisabledStarts.Clear();
            Scatternest.SET.DisabledStarts.UnionWith(settings.DisabledStarts);
            Scatternest.SET.ExplicitlyEnabledStarts.Clear();
            Scatternest.SET.ExplicitlyEnabledStarts.UnionWith(settings.ExplicitlyEnabledStarts);
            ApplyPresetOnStart.SetValue(settings.DelayedPreset.DisplayName);

            UpdateStartLocationExclusionSelector();
        }

        private RandoMenuPage(MenuPage landingPage)
        {
            // Clear settings when constructing the menu
            Scatternest.ResetMenuSettings();

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
                        RandomizerMenuAPI.Menu.pm,
                        rng);
                    Scatternest.SET.DisabledStarts.UnionWith(newlyDisabled);
                    UpdateStartLocationExclusionSelector();
                };

                ApplyPresetOnStart = new(StartLocationExclusionPage, "Apply Preset Later", generators.Select(kvp => kvp.Key).ToArray());
                ApplyPresetOnStart.SetValue(Scatternest.SET.DelayedPreset.DisplayName);
                ApplyPresetOnStart.ValueChanged += s =>
                {
                    Scatternest.SET.DelayedPreset = generators[s];
                    SetButtonColors();
                };

                SelectAll = new(StartLocationExclusionPage, "Disable All");
                SelectAll.OnClick += () =>
                {
                    Scatternest.SET.DisabledStarts.UnionWith(RandomizerMenuAPI.GenerateStartLocationDict().Keys);
                    Scatternest.SET.ExplicitlyEnabledStarts.Clear();
                    UpdateStartLocationExclusionSelector();
                };

                DeselectAll = new(StartLocationExclusionPage, "Allow All");
                DeselectAll.OnClick += () =>
                {
                    Scatternest.SET.DisabledStarts.Clear();
                    Scatternest.SET.ExplicitlyEnabledStarts.Clear();
                    UpdateStartLocationExclusionSelector();
                };

                AllowExplicitStarts = new(StartLocationExclusionPage, "Allow Force Enable");
                AllowExplicitStarts.SetValue(Scatternest.SET.AllowExplictlyEnablingStarts);
                AllowExplicitStarts.ValueChanged += v =>
                {
                    Scatternest.SET.AllowExplictlyEnablingStarts = v;
                    if (!v) Scatternest.SET.ExplicitlyEnabledStarts.Clear();
                    UpdateStartLocationExclusionSelector();
                };

                ApplyPresetNow.MoveTo(new(-300, 400));
                ApplyPresetOnStart.MoveTo(new(-300, 350));
                SelectAll.MoveTo(new(400, 400));
                DeselectAll.MoveTo(new(400, 350));
                AllowExplicitStarts.MoveTo(new(600, StartLocationExclusionPage.backButton.GameObject.transform.localPosition.y));

            }
            JumpToSleButton = new(SnPage, Localize("Excluded Starts"));
            JumpToSleButton.AddHideAndShowEvent(SnPage, StartLocationExclusionPage);
            UpdateStartLocationExclusionSelector();


            snMEF = new(SnPage, Scatternest.SET);
            snVIP = new(SnPage, new(0, 300), 75f, true, snMEF.Elements);
            snVIP.Add(JumpToSleButton);


            // Create the GS toggle button above the back button
            {
                Vector2 back = SnPage.backButton.GameObject.transform.localPosition;
                Vector2 gsTogglePos = back + SpaceParameters.VSPACE_MEDIUM * Vector2.up;
                GlobalSettingsToggle = new(SnPage, Localize("Remember Settings"));
                GlobalSettingsToggle.SetValue(Scatternest.GS.RememberScatternestSettings);
                GlobalSettingsToggle.ValueChanged += b => Scatternest.GS.RememberScatternestSettings = b;
                GlobalSettingsToggle.MoveTo(gsTogglePos);
            }

            SnPage.ResetNavigation();
            Localize(snMEF);

            foreach (IValueElement e in snMEF.Elements)
            {
                e.SelfChanged += obj => SetButtonColors();
            }


            JumpToSnButton = new(landingPage, Localize("Scatternest"));
            JumpToSnButton.AddHideAndShowEvent(landingPage, SnPage);
            SetButtonColors();
        }
    }
}
