using System;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x0200002B RID: 43
	public class KethaneDetectorAnimatorUnity : PartModule, IDetectorAnimator
	{
		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000131 RID: 305 RVA: 0x00008208 File Offset: 0x00006408
		// (set) Token: 0x06000132 RID: 306 RVA: 0x00008210 File Offset: 0x00006410
		public bool IsDetecting { private get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000133 RID: 307 RVA: 0x00008219 File Offset: 0x00006419
		// (set) Token: 0x06000134 RID: 308 RVA: 0x00008221 File Offset: 0x00006421
		public float PowerRatio { private get; set; }

		// Token: 0x06000135 RID: 309 RVA: 0x0000822C File Offset: 0x0000642C
		public override void OnStart(PartModule.StartState state)
		{
			deployStates = this.part.SetUpAnimation(DeployAnimation);
			runningStates = this.part.SetUpAnimation(RunningAnimation);

			foreach (var runningState in runningStates)
			{
				runningState.wrapMode = WrapMode.Loop;
			}
		}

		// Token: 0x06000136 RID: 310 RVA: 0x00008288 File Offset: 0x00006488
		public override void OnUpdate()
		{
			foreach (AnimationState animationState in this.deployStates)
			{
				animationState.normalizedTime = Mathf.Clamp01(animationState.normalizedTime);
				animationState.speed = (this.IsDetecting ? this.PowerRatio : -1f);
			}
			AnimationState[] array = this.runningStates;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].speed = (this.IsDetecting ? this.PowerRatio : 0f);
			}
		}

		// Token: 0x0400009B RID: 155
		[KSPField(isPersistant = false)]
		public string DeployAnimation;

		// Token: 0x0400009C RID: 156
		[KSPField(isPersistant = false)]
		public string RunningAnimation;

		// Token: 0x0400009D RID: 157
		private AnimationState[] deployStates;

		// Token: 0x0400009E RID: 158
		private AnimationState[] runningStates;
	}
}
