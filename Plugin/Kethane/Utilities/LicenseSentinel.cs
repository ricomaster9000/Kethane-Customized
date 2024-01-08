using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Kethane.Utilities
{
	// Token: 0x02000017 RID: 23
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class LicenseSentinel : MonoBehaviour
	{
		// Token: 0x06000093 RID: 147 RVA: 0x00004B0C File Offset: 0x00002D0C
		protected void Start()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = executingAssembly.Location;
			if (!Path.GetFileName(text).Equals("Kethane.dll", StringComparison.OrdinalIgnoreCase))
			{
				text += "-Kethane";
			}
			else
			{
				text = Path.ChangeExtension(text, null);
			}
			string contents = new StreamReader(executingAssembly.GetManifestResourceStream("Kethane.Kethane-LICENSE.txt")).ReadToEnd();
			File.WriteAllText(text + "-LICENSE.txt", contents);
		}
	}
}
