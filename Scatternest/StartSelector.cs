using Modding;
using RandomizerMod;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using static RandomizerMod.RC.RequestBuilder;

namespace Scatternest
{
    public class StartSelector
    {
        private static StartSelector _instance;
        public static StartSelector Instance => _instance ??= new();

        public bool SelectStarts(Random rng, GenerationSettings gs, SettingsPM pm, out StartDef def)
        {
            if (!Scatternest.GS.Enabled || Scatternest.GS.StartCount <= 1)
            {
                def = default;
                return false;
            }

            List<StartDef> collectedStartDefs = new();

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

            def = new MultiRandoStart(collectedStartDefs);

            string startString = string.Join(", ", collectedStartDefs.Select(x => x.Name).ToArray());
            gs.StartLocationSettings.StartLocation = $"MultiStart<{startString}>";

            return true;
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
