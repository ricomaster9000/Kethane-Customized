using System;
using UnityEngine;

namespace Kethane
{
	// Token: 0x0200000D RID: 13
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class SettingsManager : MonoBehaviour
	{
		// Token: 0x0600004A RID: 74 RVA: 0x00003432 File Offset: 0x00001632
		public static string GetValue(string key)
		{
			SettingsManager.load();
			return SettingsManager.node.GetValue(key);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003444 File Offset: 0x00001644
		public static void SetValue(string key, object value)
		{
			SettingsManager.load();
			if (SettingsManager.node.HasValue(key))
			{
				SettingsManager.node.RemoveValue(key);
			}
			SettingsManager.node.AddValue(key, value);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000346F File Offset: 0x0000166F
		public static void Save()
		{
			SettingsManager.load();
			Debug.LogWarning("[Kethane] Saving settings");
			SettingsManager.node.Save(SettingsManager.settingsFile);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003490 File Offset: 0x00001690
		private static void load()
		{
			if (SettingsManager.node != null)
			{
				return;
			}
			Debug.LogWarning("[Kethane] Loading settings");
			SettingsManager.node = (ConfigNode.Load(SettingsManager.settingsFile) ?? new ConfigNode());
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600004E RID: 78 RVA: 0x000034BC File Offset: 0x000016BC
		private static string settingsFile
		{
			get
			{
				return KSPUtil.ApplicationRootPath + "GameData/Kethane/settings.cfg";
			}
		}

		// Token: 0x0600004F RID: 79 RVA: 0x000034CD File Offset: 0x000016CD
		public void Awake()
		{
			DontDestroyOnLoad(this);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000034D5 File Offset: 0x000016D5
		public void Destroy()
		{
			SettingsManager.Save();
		}

		// Token: 0x04000020 RID: 32
		private static ConfigNode node;
	}
}
