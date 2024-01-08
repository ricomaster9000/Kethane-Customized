using System;
using System.Linq;

namespace Kethane.PartModules
{
	// Token: 0x02000032 RID: 50
	public class KethaneWetMassIndicator : PartModule
	{
		// Token: 0x0600016D RID: 365 RVA: 0x000097A4 File Offset: 0x000079A4
		public override string GetInfo()
		{
			return string.Format("{0}: {1}", this.Label ?? "Wet Mass", (float)base.part.Resources.Cast<PartResource>().Sum((PartResource r) => r.maxAmount * (double)PartResourceLibrary.Instance.GetDefinition(r.resourceName).density) + base.part.mass);
		}

		// Token: 0x040000CA RID: 202
		[KSPField(isPersistant = false)]
		public string Label;
	}
}
