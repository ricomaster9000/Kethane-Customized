using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Kethane
{
	// Token: 0x02000006 RID: 6
	public class KethaneController
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000011 RID: 17 RVA: 0x00002183 File Offset: 0x00000383
		public static IEnumerable<ResourceDefinition> ResourceDefinitions
		{
			get
			{
				if (KethaneController.resourceDefinitions == null)
				{
					KethaneController.resourceDefinitions = KethaneController.loadResourceDefinitions();
				}
				return KethaneController.resourceDefinitions;
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000219C File Offset: 0x0000039C
		private static IEnumerable<ResourceDefinition> loadResourceDefinitions()
		{
			SortedDictionary<string, ResourceDefinition> sortedDictionary = new SortedDictionary<string, ResourceDefinition>();
			foreach (ResourceDefinition resourceDefinition in from d in GameDatabase.Instance.GetConfigNodes("KethaneResource").Select(new Func<ConfigNode, ResourceDefinition>(KethaneController.TryLoadResourceDefinition))
			where d != null
			select d)
			{
				if (!PartResourceLibrary.Instance.resourceDefinitions.Contains(resourceDefinition.Resource))
				{
					Debug.LogWarning(string.Format("[Kethane] {0} is an unknown resource, ignoring", resourceDefinition.Resource));
				}
				else if (sortedDictionary.ContainsKey(resourceDefinition.Resource))
				{
					Debug.LogWarning(string.Format("[Kethane] Duplicate definition for {0}, ignoring", resourceDefinition.Resource));
				}
				else
				{
					sortedDictionary[resourceDefinition.Resource] = resourceDefinition;
				}
			}
			Debug.Log(string.Format("[Kethane] Loaded {0} resource definitions", sortedDictionary.Count));
			return new ReadOnlyCollection<ResourceDefinition>(sortedDictionary.Values.ToArray<ResourceDefinition>());
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000022B0 File Offset: 0x000004B0
		private static ResourceDefinition TryLoadResourceDefinition(ConfigNode node)
		{
			ResourceDefinition result;
			try
			{
				result = new ResourceDefinition(node);
			}
			catch (Exception arg)
			{
				Debug.LogError(string.Format("[Kethane] Error loading resource definition:\n\n{0}", arg));
				result = null;
			}
			return result;
		}

		// Token: 0x04000004 RID: 4
		private static IEnumerable<ResourceDefinition> resourceDefinitions;
	}
}
