using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x02000034 RID: 52
	internal static class PartExtensions
	{
		// Token: 0x06000171 RID: 369 RVA: 0x000098F4 File Offset: 0x00007AF4
		public static void GetConnectedResourceTotals(this Part part, string resourceName, out double amount,
			out double maxAmount, bool pulling = true)
		{
			var resourceDef = PartResourceLibrary.Instance.GetDefinition(resourceName);
			part.GetConnectedResourceTotals(resourceDef.id, resourceDef.resourceFlowMode, out amount, out maxAmount,
				pulling);
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00009924 File Offset: 0x00007B24
		public static AnimationState[] SetUpAnimation(this Part part, string animationName)
		{
			var states = new List<AnimationState>();
			foreach (var animation in part.FindModelAnimators(animationName))
			{
				var animationState = animation[animationName];
				animationState.speed = 0;
				animationState.enabled = true;
				animationState.wrapMode = WrapMode.ClampForever;
				animation.Blend(animationName);
				states.Add(animationState);
			}

			return states.ToArray();
		}
	}
}
