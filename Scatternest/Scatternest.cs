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
        public static string PrimaryStartName { get; set; } = null;

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

            if (PrimaryStartName is not null)
            {
                MultiItemchangerStart.Instance?.SetPrimaryIndex(PrimaryStartName);
            }

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
            ItemSyncUtil.HookPrimaryStart();
        }

        /// <summary>
        /// Generate a consistent mapping from player id to start name with the following properties:
        /// - Primary starts are respected
        /// - Start counts are as balanced as possible.
        /// 
        /// We do so by iteratively assigning random start players to the least popular start (breaking ties uniformly).
        /// </summary>
        private Dictionary<int, string> SelectPrimaryStarts(List<string> startNames, int seed)
        {
            Dictionary<int, string> assignments = new();
            Dictionary<string, int> assignmentCounts = new();
            foreach (string startName in startNames)
            {
                assignmentCounts[startName] = 0;
            }

            void assignStart(int playerId, string startName)
            {
                if (!assignmentCounts.ContainsKey(startName))
                {
                    LogWarn($"Could not assign start {startName} to player {playerId}: start missing");
                    return;
                }
                assignments[playerId] = startName;
                assignmentCounts[startName]++;
            }

            // Account for primary starts.
            Dictionary<int, string> primaryStarts = ItemSyncUtil.GetPrimaryStarts();

            foreach ((int playerId, string startName) in primaryStarts)
            {
                assignStart(playerId, startName);
            }

            // Assign all unassigned players in a random order.
            Random rng = new(seed);
            List<int> players = Enumerable.Range(0, ItemSyncUtil.PlayerCount()).ToList();
            List<string> permutedStarts = new(startNames);
            rng.PermuteInPlace(players);
            rng.PermuteInPlace(permutedStarts);

            foreach (int playerId in players)
            {
                if (assignments.ContainsKey(playerId)) continue;

                string nextStart = permutedStarts.OrderBy(x => assignmentCounts[x]).First();
                assignStart(playerId, nextStart);
            }

            return assignments;
        }

        private void SelectStartIndex(RandoController rc)
        {
            if (!SET.AddedStarts) return;
            if (!ItemSyncUtil.IsItemSync()) return;
            if (PrimaryStartName is not null) return;
            if (MultiItemchangerStart.Instance is not MultiItemchangerStart start) return;

            Dictionary<int, string> assignments = SelectPrimaryStarts(start.InnerStartNames, rc.gs.Seed + 163);
            start.SetPrimaryIndex(assignments[ItemSyncUtil.PlayerID()]);
        }
    }
}