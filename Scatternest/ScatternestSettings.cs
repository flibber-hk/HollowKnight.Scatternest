using Newtonsoft.Json;
using System.Collections.Generic;

namespace Scatternest
{
    public class ScatternestSettings
    {
        public bool Enabled = false;

        public int StartCount = 2;

        public HashSet<string> DisabledStarts = new();

        [JsonIgnore] public bool AddedStarts => Enabled && StartCount > 1;
    }
}
