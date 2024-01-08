using System;
using System.Collections.Generic;
using Kethane.Particles;
using UnityEngine;

namespace Kethane.ShaderLoader
{
	// Token: 0x02000013 RID: 19
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class KethaneShaderLoader : MonoBehaviour
	{
		// Token: 0x06000084 RID: 132 RVA: 0x000043FC File Offset: 0x000025FC
		private void Start()
		{
			ParticleSystemCfg.FindShader = new ParticleSystemCfg.FindShaderDelegate(KethaneShaderLoader.FindShader);
			this.LoadShaders();
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00004418 File Offset: 0x00002618
		private void LoadShaders()
		{
			string text = "kethane.ksp";
			if (KethaneShaderLoader.shaderDictionary == null)
			{
				KethaneShaderLoader.shaderDictionary = new Dictionary<string, Shader>();
				AssetBundle assetBundle = AssetBundle.LoadFromFile(KSPUtil.ApplicationRootPath + "GameData/Kethane/" + text);
				if (!assetBundle)
				{
					Debug.LogFormat("[Kethane] Could not load {0}", new object[]
					{
						text
					});
					return;
				}
				foreach (Shader shader in assetBundle.LoadAllAssets<Shader>())
				{
					Debug.LogFormat("[Kethane] Shader {0} loaded", new object[]
					{
						shader.name
					});
					KethaneShaderLoader.shaderDictionary[shader.name] = shader;
				}
				assetBundle.Unload(false);
				KethaneShaderLoader.loaded = true;
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000044C6 File Offset: 0x000026C6
		public static Shader FindShader(string name)
		{
			if (KethaneShaderLoader.shaderDictionary == null)
			{
				Debug.Log("[Kethane] Trying to find shader before assets loaded");
				return null;
			}
			if (KethaneShaderLoader.shaderDictionary.ContainsKey(name))
			{
				return KethaneShaderLoader.shaderDictionary[name];
			}
			MonoBehaviour.print("[Kethane] Could not find shader " + name);
			return null;
		}

		// Token: 0x04000038 RID: 56
		private const string gamedata = "GameData/Kethane/";

		// Token: 0x04000039 RID: 57
		private static Dictionary<string, Shader> shaderDictionary;

		// Token: 0x0400003A RID: 58
		public static bool loaded;
	}
}
