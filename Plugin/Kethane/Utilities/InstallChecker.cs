using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kethane.Utilities
{
	// Token: 0x02000015 RID: 21
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	internal class InstallChecker : MonoBehaviour
	{
		// Token: 0x0600008F RID: 143 RVA: 0x00004920 File Offset: 0x00002B20
		protected void Start()
		{
			IEnumerable<AssemblyLoader.LoadedAssembly> source = from a in AssemblyLoader.loadedAssemblies
			where a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name
			where a.url != "Kethane/Plugins"
			select a;
			if (source.Any<AssemblyLoader.LoadedAssembly>())
			{
				IEnumerable<string> source2 = from a in source
				select a.path into p
				select Uri.UnescapeDataString(new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath)).MakeRelativeUri(new Uri(p)).ToString().Replace('/', Path.DirectorySeparatorChar));
				PopupDialog.SpawnPopupDialog(new Vector2(0f, 0f), new Vector2(0f, 0f), "InstallChecker", "Incorrect Kethane Installation", "Kethane has been installed incorrectly and will not function properly. All Kethane files should be located in KSP/GameData/Kethane. Do not move any files from inside the Kethane folder.\n\nIncorrect path(s):\n" + string.Join("\n", source2.ToArray<string>()), "OK", false, HighLogic.UISkin, true, "");
			}
		}
	}
}
