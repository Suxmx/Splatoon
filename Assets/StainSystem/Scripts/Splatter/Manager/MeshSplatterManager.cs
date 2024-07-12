using UnityEngine;

namespace SplatterSystem {
    
    [System.Serializable]
    public class MeshSplatterManager : AbstractSplatterManager {
        [SerializeField]
        public SplatterSettings defaultSettings;

        [SerializeField][HideInInspector]
        public bool advancedSettings = false;

        private SplatterParticleProvider particles;
        private GameObject splatterBranchPrefab;
        
        void Awake() {
            splatterBranchPrefab = (GameObject) Resources.Load("MeshSplatterBranch");
            if (splatterBranchPrefab == null) {
                Debug.LogError("<b>[Stain System]</b> Can't find SplatterBranch prefab");
                enabled = false;
                return;
            }
            
            particles = gameObject.GetComponentInChildren<SplatterParticleProvider>();
            if (particles == null) {
                Debug.LogError("<b>[Stain System]</b> Can't find SplatterParticleProvider");
                enabled = false;
                return;
            }

            if (defaultSettings != null) {
                particles.Configure(defaultSettings);
            }
        }

        public override void SetDefaultSettings(SplatterSettings settings) {
            defaultSettings = settings;
            particles.Configure(settings);
        }
		
        public override void Spawn(Vector3 position) {
            Spawn(position, null, null);
        }

        public override void Spawn(Vector3 position, Vector3 direction) {
            Spawn(position, direction, null);
        }

        public override void Spawn(Vector3 position, Color color) {
            Spawn(position, null, color);
        }

        public override void Spawn(Vector3 position, Vector3? direction, Color? color) {
            if (defaultSettings == null) {
                Debug.LogError("<b>[Stain System]</b> No default settings is assigned in SplatterManager.");
                return;
            }

            Spawn(defaultSettings, position, direction, color);
        }

        public override void Spawn(SplatterSettings settings, Vector3 position, Vector3? direction, Color? color) {
            SplatterUtils.SpawnBranch(splatterBranchPrefab, transform, particles, settings, position, direction, color);
        }

        public override void Clear() {
            particles.Clear();
        }

        public void FadeOut() {
            particles.FadeOut();
        }

    }
}