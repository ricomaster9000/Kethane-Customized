using System;

namespace Kethane.PartModules
{
	// Token: 0x02000025 RID: 37
	public interface IDetectorAnimator
	{
		// Token: 0x17000020 RID: 32
		// (set) Token: 0x06000105 RID: 261
		bool IsDetecting { set; }

		// Token: 0x17000021 RID: 33
		// (set) Token: 0x06000106 RID: 262
		float PowerRatio { set; }
	}
}
