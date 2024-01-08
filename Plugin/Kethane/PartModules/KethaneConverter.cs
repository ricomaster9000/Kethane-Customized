using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kethane.PartModules
{
	// Token: 0x02000028 RID: 40
	public class KethaneConverter : PartModule
	{
		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600010A RID: 266 RVA: 0x0000760C File Offset: 0x0000580C
		public Dictionary<string, double> ResourceActivity
		{
			get
			{
				return new Dictionary<string, double>(this.resourceActivity);
			}
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00007619 File Offset: 0x00005819
		[KSPEvent(guiName = "Activate Converter", guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void ActivateConverter()
		{
			this.IsEnabled = true;
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00007622 File Offset: 0x00005822
		[KSPEvent(guiName = "Deactivate Converter", guiActive = true, active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
		public void DeactivateConverter()
		{
			this.IsEnabled = false;
		}

		// Token: 0x0600010D RID: 269 RVA: 0x0000762B File Offset: 0x0000582B
		[KSPAction("Activate Converter")]
		public void ActivateConverterAction(KSPActionParam param)
		{
			this.ActivateConverter();
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00007633 File Offset: 0x00005833
		[KSPAction("Deactivate Converter")]
		public void DeactivateConverterAction(KSPActionParam param)
		{
			this.DeactivateConverter();
		}

		// Token: 0x0600010F RID: 271 RVA: 0x0000763B File Offset: 0x0000583B
		[KSPAction("Toggle Converter")]
		public void ToggleConverterAction(KSPActionParam param)
		{
			this.IsEnabled = !this.IsEnabled;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x0000764C File Offset: 0x0000584C
		public override string GetInfo()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.HeatProduction > 0f)
			{
				stringBuilder.AppendFormat("<b>Waste heat production:</b> {0:F1}", this.HeatProduction);
				stringBuilder.AppendLine();
			}
			KethaneConverter.getRateGroupInfo(stringBuilder, "Inputs", this.inputRates);
			KethaneConverter.getRateGroupInfo(stringBuilder, "Outputs", this.outputRates);
			return stringBuilder.ToString();
		}

		// Token: 0x06000111 RID: 273 RVA: 0x000076B4 File Offset: 0x000058B4
		private static void getRateGroupInfo(StringBuilder sb, string heading, IEnumerable<KethaneConverter.ResourceRate> rates)
		{
			sb.Append("<b><color=#99ff00ff>");
			sb.Append(heading);
			sb.AppendLine(":</color></b>");
			foreach (KethaneConverter.ResourceRate resourceRate in rates)
			{
				sb.AppendFormat("- <b>{0}</b>: {1:N2}/s", resourceRate.Resource, resourceRate.Rate);
				if (resourceRate.Optional)
				{
					sb.Append(" (optional)");
				}
				sb.AppendLine();
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00007750 File Offset: 0x00005950
		public override void OnLoad(ConfigNode config)
		{
			if (this.configString == null)
			{
				this.configString = config.ToString();
			}
			this.loadConfig();
		}

		// Token: 0x06000113 RID: 275 RVA: 0x0000776C File Offset: 0x0000596C
		private void loadConfig()
		{
			ConfigNode node = Misc.Parse(this.configString).GetNode("MODULE");
			PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
			this.inputRates = KethaneConverter.loadRates(node.GetNode("InputRates")).ToArray<KethaneConverter.ResourceRate>();
			double inputMassRate = this.inputRates.Sum((KethaneConverter.ResourceRate p) => p.Rate * (double)definitions[p.Resource].density);
			List<KethaneConverter.ResourceRate> source = KethaneConverter.loadRates(node.GetNode("OutputRatios")).ToList<KethaneConverter.ResourceRate>();
			IEnumerable<KethaneConverter.ResourceRate> first = from r in source
			where !r.Optional
			select r * (inputMassRate / (double)definitions[r.Resource].density) into r
			group r by r.Resource into g
			select new KethaneConverter.ResourceRate(g.Key, g.Sum((KethaneConverter.ResourceRate r) => r.Rate), false);
			IEnumerable<KethaneConverter.ResourceRate> second = from r in source
			where r.Optional
			select r * (inputMassRate / (double)definitions[r.Resource].density) into r
			group r by r.Resource into g
			select new KethaneConverter.ResourceRate(g.Key, g.Sum((KethaneConverter.ResourceRate r) => r.Rate), true);
			this.outputRates = first.Concat(second).Concat(KethaneConverter.loadRates(node.GetNode("OutputRates"))).ToArray<KethaneConverter.ResourceRate>();
			if (this.Label == null)
			{
				this.Label = string.Join("/", (from r in this.outputRates
				select r.Resource).ToArray<string>());
			}
		}

		// Token: 0x06000114 RID: 276 RVA: 0x0000795D File Offset: 0x00005B5D
		private static IEnumerable<KethaneConverter.ResourceRate> loadRates(ConfigNode config)
		{
			if (config == null)
			{
				yield break;
			}
			foreach (ConfigNode.Value value in config.values.Cast<ConfigNode.Value>())
			{
				string text = value.name;
				bool flag = text.EndsWith("*");
				if (flag)
				{
					text = text.Substring(0, text.Length - 1);
				}
				double num = Misc.Parse(value.value, 0.0);
				if (PartResourceLibrary.Instance.GetDefinition(text) != null && num > 0.0)
				{
					yield return new KethaneConverter.ResourceRate(text, num, flag);
				}
			}
			IEnumerator<ConfigNode.Value> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00007970 File Offset: 0x00005B70
		public override void OnStart(PartModule.StartState state)
		{
			loadConfig();

			Actions["ActivateConverterAction"].guiName = Events["ActivateConverter"].guiName = String.Format("Activate {0} Converter", Label);
			Actions["DeactivateConverterAction"].guiName = Events["DeactivateConverter"].guiName = String.Format("Deactivate {0} Converter", Label);
			Actions["ToggleConverterAction"].guiName = String.Format("Toggle {0} Converter", Label);

			Events["ActivateConverter"].guiActive = Events["DeactivateConverter"].guiActive = !AlwaysActive;

			if (state == StartState.Editor) { return; }
			this.part.force_activate();
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00007A68 File Offset: 0x00005C68
		public override void OnUpdate()
		{
			base.Events["ActivateConverter"].active = !this.IsEnabled;
			base.Events["DeactivateConverter"].active = this.IsEnabled;
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00007AA4 File Offset: 0x00005CA4
		private double ResouceCapacity(KethaneConverter.ResourceRate r)
		{
			double result;
			double num;
			base.part.GetConnectedResourceTotals(r.Resource, out result, out num, r.Rate > 0.0);
			return result;
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00007ADC File Offset: 0x00005CDC
		public override void OnFixedUpdate()
		{
			this.resourceActivity.Clear();
			if (!this.IsEnabled && !this.AlwaysActive)
			{
				return;
			}
			KethaneConverter.ResourceRate[] array = (from r in (from r in this.outputRates
			select r * -1.0).Concat(this.inputRates)
			select r * (double)TimeWarp.fixedDeltaTime).ToArray<KethaneConverter.ResourceRate>();
			double num = (from r in array
			where !r.Optional
			select this.ResouceCapacity(r) / Math.Abs(r.Rate) into r
			where r < 1.0
			select r).DefaultIfEmpty(1.0).Min();
			HeatSinkAnimator heatSinkAnimator = base.part.Modules.OfType<HeatSinkAnimator>().SingleOrDefault<HeatSinkAnimator>();
			if (num > 0.0 && heatSinkAnimator != null)
			{
				float num2 = (float)num * this.HeatProduction * TimeWarp.fixedDeltaTime;
				num *= (double)(heatSinkAnimator.AddHeat(num2) / num2);
			}
			foreach (KethaneConverter.ResourceRate resourceRate in array)
			{
				this.resourceActivity[resourceRate.Resource] = base.part.RequestResource(resourceRate.Resource, resourceRate.Rate * num);
			}
		}

		// Token: 0x04000083 RID: 131
		[KSPField(isPersistant = false)]
		public bool AlwaysActive;

		// Token: 0x04000084 RID: 132
		[KSPField(isPersistant = false)]
		public string Label;

		// Token: 0x04000085 RID: 133
		[KSPField(isPersistant = false)]
		public float HeatProduction;

		// Token: 0x04000086 RID: 134
		[KSPField(isPersistant = true)]
		public bool IsEnabled;

		// Token: 0x04000087 RID: 135
		public string configString;

		// Token: 0x04000088 RID: 136
		private KethaneConverter.ResourceRate[] inputRates;

		// Token: 0x04000089 RID: 137
		private KethaneConverter.ResourceRate[] outputRates;

		// Token: 0x0400008A RID: 138
		private Dictionary<string, double> resourceActivity = new Dictionary<string, double>();

		// Token: 0x0200004E RID: 78
		private struct ResourceRate
		{
			// Token: 0x17000034 RID: 52
			// (get) Token: 0x060001E1 RID: 481 RVA: 0x0000A5F7 File Offset: 0x000087F7
			// (set) Token: 0x060001E2 RID: 482 RVA: 0x0000A5FF File Offset: 0x000087FF
			public string Resource { readonly get; private set; }

			// Token: 0x17000035 RID: 53
			// (get) Token: 0x060001E3 RID: 483 RVA: 0x0000A608 File Offset: 0x00008808
			// (set) Token: 0x060001E4 RID: 484 RVA: 0x0000A610 File Offset: 0x00008810
			public double Rate { readonly get; private set; }

			// Token: 0x17000036 RID: 54
			// (get) Token: 0x060001E5 RID: 485 RVA: 0x0000A619 File Offset: 0x00008819
			// (set) Token: 0x060001E6 RID: 486 RVA: 0x0000A621 File Offset: 0x00008821
			public bool Optional { readonly get; private set; }

			// Token: 0x060001E7 RID: 487 RVA: 0x0000A62A File Offset: 0x0000882A
			public ResourceRate(string resource, double rate)
			{
				this = new KethaneConverter.ResourceRate(resource, rate, false);
			}

			// Token: 0x060001E8 RID: 488 RVA: 0x0000A635 File Offset: 0x00008835
			public ResourceRate(string resource, double rate, bool optional)
			{
				this = default(KethaneConverter.ResourceRate);
				this.Resource = resource;
				this.Rate = rate;
				this.Optional = optional;
			}

			// Token: 0x060001E9 RID: 489 RVA: 0x0000A653 File Offset: 0x00008853
			public static KethaneConverter.ResourceRate operator *(KethaneConverter.ResourceRate rate, double multiplier)
			{
				return new KethaneConverter.ResourceRate(rate.Resource, rate.Rate * multiplier, rate.Optional);
			}
		}
	}
}
