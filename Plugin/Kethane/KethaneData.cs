using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kethane.UserInterface;
using UnityEngine;

namespace Kethane
{
	// Token: 0x02000007 RID: 7
	[KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[]
	{
		GameScenes.SPACECENTER,
		GameScenes.FLIGHT,
		GameScenes.TRACKSTATION
	})]
	public class KethaneData : ScenarioModule
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000015 RID: 21 RVA: 0x000022EC File Offset: 0x000004EC
		// (set) Token: 0x06000016 RID: 22 RVA: 0x000022F3 File Offset: 0x000004F3
		public static KethaneData Current { get; private set; }

		// Token: 0x17000005 RID: 5
		public ResourceData this[string resourceName]
		{
			get
			{
				return this.resources[resourceName];
			}
		}
		
		public bool resourceExists(string resourceName)
		{
			return this.resources.ContainsKey(resourceName);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002309 File Offset: 0x00000509
		public void ResetGeneratorConfig(ResourceDefinition resource)
		{
			this.resources[resource.Resource] = ResourceData.Load(resource, new ConfigNode());
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002328 File Offset: 0x00000528
		public override void OnLoad(ConfigNode config)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			this.resources.Clear();
			ConfigNode[] nodes = config.GetNodes("Resource");
			foreach (ResourceDefinition resourceDefinition in KethaneController.ResourceDefinitions)
			{
				string resourceName = resourceDefinition.Resource;
				ConfigNode resourceNode = nodes.SingleOrDefault((ConfigNode n) => n.GetValue("Resource") == resourceName) ?? new ConfigNode();
				this.resources[resourceName] = ResourceData.Load(resourceDefinition, resourceNode);
			}
			stopwatch.Stop();
			UnityEngine.Debug.LogWarning(string.Format("Kethane deposits loaded ({0}ms)", stopwatch.ElapsedMilliseconds));
			if (MapOverlay.Instance != null)
			{
				MapOverlay.Instance.ClearBody();
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002410 File Offset: 0x00000610
		public override void OnSave(ConfigNode configNode)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			configNode.AddValue("Version", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
			foreach (KeyValuePair<string, ResourceData> keyValuePair in this.resources)
			{
				ConfigNode configNode2 = new ConfigNode("Resource");
				configNode2.AddValue("Resource", keyValuePair.Key);
				keyValuePair.Value.Save(configNode2);
				configNode.AddNode(configNode2);
			}
			stopwatch.Stop();
			UnityEngine.Debug.LogWarning(string.Format("Kethane deposits saved ({0}ms)", stopwatch.ElapsedMilliseconds));
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000024D4 File Offset: 0x000006D4
		public override void OnAwake()
		{
			Current = this;
			UnityEngine.Debug.LogFormat("[KethaneData] OnAwake");
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000024EB File Offset: 0x000006EB
		private void OnDestroy()
		{
			KethaneData.Current = null;
		}

		// Token: 0x04000005 RID: 5
		public const int GridLevel = 5;

		// Token: 0x04000007 RID: 7
		private Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();
	}
}
