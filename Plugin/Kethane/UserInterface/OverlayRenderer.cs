using System;
using System.Collections.Generic;
using GeodesicGrid;
using Kethane.ShaderLoader;
using UnityEngine;

namespace Kethane.UserInterface
{
	// Token: 0x02000020 RID: 32
	internal class OverlayRenderer : MonoBehaviour
	{
		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000CB RID: 203 RVA: 0x0000642C File Offset: 0x0000462C
		// (set) Token: 0x060000CC RID: 204 RVA: 0x00006456 File Offset: 0x00004656
		public bool IsVisible
		{
			get
			{
				MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
				return !(component == null) && component.enabled;
			}
			set
			{
				MeshRenderer component = base.gameObject.GetComponent<MeshRenderer>();
				if (component == null)
				{
					throw new InvalidOperationException("OverlayRenderer has not started");
				}
				component.enabled = value;
			}
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000647D File Offset: 0x0000467D
		protected void Awake()
		{
			this.setUpComponents();
			this.updateArrays();
			this.updateTriangles();
			this.updateVertices();
			this.updateTarget();
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000649D File Offset: 0x0000469D
		protected void Update()
		{
			if (this.colorsDirty)
			{
				this.colorsDirty = false;
				this.mesh.colors32 = this.colors32;
			}
		}

		// Token: 0x060000CF RID: 207 RVA: 0x000064BF File Offset: 0x000046BF
		public void SetGridLevel(int gridLevel)
		{
			this.SetGridLevelAndHeightMap(gridLevel, this.heightMap);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x000064CE File Offset: 0x000046CE
		public void SetHeightMap(Func<Cell, float> heightMap)
		{
			this.SetGridLevelAndHeightMap(this.gridLevel, heightMap);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x000064E0 File Offset: 0x000046E0
		public void SetGridLevelAndHeightMap(int gridLevel, Func<Cell, float> heightMap)
		{
			if (gridLevel < 0)
			{
				throw new ArgumentOutOfRangeException("gridLevel");
			}
			if (heightMap == null)
			{
				throw new ArgumentNullException("heightMap");
			}
			if (gridLevel != this.gridLevel)
			{
				this.gridLevel = gridLevel;
				this.updateArrays();
				this.updateTriangles();
			}
			else if (heightMap == this.heightMap)
			{
				return;
			}
			this.heightMap = heightMap;
			this.updateVertices();
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x00006544 File Offset: 0x00004744
		public void SetRadiusMultiplier(float radiusMultiplier)
		{
			if (radiusMultiplier < 0f)
			{
				throw new ArgumentOutOfRangeException("radiusMultiplier");
			}
			if (radiusMultiplier != this.radiusMultiplier)
			{
				this.radiusMultiplier = radiusMultiplier;
				this.updateScale();
			}
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x0000656F File Offset: 0x0000476F
		public void SetTarget(Transform target)
		{
			if (target != this.target)
			{
				this.target = target;
				this.updateTarget();
			}
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x0000658C File Offset: 0x0000478C
		public void SetCellColor(Cell cell, Color32 color)
		{
			this.setCellColor(cell, color);
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00006596 File Offset: 0x00004796
		public void SetCellColors(IDictionary<Cell, Color32> assignments)
		{
			this.setCellColors(assignments);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00006596 File Offset: 0x00004796
		public void SetCellColors(CellMap<Color32> assignments)
		{
			this.setCellColors(assignments);
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x000065A0 File Offset: 0x000047A0
		private void setCellColors(IEnumerable<KeyValuePair<Cell, Color32>> assignments)
		{
			foreach (KeyValuePair<Cell, Color32> keyValuePair in assignments)
			{
				this.setCellColor(keyValuePair.Key, keyValuePair.Value);
			}
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x000065F8 File Offset: 0x000047F8
		private void setCellColor(Cell cell, Color32 color)
		{
			uint num = cell.Index * 6U;
			for (uint num2 = num; num2 < num + 6U; num2 += 1U)
			{
				this.colors32[(int)num2] = color;
			}
			this.colorsDirty = true;
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x00006634 File Offset: 0x00004834
		private void setUpComponents()
		{
			this.mesh = base.gameObject.AddComponent<MeshFilter>().mesh;
			MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			meshRenderer.enabled = false;
			meshRenderer.shadowCastingMode = 0;
			meshRenderer.receiveShadows = false;
			Shader shader = KethaneShaderLoader.FindShader("Kethane/AlphaUnlitVertexColored");
			if (shader != null)
			{
				Material material = new Material(shader);
				Color white = Color.white;
				white.a = 0.4f;
				material.color = white;
				meshRenderer.material = material;
			}
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000066B4 File Offset: 0x000048B4
		private void updateArrays()
		{
			this.mesh.Clear();
			uint num = Cell.CountAtLevel(this.gridLevel);
			this.vertices = new Vector3[num * 6U];
			this.colors32 = new Color32[this.vertices.Length];
			this.triangles = new int[3U * (4U * (num - 12U) + 60U)];
			this.mesh.vertices = this.vertices;
			this.mesh.colors32 = this.colors32;
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00006734 File Offset: 0x00004934
		private void updateTriangles()
		{
			int num = 0;
			foreach (Cell cell in Cell.AtLevel(this.gridLevel))
			{
				int num2 = (int)(cell.Index * 6U);
				if (cell.IsPentagon)
				{
					int i = 0;
					while (i < 5)
					{
						this.triangles[num] = num2 + 1 + i;
						this.triangles[num + 1] = num2 + 1 + (i + 1) % 5;
						this.triangles[num + 2] = num2;
						i++;
						num += 3;
					}
				}
				else
				{
					int j = 0;
					while (j < 3)
					{
						this.triangles[num] = num2 + j * 2;
						this.triangles[num + 1] = num2 + 1 + j * 2;
						this.triangles[num + 2] = num2 + (2 + j * 2) % 6;
						j++;
						num += 3;
					}
					this.triangles[num] = num2;
					this.triangles[num + 1] = num2 + 2;
					this.triangles[num + 2] = num2 + 4;
					num += 3;
				}
			}
			this.mesh.triangles = this.triangles;
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00006860 File Offset: 0x00004A60
		private void updateVertices()
		{
			int num = 0;
			foreach (Cell arg in Cell.AtLevel(this.gridLevel))
			{
				Vector3 vector = arg.Position * this.heightMap(arg);
				if (arg.IsPentagon)
				{
					this.vertices[num++] = vector;
				}
				float num2 = 0.08f;
				vector *= num2;
				foreach (Vector3 vector2 in arg.GetVertices(this.gridLevel, this.heightMap))
				{
					this.vertices[num++] = vector + vector2 * (1f - num2);
				}
			}
			this.mesh.vertices = this.vertices;
			this.mesh.RecalculateBounds();
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00006980 File Offset: 0x00004B80
		private void updateTarget()
		{
			if (this.target != null)
			{
				base.gameObject.layer = this.target.gameObject.layer;
			}
			base.gameObject.transform.parent = this.target;
			base.gameObject.transform.localPosition = Vector3.zero;
			base.gameObject.transform.localRotation = Quaternion.identity;
			this.updateScale();
		}

		// Token: 0x060000DE RID: 222 RVA: 0x000069FC File Offset: 0x00004BFC
		private void updateScale()
		{
			base.gameObject.transform.localScale = Vector3.one * 1000f * this.radiusMultiplier;
		}

		// Token: 0x0400005E RID: 94
		private Mesh mesh;

		// Token: 0x0400005F RID: 95
		private int gridLevel;

		// Token: 0x04000060 RID: 96
		private Func<Cell, float> heightMap = (Cell c) => 1f;

		// Token: 0x04000061 RID: 97
		private float radiusMultiplier = 1f;

		// Token: 0x04000062 RID: 98
		private Transform target;

		// Token: 0x04000063 RID: 99
		private Vector3[] vertices;

		// Token: 0x04000064 RID: 100
		private Color32[] colors32;

		// Token: 0x04000065 RID: 101
		private int[] triangles;

		// Token: 0x04000066 RID: 102
		private bool colorsDirty;
	}
}
