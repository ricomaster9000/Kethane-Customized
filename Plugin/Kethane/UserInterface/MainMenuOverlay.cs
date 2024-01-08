using System;
using System.Collections.Generic;
using System.Linq;
using GeodesicGrid;
using Kethane.Utilities.Kopernicus;
using UnityEngine;

namespace Kethane.UserInterface
{
	// Token: 0x0200001E RID: 30
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	internal class MainMenuOverlay : MonoBehaviour
	{
		// Token: 0x060000A8 RID: 168 RVA: 0x00005014 File Offset: 0x00003214
		private GameObject FindKerbin()
		{
			var objects = FindObjectsOfType(typeof(GameObject));
			if (objects.Any(o => o.name == "LoadingBuffer")) return null;
			var kerbin = objects.OfType<GameObject>().Where(b => b.name == "Kerbin").LastOrDefault();

			if (kerbin == null) Debug.LogWarning("[Kethane] Couldn't find Kerbin!");
			return kerbin;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x0000509C File Offset: 0x0000329C
		protected void Start()
		{
			if (KopernicusWrapper.Initialize())
			{
				if (MainMenuUpdateCatcher.MenuBody != null)
				{
					Debug.Log("[Kethane] adding overlay to: " + MainMenuUpdateCatcher.MenuBody.transform.name);
					OverlayRenderer overlayRenderer = base.gameObject.AddComponent<OverlayRenderer>();
					overlayRenderer.SetGridLevel(5);
					overlayRenderer.IsVisible = this.startMenuOverlay(overlayRenderer, MainMenuUpdateCatcher.MenuBody);
					return;
				}
			}
			else
			{
				OverlayRenderer overlayRenderer2 = base.gameObject.AddComponent<OverlayRenderer>();
				overlayRenderer2.SetGridLevel(5);
				overlayRenderer2.IsVisible = this.startMenuOverlay(overlayRenderer2, this.FindKerbin());
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00005128 File Offset: 0x00003328
		private bool startMenuOverlay(OverlayRenderer overlayRenderer, GameObject menuBody)
		{
			Debug.Log($"[Kethane] startMenuOverlay {menuBody}");
			if (!Misc.Parse(SettingsManager.GetValue("ShowInMenu"), true)) return false;
			if (menuBody == null) return false;


			overlayRenderer.SetTarget(menuBody.transform);
			overlayRenderer.SetRadiusMultiplier(1.02f);

			var random = new System.Random();
			var colors = new CellMap<Color32>(KethaneData.GridLevel);

			foreach (var cell in Cell.AtLevel(KethaneData.GridLevel))
			{
				var rand = random.Next(100);
				Color32 color;
				if (rand < 16)
				{
					color = rand < 4 ? new Color32(21, 176, 26, 255) : new Color32(128, 128, 128, 192);
					foreach (var neighbor in cell.GetNeighbors(KethaneData.GridLevel))
						if (random.Next(2) < 1)
							colors[neighbor] = color;
				}
				else
				{
					color = new Color32(0, 0, 0, 128);
				}

				colors[cell] = color;
			}

			overlayRenderer.SetCellColors(colors);

			return true;
		}
	}
}
