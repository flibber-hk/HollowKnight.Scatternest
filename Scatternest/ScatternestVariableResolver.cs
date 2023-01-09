using RandomizerCore.Logic;
using RandomizerMod.RC;
using RandomizerMod.RC.LogicInts;

namespace Scatternest
{
    public class MultiStartLocationDelta : StartLocationDelta
    {
        public MultiStartLocationDelta(StartLocationDelta template, LogicManager lm) : base(template.Name, lm, template.Location) { }

        public override int GetValue(object sender, ProgressionManager pm)
        {
            string settingsLoc = ((RandoModContext)pm.ctx).GenerationSettings.StartLocationSettings.StartLocation;

            if (settingsLoc.Contains($"|{this.Location}|"))
            {
                return TRUE;
            }

            return base.GetValue(sender, pm);
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
