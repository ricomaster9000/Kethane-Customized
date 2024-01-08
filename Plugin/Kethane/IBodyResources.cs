using System;
using GeodesicGrid;

namespace Kethane
{
	// Token: 0x02000004 RID: 4
	public interface IBodyResources
	{
		// Token: 0x06000009 RID: 9
		ConfigNode Save();

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000A RID: 10
		double MaxQuantity { get; }

		// Token: 0x0600000B RID: 11
		double? GetQuantity(Cell cell);

		// Token: 0x0600000C RID: 12
		double Extract(Cell cell, double amount);
	}
}
