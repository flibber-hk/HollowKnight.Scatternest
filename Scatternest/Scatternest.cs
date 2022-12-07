using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;

namespace Scatternest
{
    public class Scatternest : Mod
    {
        internal static Scatternest instance;
        
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
            
            
        }
    }
}