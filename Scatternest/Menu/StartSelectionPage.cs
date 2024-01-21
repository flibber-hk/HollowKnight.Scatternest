using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerCore.Extensions;
using RandomizerMod.RC;
using System.Linq;
using UnityEngine;
using Random = System.Random;
using static RandomizerMod.Localization;

namespace Scatternest.Menu
{
    internal class StartSelectionPage
    {
        private const string Random = "Random";

        internal BigButton JumpToSSButton;

        internal MenuPage SSMenuPage;

        internal RadioSwitch ssSwitch;
        internal VerticalItemPanel ssVIP;
        private string[] _starts;

        private static Sprite ButtonSprite
        {
            get
            {
                char[] options = new[] { 'Y', 'B', 'R', 'W' };
                double[] weights = new[] { 1000d, 100d, 10d, 1d };
                Random rng = new();
                // char sel = rng.Next(options, weights);  // Does not work because of a bug in RC right now
                char sel = options[0];

                return ItemChanger.Internal.SpriteManager.Instance.GetSprite($"ShopIcons.Marker_{sel}");
            }
        }

        
        
        // { get; } = ItemChanger.Internal.SpriteManager.Instance.GetSprite("Marker_Y");

        public void ResetRadioSwitch(RandoModContext ctx)
        {
            Scatternest.PrimaryStartName = null;

            if (ctx?.StartDef is not MultiRandoStart mrs)
            {
                _starts = null;
                return;
            }

            _starts = mrs.InnerDefs.Select(s => s.Name).ToArray();
            string[] _buttonNames = _starts.Prepend(Random).ToArray();

            ssSwitch = new(SSMenuPage, _buttonNames);
            ssSwitch.Changed += s => Scatternest.PrimaryStartName = s == Random ? null : s;
            
            foreach (IMenuElement e in ssVIP.Items)
            {
                e.Destroy();
            }
            ssVIP.Items.Clear();

            foreach (ToggleButton e in ssSwitch.Elements)
            {
                ssVIP.Add(e);
            }
            ssVIP.Reposition();
        }

        internal bool HandleButton(RandoModContext ctx, out BaseButton button)
        {
            ResetRadioSwitch(ctx);
            button = JumpToSSButton;

            bool shouldShowButton = _starts is not null;

            if (!shouldShowButton) button?.Hide();
            else button?.Show();

            return shouldShowButton;
        }

        public StartSelectionPage(MenuPage parent)
        {
            SSMenuPage = new MenuPage(Localize("Scatternest Start Selection"), parent);

            ssVIP = new(SSMenuPage, new(0, 300), 50f, true);

            JumpToSSButton = new(parent, ButtonSprite, Localize("Start Selection"));
            JumpToSSButton.AddHideAndShowEvent(parent, SSMenuPage);
        }
    }
}
