using Newtonsoft.Json;
using Scatternest.ExclusionPresets;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Scatternest
{
    public class ScatternestSettings
    {
        public bool Enabled = false;

        public int StartCount = 2;

        public HashSet<string> DisabledStarts { get; set; } = new();

        [JsonIgnore] public StartPresetGenerator DelayedPreset = new EmptyPreset();
        [JsonProperty] private string _delayedPresetName;

        [OnSerializing] private void SetPresetName(StreamingContext _) => _delayedPresetName = DelayedPreset.DisplayName;
        [OnDeserialized] private void SetDelayedPreset(StreamingContext _)
        {
            if (StartPresetGenerator.CreateDict().TryGetValue(_delayedPresetName, out StartPresetGenerator gen))
            {
                DelayedPreset = gen;
                return;
            }

            Scatternest.instance.LogWarn($"Did not recognize {nameof(StartPresetGenerator)}: {_delayedPresetName}");
            DelayedPreset = new EmptyPreset();
        }


        [JsonIgnore] public bool AddedStarts => Enabled && StartCount > 1;
        [JsonIgnore] public bool AnyStartsDisabled => DisabledStarts.Count > 0 || DelayedPreset is not EmptyPreset;
    }
}
