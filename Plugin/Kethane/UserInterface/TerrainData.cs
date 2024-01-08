using System;
using System.Collections.Generic;
using GeodesicGrid;

namespace Kethane.UserInterface
{
	// Token: 0x02000021 RID: 33
	public class TerrainData
	{
		// Token: 0x060000E0 RID: 224 RVA: 0x00006A60 File Offset: 0x00004C60
		public static void Clear()
		{
			TerrainData.bodies.Clear();
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x00006A6C File Offset: 0x00004C6C
		public static TerrainData ForBody(CelestialBody body)
		{
			if (body == null)
			{
				throw new ArgumentException("Body may not be null");
			}
			if (!TerrainData.bodies.ContainsKey(body.name))
			{
				TerrainData.bodies[body.name] = new TerrainData(body);
			}
			return TerrainData.bodies[body.name];
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x00006AC8 File Offset: 0x00004CC8
		private TerrainData(CelestialBody body)
		{
			if (body.pqsController == null)
			{
				throw new ArgumentException("Body doesn't have a PQS controller");
			}
			this.heightRatios = new CellMap<float>(5, (Cell c) => (float)(body.pqsController.GetSurfaceHeight(c.Position) / body.pqsController.radius));
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00006B1E File Offset: 0x00004D1E
		public float GetHeightRatio(Cell cell)
		{
			return this.heightRatios[cell];
		}

		// Token: 0x04000067 RID: 103
		private static readonly Dictionary<string, TerrainData> bodies = new Dictionary<string, TerrainData>();

		// Token: 0x04000068 RID: 104
		private readonly CellMap<float> heightRatios;
	}
}
