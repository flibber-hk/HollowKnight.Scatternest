using Modding;
using RandomizerMod;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using StartResolver = RandomizerMod.RC.RequestBuilder.StartResolver;

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
            if (!Scatternest.GS.Enabled || Scatternest.GS.StartCount <= 1)
            {
                def = default;
                return false;
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
            // Try-Finally just to be safe
            try
            {
                def = SelectStartsInternal(rng, gs, pm, collectedStartDefs);
            }
            finally
            {
                RandomizerMenuAPI.OnGenerateStartLocationDict -= RemoveSelectedStarts;
            }

            return true;
        }

        private StartDef SelectStartsInternal(Random rng, GenerationSettings gs, SettingsPM pm, List<StartDef> collectedStartDefs)
        {
            StartDef def;
            var owner = ReflectionHelper.GetField<PriorityEvent<StartResolver>.IPriorityEventOwner>(
                            typeof(RequestBuilder), "_onSelectStartOwner");

            List<StartResolver> resolvers = owner.GetSubscribers().Where(r => !ReferenceEquals(r.Target, this)).ToList();

            collectedStartDefs.Add(SelectSingleStart(resolvers, rng, gs, pm));

            if (gs.StartLocationSettings.StartLocationType == StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                gs.StartLocationSettings.StartLocationType = StartLocationSettings.RandomizeStartLocationType.Random;
            }

            while (collectedStartDefs.Count < Scatternest.GS.StartCount)
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
