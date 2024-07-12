using System.Collections.Generic;
using UnityEngine;

namespace SplatterSystem.Platformer {
	[RequireComponent(typeof(Collider2D))]
	public class Pickup : MonoBehaviour {
		public AbstractSplatterManager splatterManager;
		public Color splatterColor;

		private static Shaker _screenShake;
		private ParticleSystem _particles;
		private List<ParticleCollisionEvent> _collisionEvents;

		private void Start() {
			_particles = GetComponent<ParticleSystem>();

			_collisionEvents = new List<ParticleCollisionEvent>(16);

			if (_screenShake == null) {
				#if UNITY_2023_2_OR_NEWER
				var shakers = FindObjectsByType<Shaker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
				#else
				var shakers = FindObjectsOfType<Shaker>();
				#endif

				foreach (var shaker in shakers) {
					if (shaker.targetCamera) {
						_screenShake = shaker;
						break;
					}
				}
			}
		}

		private void OnTriggerEnter2D(Collider2D collider2d) {
			if (string.Equals(collider2d.tag, "Player")) {
				HandlePickup();
			}
		}

		private void HandlePickup() {
			var r = GetComponent<Renderer>();
			r.enabled = false;

			var collider2d = GetComponent<Collider2D>();
			collider2d.enabled = false;

			var particles = GetComponent<ParticleSystem>();
			if (particles != null) {
				particles.Play();
			}
		}

		private void OnParticleCollision(GameObject other) {
			int safeLength = _particles.GetSafeCollisionEventSize();

			if (_collisionEvents.Count < safeLength) {
				_collisionEvents = new List<ParticleCollisionEvent>(safeLength);
			}
			
			int numCollisionEvents = _particles.GetCollisionEvents(other, _collisionEvents);
			for (int i = 0; i < numCollisionEvents; i++) {
				Vector3 position = _collisionEvents[i].intersection;
				Vector3 velocity = _collisionEvents[i].velocity;
				HandleParticleCollision(position, velocity);
			}
		}

        private void HandleParticleCollision(Vector3 position, Vector3 velocity) {
            if (splatterManager != null) {
				splatterManager.Spawn(position, velocity.normalized, splatterColor);
			}

            // Screen shake on landing.
			if (_screenShake != null) {
				_screenShake.Shake();
			}
        }
    }	
}
