using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace SplatterSystem.TopDown {
		
	public class Target : MonoBehaviour {
		public MeshSplatterManager splatter;
		public float splatOffset = 0f;
		public Shaker screenShake;
		public SplatterSettings hitSplatterSettings;
		public Color hitSplatterColor = Color.red;
		// ReSharper disable once StringLiteralTypo
		[FormerlySerializedAs("dieSplatterSettins")]
		public SplatterSettings dieSplatterSettings;
		public Color dieSplatterColor = Color.red;

		[Space(10)]
		public float healthPoints = 100f;

		private static int _numTargets = 0;
		private float _shakeMagnitude;
		private float _shakeDuration;
		private bool _isDead = false;

		void Start() {
            if (splatter == null) {
                Debug.LogError("<b>[Stain System]</b> No splatter manager attached to target.");
                return;
            }

			if (screenShake != null) {
				_shakeMagnitude = screenShake.magnitude * healthPoints / 300f;
				_shakeDuration = screenShake.duration * healthPoints / 300f;
			}

			if (_numTargets == 0) {
#if UNITY_2023_2_OR_NEWER
				_numTargets = FindObjectsByType<Target>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;			
#else
				_numTargets = FindObjectsOfType<Target>().Length;
#endif
			}
		}

		public void HandleHit(float damage, Vector2 direction) {
			healthPoints = Mathf.Max(healthPoints - damage, 0f);
			if (healthPoints <= 0f) {
				HandleDeath();
			} else {
				Vector2 hitPos = (Vector2) transform.position + splatOffset * direction;
				splatter.Spawn(hitSplatterSettings, hitPos, direction, hitSplatterColor);
			}
		}

		private void HandleDeath() {
			if (_isDead) return;
			_isDead = true;

			splatter.Spawn(dieSplatterSettings, transform.position, null, dieSplatterColor);
			
			if (screenShake != null) {
				screenShake.magnitude = _shakeMagnitude;
				screenShake.duration = _shakeDuration;
				screenShake.Shake();
			}
			
			// If this is last target - restart.
			_numTargets--;
			if (_numTargets <= 0) {
				StartCoroutine(HandleGameCompleted());
			} else {
				gameObject.SetActive(false);
			}
		}

		private IEnumerator HandleGameCompleted() {
			var r = GetComponent<Renderer>();
			r.enabled = false;

			yield return new WaitForSeconds(1.0f);
			splatter.FadeOut();

			yield return new WaitForSeconds(2.0f);
			#if UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			#endif
		}
	}

}