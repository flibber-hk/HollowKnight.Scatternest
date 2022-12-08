using ItemChanger;
using Modding;
using RandomizerCore.Extensions;
using RandomizerMod.IC;
using RandomizerMod.Logging;
using RandomizerMod.RC;
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

        public static GlobalSettings GS = new();
        public GlobalSettings OnSaveGlobal() => GS;
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;

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

            RequestBuilder.OnSelectStart.Subscribe(float.MinValue, StartSelector.Instance.SelectStarts);
            RandoController.OnCalculateHash += ModifyHash;
            RandoController.OnExportCompleted += AddDeployers;
            SettingsLog.AfterLogSettings += LogScatternestSettings;
            RandoMenuPage.Hook();

            if (ItemSyncUtil.ItemSyncInstalled())
            {
                HookItemSync();
            }
        }

        private void LogScatternestSettings(LogArguments args, TextWriter tw)
        {
            if (!GS.Enabled)
            {
                tw.WriteLine("Scatternest settings: Disabled");
                return;
            }
            tw.WriteLine($"Scatternest settings: {GS.StartCount} starts selected");
            string[] starts = args.gs.StartLocationSettings.StartLocation.Split('|');

            for (int i = 1; i < starts.Length - 1; i++)
            {
                tw.WriteLine($"- {starts[i]}");
            }
        }

        private int ModifyHash(RandoController rc, int value)
        {
            if (!GS.Enabled || GS.StartCount < 2) return 0;

            return rc.gs.StartLocationSettings.StartLocation.GetStableHashCode();
        }

        private void AddDeployers(RandoController rc)
        {
            if (!GS.Enabled) return;

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
            if (!GS.Enabled || GS.StartCount <= 2) return;
            if (!ItemSyncUtil.IsItemSync()) return;

            // Select a consistent ordering of the players

            List<int> indices = Enumerable.Range(0, ItemSyncUtil.PlayerCount())
                .Select(x => x % GS.StartCount)
                .ToList();

            Random rng = new(rc.gs.Seed + 163);
            rng.PermuteInPlace(indices);

            int playerIndex = indices[ItemSyncUtil.PlayerID()];

            if (MultiItemchangerStart.Instance is MultiItemchangerStart start)
            {
                start.Index = playerIndex;
                start.PrimaryIndex = playerIndex;
            }
        }
    }
}