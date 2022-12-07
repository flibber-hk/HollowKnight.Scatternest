using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using RandomizerMod.RC;
using UnityEngine;

namespace Scatternest
{
    public class Scatternest : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static Scatternest instance;

        public static GlobalSettings GS = new();
        public GlobalSettings OnSaveGlobal() => GS;
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;

        public Scatternest() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            RequestBuilder.OnSelectStart.Subscribe(float.MinValue, StartSelector.Instance.SelectStarts);
        }
    }
}