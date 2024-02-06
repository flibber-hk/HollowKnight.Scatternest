using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC;
using RandomizerMod.RC.LogicInts;

namespace Scatternest
{
    // Copy of StartLocationDelta, but checks for "gs.Start == ?" replaced with "gs.Start.Contains(|?|)"
    public class MultiStartLocationDelta : StartLocationDelta
    {
        public MultiStartLocationDelta(StartLocationDelta template, LogicManager lm) : base(template.Name, lm, template.Location) { }

        public override StateUnion GetInputState(object sender, ProgressionManager pm)
        {
            string settingsLoc = ((RandoModContext)pm.ctx).GenerationSettings.StartLocationSettings.StartLocation;

            return settingsLoc.Contains($"|{this.Location}|")
                ? StartStateTerm is not null
                ? pm.GetState(StartStateTerm)
                : StateUnion.Empty
                : null;
        }
    }

    public class ScatternestVariableResolver : VariableResolver
    {
        public override bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (!StartLocationDelta.TryMatch(lm, term, out variable))
            {
                return base.TryMatch(lm, term, out variable);
            }

            if (variable is not StartLocationDelta delta)
            {
                return base.TryMatch(lm, term, out variable);
            }

            variable = new MultiStartLocationDelta(delta, lm);
            return true;
        }

    }
}
