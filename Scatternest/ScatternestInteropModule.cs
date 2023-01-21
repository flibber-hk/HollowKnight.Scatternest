using ItemChanger.Modules;

namespace Scatternest
{
    public class ScatternestInteropModule : Module, IInteropModule
    {
        public string Message => "ScatternestData";

        public ScatternestSettings Settings { get; set; }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            switch (propertyName)
            {
                case "ScatternestSettings" when Settings is T ret:
                    value = ret;
                    return true;
                case "StartCount" when Settings.StartCount is T ret:
                    value = ret;
                    return true;
            }

            value = default;
            return false;
        }

        public override void Initialize() { }
        public override void Unload() { }
    }
}
