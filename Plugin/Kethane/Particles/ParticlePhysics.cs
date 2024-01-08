using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Kethane.Particles
{

	public class ParticlePhysics : MonoBehaviour
	{

		public class FloatVolume
		{
			float []data;
			public Vector3 metric;
			public Vector3 scale;
			public Vector3 center;
			public Vector3Int size;

			Vector3Int mapPos (Vector3 position)
			{
				position = Vector3.Scale (position, scale) + center;
				return new Vector3Int ((int) (position.x * (size.x - 1) + 0.5),
									   (int) (position.y * (size.y - 1) + 0.5),
									   (int) (position.z * (size.z - 1) + 0.5));
			}

			int mapPos (Vector3Int pos)
			{
				if (pos.x < 0 || pos.y < 0 || pos.z < 0
					|| pos.x >= size.x || pos.y >= size.y || pos.z >= size.z) {
					return -1;
				}
				int ind = (pos.y * size.z + pos.z) * size.x + pos.x;
				return ind;
			}

			float GetData (Vector3Int vind)
			{
				int ind = mapPos (vind);
				if (ind < 0) {
					return 0;
				}
				return data[ind];
			}

			public float this[Vector3 position]
			{
				get {
					return GetData (mapPos (position));
				}
				set {
					int ind = mapPos (mapPos(position));
					if (ind >= 0) {
						data[ind] = value;
					}
				}
			}

			Vector3Int X = new Vector3Int(1, 0, 0);
			Vector3Int Y = new Vector3Int(0, 1, 0);
			Vector3Int Z = new Vector3Int(0, 0, 1);

			float delta (Vector3Int ind, Vector3Int dir, float b, float x)
			{
				float a = GetData (ind - dir);
				float c = GetData (ind + dir);
				return (c - a) / 2 + x * (c + a - 2 * b);
			}

			public Vector3 Gradient (Vector3 position)
			{
				Vector3Int ind = mapPos (position);
				position = Vector3.Scale(position, scale) + center;
				position.x *= size.x - 1;
				position.y *= size.y - 1;
				position.z *= size.z - 1;
				position -= ind;
				Vector3 grad = Vector3.zero;
				float b = GetData (ind);
				grad.x = delta (ind, X, b, position.x);
				grad.y = delta (ind, Y, b, position.y);
				grad.z = delta (ind, Z, b, position.z);
				return Vector3.Scale (metric, grad);
			}

			public FloatVolume (Vector3 size, int sizeX, int sizeY, int sizeZ)
			{
				this.size.x = sizeX;
				this.size.y = sizeY;
				this.size.z = sizeZ;
				center = new Vector3 (0.5f, 0.5f, 0.5f);
				scale.x = 1f / size.x;
				scale.y = 1f / size.y;
				scale.z = 1f / size.z;
				metric = Vector3.Scale (scale, scale);
				data = new float[sizeX * sizeY * sizeZ];
			}

			public void Clear ()
			{
				int ind = 0;
				for (int i = data.Length; i-- > 0; ) {
					data[ind++] = 0;
				}
			}
		}

		public ParticleSystem psystem;
		public Vector3 gravity = new Vector3 (0, -9.8f, 0);
		public float dispersion = 500;
		public float drag = 0.05f;
		public float density = 1.2f;
		public float densityExp = -0.0001f;
		public float particleDensity = 1.1965f;
		public FloatVolume pressure;
		ParticleSystem.Particle []particles;
		float []part_sizes;
		List<ParticleManager.Emitter> emitters;
		int lastEmitter;

		public void Load (ConfigNode node)
		{
			gravity = Utils.ParseVector3 (node, "gravity", gravity);
			dispersion = Utils.ParseFloat (node, "dispersion", dispersion);
			drag = Utils.ParseFloat (node, "drag", drag);
			density = Utils.ParseFloat (node, "density", density);
			densityExp = Utils.ParseFloat (node, "densityExp", densityExp);
			particleDensity = Utils.ParseFloat (node, "particleDensity", particleDensity);
		}

		public void AddEmitter (ParticleManager.Emitter emitter)
		{
			emitters.Add (emitter);
		}

		void Awake ()
		{
			emitters = new List<ParticleManager.Emitter> ();
		}

		void Start ()
		{
			pressure = new FloatVolume (new Vector3 (400, 400, 400), 200, 200, 200);
		}

		void FixedUpdate ()
		{
			if (psystem == null) {
				return;
			}
			float deltaTime = Time.fixedDeltaTime;
			InitializeIfNeeded ();
			Vector3 up = Vector3.up;
			if (FlightGlobals.ActiveVessel != null) {
				var body = FlightGlobals.ActiveVessel.mainBody;
				gravity = FlightGlobals.ActiveVessel.gravityForPos;
				double altitude = FlightGlobals.getAltitudeAtPos(Vector3d.zero, body);
				double pressure = FlightGlobals.getStaticPressure (altitude, body);
				double temperature = body.GetTemperature (altitude);
				density = (float) FlightGlobals.getAtmDensity (pressure, temperature, body);
				//Debug.Log ($"[ParticlePhysics] density: {density}");
				up = -gravity.normalized;
			}

			int numParticles = psystem.GetParticles (particles);
			//Debug.Log($"[ParticleSystem] {gameObject.name} {numParticles}");

			for (int i = numParticles; i-- > 0; ) {
				float p = particles[i].GetCurrentSize(psystem);
				part_sizes[i] = p;
				pressure[particles[i].position] += p * Mathf.Exp(-p);
			}
			for (int i = numParticles; i-- > 0; ) {
				Vector3 pos = particles[i].position;
				Vector3 vel = particles[i].velocity;
				Vector3 grad = pressure.Gradient (pos);
				float dens = density * Mathf.Exp(densityExp * Vector3.Dot(pos, up));
				float buoy = (particleDensity - dens) / particleDensity;
				//Debug.Log ($"[ParticlePhysics] {pos} {pressure[pos]} {grad * 1000}");
				Vector3 force = gravity * buoy;
				force += -grad * part_sizes[i] * dispersion;
				force -= vel * drag * dens;
				if (pos.y < 0) {
					force.y = -pos.y;
				}
				Vector3 ov = vel;
				particles[i].velocity = ov + force * deltaTime;
				//if (i == 0) {
				//	Debug.Log ($"[ParticlePhysics] {pos} {grad} {force} {buoy} {gravity} {buoy * gravity} {particles[i].velocity} {dens} {drag}");
				//}
			}
			int maxParticles = psystem.main.maxParticles;
			int count = emitters.Count;
			int eind = lastEmitter;
			for (int i = 0; i < count && numParticles < maxParticles; i++) {
				eind = (lastEmitter + i) % count;
				var emitter = emitters[eind];
				if (!emitter.enabled) {
					continue;
				}
				emitter.emission += emitter.rate * deltaTime;
				int num = (int) emitter.emission;
				emitter.emission -= num;
				//Debug.Log ($"[ParticlePhysics] emit {num} {emitter.emission}");
				while (num-- > 0 && numParticles < maxParticles) {
					SetParticle (emitter, ref particles[numParticles++]);
				}
			}
			lastEmitter = (eind + 1) % count;
			psystem.SetParticles (particles, numParticles);
		}

		void SetParticle (ParticleManager.Emitter emitter, ref ParticleSystem.Particle particle)
		{
			float x = UnityEngine.Random.value;
			float y = UnityEngine.Random.value;
			var main = psystem.main;
			particle.angularVelocity = (x * 2) - 1;
			particle.startColor = main.startColor.Evaluate (x, y);;
			particle.position = emitter.position;
			particle.rotation = main.startRotation.Evaluate (x, y);
			particle.startSize = main.startSize.Evaluate (x, y);
			particle.startLifetime = main.startLifetime.Evaluate (x, y);
			particle.remainingLifetime = particle.startLifetime;
			particle.velocity = main.startSpeed.Evaluate (x, y) * (emitter.direction + x * emitter.spreadX + y * emitter.spreadY);
		}

		void InitializeIfNeeded ()
		{
			if (particles == null || particles.Length < psystem.main.maxParticles) {
				particles = new ParticleSystem.Particle[psystem.main.maxParticles];
				part_sizes = new float[psystem.main.maxParticles];
			}
		}

		void OnDestroy ()
		{
			//Debug.Log ($"[ParticlePhysics] OnDestroy");
		}
	}

}
