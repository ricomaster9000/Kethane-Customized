using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kethane
{
	// Token: 0x0200000B RID: 11
	public class ResourceData
	{
		// Token: 0x0600003B RID: 59 RVA: 0x00002FE0 File Offset: 0x000011E0
		protected ResourceData(IResourceGenerator generator, ConfigNode generatorNode, IDictionary<CelestialBody, BodyResourceData> bodies)
		{
			this.generator = generator;
			this.generatorNode = generatorNode;
			this.bodies = new Dictionary<CelestialBody, BodyResourceData>(bodies);
		}

		// Token: 0x17000006 RID: 6
		public BodyResourceData this[CelestialBody body]
		{
			get
			{
				return this.bodies[body];
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x0000301B File Offset: 0x0000121B
		public void ResetBodyData(CelestialBody body)
		{
			this.bodies[body] = BodyResourceData.Load(this.generator, body, null);
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003038 File Offset: 0x00001238
		public static ResourceData Load(ResourceDefinition resource, ConfigNode resourceNode)
		{
			Dictionary<CelestialBody, BodyResourceData> dictionary = new Dictionary<CelestialBody, BodyResourceData>();
			ConfigNode configNode = resourceNode.GetNode("Generator") ?? resource.Generator;
			IResourceGenerator resourceGenerator = ResourceData.createGenerator(configNode.CreateCopy());
			if (resourceGenerator == null)
			{
				Debug.LogWarning("[Kethane] Defaulting to empty generator for " + resource.Resource);
				resourceGenerator = new EmptyResourceGenerator();
			}
			ConfigNode[] nodes = resourceNode.GetNodes("Body");
			using (List<CelestialBody>.Enumerator enumerator = FlightGlobals.Bodies.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CelestialBody body = enumerator.Current;
					ConfigNode bodyNode = nodes.SingleOrDefault((ConfigNode n) => n.GetValue("Name") == body.name);
					dictionary[body] = BodyResourceData.Load(resourceGenerator, body, bodyNode);
				}
			}
			return new ResourceData(resourceGenerator, configNode, dictionary);
		}

		// Token: 0x0600003F RID: 63 RVA: 0x0000311C File Offset: 0x0000131C
		public void Save(ConfigNode resourceNode)
		{
			resourceNode.AddNode(this.generatorNode);
			foreach (KeyValuePair<CelestialBody, BodyResourceData> keyValuePair in this.bodies)
			{
				ConfigNode configNode = new ConfigNode("Body");
				configNode.AddValue("Name", keyValuePair.Key.name);
				keyValuePair.Value.Save(configNode);
				resourceNode.AddNode(configNode);
			}
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000031AC File Offset: 0x000013AC
		private static IResourceGenerator createGenerator(ConfigNode generatorNode)
		{
			var name = generatorNode.GetValue("name");
			if (name == null)
			{
				Debug.LogError("[Kethane] Could not find generator name");
				return null;
			}

			ConstructorInfo constructor = null;
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				try
				{
					constructor = assembly.GetTypes()
						.Where(t => t.Name == name)
						.Where(t => t.GetInterfaces().Contains(typeof(IResourceGenerator)))
						.Select(t => t.GetConstructor(new[] { typeof(ConfigNode) }))
						.FirstOrDefault(c => c != null);

					if (constructor != null) break;
				}
				catch (Exception e)
				{
					Debug.LogWarning("[Kethane] Error inspecting assembly '" + assembly.GetName().Name + "': \n" + e);
				}

			if (constructor == null)
			{
				Debug.LogError("[Kethane] Could not find appropriate constructor for " + name);
				return null;
			}

			try
			{
				return (IResourceGenerator)constructor.Invoke(new object[] { generatorNode });
			}
			catch (Exception e)
			{
				Debug.LogError("[Kethane] Could not instantiate " + name + ":\n" + e);
				return null;
			}
		}

		// Token: 0x04000019 RID: 25
		private IResourceGenerator generator;

		// Token: 0x0400001A RID: 26
		private ConfigNode generatorNode;

		// Token: 0x0400001B RID: 27
		private Dictionary<CelestialBody, BodyResourceData> bodies = new Dictionary<CelestialBody, BodyResourceData>();
	}
}
