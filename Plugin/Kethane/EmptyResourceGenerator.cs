using System;
using GeodesicGrid;

namespace Kethane
{
	// Token: 0x02000005 RID: 5
	internal class EmptyResourceGenerator : IResourceGenerator
	{
		// Token: 0x0600000D RID: 13 RVA: 0x00002168 File Offset: 0x00000368
		public EmptyResourceGenerator()
		{
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002168 File Offset: 0x00000368
		public EmptyResourceGenerator(ConfigNode node)
		{
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002170 File Offset: 0x00000370
		public IBodyResources Load(CelestialBody body, ConfigNode node)
		{
			return EmptyResourceGenerator.bodyResources;
		}

		// Token: 0x04000003 RID: 3
		private static readonly IBodyResources bodyResources = new EmptyResourceGenerator.BodyResources();

		// Token: 0x02000039 RID: 57
		private class BodyResources : IBodyResources
		{
			// Token: 0x17000030 RID: 48
			// (get) Token: 0x0600017E RID: 382 RVA: 0x00009B9F File Offset: 0x00007D9F
			public double MaxQuantity
			{
				get
				{
					return 0.0;
				}
			}

			// Token: 0x0600017F RID: 383 RVA: 0x00009BAA File Offset: 0x00007DAA
			public ConfigNode Save()
			{
				return new ConfigNode();
			}

			// Token: 0x06000180 RID: 384 RVA: 0x00009BB4 File Offset: 0x00007DB4
			public double? GetQuantity(Cell cell)
			{
				return null;
			}

			// Token: 0x06000181 RID: 385 RVA: 0x00009BCA File Offset: 0x00007DCA
			public double Extract(Cell cell, double amount)
			{
				throw new Exception("No deposit here");
			}
		}
	}
}
