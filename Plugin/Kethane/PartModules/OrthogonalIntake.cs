using System;
using System.Linq;

namespace Kethane.PartModules
{
	// Token: 0x02000033 RID: 51
	public class OrthogonalIntake : PartModule
	{
		// Token: 0x0600016F RID: 367 RVA: 0x00009810 File Offset: 0x00007A10
		public void FixedUpdate()
		{
			if (!HighLogic.LoadedSceneIsFlight)
			{
				return;
			}
			double num = (double)PartResourceLibrary.Instance.GetDefinition(this.Resource).density;
			double atmDensity = base.part.vessel.atmDensity;
			double num2 = (double)this.BaseFlowRate;
			ModuleEngines moduleEngines = base.part.Modules.OfType<ModuleEngines>().Single<ModuleEngines>();
			double num3 = (double)(moduleEngines.finalThrust / moduleEngines.maxThrust);
			num2 += num3 * (double)this.PowerFlowRate;
			double magnitude = base.part.vessel.srf_velocity.magnitude;
			num2 += magnitude * (double)this.SpeedFlowRate;
			num2 = Math.Max(num2, 0.0);
			this.AirFlow = (float)(num2 * 1000.0);
			num2 *= (double)TimeWarp.fixedDeltaTime;
			num2 *= atmDensity / num;
			base.part.RequestResource(this.Resource, -num2);
		}

		// Token: 0x040000CB RID: 203
		[KSPField(isPersistant = false)]
		public string Resource;

		// Token: 0x040000CC RID: 204
		[KSPField(isPersistant = false)]
		public float BaseFlowRate;

		// Token: 0x040000CD RID: 205
		[KSPField(isPersistant = false)]
		public float PowerFlowRate;

		// Token: 0x040000CE RID: 206
		[KSPField(isPersistant = false)]
		public float SpeedFlowRate;

		// Token: 0x040000CF RID: 207
		[KSPField(guiName = "Intake Flow Rate", guiUnits = "L/s", isPersistant = false, guiActive = true, guiFormat = "F1")]
		public float AirFlow;
	}
}
