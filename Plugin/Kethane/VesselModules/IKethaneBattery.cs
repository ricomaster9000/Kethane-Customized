using System;

namespace Kethane.VesselModules
{
	// Token: 0x0200000E RID: 14
	public interface IKethaneBattery
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000052 RID: 82
		// (set) Token: 0x06000053 RID: 83
		uint flightID { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000054 RID: 84
		// (set) Token: 0x06000055 RID: 85
		double amount { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000056 RID: 86
		double maxAmount { get; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000057 RID: 87
		bool flowState { get; }
	}
}
