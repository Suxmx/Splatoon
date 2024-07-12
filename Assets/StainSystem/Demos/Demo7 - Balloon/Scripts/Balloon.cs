using System.Collections;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace SplatterSystem.Demos {
	
	public class Balloon : MonoBehaviour {
		public RectTransform hitBox;
		public AbstractSplatterManager splatter;
		public Shaker screenShake;

		private Vector3 _offset;

		private void Start() {
			if (splatter == null) {
				#if UNITY_2023_2_OR_NEWER
				splatter = FindAnyObjectByType<AbstractSplatterManager>(FindObjectsInactive.Exclude);
				#else
				splatter = FindObjectOfType<AbstractSplatterManager>();
				#endif
			}
		}

		private void Update () {
			bool justClicked = Input.GetMouseButtonDown(0);
			if (justClicked) {
				Debug.Assert(Camera.main != null, "Camera.main != null");
				Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				if (hitBox.rect.Contains(worldPos - (Vector2)hitBox.transform.position)) {
					StartCoroutine(HandleBalloonPop(worldPos));
				}
			}
		}

		private void LateUpdate() {
			transform.position += _offset;
		}

		private IEnumerator HandleBalloonPop(Vector3 pos) {
			var r = GetComponent<SpriteRenderer>();
			r.enabled = false;

			var audioSource = GetComponent<AudioSource>();
			if (audioSource != null) {
				audioSource.Play();
			}

			splatter.Spawn(pos);
			if (screenShake != null) {
				screenShake.Shake();
			}

			yield return new WaitForSeconds(0.5f);
			r.enabled = true;
			var color = Color.white;
			color.a = 0f;
			r.color = color;
			var wait = new WaitForEndOfFrame();
			while (color.a < 1f) {
				color.a += 5f * Time.deltaTime;
				_offset.y = -(1f - color.a) * 1.5f;
				r.color = color;
				yield return wait;
			}
			_offset = Vector3.zero;

			yield return 0;
		}
	}
}
