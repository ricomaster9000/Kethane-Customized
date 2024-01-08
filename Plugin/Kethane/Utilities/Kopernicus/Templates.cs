using System;
using System.Reflection;
using UnityEngine;

namespace Kethane.Utilities.Kopernicus
{
	// Token: 0x02000019 RID: 25
	public class Templates
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000097 RID: 151 RVA: 0x00004C00 File Offset: 0x00002E00
		public static string MenuBody
		{
			get
			{
				return (string)Templates.kop_menuBody.GetValue(null);
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00004C14 File Offset: 0x00002E14
		internal static bool Initialize(Assembly kopAsm)
		{
			foreach (Type type in kopAsm.GetTypes())
			{
				if (type.Name == "Templates")
				{
					Templates.KopernicusTemplates_class = type;
					Templates.kop_menuBody = Templates.KopernicusTemplates_class.GetField("MenuBody", BindingFlags.Static | BindingFlags.Public);
					Debug.Log(string.Format("[Kethane] Kopernicus.Templates `{0}'", Templates.kop_menuBody));
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400003C RID: 60
		private static Type KopernicusTemplates_class;

		// Token: 0x0400003D RID: 61
		private static FieldInfo kop_menuBody;
	}
}
