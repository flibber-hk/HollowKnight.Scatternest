using RandomizerMod.RandomizerData;
using System;
using System.Collections.Generic;
using System.Linq;
using Scatternest.Util;
using RandomizerCore.Extensions;
using RandomizerMod.Settings;
using System.ComponentModel;

namespace Scatternest.ExclusionPresets
{
    /// <summary>
    /// Base class to generate start exclusions for the menu.
    /// It is allowed to assume that:
    /// * Non-base starts are never excluded
    /// * Base starts behave as they do in base rando wrt access
    /// * In some cases, that no transitions are randomized
    /// </summary>
    public abstract class StartPresetGenerator
    {
        public string DisplayName { get; protected set; }
        public StartPresetGenerator()
        {
            DisplayName = GetType().Name.FromCamelCase();
        }

        public static Dictionary<string, StartPresetGenerator> CreateDict()
        {
            List<StartPresetGenerator> generators = new()
            {
                null,
                new ExcludeEquivalentStarts(),
                new ExcludeSimilarStarts(),
                new ExcludeItemRandoStarts(),
                new ExcludeRestrictedStarts(),
            };

            return generators.ToDictionary(x => x?.DisplayName ?? "None");
        }

        protected static HashSet<string> GetAvailableStarts(Dictionary<string, StartDef> data, GenerationSettings gs, SettingsPM pm)
        {
            bool IsAvailable(StartDef def) => pm is null ? true : def.CanBeRandomized(pm);
            return new(data.Where(kvp => IsAvailable(kvp.Value)).Select(kvp => kvp.Key));
        }

        public abstract HashSet<string> CreateExclusionList(Dictionary<string, StartDef> data, GenerationSettings gs, SettingsPM pm, Random rng);
    }

    public class ExcludeEquivalentStarts : StartPresetGenerator
    {
        public override HashSet<string> CreateExclusionList(Dictionary<string, StartDef> data, GenerationSettings gs, SettingsPM pm, Random rng)
        {
            HashSet<string> available = GetAvailableStarts(data, gs, pm);

            List<string> crossroads = new HashSet<string>()
            {
                StartNames.WestCrossroads,
                StartNames.EastCrossroads,
                StartNames.AncestralMound
            }.Where(x => available.Contains(x)).OrderBy(x => x, StringComparer.InvariantCulture).ToList();

            List<string> greenpath = new HashSet<string>()
            {
                StartNames.Greenpath,
                StartNames.QueensStation,
                StartNames.WestFogCanyon
            }.Where(x => available.Contains(x)).OrderBy(x => x, StringComparer.InvariantCulture).ToList();

            HashSet<string> excluded = new();

            excluded.UnionWith(crossroads.AllButOne(rng));
            excluded.UnionWith(greenpath.AllButOne(rng));

            return excluded;
        }
    }

    public class ExcludeSimilarStarts : StartPresetGenerator
    {
        public override HashSet<string> CreateExclusionList(Dictionary<string, StartDef> data, GenerationSettings gs, SettingsPM pm, Random rng)
        {
            HashSet<string> available = GetAvailableStarts(data, gs, pm);

            List<string> crossroads = new HashSet<string>()
            {
                StartNames.WestCrossroads,
                StartNames.EastCrossroads,
                StartNames.AncestralMound,
                StartNames.KingsPass,
                StartNames.EastBlueLake
            }.Where(x => available.Contains(x)).OrderBy(x => x, StringComparer.InvariantCulture).ToList();

            List<string> greenpath = new HashSet<string>()
            {
                StartNames.Greenpath,
                StartNames.QueensStation,
                StartNames.WestFogCanyon,
                StartNames.EastFogCanyon,
                StartNames.LowerGreenpath
            }.Where(x => available.Contains(x)).OrderBy(x => x, StringComparer.InvariantCulture).ToList();

            HashSet<string> excluded = new();

            excluded.UnionWith(crossroads.AllButOne(rng));
            excluded.UnionWith(greenpath.AllButOne(rng));

            return excluded;
        }
    }

    /// <summary>
    /// Exclude starts that are available in item rando with the default settings. (To be used in transition rando)
    /// </summary>
    public class ExcludeItemRandoStarts : StartPresetGenerator
    {
        public override HashSet<string> CreateExclusionList(Dictionary<string, StartDef> data, GenerationSettings gs, SettingsPM pm, Random rng)
        {
            return new()
            {
                StartNames.KingsPass,
                StartNames.StagNest,
                StartNames.WestCrossroads,
                StartNames.EastCrossroads,
                StartNames.AncestralMound,
                StartNames.WestFogCanyon,
                StartNames.EastFogCanyon,
                StartNames.QueensStation,
                StartNames.FungalWastes,
                StartNames.Greenpath,
                StartNames.LowerGreenpath,
                StartNames.WestBlueLake,
                StartNames.CityStorerooms,
                StartNames.KingsStation,
                StartNames.OutsideColosseum,
                StartNames.CrystallizedMound,
            };
        }
    }

    /// <summary>
    /// Exclude starts which might not lead to much access.
    /// In transition rando, exclude starts with only one randomized transition and limited access
    /// </summary>
    public class ExcludeRestrictedStarts : StartPresetGenerator
    {
        public override HashSet<string> CreateExclusionList(Dictionary<string, StartDef> data, GenerationSettings gs, SettingsPM pm, Random rng)
        {
            switch (gs.TransitionSettings.Mode)
            {
                case TransitionSettings.TransitionMode.None:
                    return new()
                    {
                        StartNames.CrystallizedMound,
                        StartNames.CityStorerooms,
                    };
                case TransitionSettings.TransitionMode.MapAreaRandomizer:
                    return new()
                    {
                        StartNames.KingsPass,
                        StartNames.EastCrossroads,
                        StartNames.FarGreenpath,
                        StartNames.WestBlueLake,
                        StartNames.CrystallizedMound,
                        StartNames.WestWaterways,
                    };
                case TransitionSettings.TransitionMode.FullAreaRandomizer:
                    return new()
                    {
                        StartNames.KingsPass,
                        StartNames.StagNest,
                        StartNames.EastCrossroads,
                        StartNames.AncestralMound,
                        StartNames.FarGreenpath,
                        StartNames.WestBlueLake,
                        StartNames.EastBlueLake,
                        StartNames.CrystallizedMound,
                        StartNames.WestWaterways,
                    };
                case TransitionSettings.TransitionMode.RoomRandomizer:
                    return new()
                    {
                        StartNames.AncestralMound,
                        StartNames.LowerGreenpath,
                        StartNames.WestFogCanyon,
                        StartNames.FarGreenpath,
                        StartNames.WestBlueLake,
                        StartNames.EastBlueLake,
                        StartNames.CrystallizedMound,
                        StartNames.RoyalWaterways,
                        StartNames.DistantVillage,
                        StartNames.CityofTears,
                        StartNames.KingdomsEdge,
                        StartNames.MantisVillage,
                        StartNames.WestWaterways,
                    };
            }

            return new();
        }
    }
}
