using System;

namespace Kethane.VesselModules
{
	// Token: 0x0200000F RID: 15
	public class KethaneBattery : IKethaneBattery
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000058 RID: 88 RVA: 0x000034DC File Offset: 0x000016DC
		// (set) Token: 0x06000059 RID: 89 RVA: 0x000034E4 File Offset: 0x000016E4
		public uint flightID { get; set; }

		// Token: 0x0600005A RID: 90 RVA: 0x000034ED File Offset: 0x000016ED
		public KethaneBattery(PartResource res)
		{
			this.partResource = res;
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600005B RID: 91 RVA: 0x000034FC File Offset: 0x000016FC
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00003509 File Offset: 0x00001709
		public double amount
		{
			get
			{
				return this.partResource.amount;
			}
			set
			{
				this.partResource.amount = value;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003517 File Offset: 0x00001717
		public double maxAmount
		{
			get
			{
				return this.partResource.maxAmount;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600005E RID: 94 RVA: 0x00003524 File Offset: 0x00001724
		public bool flowState
		{
			get
			{
				return this.partResource.flowState;
			}
		}

		// Token: 0x04000022 RID: 34
		private PartResource partResource;
	}
}
