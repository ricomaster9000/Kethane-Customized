using System;
using System.Linq;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x0200002F RID: 47
	public class KethaneGenerator : PartModule
	{
		// Token: 0x06000155 RID: 341 RVA: 0x00009125 File Offset: 0x00007325
		[KSPEvent(guiName = "Enable Generator", guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void Enable()
		{
			this.Enabled = true;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x0000912E File Offset: 0x0000732E
		[KSPEvent(guiName = "Disable Generator", guiActive = true, active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void Disable()
		{
			this.Enabled = false;
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00009137 File Offset: 0x00007337
		[KSPAction("Enable Generator")]
		public void EnableAction(KSPActionParam param)
		{
			this.Enable();
		}

		// Token: 0x06000158 RID: 344 RVA: 0x0000913F File Offset: 0x0000733F
		[KSPAction("Disable Generator")]
		public void DisableAction(KSPActionParam param)
		{
			this.Disable();
		}

		// Token: 0x06000159 RID: 345 RVA: 0x00009147 File Offset: 0x00007347
		[KSPAction("Toggle Generator")]
		public void ToggleAction(KSPActionParam param)
		{
			this.Enabled = !this.Enabled;
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00009158 File Offset: 0x00007358
		public override void OnStart(PartModule.StartState state)
		{
			if (state == StartState.Editor)
			{
				return;
			}
			base.part.force_activate();
			this.fanStates = base.part.SetUpAnimation("generatorFan_anim");
			this.slatStates = base.part.SetUpAnimation("generatorSlats_anim");
			AnimationState[] array = this.fanStates;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].wrapMode = WrapMode.Loop;
			}
			this.exhaustEmitter = base.part.Modules.OfType<KethaneParticleEmitter>().First((KethaneParticleEmitter e) => e.Label == "exhaust");
		}

		// Token: 0x0600015B RID: 347 RVA: 0x000091FC File Offset: 0x000073FC
		public override void OnUpdate()
		{
			base.Events["Enable"].active = !this.Enabled;
			base.Events["Disable"].active = this.Enabled;
			this.exhaustEmitter.Emit = (this.Output > 0.0);
			AnimationState[] array = this.fanStates;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].speed = (float)this.fanSpeed.Average * 2f;
			}
			foreach (AnimationState animationState in this.slatStates)
			{
				animationState.normalizedTime = Mathf.Clamp01(animationState.normalizedTime);
				animationState.speed = (float)((this.Output > 0.0) ? -1 : 1);
			}
		}

		// Token: 0x0600015C RID: 348 RVA: 0x000092D4 File Offset: 0x000074D4
		public override void OnFixedUpdate()
		{
			double num;
			double num2;
			base.part.GetConnectedResourceTotals("ElectricCharge", out num, out num2, true);
			double num3 = this.Enabled ? this.logistic(num / num2) : 0.0;
			if (num3 < 0.10000000149011612)
			{
				num3 = 0.0;
			}
			double staticPressure = FlightGlobals.getStaticPressure(base.part.transform.position);
			this.fanSpeed.Update((double)TimeWarp.fixedDeltaTime, num3 * (2.0 * staticPressure) / (staticPressure * staticPressure + 1.0));
			float num4 = 0.5f;
			double num5 = num3 * (double)this.KethaneRate * (double)TimeWarp.fixedDeltaTime / (1.0 + this.fanSpeed.Average * staticPressure * (double)num4);
			double num6 = base.part.RequestResource("Kethane", num5);
			this.output.Update((double)TimeWarp.fixedDeltaTime, (num5 > 0.0) ? (num3 * num6 / num5) : 0.0);
			base.part.RequestResource("XenonGas", -num6 * (double)this.XenonMassRatio * (double)PartResourceLibrary.Instance.GetDefinition("Kethane").density / (double)PartResourceLibrary.Instance.GetDefinition("XenonGas").density);
			this.Output = this.output.Average;
			base.part.RequestResource("ElectricCharge", -this.Output * (double)this.PowerRate * (double)TimeWarp.fixedDeltaTime);
		}

		// Token: 0x0600015D RID: 349 RVA: 0x00009470 File Offset: 0x00007670
		public override string GetInfo()
		{
			return string.Format("Kethane Consumption: {0:F1}L/s\nPower Generation: {1:F1}/s\nXenonGas Byproduct: {2:F2}L/s", this.KethaneRate, this.PowerRate, this.KethaneRate * this.XenonMassRatio * PartResourceLibrary.Instance.GetDefinition("Kethane").density / PartResourceLibrary.Instance.GetDefinition("XenonGas").density);
		}

		// Token: 0x040000B6 RID: 182
		[KSPField(isPersistant = false)]
		public float KethaneRate;

		// Token: 0x040000B7 RID: 183
		[KSPField(isPersistant = false)]
		public float PowerRate;

		// Token: 0x040000B8 RID: 184
		[KSPField(isPersistant = false)]
		public float XenonMassRatio;

		// Token: 0x040000B9 RID: 185
		[KSPField(isPersistant = false)]
		public float MaxEmission;

		// Token: 0x040000BA RID: 186
		[KSPField(isPersistant = false)]
		public float MinEmission;

		// Token: 0x040000BB RID: 187
		[KSPField(guiName = "Output", isPersistant = false, guiActive = true, guiFormat = "P1")]
		public double Output;

		// Token: 0x040000BC RID: 188
		[KSPField(isPersistant = true)]
		public bool Enabled;

		// Token: 0x040000BD RID: 189
		private AnimationState[] fanStates;

		// Token: 0x040000BE RID: 190
		private AnimationState[] slatStates;

		// Token: 0x040000BF RID: 191
		private KethaneParticleEmitter exhaustEmitter;

		// Token: 0x040000C0 RID: 192
		private TimedMovingAverage output = new TimedMovingAverage(3.0, 0.0);

		// Token: 0x040000C1 RID: 193
		private TimedMovingAverage fanSpeed = new TimedMovingAverage(1.0, 0.0);

		// Token: 0x040000C2 RID: 194
		private Func<double, double> logistic = (double x) => 1.0 / (Math.Exp(15.0 * x - 10.5) + 1.0);
	}
}
