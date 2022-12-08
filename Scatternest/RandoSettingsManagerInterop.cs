using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace Scatternest
{
    internal static class RandoSettingsManagerInterop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new ScatternestSettingsProxy());
        }
    }

    internal class ScatternestSettingsProxy : RandoSettingsProxy<GlobalSettings, string>
    {
        public override string ModKey => Scatternest.instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new EqualityVersioningPolicy<string>(Scatternest.instance.GetVersion());

        public override void ReceiveSettings(GlobalSettings settings)
        {
            settings ??= new();
            RandoMenuPage.Instance.snMEF.SetMenuValues(settings);
        }

        public override bool TryProvideSettings(out GlobalSettings settings)
        {
            settings = Scatternest.GS;
            return settings.Enabled;
        }
    }
}