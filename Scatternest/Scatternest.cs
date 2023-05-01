using ItemChanger;
using Modding;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerMod.IC;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using Scatternest.Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SD = ItemChanger.Util.SceneDataUtil;

namespace Scatternest
{
    public class Scatternest : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static Scatternest instance;

        public static ScatternestSettings SET => GS.MenuSettings;

        public static GlobalSettings GS { get; private set; } = new();
        GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal() => GS;
        void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings gs) => GS = gs;

        public static void ResetMenuSettings()
        {
            if (!GS.RememberScatternestSettings)
            {
                GS.MenuSettings = new();
            }
        }

        public Scatternest() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            DebugInterop.Hook();
            StartSelector.Instance.Hook();

            RCData.RuntimeLogicOverride.Subscribe(0f, AddResolver);
            RandoController.OnCalculateHash += ModifyHash;
            RandoController.OnExportCompleted += OnExportCompleted;
            SettingsLog.AfterLogSettings += LogScatternestSettings;
            RandoMenuPage.Hook();
            StartSelectionPage.Hook();

            if (ItemSyncUtil.ItemSyncInstalled())
            {
                HookItemSync();
            }

            if (ModHooks.GetMod("RandoSettingsManager") is not null)
            {
                RandoSettingsManagerInterop.Hook();
            }
        }

        private void AddResolver(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!SET.Enabled) return;

            VariableResolver inner = lmb.VariableResolver;

            lmb.VariableResolver = new ScatternestVariableResolver() { Inner = inner };
        }

        private void LogScatternestSettings(LogArguments args, TextWriter tw)
        {
            if (!SET.Enabled)
            {
                tw.WriteLine("Scatternest settings: Disabled");
                return;
            }
            tw.WriteLine($"Scatternest settings: {SET.StartCount} starts selected");
            string[] starts = args.gs.StartLocationSettings.StartLocation.Split('|');

            for (int i = 1; i < starts.Length - 1; i++)
            {
                tw.WriteLine($"- {starts[i]}");
            }
        }

        private int ModifyHash(RandoController rc, int value)
        {
            if (!SET.Enabled) return 0;

            int startLocationModifier = rc.gs.StartLocationSettings.StartLocation.GetStableHashCode();

            int excludedStartsModifier;
            if (SET.AnyStartsModified)
            {
                excludedStartsModifier = string.Join(", ", SET.DisabledStarts.OrderBy(x => x, StringComparer.InvariantCulture)).GetStableHashCode();
            }
            else
            {
                excludedStartsModifier = 0;
            }

            int ret;
            unchecked
            {
                ret = startLocationModifier + 163 * excludedStartsModifier;
            }

            return ret;
        }

        private void OnExportCompleted(RandoController rc)
        {
            if (!SET.Enabled) return;

            ScatternestInteropModule mod = ItemChangerMod.Modules.Add<ScatternestInteropModule>();
            mod.Settings = SET.Clone();

            if (rc.gs.StartLocationSettings.StartLocation.Contains("|Hive|"))
            {
                ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 134f, Test = PlatformList.lacksRightClaw });
                ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Hive_03, X = 58.5f, Y = 138.5f, Test = PlatformList.lacksAnyVertical });
            }

            if (rc.gs.StartLocationSettings.StartLocation.Contains("|Far Greenpath|"))
            {
                ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 45f, Y = 16.5f, Test = PlatformList.lacksLeftClaw });
                ItemChangerMod.AddDeployer(new SmallPlatform { SceneName = SceneNames.Fungus1_13, X = 64f, Y = 16.5f, Test = PlatformList.lacksRightClaw });
                SD.Save(SceneNames.Fungus1_13, "Vine Platform (1)");
                SD.Save(SceneNames.Fungus1_13, "Vine Platform (2)");
            }

            if (rc.gs.StartLocationSettings.StartLocation.Contains("|Lower Greenpath|"))
            {
                if (rc.gs.NoveltySettings.RandomizeNail) SD.Save(SceneNames.Fungus1_13, "Vine Platform");
            }
        }

        public void HookItemSync()
        {
            RandoController.OnExportCompleted += SelectStartIndex;
        }

        private void SelectStartIndex(RandoController rc)
        {
            if (!SET.AddedStarts) return;
            if (!ItemSyncUtil.IsItemSync()) return;

            // Select a consistent ordering of the players

            List<int> indices = Enumerable.Range(0, ItemSyncUtil.PlayerCount())
                .Select(x => x % SET.StartCount)
                .ToList();

            Random rng = new(rc.gs.Seed + 163);
            rng.PermuteInPlace(indices);

            int playerIndex = indices[ItemSyncUtil.PlayerID()];

            if (MultiItemchangerStart.Instance is MultiItemchangerStart start)
            {
                start.SetPrimaryIndex(playerIndex);
            }
        }
    }
}