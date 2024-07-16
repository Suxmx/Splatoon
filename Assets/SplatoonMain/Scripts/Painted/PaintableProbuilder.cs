using UnityEngine;
using UnityEngine.ProBuilder;

namespace Splatoon
{
    public class PaintableProbuilder : PaintableObject
    {
        private Renderer _Renderer;

        private Rect _Rect;
        private Vector2 _Size;
        private Vector2 _Middle;
        private Vector2 _LeftBottom;

        protected override void ComputePixelCount()
        {
            _Renderer = GetComponent<Renderer>();
            var bounds = _Renderer.bounds.size;
            _Middle = new Vector2(transform.position.x, transform.position.z);
            _Size = new Vector2(bounds.x, bounds.z);
            _Rect = new Rect(_Middle, _Size);
            _LeftBottom = _Middle - _Size / 2.0f;
            ObjPixelWidth = (int)(bounds.x * 256);
            ObjPixelHeight = (int)(bounds.z * 256);
            _PixelCount = ObjPixelWidth * ObjPixelHeight;
        }

        protected override bool TryHitPos2UV(Vector2 hitPos, out Vector2 uv)
        {
            uv = Vector2.zero;
            // if (!_Rect.Contains(hitPos))
            // {
            //     return false;
            // }
            
            uv = (hitPos - _LeftBottom) / (_Size);
            // Debug.Log($"HitPos:{hitPos} HitUV:{uv}");
            return true; 
        }
    }
}