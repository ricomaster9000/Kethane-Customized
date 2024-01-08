using System;

namespace Kethane
{
	// Token: 0x02000003 RID: 3
	public interface IResourceGenerator
	{
		// Token: 0x06000008 RID: 8
		IBodyResources Load(CelestialBody body, ConfigNode node);
	}
}
