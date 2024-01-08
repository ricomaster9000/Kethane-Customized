using System;
using GeodesicGrid;
using UnityEngine;

namespace Kethane
{
	// Token: 0x02000002 RID: 2
	public class BodyResourceData
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		// (set) Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
		public IBodyResources Resources { get; private set; }

		// Token: 0x06000003 RID: 3 RVA: 0x00002061 File Offset: 0x00000261
		protected BodyResourceData(IBodyResources resources, CellSet scans)
		{
			this.Resources = resources;
			this.scans = scans;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002077 File Offset: 0x00000277
		public bool IsCellScanned(Cell cell)
		{
			return this.scans[cell];
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002085 File Offset: 0x00000285
		public void ScanCell(Cell cell)
		{
			this.scans[cell] = true;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002094 File Offset: 0x00000294
		public static BodyResourceData Load(IResourceGenerator generator, CelestialBody body, ConfigNode bodyNode)
		{
			if (bodyNode == null)
			{
				bodyNode = new ConfigNode();
			}
			IBodyResources resources = generator.Load(body, bodyNode.GetNode("GeneratorData"));
			CellSet cellSet = new CellSet(5);
			string value = bodyNode.GetValue("ScanMask");
			if (value != null)
			{
				try
				{
					cellSet = new CellSet(5, Misc.FromBase64String(value));
				}
				catch (FormatException ex)
				{
					Debug.LogError(string.Format("[Kethane] Failed to parse {0} scan string, resetting ({1})", body.name, ex.Message));
				}
			}
			return new BodyResourceData(resources, cellSet);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002118 File Offset: 0x00000318
		public void Save(ConfigNode bodyNode)
		{
			bodyNode.AddValue("ScanMask", Misc.ToBase64String(this.scans.ToByteArray()));
			ConfigNode configNode = this.Resources.Save() ?? new ConfigNode();
			configNode.name = "GeneratorData";
			bodyNode.AddNode(configNode);
		}

		// Token: 0x04000001 RID: 1
		private readonly CellSet scans;
	}
}
