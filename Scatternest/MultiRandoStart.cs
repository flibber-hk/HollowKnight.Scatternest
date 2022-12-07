using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scatternest
{
    internal record MultiRandoStart : StartDef
    {
        public List<StartDef> InnerDefs;

        public MultiRandoStart(List<StartDef> innerDefs) : base(innerDefs[0])
        {
            InnerDefs = innerDefs;
        }

        public override IEnumerable<TermValue> GetStartLocationProgression(LogicManager lm)
            => InnerDefs.SelectMany(def => def.GetStartLocationProgression(lm));

        public override ItemChanger.StartDef ToItemChangerStartDef()
        {
            return new MultiItemchangerStart(InnerDefs.Select(def => def.ToItemChangerStartDef()).ToList())
            {
                InnerStartNames = InnerDefs.Select(def => def.Name).ToList()
            };
        }
    }
}
