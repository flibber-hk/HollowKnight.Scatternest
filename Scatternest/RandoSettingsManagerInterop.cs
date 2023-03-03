using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using Scatternest.Menu;

namespace Scatternest
{
    internal static class RandoSettingsManagerInterop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new ScatternestSettingsProxy());
        }
    }

    internal class ScatternestSettingsProxy : RandoSettingsProxy<ScatternestSettings, string>
    {
        public override string ModKey => Scatternest.instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new EqualityVersioningPolicy<string>(Scatternest.instance.GetVersion());

        public override void ReceiveSettings(ScatternestSettings settings)
        {
            RandoMenuPage.Instance.Apply(settings);
        }

        public override bool TryProvideSettings(out ScatternestSettings settings)
        {
            settings = Scatternest.SET;
            return settings.Enabled;
        }
    }
}