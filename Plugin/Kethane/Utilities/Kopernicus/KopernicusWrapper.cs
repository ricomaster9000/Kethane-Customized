using System;
using UnityEngine;

namespace Kethane.Utilities.Kopernicus
{
	// Token: 0x0200001B RID: 27
	public class KopernicusWrapper
	{
		// Token: 0x0600009D RID: 157 RVA: 0x00004D00 File Offset: 0x00002F00
		public static bool Initialize()
		{
			if (!KopernicusWrapper.inited)
			{
				KopernicusWrapper.inited = true;
				AssemblyLoader.LoadedAssembly loadedAssembly = null;
				foreach (AssemblyLoader.LoadedAssembly loadedAssembly2 in AssemblyLoader.loadedAssemblies)
				{
					if (loadedAssembly2.assembly.GetName().Name.Equals("Kopernicus", StringComparison.InvariantCultureIgnoreCase))
					{
						loadedAssembly = loadedAssembly2;
					}
				}
				if (loadedAssembly != null)
				{
					Debug.Log(string.Format("[Kethane] found Kopernicus {0}", loadedAssembly));
					Events.Initialize(loadedAssembly.assembly);
					Templates.Initialize(loadedAssembly.assembly);
					KopernicusWrapper.haveKopernicus = true;
				}
			}
			return KopernicusWrapper.haveKopernicus;
		}

		// Token: 0x04000040 RID: 64
		private static bool inited;

		// Token: 0x04000041 RID: 65
		private static bool haveKopernicus;
	}
}
