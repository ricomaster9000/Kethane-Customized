using System;
using System.Collections.Generic;
using System.Linq;
using GeodesicGrid;
using Kethane.UserInterface;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x0200002E RID: 46
	public class KethaneExtractor : PartModule
	{
		// Token: 0x06000145 RID: 325 RVA: 0x00008AFC File Offset: 0x00006CFC
		public override void OnStart(PartModule.StartState state)
		{
			this.part.force_activate();
			animator = part.Modules.OfType<IExtractorAnimator>().SingleOrDefault();

			if (animator == null)
			{
				animator = new DefaultExtractorAnimator();
			}
			else
			{
				Events["DeployDrill"].guiActiveEditor = true;
				Events["RetractDrill"].guiActiveEditor = true;
			}

			headTransform = this.part.FindModelTransform(HeadTransform);
			tailTransform = this.part.FindModelTransform(TailTransform);

			if (state == StartState.Editor) { return; }
			if (FlightGlobals.fetch == null) { return; }

			emitters = part.Modules.OfType<KethaneParticleEmitter>().ToArray();
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00008BCC File Offset: 0x00006DCC
		public override void OnLoad(ConfigNode config)
		{
			if (this.configString == null)
			{
				this.configString = config.ToString();
			}
			config = Misc.Parse(this.configString).GetNode("MODULE");
			this.resources = (from n in config.GetNodes("Resource")
			select new KethaneExtractor.Resource(n)).ToList<KethaneExtractor.Resource>();
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00008C3E File Offset: 0x00006E3E
		[KSPEvent(guiName = "Deploy Drill", guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void DeployDrill()
		{
			this.animator.Deploy();
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00008C4B File Offset: 0x00006E4B
		[KSPEvent(guiName = "Retract Drill", guiActive = true, active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void RetractDrill()
		{
			this.animator.Retract();
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00008C58 File Offset: 0x00006E58
		[KSPAction("Deploy Drill")]
		public void DeployDrillAction(KSPActionParam param)
		{
			this.DeployDrill();
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00008C60 File Offset: 0x00006E60
		[KSPAction("Retract Drill")]
		public void RetractDrillAction(KSPActionParam param)
		{
			this.RetractDrill();
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00008C68 File Offset: 0x00006E68
		[KSPAction("Toggle Drill")]
		public void ToggleDrillAction(KSPActionParam param)
		{
			if (this.animator.CurrentState == ExtractorState.Deployed || this.animator.CurrentState == ExtractorState.Deploying)
			{
				this.RetractDrill();
				return;
			}
			if (this.animator.CurrentState == ExtractorState.Retracted || this.animator.CurrentState == ExtractorState.Retracting)
			{
				this.DeployDrill();
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00008CBC File Offset: 0x00006EBC
		public override string GetInfo()
		{
			return string.Concat((from r in this.resources
			select string.Format("{0} Rate: {1:F2}L/s\n", r.Name, r.Rate)).ToArray<string>()) + string.Format("Power Consumption: {0:F2}/s", this.PowerConsumption);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00008D18 File Offset: 0x00006F18
		public void Update()
		{
			var retracted = (animator.CurrentState == ExtractorState.Retracted);
			var deployed = (animator.CurrentState == ExtractorState.Deployed);
			if (Events["DeployDrill"].active != retracted || Events["RetractDrill"].active != deployed)
			{
				Events["DeployDrill"].active = retracted;
				Events["RetractDrill"].active = deployed;
				foreach (var window in GameObject.FindObjectsOfType(typeof(UIPartActionWindow)).OfType<UIPartActionWindow>().Where(w => w.part == part))
				{
					window.displayDirty = true;
				}
			}
			Status = animator.CurrentState.ToString();

			if (!HighLogic.LoadedSceneIsFlight) { return; }

			if (animator.CurrentState != ExtractorState.Retracted) {
				RaycastHit hitInfo;
				var hit = raycastGround(out hitInfo);

				foreach (var emitter in emitters) {
					if (hit) {
						emitter.Position = hitInfo.point;
						emitter.Direction = tailTransform.position - headTransform.position;
					}
					if (emitter.Label != "gas") {
						emitter.Emit = hit;
						emitter.Rate = 20;
					} else {
						var bodyResource = getBodyResources("Kethane");
						if (bodyResource != null && animator.CurrentState == ExtractorState.Deployed) {
							emitter.Emit = hit && bodyResource.GetQuantity(getCellUnder()) != null;
							emitter.Rate = 20;
						} else {
							emitter.Emit = false;
						}
					}
				}
			} else {
				foreach (var emitter in emitters) {
					emitter.Emit = false;
				}
			}
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00008F44 File Offset: 0x00007144
		public override void OnFixedUpdate()
		{
			if (this.animator.CurrentState != ExtractorState.Deployed)
			{
				return;
			}
			if (!this.raycastGround())
			{
				return;
			}
			double num = (double)(this.PowerConsumption * TimeWarp.fixedDeltaTime);
			double num2 = base.part.RequestResource("ElectricCharge", num) / num;
			foreach (KethaneExtractor.Resource resource in this.resources)
			{
				Cell cellUnder = this.getCellUnder();
				IBodyResources bodyResources = this.getBodyResources(resource.Name);
				if (bodyResources != null)
				{
					double? quantity = bodyResources.GetQuantity(cellUnder);
					if (quantity != null)
					{
						double num3 = (double)(TimeWarp.fixedDeltaTime * resource.Rate) * num2;
						num3 = Math.Min(num3, quantity.Value);
						bodyResources.Extract(cellUnder, -base.part.RequestResource(resource.Name, -num3));
					}
				}
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00009038 File Offset: 0x00007238
		private Cell getCellUnder()
		{
			return MapOverlay.GetCellUnder(base.vessel.mainBody, base.vessel.transform.position);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x0000905A File Offset: 0x0000725A
		private IBodyResources getBodyResources(string resourceName)
		{
			if (KethaneData.Current == null)
			{
				return null;
			}
			return KethaneData.Current[resourceName][base.vessel.mainBody].Resources;
		}

		// Token: 0x06000151 RID: 337 RVA: 0x0000908C File Offset: 0x0000728C
		private bool raycastGround()
		{
			RaycastHit raycastHit;
			return this.raycastGround(out raycastHit);
		}

		// Token: 0x06000152 RID: 338 RVA: 0x000090A4 File Offset: 0x000072A4
		private bool raycastGround(out RaycastHit hitInfo)
		{
			var mask = 1 << 15;
			var direction = headTransform.position - tailTransform.position;
			return Physics.Raycast(tailTransform.position - direction.normalized * TailOffset, direction, out hitInfo, direction.magnitude + HeadOffset + TailOffset, mask);
		}

		// Token: 0x040000AA RID: 170
		private IExtractorAnimator animator;

		// Token: 0x040000AB RID: 171
		private List<KethaneExtractor.Resource> resources;

		// Token: 0x040000AC RID: 172
		[KSPField(isPersistant = false)]
		public float PowerConsumption;

		// Token: 0x040000AD RID: 173
		[KSPField(guiName = "Status", isPersistant = false, guiActive = true)]
		public string Status;

		// Token: 0x040000AE RID: 174
		[KSPField(isPersistant = false)]
		public string HeadTransform;

		// Token: 0x040000AF RID: 175
		[KSPField(isPersistant = false)]
		public string TailTransform;

		// Token: 0x040000B0 RID: 176
		[KSPField(isPersistant = false)]
		public float HeadOffset;

		// Token: 0x040000B1 RID: 177
		[KSPField(isPersistant = false)]
		public float TailOffset;

		// Token: 0x040000B2 RID: 178
		public string configString;

		// Token: 0x040000B3 RID: 179
		private Transform headTransform;

		// Token: 0x040000B4 RID: 180
		private Transform tailTransform;

		// Token: 0x040000B5 RID: 181
		private KethaneParticleEmitter[] emitters;

		// Token: 0x02000055 RID: 85
		public class Resource
		{
			// Token: 0x17000039 RID: 57
			// (get) Token: 0x0600020E RID: 526 RVA: 0x0000A9AF File Offset: 0x00008BAF
			// (set) Token: 0x0600020F RID: 527 RVA: 0x0000A9B7 File Offset: 0x00008BB7
			public string Name { get; private set; }

			// Token: 0x1700003A RID: 58
			// (get) Token: 0x06000210 RID: 528 RVA: 0x0000A9C0 File Offset: 0x00008BC0
			// (set) Token: 0x06000211 RID: 529 RVA: 0x0000A9C8 File Offset: 0x00008BC8
			public float Rate { get; private set; }

			// Token: 0x06000212 RID: 530 RVA: 0x0000A9D1 File Offset: 0x00008BD1
			public Resource(ConfigNode node)
			{
				this.Name = node.GetValue("Name");
				this.Rate = float.Parse(node.GetValue("Rate"));
			}
		}

		// Token: 0x02000056 RID: 86
		private class DefaultExtractorAnimator : IExtractorAnimator
		{
			// Token: 0x1700003B RID: 59
			// (get) Token: 0x06000213 RID: 531 RVA: 0x0000AA00 File Offset: 0x00008C00
			// (set) Token: 0x06000214 RID: 532 RVA: 0x0000AA08 File Offset: 0x00008C08
			public ExtractorState CurrentState { get; private set; }

			// Token: 0x06000215 RID: 533 RVA: 0x0000AA11 File Offset: 0x00008C11
			public void Deploy()
			{
				this.CurrentState = ExtractorState.Deployed;
			}

			// Token: 0x06000216 RID: 534 RVA: 0x0000AA1A File Offset: 0x00008C1A
			public void Retract()
			{
				this.CurrentState = ExtractorState.Retracted;
			}

			// Token: 0x06000217 RID: 535 RVA: 0x0000AA23 File Offset: 0x00008C23
			public DefaultExtractorAnimator()
			{
				this.CurrentState = ExtractorState.Retracted;
			}
		}
	}
}
