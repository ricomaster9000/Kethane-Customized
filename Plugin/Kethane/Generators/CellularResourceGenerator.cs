using System;
using System.Collections.Generic;
using System.Linq;
using GeodesicGrid;

namespace Kethane.Generators
{
	// Token: 0x02000036 RID: 54
	public abstract class CellularResourceGenerator : IResourceGenerator
	{
		// Token: 0x06000176 RID: 374 RVA: 0x00009A94 File Offset: 0x00007C94
		public IBodyResources Load(CelestialBody body, ConfigNode node)
		{
			if (node == null)
			{
				CellMap<double> cellMap = new CellMap<double>(5);
				this.Initialize(body, cellMap);
				return new CellularResourceGenerator.BodyResources(new CellMap<double>(cellMap));
			}
			byte[] array = Misc.FromBase64String(node.GetValue("amounts"));
			CellularResourceGenerator.ensureBigEndian(array);
			CellMap<double> cellMap2 = new CellMap<double>(5);
			uint num = Cell.CountAtLevel(5);
			for (uint num2 = 0U; num2 < num; num2 += 1U)
			{
				cellMap2[new Cell(num2)] = BitConverter.ToDouble(array, (int)(num2 * 8U));
			}
			return new CellularResourceGenerator.BodyResources(cellMap2);
		}

		// Token: 0x06000177 RID: 375
		public abstract void Initialize(CelestialBody body, CellMap<double> amounts);

		// Token: 0x06000178 RID: 376 RVA: 0x00009B14 File Offset: 0x00007D14
		private static void ensureBigEndian(byte[] bytes)
		{
			if (BitConverter.IsLittleEndian)
			{
				for (int i = 0; i < bytes.Length; i += 8)
				{
					for (int j = 0; j < 4; j++)
					{
						byte b = bytes[i + j];
						bytes[i + j] = bytes[i + 7 - j];
						bytes[i + 7 - j] = b;
					}
				}
			}
		}

		// Token: 0x0200005B RID: 91
		private class BodyResources : IBodyResources
		{
			// Token: 0x06000224 RID: 548 RVA: 0x0000AAEC File Offset: 0x00008CEC
			public BodyResources(CellMap<double> amounts)
			{
				this.amounts = amounts;
				this.MaxQuantity = amounts.Max((KeyValuePair<Cell, double> p) => p.Value);
			}

			// Token: 0x06000225 RID: 549 RVA: 0x0000AB28 File Offset: 0x00008D28
			public ConfigNode Save()
			{
				uint num = Cell.CountAtLevel(5);
				byte[] array = new byte[num * 8U];
				for (uint num2 = 0U; num2 < num; num2 += 1U)
				{
					byte[] bytes = BitConverter.GetBytes(this.amounts[new Cell(num2)]);
					for (int i = 0; i < 8; i++)
					{
						array[(int)(checked((IntPtr)(unchecked((ulong)(num2 * 8U) + (ulong)((long)i)))))] = bytes[i];
					}
				}
				CellularResourceGenerator.ensureBigEndian(array);
				ConfigNode configNode = new ConfigNode();
				configNode.AddValue("amounts", Misc.ToBase64String(array));
				return configNode;
			}

			// Token: 0x1700003C RID: 60
			// (get) Token: 0x06000226 RID: 550 RVA: 0x0000ABA3 File Offset: 0x00008DA3
			// (set) Token: 0x06000227 RID: 551 RVA: 0x0000ABAB File Offset: 0x00008DAB
			public double MaxQuantity { get; private set; }

			// Token: 0x06000228 RID: 552 RVA: 0x0000ABB4 File Offset: 0x00008DB4
			public double? GetQuantity(Cell cell)
			{
				double? num = new double?(this.amounts[cell]);
				double? num2 = num;
				double num3 = 0.0;
				if (!(num2.GetValueOrDefault() > num3 & num2 != null))
				{
					return null;
				}
				return num;
			}

			// Token: 0x06000229 RID: 553 RVA: 0x0000AC00 File Offset: 0x00008E00
			public double Extract(Cell cell, double amount)
			{
				double num = this.amounts[cell];
				double num2 = Math.Min(num, Math.Max(0.0, amount));
				this.amounts[cell] = num - num2;
				return num2;
			}

			// Token: 0x0400015A RID: 346
			private readonly CellMap<double> amounts;
		}
	}
}
