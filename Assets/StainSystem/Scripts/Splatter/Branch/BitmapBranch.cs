using UnityEngine;

namespace SplatterSystem {
	public class BitmapBranch : BaseBranch {
		private SplatterArea area;

		public override void SetParticleProvider(MonoBehaviour particleProvider) {
			area = (SplatterArea) particleProvider;
		}

        protected override void SpawnParticle(Vector3 position, float scale, Color color) {
			Vector3 planeNormal = area.rectTransform.forward;
			var distance = -Vector3.Dot(planeNormal.normalized, position - area.rectTransform.position);
			Vector3 projectedPosition = position + planeNormal * distance;
			area.SpawnParticle(settings.particleMode, projectedPosition, scale, color);
        }
    }
}
