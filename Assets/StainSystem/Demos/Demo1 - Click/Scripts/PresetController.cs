using UnityEngine;

namespace SplatterSystem {
	
	public class PresetController : MonoBehaviour {
		public AbstractSplatterManager manager;
		public SplatterSettings[] presets;
		public int startAtElement = 0;

		private int _currentIndex = 0;

		void Start () {
			// There can be only one.
			#if UNITY_2023_2_OR_NEWER
			var pc = FindObjectsByType<PresetController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			#else
			var pc = FindObjectsOfType<PresetController>();
			#endif
			
			if (pc.Length > 1) {
				Destroy(gameObject);
				return;
			}

			DontDestroyOnLoad(gameObject);

			_currentIndex = startAtElement;
			Apply();
		}
		
		public virtual void ApplyNextPreset() {
			_currentIndex = (_currentIndex + 1) % presets.Length;
			Apply();
		}

		public virtual void ApplyPreviousPreset() {
			_currentIndex = (_currentIndex + presets.Length - 1) % presets.Length;
			Apply();
		}

		private void Apply() {
			manager.SetDefaultSettings(presets[_currentIndex]);
			manager.Clear();
		}
	}

}