using System;
using System.Collections.Generic;
using GeodesicGrid;
using Kethane.PartModules;
using Kethane.UserInterface;
using UnityEngine;

namespace Kethane.VesselModules
{
	// Token: 0x02000012 RID: 18
	public class KethaneVesselScanner : VesselModule
	{
		// Token: 0x0600006B RID: 107 RVA: 0x000037DC File Offset: 0x000019DC
		private void FindDetectors()
		{
			this.detectors = this.vessel.FindPartModulesImplementing<KethaneDetector>();
			this.protoDetectors = new List<KethaneProtoDetector>();
			foreach (KethaneDetector kethaneDetector in this.detectors)
			{
				kethaneDetector.scanner = new KethaneProtoDetector(kethaneDetector, this);
				this.protoDetectors.Add(kethaneDetector.scanner);
			}
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00003864 File Offset: 0x00001A64
		private static void FindBatteries(Dictionary<uint, IKethaneBattery> batteries, List<IKethaneBattery> batteryList, List<Part> parts)
		{
			int hashCode = "ElectricCharge".GetHashCode();
			foreach (Part part in parts)
			{
				PartResource partResource = part.Resources.Get(hashCode);
				if (partResource != null)
				{
					KethaneBattery kethaneBattery = new KethaneBattery(partResource);
					batteries[part.flightID] = kethaneBattery;
					batteryList.Add(kethaneBattery);
				}
			}
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000038E4 File Offset: 0x00001AE4
		private static void FindBatteries(Dictionary<uint, IKethaneBattery> batteries, List<IKethaneBattery> batteryList, List<ProtoPartSnapshot> parts)
		{
			foreach (ProtoPartSnapshot protoPartSnapshot in parts)
			{
				foreach (ProtoPartResourceSnapshot protoPartResourceSnapshot in protoPartSnapshot.resources)
				{
					if (protoPartResourceSnapshot.resourceName == "ElectricCharge")
					{
						KethaneProtoBattery kethaneProtoBattery = new KethaneProtoBattery(protoPartResourceSnapshot);
						batteries[protoPartSnapshot.flightID] = kethaneProtoBattery;
						batteryList.Add(kethaneProtoBattery);
					}
				}
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00003998 File Offset: 0x00001B98
		private void FindBatteries()
		{
			this.batteries = new Dictionary<uint, IKethaneBattery>();
			this.batteryList = new List<IKethaneBattery>();
			if (this.vessel.loaded)
			{
				KethaneVesselScanner.FindBatteries(this.batteries, this.batteryList, this.vessel.parts);
				return;
			}
			KethaneVesselScanner.FindBatteries(this.batteries, this.batteryList, this.vessel.protoVessel.protoPartSnapshots);
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00003A08 File Offset: 0x00001C08
		private void FindGenerators()
		{
			List<ModuleGenerator> list = this.vessel.FindPartModulesImplementing<ModuleGenerator>();
			this.generatorEC = 0.0;
			int count = list.Count;
			while (count-- > 0)
			{
				ModuleGenerator moduleGenerator = list[count];
				ModuleResourceHandler resHandler = moduleGenerator.resHandler;
				if (moduleGenerator.moduleIsEnabled && (moduleGenerator.isAlwaysActive || moduleGenerator.generatorIsActive) && resHandler.inputResources.Count == 0 && resHandler.outputResources.Count == 1 && resHandler.outputResources[0].name == "ElectricCharge")
				{
					this.generatorEC += resHandler.outputResources[0].rate;
				}
			}
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00003ACC File Offset: 0x00001CCC
		private void FindSolarPanels()
		{
			List<ModuleDeployableSolarPanel> list = this.vessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>();
			this.solarEC = 0.0;
			int count = list.Count;
			while (count-- > 0)
			{
				ModuleDeployableSolarPanel moduleDeployableSolarPanel = list[count];
				ModuleResourceHandler resHandler = moduleDeployableSolarPanel.resHandler;
				if (resHandler.inputResources.Count == 0 && resHandler.outputResources.Count == 1 && resHandler.outputResources[0].name == "ElectricCharge")
				{
					this.solarEC += moduleDeployableSolarPanel._flowRate * resHandler.outputResources[0].rate;
				}
			}
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00003B71 File Offset: 0x00001D71
		public void UpdateDetecting()
		{
			this.isScanning = true;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003B7A File Offset: 0x00001D7A
		public override bool ShouldBeActive()
		{
			return base.ShouldBeActive() & (!this.started || this.isScanning);
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00003B94 File Offset: 0x00001D94
		private void onVesselCreate(Vessel v)
		{
			if (v == this.vessel)
			{
				GameEvents.onVesselCreate.Remove(new EventData<Vessel>.OnEvent(this.onVesselCreate));
				this.FindBatteries();
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003BC0 File Offset: 0x00001DC0
		protected override void OnLoad(ConfigNode node)
		{
			this.protoDetectors = new List<KethaneProtoDetector>();
			for (int i = 0; i < node.nodes.Count; i++)
			{
				ConfigNode configNode = node.nodes[i];
				if (configNode.name == "Detector")
				{
					KethaneProtoDetector item = new KethaneProtoDetector(configNode, this);
					this.protoDetectors.Add(item);
				}
			}
			this.generatorEC = 0.0;
			if (node.HasValue("generatorEC"))
			{
				double.TryParse(node.GetValue("generatorEC"), out this.generatorEC);
			}
			this.solarEC = 0.0;
			if (node.HasValue("solarEC"))
			{
				double.TryParse(node.GetValue("solarEC"), out this.solarEC);
			}
			GameEvents.onVesselCreate.Add(new EventData<Vessel>.OnEvent(this.onVesselCreate));
			this.isScanning = true;
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003CA4 File Offset: 0x00001EA4
		protected override void OnSave(ConfigNode node)
		{
			if (this.protoDetectors == null)
			{
				return;
			}
			if (this.vessel.loaded)
			{
				this.FindGenerators();
				this.FindSolarPanels();
			}
			node.AddValue("generatorEC", this.generatorEC);
			node.AddValue("solarEC", this.solarEC);
			for (int i = 0; i < this.protoDetectors.Count; i++)
			{
				this.protoDetectors[i].Save(node);
			}
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00003D1D File Offset: 0x00001F1D
		private void onVesselWasModified(Vessel v)
		{
			if (v == this.vessel)
			{
				this.FindDetectors();
				this.FindBatteries();
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003D39 File Offset: 0x00001F39
		protected override void OnAwake()
		{
			GameEvents.onVesselWasModified.Add(new EventData<Vessel>.OnEvent(this.onVesselWasModified));
			this.planetLayerMask = 1 << LayerMask.NameToLayer("Scaled Scenery");
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00003D66 File Offset: 0x00001F66
		private void OnDestroy()
		{
			GameEvents.onVesselWasModified.Remove(new EventData<Vessel>.OnEvent(this.onVesselWasModified));
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00003D7E File Offset: 0x00001F7E
		private bool ValidVesselType(VesselType type)
		{
			return type != VesselType.Flag &&
			       type != VesselType.Unknown &&
			       type != VesselType.Debris &&
			       type != VesselType.EVA &&
			       type != VesselType.DeployedSciencePart &&
			       type != VesselType.DeployedScienceController &&
			       type != VesselType.DeployedGroundPart &&
			       type != VesselType.DroppedPart;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00003D94 File Offset: 0x00001F94
		protected override void OnStart()
		{
			this.started = true;
			if (!this.ValidVesselType(this.vessel.vesselType))
			{
				this.vessel.vesselModules.Remove(this);
				Destroy(this);
				return;
			}
			this.PingEmpty = base.gameObject.AddComponent<AudioSource>();
			this.PingEmpty.clip = GameDatabase.Instance.GetAudioClip("Kethane/Sounds/echo_empty");
			this.PingEmpty.volume = 1f;
			this.PingEmpty.loop = false;
			this.PingEmpty.Stop();
			this.PingDeposit = base.gameObject.AddComponent<AudioSource>();
			this.PingDeposit.clip = GameDatabase.Instance.GetAudioClip("Kethane/Sounds/echo_deposit");
			this.PingDeposit.volume = 1f;
			this.PingDeposit.loop = false;
			this.PingDeposit.Stop();
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00003E78 File Offset: 0x00002078
		public override void OnLoadVessel()
		{
			this.FindDetectors();
			this.FindBatteries();
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00003E88 File Offset: 0x00002088
		public override void OnUnloadVessel()
		{
			Debug.LogFormat("[KethaneVesselScanner] OnUnloadVessel: {0} {1} {2}", new object[]
			{
				this.vessel.gameObject == base.gameObject,
				base.enabled,
				base.gameObject.activeInHierarchy
			});
			this.FindGenerators();
			this.FindSolarPanels();
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00003EF0 File Offset: 0x000020F0
		public void OnVesselUnload()
		{
			Debug.LogFormat("[KethaneVesselScanner] OnVesselUnload: {0}", new object[]
			{
				this.vessel.name
			});
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00003F10 File Offset: 0x00002110
		private double DrawEC(double amount)
		{
			double num = 0.0;
			int count = this.batteryList.Count;
			while (count-- > 0)
			{
				if (this.batteryList[count].flowState)
				{
					num += this.batteryList[count].amount;
				}
			}
			if (amount >= num)
			{
				amount = num;
				if (num > 0.0)
				{
					int count2 = this.batteryList.Count;
					while (count2-- > 0)
					{
						this.batteryList[count2].amount = 0.0;
					}
				}
			}
			else
			{
				int count3 = this.batteryList.Count;
				while (count3-- > 0)
				{
					IKethaneBattery kethaneBattery = this.batteryList[count3];
					if (kethaneBattery.flowState)
					{
						double amount2 = kethaneBattery.amount;
						double num2 = amount * amount2 / num;
						if (num2 > amount2)
						{
							num2 = amount2;
						}
						kethaneBattery.amount = amount2 - num2;
					}
				}
			}
			return amount;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00004004 File Offset: 0x00002204
		private double PushEC(double amount)
		{
			double num = 0.0;
			int count = this.batteryList.Count;
			while (count-- > 0)
			{
				if (this.batteryList[count].flowState)
				{
					num += this.batteryList[count].maxAmount - this.batteryList[count].amount;
				}
			}
			if (amount >= num)
			{
				amount = num;
				if (num > 0.0)
				{
					int count2 = this.batteryList.Count;
					while (count2-- > 0)
					{
						this.batteryList[count2].amount = this.batteryList[count2].maxAmount;
					}
				}
			}
			else
			{
				int count3 = this.batteryList.Count;
				while (count3-- > 0)
				{
					IKethaneBattery kethaneBattery = this.batteryList[count3];
					if (kethaneBattery.flowState)
					{
						double amount2 = kethaneBattery.amount;
						double num2 = kethaneBattery.maxAmount - amount2;
						double num3 = amount * num2 / num;
						if (num3 > num2)
						{
							num3 = num2;
						}
						kethaneBattery.amount = amount2 + num3;
					}
				}
			}
			return amount;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x0000411B File Offset: 0x0000231B
		private void RunGenerators()
		{
			this.PushEC(this.generatorEC * (double)TimeWarp.fixedDeltaTime);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00004134 File Offset: 0x00002334
		private void RunSolarPanels()
		{
			float maxValue = float.MaxValue;
			CelestialBody sun = Planetarium.fetch.Sun;
			Transform transform = sun.scaledBody.transform;
			Vector3d vector3d = ScaledSpace.LocalToScaledSpace(this.vessel.transform.position);
			Vector3d vector3d2 = ScaledSpace.LocalToScaledSpace(sun.transform.position);
			RaycastHit raycastHit = default;
			if (!Physics.Raycast(new Ray(vector3d, (vector3d2 - vector3d).normalized), out raycastHit, maxValue, this.planetLayerMask) || raycastHit.transform == transform)
			{
				this.PushEC(this.solarEC * (double)TimeWarp.fixedDeltaTime);
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x000041E0 File Offset: 0x000023E0
		private void FixedUpdate()
		{
			if (this.protoDetectors == null)
			{
				return;
			}
			double trueAltitude = this.vessel.getTrueAltitude();
			CelestialBody mainBody = this.vessel.mainBody;
			Vector3 position = this.vessel.transform.position;
			bool flag = false;
			bool flag2 = false;
			this.isScanning = false;
			Cell cellUnder = MapOverlay.GetCellUnder(mainBody, position);
			int count = this.protoDetectors.Count;
			while (count-- > 0)
			{
				KethaneProtoDetector kethaneProtoDetector = this.protoDetectors[count];
				if (kethaneProtoDetector.IsDetecting)
				{
					this.isScanning = true;
					if (trueAltitude < (double)kethaneProtoDetector.DetectingHeight)
					{
						double num = (double)(kethaneProtoDetector.PowerConsumption * TimeWarp.fixedDeltaTime);
						double num2 = this.DrawEC(num);
						kethaneProtoDetector.powerRatio = num2 / num;
						kethaneProtoDetector.TimerEcho += (double)TimeWarp.deltaTime * kethaneProtoDetector.powerRatio;
						double num3 = (double)kethaneProtoDetector.DetectingPeriod * (1.0 + trueAltitude * 2E-06);
						if (kethaneProtoDetector.TimerEcho >= num3)
						{
							int count2 = kethaneProtoDetector.resources.Count;
							while (count2-- > 0)
							{
								string resourceName = kethaneProtoDetector.resources[count2];
								BodyResourceData bodyResourceData = KethaneData.Current[resourceName][mainBody];
								if (!bodyResourceData.IsCellScanned(cellUnder))
								{
									flag2 = true;
									bodyResourceData.ScanCell(cellUnder);
									if (bodyResourceData.Resources.GetQuantity(cellUnder) != null)
									{
										flag = true;
									}
								}
							}
							MapOverlay.Instance.RefreshCellColor(cellUnder, mainBody);
							kethaneProtoDetector.TimerEcho = 0.0;
						}
					}
				}
			}
			if (flag2 && KethaneDetector.ScanningSound && this.vessel == FlightGlobals.ActiveVessel)
			{
				(flag ? this.PingDeposit : this.PingEmpty).Play();
				(flag ? this.PingDeposit : this.PingEmpty).loop = false;
			}
			if (!this.vessel.loaded)
			{
				this.RunGenerators();
				if (this.solarEC > 0.0)
				{
					this.RunSolarPanels();
				}
			}
		}

		// Token: 0x0400002D RID: 45
		private List<KethaneDetector> detectors;

		// Token: 0x0400002E RID: 46
		private List<KethaneProtoDetector> protoDetectors;

		// Token: 0x0400002F RID: 47
		private Dictionary<uint, IKethaneBattery> batteries;

		// Token: 0x04000030 RID: 48
		private List<IKethaneBattery> batteryList;

		// Token: 0x04000031 RID: 49
		private AudioSource PingEmpty;

		// Token: 0x04000032 RID: 50
		private AudioSource PingDeposit;

		// Token: 0x04000033 RID: 51
		private bool started;

		// Token: 0x04000034 RID: 52
		private bool isScanning;

		// Token: 0x04000035 RID: 53
		private double generatorEC;

		// Token: 0x04000036 RID: 54
		private double solarEC;

		// Token: 0x04000037 RID: 55
		private int planetLayerMask;
	}
}
