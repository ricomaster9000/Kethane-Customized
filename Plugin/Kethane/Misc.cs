using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kethane
{
	// Token: 0x02000008 RID: 8
	internal static class Misc
	{
		// Token: 0x0600001E RID: 30 RVA: 0x00002508 File Offset: 0x00000708
		public static float Parse(string s, float defaultValue)
		{
			float result;
			if (!float.TryParse(s, out result))
			{
				result = defaultValue;
			}
			return result;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002524 File Offset: 0x00000724
		public static double Parse(string s, double defaultValue)
		{
			double result;
			if (!double.TryParse(s, out result))
			{
				result = defaultValue;
			}
			return result;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002540 File Offset: 0x00000740
		public static int Parse(string s, int defaultValue)
		{
			int result;
			if (!int.TryParse(s, out result))
			{
				result = defaultValue;
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000255C File Offset: 0x0000075C
		public static bool Parse(string s, bool defaultValue)
		{
			bool result;
			if (!bool.TryParse(s, out result))
			{
				result = defaultValue;
			}
			return result;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002578 File Offset: 0x00000778
		public static Vector3 Parse(string s, Vector3 defaultValue)
		{
			Vector3 result;
			try
			{
				result = ConfigNode.ParseVector3(s);
			}
			catch
			{
				result = defaultValue;
			}
			return result;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x000025A4 File Offset: 0x000007A4
		public static Color32 Parse(string s, Color32 defaultValue)
		{
			if (s == null)
			{
				return defaultValue;
			}
			return ConfigNode.ParseColor32(s);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000025B4 File Offset: 0x000007B4
		public static ConfigNode Parse(string s)
		{
			string[] array = s.Split(new char[]
			{
				'\n',
				'\r'
			});
			object obj = Misc.PreFormatConfig.Invoke(null, new object[]
			{
				array
			});
			return (ConfigNode)Misc.RecurseFormat.Invoke(null, new object[]
			{
				obj
			});
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002608 File Offset: 0x00000808
		public static ParticleSystemRenderMode Parse(string s, ParticleSystemRenderMode defaultValue)
		{
			ParticleSystemRenderMode result;
			try
			{
				result = (ParticleSystemRenderMode)Enum.Parse(typeof(ParticleSystemRenderMode), s);
			}
			catch
			{
				result = defaultValue;
			}
			return result;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002644 File Offset: 0x00000844
		public static byte[] FromBase64String(string encoded)
		{
			return Convert.FromBase64String(encoded.Replace('.', '/').Replace('%', '='));
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000265E File Offset: 0x0000085E
		public static string ToBase64String(byte[] data)
		{
			return Convert.ToBase64String(data).Replace('/', '.').Replace('=', '%');
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002678 File Offset: 0x00000878
		public static double getTrueAltitude(this Vessel vessel)
		{
			Vector3 coM = vessel.CoM;
			Vector3 vector = (coM - vessel.mainBody.position).normalized;
			double altitude = vessel.mainBody.GetAltitude(coM);
			RaycastHit raycastHit = default;
			double result;
			if (Physics.Raycast(coM, -vector, out raycastHit, (float)altitude + 10000f, 32768))
			{
				result = (double)raycastHit.distance;
			}
			else if (vessel.mainBody.pqsController != null)
			{
				result = vessel.mainBody.GetAltitude(coM) - (vessel.mainBody.pqsController.GetSurfaceHeight(QuaternionD.AngleAxis(vessel.mainBody.GetLongitude(coM, false), Vector3d.down) * QuaternionD.AngleAxis(vessel.mainBody.GetLatitude(coM, false), Vector3d.forward) * Vector3d.right) - vessel.mainBody.pqsController.radius);
			}
			else
			{
				result = vessel.mainBody.GetAltitude(coM);
			}
			return result;
		}

		// Token: 0x04000008 RID: 8
		private static MethodInfo PreFormatConfig = (from m in typeof(ConfigNode).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
		where m.Name == "PreFormatConfig" && m.GetParameters().Length == 1
		select m).FirstOrDefault<MethodInfo>();

		// Token: 0x04000009 RID: 9
		private static MethodInfo RecurseFormat = (from m in typeof(ConfigNode).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
		where m.Name == "RecurseFormat" && m.GetParameters().Length == 1
		select m).FirstOrDefault<MethodInfo>();
	}
}
