using ItemChanger;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Scatternest
{
    internal class MultiItemchangerStart : StartDef
    {
        public static MultiItemchangerStart Instance
        {
            get
            {
                if (ItemChanger.Internal.Ref.Settings.Start is MultiItemchangerStart start)
                {
                    return start;
                }
                return null;
            }
            
        }

        [JsonProperty] private int _index;
        public List<StartDef> InnerDefs;
        public List<string> InnerStartNames { get; init; }

        [JsonIgnore] public string Name => InnerStartNames[Index];

        private StartDef ActiveStart => InnerDefs[_index];

        [JsonIgnore] public int Index
        {
            get => _index;
            set
            {
                _index = value;
                if (PlayerData.instance != null)
                {
                    ApplyToPlayerData(PlayerData.instance);
                }
            }
        }

        [JsonProperty] public int PrimaryIndex { get; internal set; }

        public void CycleIndex() => Index = (Index + 1) % InnerDefs.Count;
        public void ResetIndex() => Index = PrimaryIndex;

        public MultiItemchangerStart(List<StartDef> innerDefs) : this(innerDefs, 0) { }

        [JsonConstructor] public MultiItemchangerStart(List<StartDef> innerDefs, int index)
        {
            _index = index;
            InnerDefs = innerDefs;
        }

        public override string SceneName => ActiveStart.SceneName;
        public override float X => ActiveStart.X;
        public override float Y => ActiveStart.Y;
        public override int MapZone => ActiveStart.MapZone;
        public override bool RespawnFacingRight => ActiveStart.RespawnFacingRight;
        public override SpecialStartEffects SpecialEffects => ActiveStart.SpecialEffects;
    }
}
