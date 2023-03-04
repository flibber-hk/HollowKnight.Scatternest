using MenuChanger;
using MenuChanger.MenuElements;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scatternest.Menu
{
    public class ColouredMenuButton<T> : MenuItem<T> where T : Enum
    {
        public ColouredMenuButton(MenuPage page, string name, Dictionary<T, Color> colorMap) : base(
            page, 
            name, 
            ((T[])Enum.GetValues(typeof(T))).ToList(),
            new ToggleButtonFormatter()
            )
        {
            this.ValueChanged += v =>
            {
                Text.color = colorMap[v];
            };
        }
    }
}
