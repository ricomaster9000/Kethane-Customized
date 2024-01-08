using System;
using UnityEngine;

namespace Kethane
{
	// Token: 0x0200000C RID: 12
	public class ResourceDefinition
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000041 RID: 65 RVA: 0x0000336C File Offset: 0x0000156C
		// (set) Token: 0x06000042 RID: 66 RVA: 0x00003374 File Offset: 0x00001574
		public string Resource { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000043 RID: 67 RVA: 0x0000337D File Offset: 0x0000157D
		// (set) Token: 0x06000044 RID: 68 RVA: 0x00003385 File Offset: 0x00001585
		public Color ColorFull { get; private set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000045 RID: 69 RVA: 0x0000338E File Offset: 0x0000158E
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00003396 File Offset: 0x00001596
		public Color ColorEmpty { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000047 RID: 71 RVA: 0x0000339F File Offset: 0x0000159F
		// (set) Token: 0x06000048 RID: 72 RVA: 0x000033A7 File Offset: 0x000015A7
		public ConfigNode Generator { get; private set; }

		// Token: 0x06000049 RID: 73 RVA: 0x000033B0 File Offset: 0x000015B0
		public ResourceDefinition(ConfigNode node)
		{
			this.Resource = node.GetValue("Resource");
			string value = node.GetValue("ColorFull");
			this.ColorFull = ((value != null) ? ConfigNode.ParseColor(value) : Color.white);
			string value2 = node.GetValue("ColorEmpty");
			this.ColorEmpty = ((value2 != null) ? ConfigNode.ParseColor(value2) : Color.white);
			this.Generator = (node.GetNode("Generator") ?? new ConfigNode());
		}
	}
}
