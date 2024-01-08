using System;
using Kethane.Particles;
using UnityEngine;

namespace Kethane.PartModules
{
	// Token: 0x02000031 RID: 49
	public class KethaneParticleEmitter : PartModule
	{
		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000161 RID: 353 RVA: 0x000095C0 File Offset: 0x000077C0
		// (set) Token: 0x06000162 RID: 354 RVA: 0x000095D8 File Offset: 0x000077D8
		public bool Emit
		{
			get
			{
				return this.emitter != null && this.emitter.enabled;
			}
			set
			{
				if (!this.bad && value && this.emitter == null)
				{
					this.emitter = ParticleManager.CreateEmitter(this.System);
					if (this.emitter == null)
					{
						this.bad = true;
						return;
					}
					this.emitter.position = this.position;
					this.emitter.direction = this.direction;
					Utils.CalcAxes(this.emitter.direction, out this.emitter.spreadX, out this.emitter.spreadY);
					this.emitter.spreadX *= this.spread.x;
					this.emitter.spreadY *= this.spread.y;
				}
				if (this.emitter != null)
				{
					this.emitter.enabled = value;
				}
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000163 RID: 355 RVA: 0x000096BF File Offset: 0x000078BF
		// (set) Token: 0x06000164 RID: 356 RVA: 0x000096DA File Offset: 0x000078DA
		public float Rate
		{
			get
			{
				if (this.emitter != null)
				{
					return this.emitter.rate;
				}
				return 0f;
			}
			set
			{
				if (this.emitter == null)
				{
					this.Emit = false;
				}
				if (this.emitter != null)
				{
					this.emitter.rate = value;
				}
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000165 RID: 357 RVA: 0x000096FF File Offset: 0x000078FF
		// (set) Token: 0x06000166 RID: 358 RVA: 0x0000971B File Offset: 0x0000791B
		public Vector3 Position
		{
			get
			{
				if (this.emitter != null)
				{
					return this.emitter.position;
				}
				return this.position;
			}
			set
			{
				if (this.emitter == null)
				{
					this.Emit = false;
				}
				if (this.emitter != null)
				{
					this.emitter.position = value;
				}
				this.position = value;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000167 RID: 359 RVA: 0x00009747 File Offset: 0x00007947
		// (set) Token: 0x06000168 RID: 360 RVA: 0x00009762 File Offset: 0x00007962
		public Vector3 Direction
		{
			get
			{
				if (this.emitter != null)
				{
					return this.emitter.direction;
				}
				return Vector3.up;
			}
			set
			{
				if (this.emitter == null)
				{
					this.Emit = false;
				}
				if (this.emitter != null)
				{
					this.emitter.direction = value;
				}
			}
		}

		// Token: 0x06000169 RID: 361 RVA: 0x000029A8 File Offset: 0x00000BA8
		public override void OnLoad(ConfigNode config)
		{
		}

		// Token: 0x0600016A RID: 362 RVA: 0x000029A8 File Offset: 0x00000BA8
		public override void OnSave(ConfigNode config)
		{
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00009787 File Offset: 0x00007987
		public override void OnStart(PartModule.StartState state)
		{
			bool loadedSceneIsFlight = HighLogic.LoadedSceneIsFlight;
		}

		// Token: 0x040000C3 RID: 195
		private ParticleManager.Emitter emitter;

		// Token: 0x040000C4 RID: 196
		private bool bad;

		// Token: 0x040000C5 RID: 197
		[KSPField(isPersistant = false)]
		public string Label;

		// Token: 0x040000C6 RID: 198
		[KSPField(isPersistant = false)]
		public string System;

		// Token: 0x040000C7 RID: 199
		[KSPField(isPersistant = false)]
		public Vector3 position;

		// Token: 0x040000C8 RID: 200
		[KSPField(isPersistant = false)]
		public Vector3 direction = Vector3.up;

		// Token: 0x040000C9 RID: 201
		[KSPField(isPersistant = false)]
		public Vector2 spread;
	}
}
