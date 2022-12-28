using Modding;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using StartResolver = RandomizerMod.RC.RequestBuilder.StartResolver;
using SROwner = RandomizerMod.PriorityEvent<RandomizerMod.RC.RequestBuilder.StartResolver>.IPriorityEventOwner;

namespace Scatternest
{
    public class StartSelector
    {
        private static StartSelector _instance;
        public static StartSelector Instance => _instance ??= new();

        public void Hook()
        {
            RequestBuilder.OnSelectStart.Subscribe(float.MinValue, SelectStarts);
        }

        public bool SelectStarts(Random rng, GenerationSettings gs, SettingsPM pm, out StartDef def)
        {
            if (!Scatternest.SET.Enabled)
            {
                def = default;
                return false;
            }

            if (Scatternest.SET.DelayedPreset != null)
            {
                Dictionary<string, StartDef> startDict = RandomizerMenuAPI.GenerateStartLocationDict()
                    .Where(kvp => !Scatternest.SET.DisabledStarts.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                HashSet<string> newExcludedStarts = Scatternest.SET.DelayedPreset.CreateExclusionList(startDict, gs, pm, rng);
                Scatternest.SET.DisabledStarts.UnionWith(newExcludedStarts);
            }


            List<StartDef> collectedStartDefs = new();

            void RemoveSelectedStarts(Dictionary<string, StartDef> startDefs)
            {
                foreach (string startName in collectedStartDefs.Select(x => x.Name).ToList())
                {
                    if (collectedStartDefs.Contains(startDefs[startName]))
                    {
                        startDefs.Remove(startName);
                    }
                }
            }


            RandomizerMenuAPI.OnGenerateStartLocationDict += RemoveSelectedStarts;
            RandomizerMenuAPI.OnGenerateStartLocationDict += RemoveExcludedStarts;
            // Try-Finally just to be safe
            try
            {
                def = SelectStartsInternal(rng, gs, pm, collectedStartDefs);
            }
            catch (IndexOutOfRangeException e)
            {
                throw new InvalidOperationException("Not enough available starts", e);
            }
            finally
            {
                RandomizerMenuAPI.OnGenerateStartLocationDict -= RemoveSelectedStarts;
                RandomizerMenuAPI.OnGenerateStartLocationDict -= RemoveExcludedStarts;
            }

            return true;
        }

        private void RemoveExcludedStarts(Dictionary<string, StartDef> startDefs)
        {
            foreach (string excluded in Scatternest.SET.DisabledStarts)
            {
                if (!startDefs.Remove(excluded))
                {
                    Scatternest.instance.Log($"Tried to remove a non-existent start {excluded}");
                }
            }
        }

        private StartDef SelectStartsInternal(Random rng, GenerationSettings gs, SettingsPM pm, List<StartDef> collectedStartDefs)
        {
            StartDef def;
            SROwner owner = ReflectionHelper.GetField<SROwner>(typeof(RequestBuilder), "_onSelectStartOwner");

            List<StartResolver> resolvers = owner.GetSubscribers().Where(r => !ReferenceEquals(r.Target, this)).ToList();

            collectedStartDefs.Add(SelectSingleStart(resolvers, rng, gs, pm));

            if (gs.StartLocationSettings.StartLocationType == StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                gs.StartLocationSettings.StartLocationType = StartLocationSettings.RandomizeStartLocationType.Random;
            }

            while (collectedStartDefs.Count < Scatternest.SET.StartCount)
            {
                StartDef candidate = SelectSingleStart(resolvers, rng, gs, pm);
                if (!collectedStartDefs.Contains(candidate))
                {
                    collectedStartDefs.Add(candidate);
                }
            }

            def = new MultiRandoStart(collectedStartDefs.ToList());

            string startString = string.Join("|", collectedStartDefs.Select(x => x.Name).ToArray());
            gs.StartLocationSettings.StartLocation = $"MultiStart<|{startString}|>";
            return def;
        }

        private StartDef SelectSingleStart(List<StartResolver> resolvers, Random rng, GenerationSettings gs, SettingsPM pm)
        {
            foreach (StartResolver resolver in resolvers)
            {
                if (resolver?.Invoke(rng, gs, pm, out StartDef def) ?? false)
                {
                    return def;
                }
            }

            throw new InvalidOperationException("No resolver successfully selected a start!");
        }
    }
}
