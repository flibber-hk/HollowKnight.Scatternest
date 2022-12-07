using ItemChanger;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Scatternest
{
    internal class MultiItemchangerStart : StartDef
    {
        [JsonProperty] private int _index;
        public List<StartDef> InnerDefs;

        private StartDef ActiveStart => InnerDefs[_index];

        [JsonIgnore] public int Index
        {
            get => _index;
            set
            {
                _index = value;
                ApplyToPlayerData(PlayerData.instance);
            }
        }

        public MultiItemchangerStart(List<StartDef> innerDefs) : this(innerDefs, 0) { }

        public MultiItemchangerStart(List<StartDef> innerDefs, int index)
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
