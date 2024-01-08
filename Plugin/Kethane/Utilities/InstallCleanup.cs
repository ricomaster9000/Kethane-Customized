using System;
using System.IO;
using UnityEngine;

namespace Kethane.Utilities
{
	// Token: 0x02000016 RID: 22
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class InstallCleanup : MonoBehaviour
	{
		// Token: 0x06000091 RID: 145 RVA: 0x00004A2C File Offset: 0x00002C2C
		public void Start()
		{
			File.Delete(string.Concat(new string[]
			{
				KSPUtil.ApplicationRootPath,
				"GameData",
				Path.DirectorySeparatorChar.ToString(),
				"Kethane",
				Path.DirectorySeparatorChar.ToString(),
				"Plugins",
				Path.DirectorySeparatorChar.ToString(),
				"MMI_Kethane.dll"
			}));
			File.Delete(string.Concat(new string[]
			{
				KSPUtil.ApplicationRootPath,
				"GameData",
				Path.DirectorySeparatorChar.ToString(),
				"Kethane",
				Path.DirectorySeparatorChar.ToString(),
				"Plugins",
				Path.DirectorySeparatorChar.ToString(),
				"KethaneToolbar.dll"
			}));
		}
	}
}
