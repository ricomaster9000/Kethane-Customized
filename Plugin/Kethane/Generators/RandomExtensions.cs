using System;

namespace Kethane.Generators
{
	// Token: 0x02000038 RID: 56
	internal static class RandomExtensions
	{
		// Token: 0x0600017C RID: 380 RVA: 0x00009B84 File Offset: 0x00007D84
		public static float Range(this Random random, float min, float max)
		{
			return (float)random.Range((double)min, (double)max);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00009B91 File Offset: 0x00007D91
		public static double Range(this Random random, double min, double max)
		{
			return random.NextDouble() * (max - min) + min;
		}
	}
}
