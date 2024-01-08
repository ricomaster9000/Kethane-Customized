using System;

namespace Kethane.PartModules
{
	// Token: 0x02000030 RID: 48
	public class KethaneKerbalBlender : PartModule
	{
		// Token: 0x0600015F RID: 351 RVA: 0x00009550 File Offset: 0x00007750
		[KSPEvent(guiName = "Blend Kerbal", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void ConsumeKerbal()
		{
			Vessel activeVessel = FlightGlobals.ActiveVessel;
			if (!activeVessel.isEVA)
			{
				return;
			}
			if (activeVessel.GetVesselCrew()[0].isBadass)
			{
				activeVessel.rootPart.explosionPotential = 10000f;
			}
			FlightGlobals.ForceSetActiveVessel(base.vessel);
			activeVessel.rootPart.explode();
			base.part.RequestResource("Kethane", -150.0);
		}
	}
}
