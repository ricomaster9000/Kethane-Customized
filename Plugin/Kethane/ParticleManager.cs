using System;
using System.Collections.Generic;
using KethaneParticles;
using UnityEngine;

namespace Kethane
{
	// Token: 0x02000009 RID: 9
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class ParticleManager : MonoBehaviour
	{
		// Token: 0x0600002A RID: 42 RVA: 0x00002808 File Offset: 0x00000A08
		private ParticleManager.Emitter createEmitter(string system)
		{
			ParticleManager.System system2;
			if (!this.systems.TryGetValue(system, out system2))
			{
				system2 = ParticleManager.createSystem(system);
				if (system2 == null)
				{
					return null;
				}
				this.systems[system] = system2;
			}
			ParticleManager.Emitter emitter = new ParticleManager.Emitter();
			emitter.system = system2;
			system2.physics.AddEmitter(emitter);
			return emitter;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002858 File Offset: 0x00000A58
		public static ParticleManager.Emitter CreateEmitter(string system)
		{
			return ParticleManager.instance.createEmitter(system);
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002868 File Offset: 0x00000A68
		private static ParticleManager.System createSystem(string name)
		{
			ConfigNode configNode;
			if (!ParticleManager.systemCfg.TryGetValue(name, out configNode))
			{
				Debug.Log("[Kethane.ParticleManager] unknown system: " + name);
				return null;
			}
			GameObject gameObject = new GameObject("KethaneParticleSystem:" + name);
			Debug.Log("[Kethane.ParticleManager] creating " + gameObject.name);
			ParticleSystemCfg particleSystemCfg = new ParticleSystemCfg(gameObject);
			particleSystemCfg.Load(configNode.GetNode("Particles"));
			ParticleManager.System system = new ParticleManager.System();
			system.psystem = particleSystemCfg.psystem;
			system.physics = gameObject.AddComponent<ParticlePhysics>();
			system.physics.psystem = particleSystemCfg.psystem;
			system.physics.Load(configNode.GetNode("Physics"));
			system.psystem.Play();
			return system;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002924 File Offset: 0x00000B24
		private static void LoadSystemConfigs()
		{
			ConfigNode[] configNodes = GameDatabase.Instance.GetConfigNodes("KethaneParticleSystem");
			ParticleManager.systemCfg = new Dictionary<string, ConfigNode>();
			foreach (ConfigNode configNode in configNodes)
			{
				string value = configNode.GetValue("name");
				if (string.IsNullOrEmpty(value))
				{
					Debug.Log("[Kethane.ParticleManager] skipping unnamed system");
				}
				else if (ParticleManager.systemCfg.ContainsKey(value))
				{
					Debug.Log("[Kethane.ParticleManager] duplicate system name: " + value);
				}
				else
				{
					ParticleManager.systemCfg[value] = configNode;
				}
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000029A8 File Offset: 0x00000BA8
		private void onFloatingOriginShift(Vector3d refPos, Vector3d nonFrame)
		{
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000029AA File Offset: 0x00000BAA
		private void Awake()
		{
			ParticleManager.instance = this;
			this.systems = new Dictionary<string, ParticleManager.System>();
			if (ParticleManager.systemCfg == null)
			{
				ParticleManager.LoadSystemConfigs();
			}
			GameEvents.onFloatingOriginShift.Add(new EventData<Vector3d, Vector3d>.OnEvent(this.onFloatingOriginShift));
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000029DF File Offset: 0x00000BDF
		private void OnDestroy()
		{
			Debug.Log("[ParticleManager] OnDestroy");
			GameEvents.onFloatingOriginShift.Remove(new EventData<Vector3d, Vector3d>.OnEvent(this.onFloatingOriginShift));
		}

		// Token: 0x0400000A RID: 10
		private static Dictionary<string, ConfigNode> systemCfg;

		// Token: 0x0400000B RID: 11
		private static ParticleManager instance;

		// Token: 0x0400000C RID: 12
		private Dictionary<string, ParticleManager.System> systems;

		// Token: 0x0200003D RID: 61
		public class System
		{
			// Token: 0x040000D7 RID: 215
			public ParticleSystem psystem;

			// Token: 0x040000D8 RID: 216
			public ParticlePhysics physics;
		}

		// Token: 0x0200003E RID: 62
		public class Emitter
		{
			// Token: 0x040000D9 RID: 217
			public bool enabled;

			// Token: 0x040000DA RID: 218
			public float rate;

			// Token: 0x040000DB RID: 219
			public Vector3 position;

			// Token: 0x040000DC RID: 220
			public Vector3 direction;

			// Token: 0x040000DD RID: 221
			public Vector3 spreadX;

			// Token: 0x040000DE RID: 222
			public Vector3 spreadY;

			// Token: 0x040000DF RID: 223
			public ParticleManager.System system;

			// Token: 0x040000E0 RID: 224
			public float emission;
		}
	}
}
