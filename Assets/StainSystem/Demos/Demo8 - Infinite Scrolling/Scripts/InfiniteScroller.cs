using System.Collections;
using System.Collections.Generic;
using SplatterSystem;
using UnityEngine;

namespace SplatterSystem.Demos {
	public class InfiniteScroller : MonoBehaviour {
		public Camera scrollingCamera;
		public GameObject platformPrefab;

		[Space]
		public float scrollingSpeed = 1.0f;
		public float platformInterval = 3.0f;
		public float horizontalShift = 10f;

		private bool isScrolling = false;
		private float currentScrollingSpeed = 0f;
		private readonly Queue platforms = new Queue();
		private float platformSpawnBorder = 0f;

		void Awake() {
			int numPlatformsInit = 5;
			for (int i = 0; i < numPlatformsInit; i++) {
				SpawnPlatform();
			}
		}

		IEnumerator Start () {
			if (scrollingCamera == null) scrollingCamera = Camera.main;

			yield return new WaitForSeconds(1.5f);
			isScrolling = true;
		}
		
		void Update () {
			if (!isScrolling) return;
			currentScrollingSpeed = Mathf.Lerp(currentScrollingSpeed, scrollingSpeed, 0.5f * Time.deltaTime);
			var t = scrollingCamera.transform;
			Vector3 cameraPosition = t.position + Vector3.right * currentScrollingSpeed * Time.deltaTime;
			t.position = cameraPosition;

			if (cameraPosition.x > platformSpawnBorder - horizontalShift - 10f) {
				SpawnPlatform();
			}

			if (cameraPosition.x > (platforms.Peek() as GameObject).transform.position.x + horizontalShift + 10f) {
				// Remove old platform.
				SimplePool.Despawn(platforms.Dequeue() as GameObject);
			}
		}

        private void SpawnPlatform() {
			platformSpawnBorder += platformInterval;
			var platformPosition = new Vector3(
					platformSpawnBorder - horizontalShift, Random.Range(-4f, -1f), 0f);
			var platform = SimplePool.Spawn(platformPrefab, platformPosition, Quaternion.identity);
			var splatterArea = platform.GetComponentInChildren<SplatterArea>();
			if (splatterArea != null) {
				splatterArea.ResetCanvasTexture();
			}
			platforms.Enqueue(platform);
        }
    }

}
