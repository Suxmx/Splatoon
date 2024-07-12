using System;
using UnityEngine;

namespace SplatterSystem {
[Serializable]
public class BitmapSplatterManager : AbstractSplatterManager {
    public SplatterSettings defaultSettings;
    private SplatterArea[] _areas = { };
    private GameObject _splatterBranchPrefab;

    private static int CompareAreasByZ(SplatterArea a, SplatterArea b) {
        if (a.transform.position.z < b.transform.position.z) return -1;
        if (a.transform.position.z > b.transform.position.z) return 1;
        return 0;
    }

    private void Start() {
        if (defaultSettings == null) {
            Debug.LogError("<b>[Stain System]</b> Can't find default Splatter Settings");
            enabled = false;
            return;
        }

#if UNITY_2023_2_OR_NEWER
        _areas = FindObjectsByType<SplatterArea>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
        _areas = FindObjectsOfType<SplatterArea>();
#endif

        if (defaultSettings.directionMode != DirectionMode.AllDirections3D) {
            Array.Sort(_areas, CompareAreasByZ);
        }

        _splatterBranchPrefab = (GameObject)Resources.Load("BitmapSplatterBranch");
        if (_splatterBranchPrefab == null) {
            Debug.LogError("<b>[Stain System]</b> Can't find SplatterBranch prefab");
            enabled = false;
        }
    }

    public virtual SplatterArea[] GetSplatterAreas() {
        return _areas;
    }

    public override void SetDefaultSettings(SplatterSettings settings) {
        defaultSettings = settings;
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
        SplatterArea area = GetAreaAtWorldPosition(position, distanceTestAreas: true);

        if (area == null) {
            return;
        }

        SplatterUtils.SpawnBranch(_splatterBranchPrefab, transform, area, settings, position, direction, color);
    }

    /// <summary>
    /// Traverse all areas to find the one at a world position.
    /// </summary>
    /// <param name="position">Position in world coordinates.</param>
    /// <param name="distanceTestAreas">Whether to look for the closest or any area.</param>
    /// <returns></returns>
    public virtual SplatterArea GetAreaAtWorldPosition(Vector3 position, bool distanceTestAreas = true) {
        SplatterArea area = null;
        float minDistance = float.MaxValue;
        foreach (var a in _areas) {
            // The area may become null if the scene has changed.
            if (a == null) continue;

            if (a.rectTransform.rect.Contains(position - a.rectTransform.position, true)) {
                if (distanceTestAreas) {
                    float distance = Vector3.SqrMagnitude(a.transform.position - position);
                    if (distance < minDistance) {
                        minDistance = distance;
                        area = a;
                    }
                }
                else {
                    area = a;
                    break;
                }
            }
        }

        return area;
    }

    public virtual Color GetStainColorAtWorldPoint(Vector3 worldPoint) {
        SplatterArea area = GetAreaAtWorldPosition(worldPoint);
        if (area == null) {
            return Color.clear;
        }

        Vector2 pos = area.WorldToTexturePosition(ref worldPoint);
        return area.GetTexture().GetPixel((int)pos.x, (int)pos.y);
    }

    public override void Clear() {
        foreach (var area in _areas) {
            area.ResetCanvasTexture();
        }
    }
}
}