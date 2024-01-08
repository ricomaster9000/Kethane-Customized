using System;
using System.Collections.Generic;
using System.Linq;
using KerbNet;
using KSP.UI.Dialogs;
using UnityEngine;
using UnityEngine.Events;

namespace Kethane.UserInterface
{
	// Token: 0x0200001C RID: 28
	public class KerbNetModeKethane : KerbNetMode
	{
		// Token: 0x060000A0 RID: 160 RVA: 0x00004DB4 File Offset: 0x00002FB4
		public override void OnInit()
		{
			this.name = "Kethane";
			this.buttonSprite = Resources.Load<Sprite>("Scanners/resource");
			this.localCoordinateInfoLabel = "Quantity";
			this.doTerrainContourPass = true;
			this.doAnomaliesPass = true;
			this.doCoordinatePass = true;
			this.resourceDefinitions = KethaneController.ResourceDefinitions.ToList<ResourceDefinition>();
			this.resourceIndex = 0;
			this.resource = this.resourceDefinitions[this.resourceIndex];
			this.customButtonCaption = "Resource: " + this.resource.Resource;
			this.customButtonCallback = new UnityAction(this.OnResourceClick);
			this.customButtonTooltip = "Cycle Displayed Resource";
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00004E64 File Offset: 0x00003064
		private void OnResourceClick()
		{
			int num = this.resourceIndex + 1;
			this.resourceIndex = num;
			if (num >= this.resourceDefinitions.Count)
			{
				this.resourceIndex = 0;
			}
			this.resource = this.resourceDefinitions[this.resourceIndex];
			this.customButtonCaption = "Resource: " + this.resource.Resource;
			KerbNetDialog.Instance.ActivateDisplayMode(this);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00004ED3 File Offset: 0x000030D3
		public override void OnPrecache(Vessel vessel)
		{
			this.body = vessel.mainBody;
			this.bodyResources = KethaneData.Current[this.resource.Resource][this.body];
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00004F07 File Offset: 0x00003107
		public override Color GetCoordinateColor(Vessel vessel, double currentLatitude, double currentLongitude)
		{
			return MapOverlay.GetCellColor(MapOverlay.GetCellUnder(this.body, currentLatitude, currentLongitude), this.bodyResources, this.resource);
		}

		// Token: 0x04000042 RID: 66
		private CelestialBody body;

		// Token: 0x04000043 RID: 67
		private BodyResourceData bodyResources;

		// Token: 0x04000044 RID: 68
		private ResourceDefinition resource;

		// Token: 0x04000045 RID: 69
		private List<ResourceDefinition> resourceDefinitions;

		// Token: 0x04000046 RID: 70
		private int resourceIndex;
	}
}
