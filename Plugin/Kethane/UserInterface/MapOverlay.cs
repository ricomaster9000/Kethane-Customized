using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GeodesicGrid;
using KSP.IO;
using KSP.UI.Screens;
using UnityEngine;

namespace Kethane.UserInterface
{
	// Token: 0x0200001F RID: 31
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class MapOverlay : MonoBehaviour
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00005270 File Offset: 0x00003470
		// (set) Token: 0x060000AD RID: 173 RVA: 0x00005277 File Offset: 0x00003477
		public static MapOverlay Instance { get; private set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x060000AE RID: 174 RVA: 0x0000527F File Offset: 0x0000347F
		// (set) Token: 0x060000AF RID: 175 RVA: 0x00005286 File Offset: 0x00003486
		public static string SelectedResource { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x0000528E File Offset: 0x0000348E
		// (set) Token: 0x060000B1 RID: 177 RVA: 0x00005295 File Offset: 0x00003495
		public static bool ShowOverlay { get; set; }

		// Token: 0x060000B2 RID: 178 RVA: 0x0000529D File Offset: 0x0000349D
		private static void button_OnTrue()
		{
			MapOverlay.enable_control_window = true;
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000052A5 File Offset: 0x000034A5
		private static void button_OnFalse()
		{
			MapOverlay.enable_control_window = false;
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x000052AD File Offset: 0x000034AD
		private void onHideUI()
		{
			MapOverlay.hide_ui = true;
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x000052B5 File Offset: 0x000034B5
		private void onShowUI()
		{
			MapOverlay.hide_ui = false;
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x000052C0 File Offset: 0x000034C0
		static MapOverlay()
		{
			MapOverlay.controlWindowPos.x = Misc.Parse(SettingsManager.GetValue("WindowLeft"), 200f);
			MapOverlay.controlWindowPos.y = Misc.Parse(SettingsManager.GetValue("WindowTop"), 200f);
			MapOverlay.ShowOverlay = Misc.Parse(SettingsManager.GetValue("ShowOverlay"), true);
			MapOverlay.SelectedResource = "Kethane";
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x000053CD File Offset: 0x000035CD
		public static Cell GetCellUnder(CelestialBody body, Vector3 worldPosition)
		{
			return Cell.Containing(body.transform.InverseTransformPoint(worldPosition), 5);
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x000053E1 File Offset: 0x000035E1
		public static Cell GetCellUnder(CelestialBody body, double latitude, double longitude)
		{
			return Cell.Containing(body.GetRelSurfaceNVector(latitude, longitude), 5);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x000053F6 File Offset: 0x000035F6
		public void Awake()
		{
			base.enabled = true;
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00005400 File Offset: 0x00003600
		public void Start()
		{
			DontDestroyOnLoad(this);
			GameEvents.onGUIApplicationLauncherReady.Add(new EventVoid.OnEvent(this.OnGUIAppLauncherReady));
			GameEvents.onHideUI.Add(new EventVoid.OnEvent(this.onHideUI));
			GameEvents.onShowUI.Add(new EventVoid.OnEvent(this.onShowUI));
			MapOverlay.Instance = this;
			this.overlayRenderer = base.gameObject.AddComponent<OverlayRenderer>();
			this.overlayRenderer.SetGridLevel(5);
			ConfigNode configNode = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/Kethane/Grid.cfg");
			if (configNode == null)
			{
				return;
			}
			foreach (CelestialBody celestialBody in FlightGlobals.Bodies)
			{
				double value;
				if (double.TryParse(configNode.GetValue(celestialBody.name), out value))
				{
					this.bodyRadii[celestialBody] = value;
				}
			}
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000054F4 File Offset: 0x000036F4
		public void OnDestroy()
		{
			SettingsManager.SetValue("ShowOverlay", ShowOverlay);
			SettingsManager.SetValue("WindowLeft", MapOverlay.controlWindowPos.x);
			SettingsManager.SetValue("WindowTop", MapOverlay.controlWindowPos.y);
			SettingsManager.Save();

			GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
			GameEvents.onHideUI.Remove (onHideUI);
			GameEvents.onShowUI.Remove (onShowUI);
			Instance = null;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00005594 File Offset: 0x00003794
		private void OnGUIAppLauncherReady()
		{
			if (ApplicationLauncher.Ready && button == null)
			{
				var tex = GameDatabase.Instance.GetTexture("Kethane/toolbar", false);
				button = ApplicationLauncher.Instance.AddModApplication(null, null, null, null, null, null, ApplicationLauncher.AppScenes.MAINMENU | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION, tex);
				button.onTrue = button_OnTrue;
				button.onFalse = button_OnFalse;
			}
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0000560A File Offset: 0x0000380A
		public void ClearBody()
		{
			this.body = null;
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00005614 File Offset: 0x00003814
		public void Update()
		{
			if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				overlayRenderer.IsVisible = false;
				return;
			}
			if (!MapView.MapIsEnabled || !MapOverlay.ShowOverlay || MapView.MapCamera == null || KethaneData.Current == null)
			{
				this.overlayRenderer.IsVisible = false;
				return;
			}
			this.overlayRenderer.IsVisible = true;
			CelestialBody targetBody = MapOverlay.getTargetBody(MapView.MapCamera.target);
			var bodyChanged = (targetBody != null) && (targetBody != body);
			if (bodyChanged)
			{
				this.body = targetBody;
				this.heightAt = this.getHeightRatioMap();
				this.bounds = new BoundsMap(this.heightAt, 5);
				this.overlayRenderer.SetHeightMap(this.heightAt);
				double num = this.bodyRadii.ContainsKey(this.body) ? this.bodyRadii[this.body] : 1.025;
				this.overlayRenderer.SetRadiusMultiplier((float)num);
				this.overlayRenderer.SetTarget(this.body.MapObject.transform);
			}
			if (bodyChanged || this.resource == null || this.resource.Resource != MapOverlay.SelectedResource)
			{
				this.resource = (from r in KethaneController.ResourceDefinitions
				where r.Resource == MapOverlay.SelectedResource
				select r).Single<ResourceDefinition>();
				this.refreshCellColors();
			}
			Ray ray = PlanetariumCamera.Camera.ScreenPointToRay(Input.mousePosition);
			this.hoverCell = Cell.Raycast(ray, 5, this.bounds, this.heightAt, base.gameObject.transform);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000057CA File Offset: 0x000039CA
		public void RefreshCellColor(Cell cell, CelestialBody body)
		{
			if (body != this.body)
			{
				return;
			}
			this.overlayRenderer.SetCellColor(cell, this.getCellColor(cell, body, KethaneData.Current));
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000057F4 File Offset: 0x000039F4
		private void refreshCellColors()
		{
			KethaneData data = KethaneData.Current;
			CellMap<Color32> cellColors = new CellMap<Color32>(5, (Cell c) => this.getCellColor(c, this.body, data));
			this.overlayRenderer.SetCellColors(cellColors);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00005838 File Offset: 0x00003A38
		private Color32 getCellColor(Cell cell, CelestialBody body, KethaneData data)
		{
			BodyResourceData bodyResourceData = data[this.resource.Resource][body];
			double? quantity = bodyResourceData.Resources.GetQuantity(cell);
			bool flag = bodyResourceData.IsCellScanned(cell);
			if (!(MapOverlay.revealAll ? (quantity != null) : flag))
			{
				return MapOverlay.colorUnknown;
			}
			return MapOverlay.getDepositColor(this.resource, bodyResourceData, quantity);
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00005898 File Offset: 0x00003A98
		public static Color32 GetCellColor(Cell cell, BodyResourceData bodyResources, ResourceDefinition resource)
		{
			double? quantity = bodyResources.Resources.GetQuantity(cell);
			bool flag = bodyResources.IsCellScanned(cell);
			if (!(MapOverlay.revealAll ? (quantity != null) : flag))
			{
				return MapOverlay.colorUnknown;
			}
			return MapOverlay.getDepositColor(resource, bodyResources, quantity);
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x000058DC File Offset: 0x00003ADC
		private static Color32 getDepositColor(ResourceDefinition definition, BodyResourceData bodyResources, double? deposit)
		{
			Color32 result;
			if (deposit != null)
			{
				float num = (float)(deposit.Value / bodyResources.Resources.MaxQuantity);
				result = definition.ColorFull * num + definition.ColorEmpty * (1f - num);
			}
			else
			{
				result = MapOverlay.colorEmpty;
			}
			return result;
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x0000593C File Offset: 0x00003B3C
		public void OnGUI()
		{
			if (HighLogic.LoadedScene != GameScenes.FLIGHT && HighLogic.LoadedScene != GameScenes.TRACKSTATION) { return; }
			if (MapOverlay.hide_ui)
			{
				return;
			}
			if (!MapView.MapIsEnabled || MapView.MapCamera == null)
			{
				return;
			}
			if (this.skin == null)
			{
				GUI.skin = null;
				this.skin = Instantiate<GUISkin>(GUI.skin);
				GUIStyle window = this.skin.window;
				window.padding = new RectOffset(6, 6, 20, 6);
				window.fontSize = 10;
				this.skin.window = window;
				GUIStyle label = this.skin.label;
				label.margin = new RectOffset(1, 1, 1, 1);
				label.padding = new RectOffset(1, 1, 1, 1);
				label.fontSize = 10;
				this.skin.label = label;
			}
			GUI.skin = this.skin;
			if (hoverCell != null && ShowOverlay)
			{
				var mouse = Event.current.mousePosition;
				var position = new Rect(mouse.x + 16, mouse.y + 4, 160, 32);
				GUILayout.Window(12359, position, mouseWindow, "Resource Info");
			}

			if (MapOverlay.defaultSkin == null)
			{
				GUI.skin = null;
				MapOverlay.defaultSkin = Instantiate<GUISkin>(GUI.skin);
			}
			if (centeredStyle == null)
			{
				centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
				centeredStyle.alignment = TextAnchor.MiddleCenter;
			}
			if (MapOverlay.minMaxStyle == null)
			{
				MapOverlay.minMaxStyle = new GUIStyle(GUI.skin.button);
				MapOverlay.minMaxStyle.contentOffset = new Vector2(-1f, 0f);
			}
			if (MapOverlay.button == null || !MapOverlay.enable_control_window)
			{
				return;
			}
			GUI.skin = defaultSkin;
			var oldBackground = GUI.backgroundColor;
			GUI.backgroundColor = XKCDColors.Green;

			controlWindowPos = GUILayout.Window(12358, controlWindowPos, controlWindow, "Kethane Scan Map");

			GUI.backgroundColor = oldBackground;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00005B60 File Offset: 0x00003D60
		private void mouseWindow(int windowId)
		{
			var cell = hoverCell.Value;
			var pos = cell.Position;
			var lat = (float)(Math.Atan2(pos.y, Math.Sqrt(pos.x * pos.x + pos.z * pos.z)) * 180 / Math.PI);
			var lon = (float)(Math.Atan2(pos.z, pos.x) * 180 / Math.PI);

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Center:");
			GUILayout.FlexibleSpace();
			GUILayout.Label(String.Format("{0:F1} {1}, {2:F1} {3}", Math.Abs(lat), lat < 0 ? "S" : "N", Math.Abs(lon), lon < 0 ? "W" : "E"));
			GUILayout.EndHorizontal();

			foreach (var definition in KethaneController.ResourceDefinitions)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(String.Format("{0}:", definition.Resource));
				GUILayout.FlexibleSpace();
				if (revealAll || KethaneData.Current[definition.Resource][body].IsCellScanned(cell))
				{
					var deposit = KethaneData.Current[definition.Resource][body].Resources.GetQuantity(cell);
					GUILayout.Label(deposit != null ? String.Format("{0:N1}", deposit.Value) : "(none)");
				}
				else
				{
					GUILayout.Label("(no data)");
				}

				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00005D8C File Offset: 0x00003F8C
		private void controlWindow(int windowID)
		{
			 GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            var defs = KethaneController.ResourceDefinitions.Select(d => d.Resource).ToList();

            GUI.enabled = defs.Count > 1;
            if (GUILayout.Button("◀", GUILayout.ExpandWidth(false)))
            {
                SelectedResource = defs.LastOrDefault(s => s.CompareTo(SelectedResource) < 0) ?? defs.Last();
            }
            GUI.enabled = true;

            GUILayout.Label(SelectedResource, centeredStyle, GUILayout.ExpandWidth(true));

            GUI.enabled = defs.Count > 1;
            if (GUILayout.Button("▶", GUILayout.ExpandWidth(false)))
            {
                SelectedResource = defs.FirstOrDefault(s => s.CompareTo(SelectedResource) > 0) ?? defs.First();
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            ShowOverlay = GUILayout.Toggle(ShowOverlay, "Show Grid Overlay");

            if (debugEnabled)
            {
                var vessel = FlightGlobals.ActiveVessel;
                if (vessel != null && vessel.mainBody != body) { vessel = null; }

                GUILayout.BeginVertical(GUI.skin.box);

                if (revealAll != GUILayout.Toggle(revealAll, "Reveal Unscanned Cells"))
                {
                    revealAll = !revealAll;
                    refreshCellColors();
                }

                if (GUILayout.Button("Reset " + (body ? body.name : "[null]") + " Data"))
                {
                    KethaneData.Current[resource.Resource].ResetBodyData(body);
                    refreshCellColors();
                }

                if (GUILayout.Button("Reset Generator Config"))
                {
                    KethaneData.Current.ResetGeneratorConfig(resource);
                    refreshCellColors();
                }

                if (GUILayout.Button("Export Data (" + (revealAll ? "All" : "Scanned") + ")"))
                {
                    export();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, controlWindowPos.width, controlWindowPos.height));
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x0000603C File Offset: 0x0000423C
		private static void export()
		{
			var sb = new StringBuilder();

			var cells = new CellMap<string>(KethaneData.GridLevel);
			foreach (var cell in Cell.AtLevel(KethaneData.GridLevel))
			{
				var pos = cell.Position;
				var lat = (float)(Math.Atan2(pos.y, Math.Sqrt(pos.x * pos.x + pos.z * pos.z)) * 180 / Math.PI);
				var lon = (float)(Math.Atan2(pos.z, pos.x) * 180 / Math.PI);
				cells[cell] = String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},", cell.Index, lat, lon, pos.x, pos.y, pos.z);
			}

			sb.AppendLine("body,resource,cellId,lat,lon,x,y,z,scanned,quantity");

			foreach (var body in FlightGlobals.Bodies)
			{
				foreach (var resource in KethaneController.ResourceDefinitions)
				{
					foreach (var cell in Cell.AtLevel(KethaneData.GridLevel))
					{
						var scanned = KethaneData.Current[resource.Resource][body].IsCellScanned(cell);
						var deposit = KethaneData.Current[resource.Resource][body].Resources.GetQuantity(cell);

						sb.Append(String.Format("{0},{1},", body.name, resource.Resource));
						sb.Append(cells[cell]);
						sb.Append(scanned ? "true" : "false");
						if ((revealAll || scanned) && deposit != null)
						{
							sb.Append(String.Format(CultureInfo.InvariantCulture, ",{0}", deposit.Value));
						}
						else
						{
							sb.Append(",");
						}
						sb.AppendLine();
					}
				}
			}

			KSP.IO.File.WriteAllText<KethaneController>(sb.ToString(), "kethane_export.csv");
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00006360 File Offset: 0x00004560
		private static CelestialBody getTargetBody(MapObject target)
		{
			if (target.type == MapObject.ObjectType.CelestialBody)
			{
				return target.celestialBody;
			}
			else if (target.type == MapObject.ObjectType.ManeuverNode)
			{
				return target.maneuverNode.patch.referenceBody;
			}
			else if (target.type == MapObject.ObjectType.Vessel)
			{
				return target.vessel.mainBody;
			}

			return null;
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x000063B0 File Offset: 0x000045B0
		private Func<Cell, float> getHeightRatioMap()
		{
			Func<Cell, float> result;
			try
			{
				TerrainData bodyTerrain = TerrainData.ForBody(this.body);
				result = ((Cell c) => Math.Max(1f, bodyTerrain.GetHeightRatio(c)));
			}
			catch (ArgumentException)
			{
				result = ((Cell c) => 1f);
			}
			return result;
		}

		// Token: 0x0400004B RID: 75
		private CelestialBody body;

		// Token: 0x0400004C RID: 76
		private Dictionary<CelestialBody, double> bodyRadii = new Dictionary<CelestialBody, double>();

		// Token: 0x0400004D RID: 77
		private GUISkin skin;

		// Token: 0x0400004E RID: 78
		private Cell? hoverCell;

		// Token: 0x0400004F RID: 79
		private ResourceDefinition resource;

		// Token: 0x04000050 RID: 80
		private Func<Cell, float> heightAt;

		// Token: 0x04000051 RID: 81
		private BoundsMap bounds;

		// Token: 0x04000052 RID: 82
		private OverlayRenderer overlayRenderer;

		// Token: 0x04000053 RID: 83
		private static GUIStyle centeredStyle = null;

		// Token: 0x04000054 RID: 84
		private static GUIStyle minMaxStyle = null;

		// Token: 0x04000055 RID: 85
		private static GUISkin defaultSkin = null;

		// Token: 0x04000056 RID: 86
		private static Rect controlWindowPos = new Rect(0f, 0f, 160f, 0f);

		// Token: 0x04000057 RID: 87
		private static bool revealAll = false;

		// Token: 0x04000058 RID: 88
		private static bool enable_control_window = false;

		// Token: 0x04000059 RID: 89
		private static bool hide_ui = false;

		// Token: 0x0400005A RID: 90
		private static ApplicationLauncherButton button;

		// Token: 0x0400005B RID: 91
		private static readonly Color32 colorEmpty = Misc.Parse(SettingsManager.GetValue("ColorEmpty"), new Color32(128, 128, 128, 192));

		// Token: 0x0400005C RID: 92
		private static readonly Color32 colorUnknown = Misc.Parse(SettingsManager.GetValue("ColorUnknown"), new Color32(0, 0, 0, 128));

		// Token: 0x0400005D RID: 93
		private static readonly bool debugEnabled = Misc.Parse(SettingsManager.GetValue("Debug"), false);
	}
}
