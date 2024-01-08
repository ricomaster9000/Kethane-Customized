using System;
using System.Reflection;
using UnityEngine;

namespace Kethane.Utilities.Kopernicus
{
	// Token: 0x0200001A RID: 26
	public class Events
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600009A RID: 154 RVA: 0x00004C7F File Offset: 0x00002E7F
		public static EventVoid OnRuntimeUtilityUpdateMenu
		{
			get
			{
				return (EventVoid)Events.kop_oruum.GetValue(null, null);
			}
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00004C94 File Offset: 0x00002E94
		internal static bool Initialize(Assembly kopAsm)
		{
			foreach (Type type in kopAsm.GetTypes())
			{
				if (type.Name == "Events")
				{
					Events.KopernicusEvents_class = type;
					Events.kop_oruum = Events.KopernicusEvents_class.GetProperty("OnRuntimeUtilityUpdateMenu", BindingFlags.Static | BindingFlags.Public);
					Debug.Log(string.Format("[Kethane] Kopernicus.Events `{0}'", Events.kop_oruum));
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400003E RID: 62
		private static Type KopernicusEvents_class;

		// Token: 0x0400003F RID: 63
		private static PropertyInfo kop_oruum;
	}
}
