﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kethane
{
    public class KethaneKerbalBlender : PartModule
    {
        [KSPEvent(guiName = "Blend Kerbal", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void ConsumeKerbal()
        {
            var vessel = FlightGlobals.ActiveVessel;
            var kerbal = vessel.GetComponent<KerbalEVA>();
            if (kerbal.name == "kerbalEVA (Jebediah Kerman)")
            {
                vessel.rootPart.explosionPotential = 10000;
            }
            FlightGlobals.ForceSetActiveVessel(this.vessel);
            vessel.rootPart.explode();
            this.part.RequestResource("Kethane", -150);
        }
    }
}
