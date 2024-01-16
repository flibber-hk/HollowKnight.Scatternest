using ItemChangerDataLoader;
using MenuChanger.MenuElements;
using MenuChanger;
using System.Collections.Generic;

namespace Scatternest.Menu
{
    internal class ICDLInterop
    {
        internal static void HookICDL(Dictionary<MenuPage, StartSelectionPage> instances)
        {
            ICDLMenuAPI.AddStartGameOverride(
                page => instances[page] = new(page),
                (ICDLMenu.StartData data, MenuPage landingPage, out BaseButton button) =>
                {
                    StartSelectionPage.ICDLHash = data.Hash();

                    button = null;
                    return instances.TryGetValue(landingPage, out var instance) && instance.HandleButton(data.CTX, landingPage, out button);
                });
        }
    }
}
