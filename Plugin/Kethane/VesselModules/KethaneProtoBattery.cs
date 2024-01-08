using System;

namespace Kethane.VesselModules
{
	// Token: 0x02000010 RID: 16
	public class KethaneProtoBattery : IKethaneBattery
	{
		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00003531 File Offset: 0x00001731
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00003539 File Offset: 0x00001739
		public uint flightID { get; set; }

		// Token: 0x06000061 RID: 97 RVA: 0x00003542 File Offset: 0x00001742
		public KethaneProtoBattery(ProtoPartResourceSnapshot res)
		{
			this.protoResource = res;
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000062 RID: 98 RVA: 0x00003551 File Offset: 0x00001751
		// (set) Token: 0x06000063 RID: 99 RVA: 0x0000355E File Offset: 0x0000175E
		public double amount
		{
			get
			{
				return this.protoResource.amount;
			}
			set
			{
				this.protoResource.amount = value;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000064 RID: 100 RVA: 0x0000356C File Offset: 0x0000176C
		public double maxAmount
		{
			get
			{
				return this.protoResource.maxAmount;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00003579 File Offset: 0x00001779
		public bool flowState
		{
			get
			{
				return this.protoResource.flowState;
			}
		}

		// Token: 0x04000024 RID: 36
		private ProtoPartResourceSnapshot protoResource;
	}
}
