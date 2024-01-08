using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kethane.Utilities
{
	// Token: 0x02000014 RID: 20
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class CompatibilityChecker : MonoBehaviour
	{
		// Token: 0x06000088 RID: 136 RVA: 0x00004505 File Offset: 0x00002705
		public static bool IsCompatible()
		{
			return Versioning.version_major == 1 && Versioning.version_minor >= 8 && Versioning.version_minor <= 12;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00004527 File Offset: 0x00002727
		public static bool IsUnityCompatible()
		{
			return true;
		}

		// Token: 0x0600008A RID: 138 RVA: 0x0000452C File Offset: 0x0000272C
		public void Start()
		{
			FieldInfo[] source = (from t in CompatibilityChecker.getAllTypes()
			where t.Name == "CompatibilityChecker"
			select t.GetField("_version", BindingFlags.Static | BindingFlags.NonPublic) into f
			where f != null
			where f.FieldType == typeof(int)
			select f).ToArray<FieldInfo>();
			if (CompatibilityChecker._version != source.Max((FieldInfo f) => (int)f.GetValue(null)))
			{
				return;
			}
			Debug.Log(string.Format("[CompatibilityChecker] Running checker version {0} from '{1}'", CompatibilityChecker._version, Assembly.GetExecutingAssembly().GetName().Name));
			CompatibilityChecker._version = int.MaxValue;
			string[] array = (from m in (from f in source
			select f.DeclaringType.GetMethod("IsCompatible", Type.EmptyTypes) into m
			where m.IsStatic
			where m.ReturnType == typeof(bool)
			select m).Where(delegate(MethodInfo m)
			{
				bool result;
				try
				{
					result = !(bool)m.Invoke(null, new object[0]);
				}
				catch (Exception arg)
				{
					Debug.LogWarning(string.Format("[CompatibilityChecker] Exception while invoking IsCompatible() from '{0}':\n\n{1}", m.DeclaringType.Assembly.GetName().Name, arg));
					result = true;
				}
				return result;
			})
			select m.DeclaringType.Assembly.GetName().Name).ToArray<string>();
			string[] array2 = (from m in (from f in source
			select f.DeclaringType.GetMethod("IsUnityCompatible", Type.EmptyTypes) into m
			where m != null
			where m.IsStatic
			where m.ReturnType == typeof(bool)
			select m).Where(delegate(MethodInfo m)
			{
				bool result;
				try
				{
					result = !(bool)m.Invoke(null, new object[0]);
				}
				catch (Exception arg)
				{
					Debug.LogWarning(string.Format("[CompatibilityChecker] Exception while invoking IsUnityCompatible() from '{0}':\n\n{1}", m.DeclaringType.Assembly.GetName().Name, arg));
					result = true;
				}
				return result;
			})
			select m.DeclaringType.Assembly.GetName().Name).ToArray<string>();
			Array.Sort<string>(array);
			Array.Sort<string>(array2);
			string text = string.Empty;
			if (array.Length != 0 || array2.Length != 0)
			{
				text = text + ((text == string.Empty) ? "Some" : "\n\nAdditionally, some") + " installed mods may be incompatible with this version of Kerbal Space Program. Features may be broken or disabled. Please check for updates to the listed mods.";
				if (array.Length != 0)
				{
					Debug.LogWarning("[CompatibilityChecker] Incompatible mods detected: " + string.Join(", ", array));
					text += string.Format("\n\nThese mods are incompatible with KSP {0}.{1}.{2}:\n\n", Versioning.version_major, Versioning.version_minor, Versioning.Revision);
					text += string.Join("\n", array);
				}
				if (array2.Length != 0)
				{
					Debug.LogWarning("[CompatibilityChecker] Incompatible mods (Unity) detected: " + string.Join(", ", array2));
					text += string.Format("\n\nThese mods are incompatible with Unity {0}:\n\n", Application.unityVersion);
					text += string.Join("\n", array2);
				}
			}
			if (array.Length != 0 || array2.Length != 0)
			{
				PopupDialog.SpawnPopupDialog(new Vector2(0f, 0f), new Vector2(0f, 0f), "CompatibilityChecker", "Incompatible Mods Detected", text, "OK", true, HighLogic.UISkin, true, "");
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000048FC File Offset: 0x00002AFC
		public static bool IsAllCompatible()
		{
			return CompatibilityChecker.IsCompatible() && CompatibilityChecker.IsUnityCompatible();
		}

		// Token: 0x0600008C RID: 140 RVA: 0x0000490C File Offset: 0x00002B0C
		private static IEnumerable<Type> getAllTypes()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type[] array2;
				try
				{
					array2 = assembly.GetTypes();
				}
				catch (Exception)
				{
					array2 = Type.EmptyTypes;
				}
				foreach (Type type in array2)
				{
					yield return type;
				}
				Type[] array3 = null;
			}
			Assembly[] array = null;
			yield break;
		}

		// Token: 0x0400003B RID: 59
		private static int _version = 5;
	}
}
