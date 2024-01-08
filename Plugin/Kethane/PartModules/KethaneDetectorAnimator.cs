using System;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x0200002A RID: 42
	public class KethaneDetectorAnimator : PartModule, IDetectorAnimator
	{
		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000129 RID: 297 RVA: 0x00007FC4 File Offset: 0x000061C4
		// (set) Token: 0x0600012A RID: 298 RVA: 0x00007FCC File Offset: 0x000061CC
		public bool IsDetecting { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600012B RID: 299 RVA: 0x00007FD5 File Offset: 0x000061D5
		// (set) Token: 0x0600012C RID: 300 RVA: 0x00007FDD File Offset: 0x000061DD
		public float PowerRatio { get; set; }

		// Token: 0x0600012D RID: 301 RVA: 0x00007FE8 File Offset: 0x000061E8
		public override void OnUpdate()
		{
			CelestialBody mainBody = base.vessel.mainBody;
			if (mainBody == null)
			{
				return;
			}
			Transform transform = base.part.transform.Find("model");
			if (!string.IsNullOrEmpty(this.PartTransform))
			{
				transform = transform.Find(this.PartTransform);
			}
			transform = transform.Find(this.BaseTransform);
			Vector2 vector = KethaneDetectorAnimator.cartesianToPolar(transform.InverseTransformPoint(mainBody.transform.position));
			float num = (float)KethaneDetectorAnimator.normalizeAngle((double)(vector.x + 90f));
			float num2 = (float)KethaneDetectorAnimator.normalizeAngle((double)vector.y);
			Transform transform2 = transform.Find(this.HeadingTransform);
			Transform transform3 = transform2.Find(this.ElevationTransform);
			if (Math.Abs(transform2.localEulerAngles.y - num2) > 90f)
			{
				num2 += 180f;
				num = 360f - num;
			}
			float num3 = Time.deltaTime * this.PowerRatio * 60f;
			transform2.localRotation = Quaternion.RotateTowards(transform2.localRotation, Quaternion.AngleAxis(num2, new Vector3(0f, 1f, 0f)), num3);
			transform3.localRotation = Quaternion.RotateTowards(transform3.localRotation, Quaternion.AngleAxis(num, new Vector3(1f, 0f, 0f)), num3);
			if (float.IsNaN(transform2.localRotation.w))
			{
				transform2.localRotation = Quaternion.identity;
			}
			if (float.IsNaN(transform3.localRotation.w))
			{
				transform3.localRotation = Quaternion.identity;
			}
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00008176 File Offset: 0x00006376
		private static double normalizeAngle(double a)
		{
			a %= 360.0;
			if (a < 0.0)
			{
				a += 360.0;
			}
			return a;
		}

		// Token: 0x0600012F RID: 303 RVA: 0x000081A0 File Offset: 0x000063A0
		private static Vector2 cartesianToPolar(Vector3 point)
		{
			Vector2 vector = default(Vector2);
			vector.y = Mathf.Atan2(point.x, point.z);
			float magnitude = new Vector2(point.x, point.z).magnitude;
			vector.x = Mathf.Atan2(-point.y, magnitude);
			return vector * 57.29578f;
		}

		// Token: 0x04000093 RID: 147
		[KSPField(isPersistant = false)]
		public string BaseTransform;

		// Token: 0x04000094 RID: 148
		[KSPField(isPersistant = false)]
		public string PartTransform;

		// Token: 0x04000095 RID: 149
		[KSPField(isPersistant = false)]
		public string HeadingTransform;

		// Token: 0x04000096 RID: 150
		[KSPField(isPersistant = false)]
		public string ElevationTransform;
	}
}
