using System;
using System.IO;
using UnityEngine;

namespace Kethane.Utilities
{
	// Token: 0x02000018 RID: 24
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class TutorialInstaller : MonoBehaviour
	{
		// Token: 0x06000095 RID: 149 RVA: 0x00004B74 File Offset: 0x00002D74
		public void Start()
		{
			string text = Path.GetFullPath(KSPUtil.ApplicationRootPath) + "GameData/Kethane/Tutorials/";
			Uri uri = new Uri(text);
			Uri baseUri = new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath) + "saves/training/");
			foreach (FileInfo fileInfo in new DirectoryInfo(text).GetFiles("*", SearchOption.AllDirectories))
			{
				fileInfo.CopyTo(new Uri(baseUri, uri.MakeRelativeUri(new Uri(fileInfo.FullName))).LocalPath, true);
			}
		}
	}
}
