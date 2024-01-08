using System;
using System.Collections.Generic;
using Kethane.Particles;
using UnityEngine;

namespace Kethane
{
	// Token: 0x0200000A RID: 10
	public class ParticlePhysics : MonoBehaviour
	{
		// Token: 0x06000032 RID: 50 RVA: 0x00002A0C File Offset: 0x00000C0C
		public void Load(ConfigNode node)
		{
			this.gravity = Utils.ParseVector3(node, "gravity", this.gravity);
			this.dispersion = Utils.ParseFloat(node, "dispersion", this.dispersion);
			this.drag = Utils.ParseFloat(node, "drag", this.drag);
			this.density = Utils.ParseFloat(node, "density", this.density);
			this.densityExp = Utils.ParseFloat(node, "densityExp", this.densityExp);
			this.particleDensity = Utils.ParseFloat(node, "particleDensity", this.particleDensity);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002AA8 File Offset: 0x00000CA8
		public void AddEmitter(ParticleManager.Emitter emitter)
		{
			this.emitters.Add(emitter);
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002AB6 File Offset: 0x00000CB6
		private void Awake()
		{
			this.emitters = new List<ParticleManager.Emitter>();
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002AC3 File Offset: 0x00000CC3
		private void Start()
		{
			this.pressure = new ParticlePhysics.FloatVolume(new Vector3(400f, 400f, 400f), 200, 200, 200);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002AF4 File Offset: 0x00000CF4
		private void FixedUpdate()
		{
			if (this.psystem == null)
			{
				return;
			}
			float fixedDeltaTime = Time.fixedDeltaTime;
			this.InitializeIfNeeded();
			Vector3 vector = Vector3.up;
			if (FlightGlobals.ActiveVessel != null)
			{
				CelestialBody mainBody = FlightGlobals.ActiveVessel.mainBody;
				this.gravity = FlightGlobals.ActiveVessel.gravityForPos;
				double altitudeAtPos = FlightGlobals.getAltitudeAtPos(Vector3d.zero, mainBody);
				double staticPressure = FlightGlobals.getStaticPressure(altitudeAtPos, mainBody);
				double temperature = mainBody.GetTemperature(altitudeAtPos);
				this.density = (float)FlightGlobals.getAtmDensity(staticPressure, temperature, mainBody);
				vector = -this.gravity.normalized;
			}
			int num = this.psystem.GetParticles(this.particles);
			int num2 = num;
			while (num2-- > 0)
			{
				float currentSize = this.particles[num2].GetCurrentSize(this.psystem);
				this.part_sizes[num2] = currentSize;
				ParticlePhysics.FloatVolume floatVolume = this.pressure;
				Vector3 position = this.particles[num2].position;
				floatVolume[position] += currentSize * Mathf.Exp(-currentSize);
			}
			int num3 = num;
			while (num3-- > 0)
			{
				Vector3 position2 = this.particles[num3].position;
				Vector3 velocity = this.particles[num3].velocity;
				Vector3 vector2 = this.pressure.Gradient(position2);
				float num4 = this.density * Mathf.Exp(this.densityExp * Vector3.Dot(position2, vector));
				float num5 = (this.particleDensity - num4) / this.particleDensity;
				Vector3 vector3 = this.gravity * num5;
				vector3 += -vector2 * this.part_sizes[num3] * this.dispersion;
				vector3 -= velocity * this.drag * num4;
				if (position2.y < 0f)
				{
					vector3.y = -position2.y;
				}
				Vector3 vector4 = velocity;
				this.particles[num3].velocity = vector4 + vector3 * fixedDeltaTime;
			}
			int maxParticles = this.psystem.main.maxParticles;
			int count = this.emitters.Count;
			int num6 = this.lastEmitter;
			int num7 = 0;
			while (num7 < count && num < maxParticles)
			{
				num6 = (this.lastEmitter + num7) % count;
				ParticleManager.Emitter emitter = this.emitters[num6];
				if (emitter.enabled)
				{
					emitter.emission += emitter.rate * fixedDeltaTime;
					int num8 = (int)emitter.emission;
					emitter.emission -= (float)num8;
					while (num8-- > 0 && num < maxParticles)
					{
						this.SetParticle(emitter, ref this.particles[num++]);
					}
				}
				num7++;
			}
			this.lastEmitter = (num6 + 1) % count;
			this.psystem.SetParticles(this.particles, num);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00002E10 File Offset: 0x00001010
		private void SetParticle(ParticleManager.Emitter emitter, ref ParticleSystem.Particle particle)
		{
			float value = UnityEngine.Random.value;
			float value2 = UnityEngine.Random.value;
			ParticleSystem.MainModule main = this.psystem.main;
			particle.angularVelocity = value * 2f - 1f;
			particle.startColor = main.startColor.Evaluate(value, value2);
			particle.position = emitter.position;
			particle.rotation = main.startRotation.Evaluate(value, value2);
			particle.startSize = main.startSize.Evaluate(value, value2);
			particle.startLifetime = main.startLifetime.Evaluate(value, value2);
			particle.remainingLifetime = particle.startLifetime;
			particle.velocity = main.startSpeed.Evaluate(value, value2) * (emitter.direction + value * emitter.spreadX + value2 * emitter.spreadY);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00002F0C File Offset: 0x0000110C
		private void InitializeIfNeeded()
		{
			if (this.particles == null || this.particles.Length < this.psystem.main.maxParticles)
			{
				this.particles = new ParticleSystem.Particle[this.psystem.main.maxParticles];
				this.part_sizes = new float[this.psystem.main.maxParticles];
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000029A8 File Offset: 0x00000BA8
		private void OnDestroy()
		{
		}

		// Token: 0x0400000D RID: 13
		public ParticleSystem psystem;

		// Token: 0x0400000E RID: 14
		public Vector3 gravity = new Vector3(0f, -9.8f, 0f);

		// Token: 0x0400000F RID: 15
		public float dispersion = 500f;

		// Token: 0x04000010 RID: 16
		public float drag = 0.05f;

		// Token: 0x04000011 RID: 17
		public float density = 1.2f;

		// Token: 0x04000012 RID: 18
		public float densityExp = -0.0001f;

		// Token: 0x04000013 RID: 19
		public float particleDensity = 1.1965f;

		// Token: 0x04000014 RID: 20
		public ParticlePhysics.FloatVolume pressure;

		// Token: 0x04000015 RID: 21
		private ParticleSystem.Particle[] particles;

		// Token: 0x04000016 RID: 22
		private float[] part_sizes;

		// Token: 0x04000017 RID: 23
		private List<ParticleManager.Emitter> emitters;

		// Token: 0x04000018 RID: 24
		private int lastEmitter;

		// Token: 0x0200003F RID: 63
		public class FloatVolume
		{
			// Token: 0x0600018E RID: 398 RVA: 0x00009C50 File Offset: 0x00007E50
			private Vector3Int mapPos(Vector3 position)
			{
				position = Vector3.Scale(position, this.scale) + this.center;
				return new Vector3Int((int)((double)(position.x * (float)(this.size.x - 1)) + 0.5), (int)((double)(position.y * (float)(this.size.y - 1)) + 0.5), (int)((double)(position.z * (float)(this.size.z - 1)) + 0.5));
			}

			// Token: 0x0600018F RID: 399 RVA: 0x00009CE0 File Offset: 0x00007EE0
			private int mapPos(Vector3Int pos)
			{
				if (pos.x < 0 || pos.y < 0 || pos.z < 0 || pos.x >= this.size.x || pos.y >= this.size.y || pos.z >= this.size.z)
				{
					return -1;
				}
				return (pos.y * this.size.z + pos.z) * this.size.x + pos.x;
			}

			// Token: 0x06000190 RID: 400 RVA: 0x00009D78 File Offset: 0x00007F78
			private float GetData(Vector3Int vind)
			{
				int num = this.mapPos(vind);
				if (num < 0)
				{
					return 0f;
				}
				return this.data[num];
			}

			// Token: 0x17000031 RID: 49
			public float this[Vector3 position]
			{
				get
				{
					return this.GetData(this.mapPos(position));
				}
				set
				{
					int num = this.mapPos(this.mapPos(position));
					if (num >= 0)
					{
						this.data[num] = value;
					}
				}
			}

			// Token: 0x06000193 RID: 403 RVA: 0x00009DD8 File Offset: 0x00007FD8
			private float delta(Vector3Int ind, Vector3Int dir, float b, float x)
			{
				float num = this.GetData(ind - dir);
				float num2 = this.GetData(ind + dir);
				return (num2 - num) / 2f + x * (num2 + num - 2f * b);
			}

			// Token: 0x06000194 RID: 404 RVA: 0x00009E1C File Offset: 0x0000801C
			public Vector3 Gradient(Vector3 position)
			{
				Vector3Int vector3Int = this.mapPos(position);
				position = Vector3.Scale(position, this.scale) + this.center;
				position.x *= (float)(this.size.x - 1);
				position.y *= (float)(this.size.y - 1);
				position.z *= (float)(this.size.z - 1);
				position -= vector3Int;
				Vector3 zero = Vector3.zero;
				float b = this.GetData(vector3Int);
				zero.x = this.delta(vector3Int, this.X, b, position.x);
				zero.y = this.delta(vector3Int, this.Y, b, position.y);
				zero.z = this.delta(vector3Int, this.Z, b, position.z);
				return Vector3.Scale(this.metric, zero);
			}

			// Token: 0x06000195 RID: 405 RVA: 0x00009F10 File Offset: 0x00008110
			public FloatVolume(Vector3 size, int sizeX, int sizeY, int sizeZ)
			{
				this.size.x = sizeX;
				this.size.y = sizeY;
				this.size.z = sizeZ;
				this.center = new Vector3(0.5f, 0.5f, 0.5f);
				this.scale.x = 1f / size.x;
				this.scale.y = 1f / size.y;
				this.scale.z = 1f / size.z;
				this.metric = Vector3.Scale(this.scale, this.scale);
				this.data = new float[sizeX * sizeY * sizeZ];
			}

			// Token: 0x06000196 RID: 406 RVA: 0x00009FFC File Offset: 0x000081FC
			public void Clear()
			{
				int num = 0;
				int num2 = this.data.Length;
				while (num2-- > 0)
				{
					this.data[num++] = 0f;
				}
			}

			// Token: 0x040000E1 RID: 225
			private float[] data;

			// Token: 0x040000E2 RID: 226
			public Vector3 metric;

			// Token: 0x040000E3 RID: 227
			public Vector3 scale;

			// Token: 0x040000E4 RID: 228
			public Vector3 center;

			// Token: 0x040000E5 RID: 229
			public Vector3Int size;

			// Token: 0x040000E6 RID: 230
			private Vector3Int X = new Vector3Int(1, 0, 0);

			// Token: 0x040000E7 RID: 231
			private Vector3Int Y = new Vector3Int(0, 1, 0);

			// Token: 0x040000E8 RID: 232
			private Vector3Int Z = new Vector3Int(0, 0, 1);
		}
	}
}
