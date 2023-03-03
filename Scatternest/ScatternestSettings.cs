using Newtonsoft.Json;
using Scatternest.ExclusionPresets;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Scatternest
{
    public class ScatternestSettings
    {
        public bool Enabled { get; set; } = false;

        public int StartCount { get; set; } = 2;

        public HashSet<string> DisabledStarts { get; set; } = new();

        public HashSet<string> ExplicitlyEnabledStarts { get; set; } = new();

        [JsonIgnore] public StartPresetGenerator DelayedPreset { get; set; } = new EmptyPreset();
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
        [JsonIgnore] public bool AnyStartsModified => DisabledStarts.Count > 0 || DelayedPreset is not EmptyPreset || ExplicitlyEnabledStarts.Count > 0;

        public ScatternestSettings Clone() => (ScatternestSettings)MemberwiseClone();
    }
}
