using System;
using System.Linq;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x0200002C RID: 44
	public class KethaneDrillAnimator : PartModule, IExtractorAnimator
	{
		// Token: 0x06000138 RID: 312 RVA: 0x0000830C File Offset: 0x0000650C
		public override void OnStart(PartModule.StartState state)
		{
			deployStates = this.part.SetUpAnimation(DeployAnimation);
			drillStates = this.part.SetUpAnimation(DrillAnimation);

			if (CurrentState == ExtractorState.Deploying) { CurrentState = ExtractorState.Retracted; }
			else if (CurrentState == ExtractorState.Retracting) { CurrentState = ExtractorState.Deployed; }

			if (CurrentState == ExtractorState.Deployed)
			{
				foreach (var deployState in deployStates)
				{
					deployState.normalizedTime = 1;
				}
			}

			foreach (var drillState in drillStates)
			{
				drillState.enabled = false;
				drillState.wrapMode = WrapMode.Loop;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000139 RID: 313 RVA: 0x000083B8 File Offset: 0x000065B8
		// (set) Token: 0x0600013A RID: 314 RVA: 0x00008404 File Offset: 0x00006604
		public ExtractorState CurrentState
		{
			get
			{
				ExtractorState result;
				try
				{
					result = (ExtractorState)Enum.Parse(typeof(ExtractorState), this.State);
				}
				catch
				{
					this.CurrentState = ExtractorState.Retracted;
					result = this.CurrentState;
				}
				return result;
			}
			private set
			{
				this.State = Enum.GetName(typeof(ExtractorState), value);
			}
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00008421 File Offset: 0x00006621
		public void Deploy()
		{
			if (this.CurrentState != ExtractorState.Retracted)
			{
				return;
			}
			this.CurrentState = ExtractorState.Deploying;
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00008434 File Offset: 0x00006634
		public void Retract()
		{
			if (this.CurrentState != ExtractorState.Deployed)
			{
				return;
			}
			this.CurrentState = ExtractorState.Retracting;
			foreach (AnimationState animationState in this.drillStates)
			{
				animationState.enabled = false;
				animationState.normalizedTime = 0f;
				animationState.speed = 0f;
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00008488 File Offset: 0x00006688
		public void Update()
		{
			if (this.CurrentState == ExtractorState.Deploying)
			{
				if (this.deployStates.All((AnimationState s) => s.normalizedTime >= 1f))
				{
					this.CurrentState = ExtractorState.Deployed;
					foreach (AnimationState animationState in this.drillStates)
					{
						animationState.enabled = true;
						animationState.normalizedTime = 0f;
						animationState.speed = 1f;
					}
					goto IL_AE;
				}
			}
			if (this.CurrentState == ExtractorState.Retracting)
			{
				if (this.deployStates.All((AnimationState s) => s.normalizedTime <= 0f))
				{
					this.CurrentState = ExtractorState.Retracted;
				}
			}
			IL_AE:
			foreach (AnimationState animationState2 in this.deployStates)
			{
				float num = Mathf.Clamp01(animationState2.normalizedTime);
				animationState2.normalizedTime = num;
				float num2 = HighLogic.LoadedSceneIsEditor ? (1f - 10f * (num - 1f) * num) : 1f;
				animationState2.speed = ((this.CurrentState == ExtractorState.Deploying || this.CurrentState == ExtractorState.Deployed) ? num2 : (-num2));
			}
		}

		// Token: 0x0400009F RID: 159
		[KSPField(isPersistant = false)]
		public string DeployAnimation;

		// Token: 0x040000A0 RID: 160
		[KSPField(isPersistant = false)]
		public string DrillAnimation;

		// Token: 0x040000A1 RID: 161
		[KSPField(isPersistant = true)]
		public string State;

		// Token: 0x040000A2 RID: 162
		private AnimationState[] deployStates;

		// Token: 0x040000A3 RID: 163
		private AnimationState[] drillStates;
	}
}
