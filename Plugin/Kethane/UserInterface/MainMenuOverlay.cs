﻿using System.Collections;
using GeodesicGrid;
using System.Linq;
using UnityEngine;

namespace Kethane.UserInterface
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class MainMenuUpdateCatcher : MonoBehaviour
	{
		public static GameObject MenuBody = null;
		public static bool updated = false;

        protected void Start()
        {
			if (Utilities.Kopernicus.KopernicusWrapper.Initialize()) {
				Utilities.Kopernicus.Events.OnRuntimeUtilityUpdateMenu.Add (UpdateMenu);
			}
			GameObject.DontDestroyOnLoad(this);
		}

		void OnDestroy ()
		{
			if (Utilities.Kopernicus.KopernicusWrapper.Initialize()) {
				Utilities.Kopernicus.Events.OnRuntimeUtilityUpdateMenu.Remove (UpdateMenu);
			}
		}

		void UpdateMenu ()
		{
			// if we're in here, Kopernicus is known to have set up a main menu body and thus various safety checks have already been done
			MainMenu main = FindObjectOfType<MainMenu>();
			MainMenuEnvLogic logic = main.envLogic;
			GameObject space = logic.areas[1];
			string menuBody = Utilities.Kopernicus.Templates.MenuBody;
			foreach (Transform t in space.transform) {
				if (t.gameObject.activeInHierarchy && t.name == menuBody) {
					Debug.Log ($"[Kethane] UpdateMenu: {t.name}");
					MenuBody = t.gameObject;
				}
			}
			updated = true;
		}
	}

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    internal class MainMenuOverlay : MonoBehaviour
    {
		private GameObject FindKerbin ()
		{
            var objects = GameObject.FindObjectsOfType(typeof(GameObject));
            if (objects.Any(o => o.name == "LoadingBuffer")) { return null; }
            var kerbin = objects.OfType<GameObject>().Where(b => b.name == "Kerbin").LastOrDefault();

            if (kerbin == null)
            {
                Debug.LogWarning("[Kethane] Couldn't find Kerbin!");
            }
			return kerbin;
		}

        protected IEnumerator Start()
        {
			Debug.Log ($"[Kethane] MainMenuOverlay Start {MainMenuUpdateCatcher.MenuBody}");
			if (Utilities.Kopernicus.KopernicusWrapper.Initialize()) {
				while (!MainMenuUpdateCatcher.updated) {
					yield return null;
				}
				if (MainMenuUpdateCatcher.MenuBody != null) {
					// Kopernicus already fixed things up
					Debug.Log ($"[Kethane] adding overlay to: {MainMenuUpdateCatcher.MenuBody.transform.name}");
					var overlayRenderer = gameObject.AddComponent<OverlayRenderer>();
					overlayRenderer.SetGridLevel(KethaneData.GridLevel);
					overlayRenderer.IsVisible = startMenuOverlay(overlayRenderer, MainMenuUpdateCatcher.MenuBody);
				}
			} else {
				var overlayRenderer = gameObject.AddComponent<OverlayRenderer>();
				overlayRenderer.SetGridLevel(KethaneData.GridLevel);
				overlayRenderer.IsVisible = startMenuOverlay(overlayRenderer, FindKerbin ());
			}
        }

        private bool startMenuOverlay(OverlayRenderer overlayRenderer, GameObject menuBody)
        {
			Debug.Log ($"[Kethane] startMenuOverlay {menuBody}");
            if (!Misc.Parse(SettingsManager.GetValue("ShowInMenu"), true)) { return false; }
			if (menuBody == null) {
				return false;
			}


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
                    {
                        if (random.Next(2) < 1)
                        {
                            colors[neighbor] = color;
                        }
                    }
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
