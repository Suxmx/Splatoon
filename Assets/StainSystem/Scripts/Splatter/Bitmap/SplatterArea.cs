using UnityEngine;

namespace SplatterSystem {
#pragma warning disable 0414

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public sealed class SplatterArea : MonoBehaviour {
    public float pixelsPerUnit = 100f;
    public float scaleMultiplier = 1f;
    public bool debugDraw = false;

    public RectTransform rectTransform { get; private set; }

    [HideInInspector]
    public Texture2D texture;

    private SpriteRenderer spriteRenderer;
    private Sprite sprite;
    private bool isResizing;
    private bool isDirty;

    private Vector3 _cacheVec3;
    private Vector2 _cacheVec2;

    private void OnEnable() {
        rectTransform = GetComponent<RectTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (Application.isPlaying) {
            debugDraw = false;
        }

        sprite = spriteRenderer.sprite;
        if (sprite == null) {
            ResetCanvasTexture();
            sprite = spriteRenderer.sprite;
        }

        texture = sprite.texture;
    }

    private void LateUpdate() {
        if (isDirty) {
            texture.Apply();
            isDirty = false;
        }

        // Handle resizing.
#if UNITY_EDITOR
        if (!Application.isPlaying && isResizing) {
            if (Input.GetMouseButton(0)) {
                ResetSprite();
            }

            if (!Input.GetMouseButton(0)) {
                ResetCanvasTexture();
                isResizing = false;
            }
        }
#endif
    }

    private void OnRectTransformDimensionsChange() {
#if UNITY_EDITOR
        isResizing = true;
        ResetSprite();
#endif
    }

    public Texture2D GetTexture() {
        return texture;
    }

    public void SpawnParticle(ParticleMode shape, Vector3 point, float scale, Color color) {
        Vector3 uv = WorldToTexturePosition(ref point);
        int particleSize = (int)(scaleMultiplier * scale * 40f);
        if (shape == ParticleMode.Square) {
            BitmapDraw.DrawFilledSquare(texture, (int)uv.x, (int)uv.y, particleSize, color);
        }
        else {
            BitmapDraw.DrawFilledCircle(texture, (int)uv.x, (int)uv.y, particleSize, color);
        }

        isDirty = true;
    }

    public void Apply() {
        isDirty = true;
    }

    public void Clear() {
        ResetCanvasTexture();
    }

    public void ResetCanvasTexture() {
        if (!rectTransform || rectTransform.rect.width <= 0 || rectTransform.rect.height <= 0) return;

        int width = (int)(rectTransform.rect.width * pixelsPerUnit);
        int height = (int)(rectTransform.rect.height * pixelsPerUnit);
        if (width <= 0 || height <= 0) {
            Debug.LogWarning($"<b>Stain System</b> Canvas size is non-positive ({width}x{height}).", this);
            return;
        }

        texture = new Texture2D(width, height, TextureFormat.ARGB32, false) {
            wrapMode = TextureWrapMode.Clamp
        };

        // Set alpha of all pixels to a minimum non-zero value to work around Unity not showing texture otherwise.
        var pixels = texture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++) {
            pixels[i].a = (byte)(debugDraw ? 100 : 1);
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        var rect = new Rect(0, 0, texture.width, texture.height);
        sprite = Sprite.Create(texture, rect, rectTransform.pivot, pixelsPerUnit);
        spriteRenderer.sprite = sprite;
    }

    public Vector2 WorldToTexturePosition(ref Vector3 worldPos) {
        _cacheVec3 = worldPos - rectTransform.position;
        _cacheVec2.Set(Vector3.Dot(_cacheVec3, rectTransform.right), Vector3.Dot(_cacheVec3, rectTransform.up));
        return _cacheVec2 * sprite.pixelsPerUnit + sprite.pivot;
        //return (worldPos - (Vector2)rectTransform.position) * sprite.pixelsPerUnit + sprite.pivot;
    }

    private void ResetSprite() {
        if (spriteRenderer != null) {
            spriteRenderer.sprite = null;
        }
    }
}
}
