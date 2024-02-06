using System;
using System.Collections.Generic;
using System.Linq;
using Kethane.VesselModules;

namespace Kethane.PartModules
{
	// Token: 0x02000029 RID: 41
	public class KethaneDetector : PartModule
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600011B RID: 283 RVA: 0x00007C8C File Offset: 0x00005E8C
		// (set) Token: 0x0600011C RID: 284 RVA: 0x00007C9E File Offset: 0x00005E9E
		public static bool ScanningSound
		{
			get
			{
				return Misc.Parse(SettingsManager.GetValue("ScanningSound"), true);
			}
			set
			{
				SettingsManager.SetValue("ScanningSound", value);
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00007CB0 File Offset: 0x00005EB0
		[KSPEvent(guiName = "Activate Detector", guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void EnableDetection()
		{
			this.IsDetecting = true;
			if (this.scanner != null)
			{
				this.scanner.IsDetecting = this.IsDetecting;
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00007CD2 File Offset: 0x00005ED2
		[KSPEvent(guiName = "Deactivate Detector", guiActive = true, active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void DisableDetection()
		{
			this.IsDetecting = false;
			if (this.scanner != null)
			{
				this.scanner.IsDetecting = this.IsDetecting;
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00007CF4 File Offset: 0x00005EF4
		[KSPAction("Activate Detector")]
		public void EnableDetectionAction(KSPActionParam param)
		{
			this.EnableDetection();
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00007CFC File Offset: 0x00005EFC
		[KSPAction("Deactivate Detector")]
		public void DisableDetectionAction(KSPActionParam param)
		{
			this.DisableDetection();
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00007D04 File Offset: 0x00005F04
		[KSPAction("Toggle Detector")]
		public void ToggleDetectionAction(KSPActionParam param)
		{
			this.IsDetecting = !this.IsDetecting;
			if (this.scanner != null)
			{
				this.scanner.IsDetecting = this.IsDetecting;
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00007D2E File Offset: 0x00005F2E
		[KSPEvent(guiName = "Enable Scan Tone", guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void EnableSounds()
		{
			KethaneDetector.ScanningSound = true;
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00007D36 File Offset: 0x00005F36
		[KSPEvent(guiName = "Disable Scan Tone", guiActive = true, active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void DisableSounds()
		{
			KethaneDetector.ScanningSound = false;
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00007D40 File Offset: 0x00005F40
		public override string GetInfo()
		{
			return String.Format("Maximum Altitude: {0:N0}m\nPower Consumption: {1:F2}/s\nScanning Period: {2:F2}s\nDetects: {3}", DetectingHeight, PowerConsumption, DetectingPeriod, String.Join(", ", resources.ToArray()));
		}

		// Token: 0x06000125 RID: 293 RVA: 0x00007D9F File Offset: 0x00005F9F
		public override void OnStart(PartModule.StartState state)
		{
			if (state == StartState.Editor)
			{
				return;
			}
			base.part.force_activate();
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00007DB4 File Offset: 0x00005FB4
		public override void OnLoad(ConfigNode config)
		{
			if (this.configString == null)
			{
				this.configString = config.ToString();
			}
			config = Misc.Parse(this.configString).GetNode("MODULE");
			this.resources = (from n in config.GetNodes("Resource")
			select n.GetValue("Name")).ToList<string>();
			if (this.resources.Count == 0)
			{
				this.resources = (from r in KethaneController.ResourceDefinitions
				select r.Resource).ToList<string>();
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00007E68 File Offset: 0x00006068
		public override void OnUpdate()
		{
			base.Events["EnableDetection"].active = !this.IsDetecting;
			base.Events["DisableDetection"].active = this.IsDetecting;
			base.Events["EnableSounds"].active = !KethaneDetector.ScanningSound;
			base.Events["DisableSounds"].active = KethaneDetector.ScanningSound;
			if (base.vessel.getTrueAltitude() <= (double)this.DetectingHeight)
			{
				if (this.IsDetecting && this.scanner != null)
				{
					this.Status = ((this.scanner.powerRatio > 0.0) ? "Active" : "Insufficient Power");
				}
				else
				{
					this.Status = "Idle";
				}
			}
			else
			{
				this.Status = "Out Of Range";
			}
			foreach (IDetectorAnimator detectorAnimator in base.part.Modules.OfType<IDetectorAnimator>())
			{
				detectorAnimator.IsDetecting = this.IsDetecting;
				if (this.scanner != null)
				{
					detectorAnimator.PowerRatio = (float)this.scanner.powerRatio;
				}
				else
				{
					detectorAnimator.PowerRatio = 0f;
				}
			}
		}

		// Token: 0x0400008B RID: 139
		[KSPField(isPersistant = false)]
		public float DetectingPeriod;

		// Token: 0x0400008C RID: 140
		[KSPField(isPersistant = false)]
		public float DetectingHeight;

		// Token: 0x0400008D RID: 141
		[KSPField(isPersistant = false)]
		public float PowerConsumption;

		// Token: 0x0400008E RID: 142
		[KSPField(isPersistant = true)]
		public bool IsDetecting;

		// Token: 0x0400008F RID: 143
		public string configString;

		// Token: 0x04000090 RID: 144
		internal List<string> resources;

		// Token: 0x04000091 RID: 145
		internal KethaneProtoDetector scanner;

		// Token: 0x04000092 RID: 146
		[KSPField(guiName = "Status", isPersistant = false, guiActive = true)]
		public string Status;
	}
}
