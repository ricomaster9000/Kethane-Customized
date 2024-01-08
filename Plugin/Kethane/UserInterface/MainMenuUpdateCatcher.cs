using System;
using Kethane.Utilities.Kopernicus;
using UnityEngine;

namespace Kethane.UserInterface
{
	// Token: 0x0200001D RID: 29
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class MainMenuUpdateCatcher : MonoBehaviour
	{
		// Token: 0x060000A4 RID: 164 RVA: 0x00004F2C File Offset: 0x0000312C
		protected void Start()
		{
			if (KopernicusWrapper.Initialize())
			{
				Events.OnRuntimeUtilityUpdateMenu.Add(new EventVoid.OnEvent(this.UpdateMenu));
			}
			DontDestroyOnLoad(this);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00004F51 File Offset: 0x00003151
		private void OnDestroy()
		{
			if (KopernicusWrapper.Initialize())
			{
				Events.OnRuntimeUtilityUpdateMenu.Remove(new EventVoid.OnEvent(this.UpdateMenu));
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004F70 File Offset: 0x00003170
		private void UpdateMenu()
		{
			GameObject gameObject = FindObjectOfType<MainMenu>().envLogic.areas[1];
			string menuBody = Templates.MenuBody;
			foreach (object obj in gameObject.transform)
			{
				Transform transform = (Transform)obj;
				if (transform.gameObject.activeInHierarchy && transform.name == menuBody)
				{
					Debug.Log("[Kethane] UpdateMenu: " + transform.name);
					MainMenuUpdateCatcher.MenuBody = transform.gameObject;
				}
			}
		}

		// Token: 0x04000047 RID: 71
		public static GameObject MenuBody;
	}
}
