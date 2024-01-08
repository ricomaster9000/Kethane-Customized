using System;

namespace Kethane.PartModules
{
	// Token: 0x02000027 RID: 39
	public interface IExtractorAnimator
	{
		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000107 RID: 263
		ExtractorState CurrentState { get; }

		// Token: 0x06000108 RID: 264
		void Deploy();

		// Token: 0x06000109 RID: 265
		void Retract();
	}
}
