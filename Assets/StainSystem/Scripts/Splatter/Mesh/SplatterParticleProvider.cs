using UnityEngine;

namespace SplatterSystem {
	[RequireComponent(typeof(ParticleSystem))]
	public class SplatterParticleProvider : MonoBehaviour {
		private const float STANDBY_PARTICLE_LIFETIME = 36000f;

		private ParticleSystem system;
		private ParticleSystem.MainModule mainModule;
		private ParticleSystem.Particle[] particles;
		private int currentIndex = 0;
		private uint indexWrapCount = 0;

		private SplatterSettings settings;
		private bool isDirty = false;
		private bool isFadeingOut = false;

		public void Configure(SplatterSettings settings) {
			system = GetComponent<ParticleSystem>();
			mainModule = system.main;
			this.settings = settings;

			mainModule.maxParticles = settings.maxParticles;
			
			if (settings.removeParticles) {
#if UNITY_5_3_OR_NEWER
				ParticleSystem.SizeOverLifetimeModule solm = system.sizeOverLifetime;
				solm.enabled = true;
#endif
				mainModule.startLifetime = settings.particleLifetime;
			}

			ParticleSystemRenderer particleRenderer = (ParticleSystemRenderer) system.GetComponent<Renderer>();
			if (particleRenderer != null) {
				if (settings.particleMode == ParticleMode.Circle) {
					particleRenderer.material = (Material) Resources.Load(
						settings.multiplyColor? "CircleParticleMultiply" : "CircleParticleAlphaBlend");
				} else if (settings.particleMode == ParticleMode.Square) {
					particleRenderer.material = (Material) Resources.Load(
						settings.multiplyColor? "SquareParticleMultiply" : "SquareParticleAlphaBlend");
				} else if (settings.particleMode == ParticleMode.CustomTexture) {
					particleRenderer.material = (Material) Resources.Load(
						settings.multiplyColor? "CustomParticleMultiply" : "CustomParticleAlphaBlend");
					particleRenderer.material.SetTexture("_MainTex", settings.particleTexture);
				} else if (settings.particleMode == ParticleMode.CustomMaterial) {
					particleRenderer.material = settings.particleMaterial;
				}
				if (particleRenderer.material == null) {
					Debug.LogError("<b>[Stain System]</b> Can't find particle renderer material.");
				}

				particleRenderer.sortingLayerName = settings.sortingLayerName;

				particleRenderer.renderMode = settings.orientation == OrientationMode.Horizontal? 
						ParticleSystemRenderMode.HorizontalBillboard : ParticleSystemRenderMode.VerticalBillboard;
			}
		}

		void Start() {
			system = GetComponent<ParticleSystem>();

			if (!settings || !settings.removeParticles) {
				mainModule.startLifetime = STANDBY_PARTICLE_LIFETIME;
			}
			
			//system.Emit(settings.maxParticles);

			particles = new ParticleSystem.Particle[settings.maxParticles];
			currentIndex = 0;
		}

		void OnDestroy() {
			CancelInvoke();
		}

		public void SetPosition(Vector3 pos) {
			particles[currentIndex].position = pos;
			if (settings != null && settings.randomRotation) {
				particles[currentIndex].rotation = Random.value * 360f;
			}
		}

		public void SetColor(Color color) {
#if UNITY_5_3_OR_NEWER
			particles[currentIndex].startColor = color;
#else
			particles[currentIndex].color = color;
#endif
		}

		public void SetScale(float scale) {
#if UNITY_5_3_OR_NEWER
			particles[currentIndex].startSize = scale;
#else
			particles[currentIndex].size = scale;
#endif
		}

		public void MoveToNext() {
			// This is to keep correct draw order.
			if (settings && settings.removeParticles) {
				particles[currentIndex].startLifetime = settings.particleLifetime + currentIndex * 0.001f;
			} else {
				particles[currentIndex].startLifetime = STANDBY_PARTICLE_LIFETIME + currentIndex * 0.01f;
			}
			particles[currentIndex].remainingLifetime = particles[currentIndex].startLifetime;

			currentIndex++;

			if (currentIndex >= settings.maxParticles) {
				currentIndex %= settings.maxParticles;
				indexWrapCount++;
			}
		}

		public void Apply() {
			isDirty = true;

			if (settings != null && settings.removeParticles) {
				system.SetParticles(particles, settings.maxParticles);
			}
		}

		public void Clear() {
			system.Clear();
			int numDead = settings.maxParticles - system.particleCount;
			if (numDead > 0) {
				system.Emit(numDead);
			}
			int numAlive = system.GetParticles(particles);
			Debug.Assert(numAlive == settings.maxParticles);
		}

		public void FadeOut() {
			isFadeingOut = true;
		}

		public int GetNumParticlesActive() {
			return Mathf.Min(currentIndex + (int)indexWrapCount * particles.Length, 
							 settings.maxParticles);
		}

		private void FixedUpdate() {
			if (isFadeingOut) {
				for (int i = 0; i < particles.Length; i++) {
#if UNITY_5_3_OR_NEWER
					particles[i].startSize = Mathf.Lerp(particles[i].startSize, 0f, 0.1f);
#else
					particles[i].size = Mathf.Lerp(particles[i].size, 0f, 0.1f);
#endif
				}
				isDirty = true;
			}

			if (isDirty && !(settings != null && settings.removeParticles)) {
				system.SetParticles(particles, settings.maxParticles);
			}

			if ((settings != null && settings.removeParticles)) {
				int numDead = settings.maxParticles - system.particleCount;
				if (numDead > 0) {
					system.Emit(numDead);
				}
				int numAlive = system.GetParticles(particles);
				Debug.Assert(numAlive == settings.maxParticles);
			}
		}
    }
}
