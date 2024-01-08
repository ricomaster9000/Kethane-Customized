using System;
using System.Collections.Generic;
using System.Linq;
using Kethane.PartModules;

namespace Kethane.VesselModules
{
	// Token: 0x02000011 RID: 17
	public class KethaneProtoDetector
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00003586 File Offset: 0x00001786
		// (set) Token: 0x06000067 RID: 103 RVA: 0x0000358E File Offset: 0x0000178E
		public bool IsDetecting
		{
			get
			{
				return this.isDetecting;
			}
			set
			{
				this.isDetecting = value;
				this.vesselScanner.UpdateDetecting();
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000035A4 File Offset: 0x000017A4
		public KethaneProtoDetector(KethaneDetector det, KethaneVesselScanner scanner)
		{
			this.vesselScanner = scanner;
			this.DetectingPeriod = det.DetectingPeriod;
			this.DetectingHeight = det.DetectingHeight;
			this.PowerConsumption = det.PowerConsumption;
			this.resources = det.resources;
			this.IsDetecting = det.IsDetecting;
			this.TimerEcho = 0.0;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x0000360C File Offset: 0x0000180C
		public KethaneProtoDetector(ConfigNode node, KethaneVesselScanner scanner)
		{
			this.vesselScanner = scanner;
			if (node.HasValue("DetectingPeriod"))
			{
				float.TryParse(node.GetValue("DetectingPeriod"), out this.DetectingPeriod);
			}
			if (node.HasValue("DetectingHeight"))
			{
				float.TryParse(node.GetValue("DetectingHeight"), out this.DetectingHeight);
			}
			if (node.HasValue("PowerConsumption"))
			{
				float.TryParse(node.GetValue("PowerConsumption"), out this.PowerConsumption);
			}
			if (node.HasValue("IsDetecting"))
			{
				bool flag;
				bool.TryParse(node.GetValue("IsDetecting"), out flag);
				this.IsDetecting = flag;
			}
			if (node.HasValue("TimerEcho"))
			{
				double.TryParse(node.GetValue("TimerEcho"), out this.TimerEcho);
			}
			this.resources = (from n in node.GetNodes("Resource")
			select n.GetValue("Name")).ToList<string>();
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00003718 File Offset: 0x00001918
		public void Save(ConfigNode node)
		{
			ConfigNode configNode = node.AddNode("Detector");
			configNode.AddValue("DetectingPeriod", this.DetectingPeriod);
			configNode.AddValue("DetectingHeight", this.DetectingHeight);
			configNode.AddValue("PowerConsumption", this.PowerConsumption);
			configNode.AddValue("IsDetecting", this.IsDetecting);
			configNode.AddValue("TimerEcho", this.TimerEcho);
			foreach (string text in this.resources)
			{
				configNode.AddNode("Resource").AddValue("Name", text);
			}
		}

		// Token: 0x04000025 RID: 37
		public float DetectingPeriod;

		// Token: 0x04000026 RID: 38
		public float DetectingHeight;

		// Token: 0x04000027 RID: 39
		public float PowerConsumption;

		// Token: 0x04000028 RID: 40
		public List<string> resources;

		// Token: 0x04000029 RID: 41
		private KethaneVesselScanner vesselScanner;

		// Token: 0x0400002A RID: 42
		private bool isDetecting;

		// Token: 0x0400002B RID: 43
		public double TimerEcho;

		// Token: 0x0400002C RID: 44
		public double powerRatio;
	}
}
