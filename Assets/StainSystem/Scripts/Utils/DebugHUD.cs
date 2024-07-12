using UnityEngine;
using UnityEngine.UI;

namespace SplatterSystem {
[RequireComponent(typeof(Text))]
public class DebugHUD : MonoBehaviour {
    private const float FPSMeasurePeriod = 0.5f;
    private const string Display1 = "{0} FPS";
    private const string Display2 = "{0} FPS\n{1} particles";
    private int _fpsAccumulator = 0;
    private float _fpsNextPeriod = 0;
    private int _currentFps;
    private Text _text;
    private SplatterParticleProvider _particles;
    private bool _showNumParticles;

    private void Start() {
        _fpsNextPeriod = Time.realtimeSinceStartup + FPSMeasurePeriod;
        _text = GetComponent<Text>();
#if UNITY_2023_2_OR_NEWER
        _particles = FindAnyObjectByType<SplatterParticleProvider>(FindObjectsInactive.Exclude);
#else
        _particles = FindObjectOfType<SplatterParticleProvider>();
#endif
        _showNumParticles = _particles != null;
    }

    private void Update() {
        _fpsAccumulator++;
        if (Time.realtimeSinceStartup > _fpsNextPeriod) {
            _currentFps = (int)(_fpsAccumulator / FPSMeasurePeriod);
            _fpsAccumulator = 0;
            _fpsNextPeriod += FPSMeasurePeriod;
            _text.text = _showNumParticles
                ? string.Format(Display2, _currentFps, _particles.GetNumParticlesActive())
                : string.Format(Display1, _currentFps);
        }
    }
}
}