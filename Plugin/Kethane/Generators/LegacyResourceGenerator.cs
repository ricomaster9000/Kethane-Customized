using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeodesicGrid;
using UnityEngine;

namespace Kethane.Generators
{
	// Token: 0x02000037 RID: 55
	internal class LegacyResourceGenerator : IResourceGenerator
	{
		// Token: 0x0600017A RID: 378 RVA: 0x00009B5C File Offset: 0x00007D5C
		public LegacyResourceGenerator(ConfigNode node)
		{
			this.config = new LegacyResourceGenerator.GeneratorConfiguration(node);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00009B70 File Offset: 0x00007D70
		public IBodyResources Load(CelestialBody body, ConfigNode node)
		{
			return new LegacyResourceGenerator.BodyDeposits(this.config.ForBody(body), node);
		}

		// Token: 0x040000D2 RID: 210
		private LegacyResourceGenerator.GeneratorConfiguration config;

		// Token: 0x0200005C RID: 92
		private class GeneratorConfiguration
		{
			// Token: 0x1700003D RID: 61
			// (get) Token: 0x0600022A RID: 554 RVA: 0x0000AC40 File Offset: 0x00008E40
			// (set) Token: 0x0600022B RID: 555 RVA: 0x0000AC48 File Offset: 0x00008E48
			public float MinRadius { get; private set; }

			// Token: 0x1700003E RID: 62
			// (get) Token: 0x0600022C RID: 556 RVA: 0x0000AC51 File Offset: 0x00008E51
			// (set) Token: 0x0600022D RID: 557 RVA: 0x0000AC59 File Offset: 0x00008E59
			public float MaxRadius { get; private set; }

			// Token: 0x1700003F RID: 63
			// (get) Token: 0x0600022E RID: 558 RVA: 0x0000AC62 File Offset: 0x00008E62
			// (set) Token: 0x0600022F RID: 559 RVA: 0x0000AC6A File Offset: 0x00008E6A
			public double MinQuantity { get; private set; }

			// Token: 0x17000040 RID: 64
			// (get) Token: 0x06000230 RID: 560 RVA: 0x0000AC73 File Offset: 0x00008E73
			// (set) Token: 0x06000231 RID: 561 RVA: 0x0000AC7B File Offset: 0x00008E7B
			public double MaxQuantity { get; private set; }

			// Token: 0x17000041 RID: 65
			// (get) Token: 0x06000232 RID: 562 RVA: 0x0000AC84 File Offset: 0x00008E84
			// (set) Token: 0x06000233 RID: 563 RVA: 0x0000AC8C File Offset: 0x00008E8C
			public int MinVertices { get; private set; }

			// Token: 0x17000042 RID: 66
			// (get) Token: 0x06000234 RID: 564 RVA: 0x0000AC95 File Offset: 0x00008E95
			// (set) Token: 0x06000235 RID: 565 RVA: 0x0000AC9D File Offset: 0x00008E9D
			public int MaxVertices { get; private set; }

			// Token: 0x17000043 RID: 67
			// (get) Token: 0x06000236 RID: 566 RVA: 0x0000ACA6 File Offset: 0x00008EA6
			// (set) Token: 0x06000237 RID: 567 RVA: 0x0000ACAE File Offset: 0x00008EAE
			public float RadiusVariance { get; private set; }

			// Token: 0x17000044 RID: 68
			// (get) Token: 0x06000238 RID: 568 RVA: 0x0000ACB7 File Offset: 0x00008EB7
			// (set) Token: 0x06000239 RID: 569 RVA: 0x0000ACBF File Offset: 0x00008EBF
			public int DepositCount { get; private set; }

			// Token: 0x17000045 RID: 69
			// (get) Token: 0x0600023A RID: 570 RVA: 0x0000ACC8 File Offset: 0x00008EC8
			// (set) Token: 0x0600023B RID: 571 RVA: 0x0000ACD0 File Offset: 0x00008ED0
			public int NumberOfTries { get; private set; }

			// Token: 0x17000046 RID: 70
			// (get) Token: 0x0600023C RID: 572 RVA: 0x0000ACD9 File Offset: 0x00008ED9
			// (set) Token: 0x0600023D RID: 573 RVA: 0x0000ACE1 File Offset: 0x00008EE1
			public bool CanReplenish { get; private set; }

			// Token: 0x17000047 RID: 71
			// (get) Token: 0x0600023E RID: 574 RVA: 0x0000ACEA File Offset: 0x00008EEA
			// (set) Token: 0x0600023F RID: 575 RVA: 0x0000ACF2 File Offset: 0x00008EF2
			public double MinHalfLife { get; private set; }

			// Token: 0x17000048 RID: 72
			// (get) Token: 0x06000240 RID: 576 RVA: 0x0000ACFB File Offset: 0x00008EFB
			// (set) Token: 0x06000241 RID: 577 RVA: 0x0000AD03 File Offset: 0x00008F03
			public double MaxHalfLife { get; private set; }

			// Token: 0x06000242 RID: 578 RVA: 0x0000AD0C File Offset: 0x00008F0C
			public GeneratorConfiguration(ConfigNode node)
			{
				this.load(node);
				foreach (ConfigNode configNode in node.GetNodes("Body"))
				{
					LegacyResourceGenerator.GeneratorConfiguration generatorConfiguration = (LegacyResourceGenerator.GeneratorConfiguration)base.MemberwiseClone();
					generatorConfiguration.load(configNode);
					this.bodies[configNode.GetValue("name")] = generatorConfiguration;
				}
			}

			// Token: 0x06000243 RID: 579 RVA: 0x0000AD79 File Offset: 0x00008F79
			public LegacyResourceGenerator.GeneratorConfiguration ForBody(CelestialBody body)
			{
				if (!this.bodies.ContainsKey(body.name))
				{
					return this;
				}
				return this.bodies[body.name];
			}

			// Token: 0x06000244 RID: 580 RVA: 0x0000ADA4 File Offset: 0x00008FA4
			private void load(ConfigNode node)
			{
				this.MinRadius = Misc.Parse(node.GetValue("MinRadius"), this.MinRadius);
				this.MaxRadius = Misc.Parse(node.GetValue("MaxRadius"), this.MaxRadius);
				this.MinQuantity = Misc.Parse(node.GetValue("MinQuantity"), this.MinQuantity);
				this.MaxQuantity = Misc.Parse(node.GetValue("MaxQuantity"), this.MaxQuantity);
				this.MinVertices = Misc.Parse(node.GetValue("MinVertices"), this.MinVertices);
				this.MaxVertices = Misc.Parse(node.GetValue("MaxVertices"), this.MaxVertices);
				this.RadiusVariance = Misc.Parse(node.GetValue("RadiusVariance"), this.RadiusVariance);
				this.DepositCount = Misc.Parse(node.GetValue("DepositCount"), this.DepositCount);
				this.NumberOfTries = Misc.Parse(node.GetValue("NumberOfTries"), this.NumberOfTries);
				this.CanReplenish = Misc.Parse(node.GetValue("CanReplenish"), this.CanReplenish);
				this.MinHalfLife = Misc.Parse(node.GetValue("MinHalfLife"), this.MinHalfLife);
				this.MaxHalfLife = Misc.Parse(node.GetValue("MaxHalfLife"), this.MaxHalfLife);
			}

			// Token: 0x04000168 RID: 360
			private Dictionary<string, LegacyResourceGenerator.GeneratorConfiguration> bodies = new Dictionary<string, LegacyResourceGenerator.GeneratorConfiguration>();
		}

		// Token: 0x0200005D RID: 93
		private class BodyDeposits : IBodyResources
		{
			// Token: 0x17000049 RID: 73
			// (get) Token: 0x06000245 RID: 581 RVA: 0x0000AF01 File Offset: 0x00009101
			// (set) Token: 0x06000246 RID: 582 RVA: 0x0000AF09 File Offset: 0x00009109
			public double MaxQuantity { get; private set; }

			// Token: 0x1700004A RID: 74
			// (get) Token: 0x06000247 RID: 583 RVA: 0x0000AF12 File Offset: 0x00009112
			// (set) Token: 0x06000248 RID: 584 RVA: 0x0000AF1A File Offset: 0x0000911A
			public bool CanReplenish { get; private set; }

			// Token: 0x06000249 RID: 585 RVA: 0x0000AF24 File Offset: 0x00009124
			public BodyDeposits(GeneratorConfiguration resource, ConfigNode node)
		    {
		        if (node == null) node = new ConfigNode();

		        deposits = new List<Deposit>();
		        seed = Misc.Parse(node.GetValue("Seed"), seedGenerator.Next());

		        var random = new System.Random(seed);

		        for (var i = 0; i < resource.DepositCount; i++)
		        {
		            var R = random.Range(resource.MinRadius, resource.MaxRadius);
		            for (var j = 0; j < resource.NumberOfTries; j++)
		            {
		                var Pos = new Vector2(random.Range(R, 360 - R), random.Range(R, 180 - R));
		                var deposit = Deposit.Generate(Pos, R, random, resource);
		                if (!deposits.Any(d =>
		                        d.Shape.Vertices.Any(v => deposit.Shape.PointInPolygon(new Vector2(v.x, v.y)))) &&
		                    !deposit.Shape.Vertices.Any(v =>
		                        deposits.Any(d => d.Shape.PointInPolygon(new Vector2(v.x, v.y)))))
		                {
		                    deposits.Add(deposit);
		                    break;
		                }
		            }
		        }

		        if (resource.CanReplenish)
		            // generated separately to allow updating older saves without messing with their deposites due to changes in the RNG sequence.
		            for (var i = 0; i < deposits.Count; i++)
		            {
		                // MinHalfLife and MaxHalfLife are specified in hours
		                var halflife = 3600 * random.Range(resource.MinHalfLife, resource.MaxHalfLife);
		                deposits[i].Lambda = Math.Log(2) / halflife;
		            }

		        var depositValues = node.GetValues("Deposit");
		        for (var i = 0; i < Math.Min(deposits.Count, depositValues.Length); i++)
		        {
		            var split = depositValues[i].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		            deposits[i].Quantity = Misc.Parse(split[0], deposits[i].InitialQuantity);
		            if (split.Length >= 2 && resource.CanReplenish)
		                deposits[i].LastUT = Misc.Parse(split[1], deposits[i].LastUT);
		        }

		        MaxQuantity = resource.MaxQuantity;
		        CanReplenish = resource.CanReplenish;
		    }

			// Token: 0x0600024A RID: 586 RVA: 0x0000B180 File Offset: 0x00009380
			private LegacyResourceGenerator.Deposit getDeposit(Cell cell)
			{
				Vector3 position = cell.Position;
				float num = (float)(Math.Atan2((double)position.y, Math.Sqrt((double)(position.x * position.x + position.z * position.z))) * 180.0 / 3.141592653589793);
				float num2 = (float)(Math.Atan2((double)position.z, (double)position.x) * 180.0 / 3.141592653589793);
				float x = num2 + 180f;
				float y = 90f - num;
				return this.deposits.FirstOrDefault((LegacyResourceGenerator.Deposit d) => d.Shape.PointInPolygon(new Vector2(x, y)));
			}

			// Token: 0x0600024B RID: 587 RVA: 0x0000B238 File Offset: 0x00009438
			public double? GetQuantity(Cell cell)
			{
				LegacyResourceGenerator.Deposit deposit = this.getDeposit(cell);
				if (deposit == null)
				{
					return null;
				}
				return new double?(deposit.Quantity);
			}

			// Token: 0x0600024C RID: 588 RVA: 0x0000B268 File Offset: 0x00009468
			public double Extract(Cell cell, double amount)
			{
				LegacyResourceGenerator.Deposit deposit = this.getDeposit(cell);
				if (deposit == null)
				{
					return 0.0;
				}
				double quantity = deposit.Quantity;
				double num = Math.Min(quantity, Math.Max(0.0, amount));
				deposit.Quantity = quantity - num;
				return num;
			}

			// Token: 0x0600024D RID: 589 RVA: 0x0000B2B4 File Offset: 0x000094B4
			public ConfigNode Save()
			{
				ConfigNode configNode = new ConfigNode();
				configNode.AddValue("Seed", this.seed);
				foreach (LegacyResourceGenerator.Deposit deposit in this.deposits)
				{
					if (this.CanReplenish)
					{
						configNode.AddValue("Deposit", string.Format("{0:G17},{1:G17}", deposit.Quantity, deposit.LastUT));
					}
					else
					{
						configNode.AddValue("Deposit", deposit.Quantity);
					}
				}
				return configNode;
			}

			// Token: 0x04000169 RID: 361
			private readonly List<LegacyResourceGenerator.Deposit> deposits;

			// Token: 0x0400016A RID: 362
			private readonly int seed;

			// Token: 0x0400016B RID: 363
			private static System.Random seedGenerator = new System.Random();
		}

		// Token: 0x0200005E RID: 94
		private class Deposit
		{
			// Token: 0x1700004B RID: 75
			// (get) Token: 0x06000250 RID: 592 RVA: 0x0000B39D File Offset: 0x0000959D
			// (set) Token: 0x06000251 RID: 593 RVA: 0x0000B3A5 File Offset: 0x000095A5
			public double Quantity { get; set; }

			// Token: 0x1700004C RID: 76
			// (get) Token: 0x06000252 RID: 594 RVA: 0x0000B3AE File Offset: 0x000095AE
			// (set) Token: 0x06000253 RID: 595 RVA: 0x0000B3B6 File Offset: 0x000095B6
			public double InitialQuantity { get; set; }

			// Token: 0x1700004D RID: 77
			// (get) Token: 0x06000254 RID: 596 RVA: 0x0000B3BF File Offset: 0x000095BF
			// (set) Token: 0x06000255 RID: 597 RVA: 0x0000B3C7 File Offset: 0x000095C7
			public double Lambda { get; set; }

			// Token: 0x1700004E RID: 78
			// (get) Token: 0x06000256 RID: 598 RVA: 0x0000B3D0 File Offset: 0x000095D0
			// (set) Token: 0x06000257 RID: 599 RVA: 0x0000B3D8 File Offset: 0x000095D8
			public double LastUT { get; set; }

			// Token: 0x06000258 RID: 600 RVA: 0x0000B3E1 File Offset: 0x000095E1
			public Deposit(LegacyResourceGenerator.Polygon shape, double quantity, double initialQuantity)
			{
				this.Shape = shape;
				this.Quantity = quantity;
				this.InitialQuantity = initialQuantity;
				this.Lambda = 0.0;
			}

			// Token: 0x06000259 RID: 601 RVA: 0x0000B410 File Offset: 0x00009610
			public static LegacyResourceGenerator.Deposit Generate(Vector2 Pos, float radius, System.Random random, LegacyResourceGenerator.GeneratorConfiguration resource)
			{
				double num = random.Range(resource.MinQuantity, resource.MaxQuantity);
				List<Vector2> list = new List<Vector2>();
				int num2 = random.Next(resource.MinVertices, resource.MaxVertices);
				for (int i = 0; i < num2; i++)
				{
					float num3 = random.Range(resource.RadiusVariance * radius, radius);
					float num4 = 6.2831855f * ((float)i / (float)num2);
					float num5 = Pos.x + num3 * (float)Math.Cos((double)num4);
					float num6 = Pos.y - num3 * (float)Math.Sin((double)num4);
					list.Add(new Vector2(num5, num6));
				}
				return new LegacyResourceGenerator.Deposit(new LegacyResourceGenerator.Polygon(list.ToArray()), num, num);
			}

			// Token: 0x0400016E RID: 366
			public LegacyResourceGenerator.Polygon Shape;
		}

		// Token: 0x0200005F RID: 95
		private class Polygon
		{
			// Token: 0x0600025A RID: 602 RVA: 0x0000B4BE File Offset: 0x000096BE
			public Polygon(Vector2[] vertices)
			{
				this._vertices = vertices.ToArray<Vector2>();
			}

			// Token: 0x1700004F RID: 79
			// (get) Token: 0x0600025B RID: 603 RVA: 0x0000B4D2 File Offset: 0x000096D2
			public ReadOnlyCollection<Vector2> Vertices
			{
				get
				{
					return new ReadOnlyCollection<Vector2>(this._vertices);
				}
			}

			// Token: 0x0600025C RID: 604 RVA: 0x0000B4E0 File Offset: 0x000096E0
			public bool PointInPolygon(Vector2 p)
			{
				bool flag = false;
				int i = 0;
				int num = this._vertices.Length - 1;
				while (i < this._vertices.Length)
				{
					if (this._vertices[i].y > p.y != this._vertices[num].y > p.y && p.x < (this._vertices[num].x - this._vertices[i].x) * (p.y - this._vertices[i].y) / (this._vertices[num].y - this._vertices[i].y) + this._vertices[i].x)
					{
						flag = !flag;
					}
					num = i++;
				}
				return flag;
			}

			// Token: 0x04000173 RID: 371
			private Vector2[] _vertices;
		}
	}
}
