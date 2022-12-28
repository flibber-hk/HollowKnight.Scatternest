using Newtonsoft.Json;
using Scatternest.ExclusionPresets;
using System.Collections.Generic;

namespace Scatternest
{
    public class ScatternestSettings
    {
        public bool Enabled = false;

        public int StartCount = 2;

        public HashSet<string> DisabledStarts { get; set; } = new();

        public StartPresetGenerator DelayedPreset = null;


        [JsonIgnore] public bool AddedStarts => Enabled && StartCount > 1;
        [JsonIgnore] public bool AnyStartsDisabled => Enabled && (DisabledStarts.Count > 0 || DelayedPreset != null);
    }
}
