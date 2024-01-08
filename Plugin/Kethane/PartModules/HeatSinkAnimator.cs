using System;
using System.Linq;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x02000024 RID: 36
	public class HeatSinkAnimator : PartModule, IMultipleDragCube
	{
		// Token: 0x060000FA RID: 250 RVA: 0x0000726A File Offset: 0x0000546A
		private void FindAnimations()
		{
			this.deployAnimationName = this.OpenAnimation;
			this.openAnimationStates = base.part.SetUpAnimation(this.OpenAnimation);
			this.heatAnimationStates = base.part.SetUpAnimation(this.HeatAnimation);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x000072A6 File Offset: 0x000054A6
		public override void OnStart(PartModule.StartState state)
		{
			this.FindAnimations();
		}

		// Token: 0x060000FC RID: 252 RVA: 0x000072B0 File Offset: 0x000054B0
		public override void OnUpdate()
		{
			int num = 525;
			float normalizedTime = (this.temperature - (float)num) / (this.MaxTemperature - (float)num);
			AnimationState[] array = this.heatAnimationStates;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].normalizedTime = normalizedTime;
			}
			bool flag = this.temperature >= this.OpenTemperature;
			foreach (AnimationState animationState in this.openAnimationStates)
			{
				animationState.normalizedTime = Mathf.Clamp01(animationState.normalizedTime);
			}
			AnimationState animationState2 = this.openAnimationStates.First<AnimationState>();
			if ((animationState2.normalizedTime <= 0f || animationState2.normalizedTime >= 1f) && animationState2.normalizedTime == 1f != flag)
			{
				array = this.openAnimationStates;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].speed = (float)(flag ? 1 : -1);
				}
			}
		}

		// Token: 0x060000FD RID: 253 RVA: 0x000073A8 File Offset: 0x000055A8
		public override void OnFixedUpdate()
		{
			Vector3 position = base.part.transform.position;
			double externalTemperature = FlightGlobals.getExternalTemperature((double)FlightGlobals.getAltitudeAtPos(position), FlightGlobals.getMainBody());
			double staticPressure = FlightGlobals.getStaticPressure(position);
			Vector3 srfVelocity = base.vessel.GetSrfVelocity();
			Vector3 vector = base.part.transform.InverseTransformDirection(this.RadiatorNormal);
			float magnitude = (srfVelocity - Vector3.Dot(srfVelocity, vector) * vector).magnitude;
			float num = Mathf.Clamp01(this.openAnimationStates.First<AnimationState>().normalizedTime);
			double num2 = (double)this.InternalDissipation + (double)num * ((double)this.HeatSinkDissipation + staticPressure * (double)(this.PressureDissipation + this.AirSpeedDissipation * magnitude));
			this.SetDragState(num);
			this.temperature = (float)(externalTemperature + ((double)this.temperature - externalTemperature) * Math.Exp(-num2 * (double)TimeWarp.fixedDeltaTime));
			this.CoolingEfficiency = ((this.requested == 0f) ? 1f : (this.dissipated / this.requested));
			this.lastRequested = this.requested;
			this.requested = (this.dissipated = 0f);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x000074D4 File Offset: 0x000056D4
		public float AddHeat(float heat)
		{
			this.requested += heat;
			float num = this.MaxTemperature - (this.temperature - this.dissipated);
			float num2 = (this.lastRequested == 0f) ? 1f : Math.Min(heat / this.lastRequested, 1f);
			heat = Math.Min(heat, num * num2);
			this.temperature += heat;
			this.dissipated += heat;
			return heat;
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00007553 File Offset: 0x00005753
		private void SetDragState(float t)
		{
			base.part.DragCubes.SetCubeWeight("A", t);
			base.part.DragCubes.SetCubeWeight("B", 1f - t);
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00007587 File Offset: 0x00005787
		public string[] GetDragCubeNames()
		{
			return new string[]
			{
				"A",
				"B"
			};
		}

		// Token: 0x06000101 RID: 257 RVA: 0x000075A0 File Offset: 0x000057A0
		public void AssumeDragCubePosition(string name)
		{
			this.FindAnimations();
			float normalizedTime = 0f;
			if (!(name == "A"))
			{
				if (name == "B")
				{
					normalizedTime = 0f;
				}
			}
			else
			{
				normalizedTime = 1f;
			}
			AnimationState[] array = this.openAnimationStates;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].normalizedTime = normalizedTime;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000102 RID: 258 RVA: 0x00007601 File Offset: 0x00005801
		public bool IsMultipleCubesActive
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00007601 File Offset: 0x00005801
		public bool UsesProceduralDragCubes()
		{
			return false;
		}

		// Token: 0x0400006D RID: 109
		[KSPField(isPersistant = false)]
		public string HeatAnimation;

		// Token: 0x0400006E RID: 110
		[KSPField(isPersistant = false)]
		public string OpenAnimation;

		// Token: 0x0400006F RID: 111
		public string deployAnimationName;

		// Token: 0x04000070 RID: 112
		[KSPField(isPersistant = false)]
		public float OpenTemperature;

		// Token: 0x04000071 RID: 113
		[KSPField(isPersistant = false)]
		public float MaxTemperature;

		// Token: 0x04000072 RID: 114
		[KSPField(isPersistant = false)]
		public float InternalDissipation;

		// Token: 0x04000073 RID: 115
		[KSPField(isPersistant = false)]
		public float HeatSinkDissipation;

		// Token: 0x04000074 RID: 116
		[KSPField(isPersistant = false)]
		public float PressureDissipation;

		// Token: 0x04000075 RID: 117
		[KSPField(isPersistant = false)]
		public float AirSpeedDissipation;

		// Token: 0x04000076 RID: 118
		[KSPField(isPersistant = false)]
		public Vector3 RadiatorNormal;

		// Token: 0x04000077 RID: 119
		private AnimationState[] heatAnimationStates;

		// Token: 0x04000078 RID: 120
		private AnimationState[] openAnimationStates;

		// Token: 0x04000079 RID: 121
		private float temperature;

		// Token: 0x0400007A RID: 122
		private float requested;

		// Token: 0x0400007B RID: 123
		private float lastRequested;

		// Token: 0x0400007C RID: 124
		private float dissipated;

		// Token: 0x0400007D RID: 125
		[KSPField(guiName = "Cooling Efficiency", isPersistant = false, guiActive = true, guiFormat = "P1")]
		public float CoolingEfficiency;
	}
}
