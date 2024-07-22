using UnityEngine;

namespace Splatoon
{
    public class PaintablePlaneObj : Paintable
    {
        protected override void ComputePixelCount()
        {
            ObjPixelWidth = (int)(transform.localScale.x * 320);
            ObjPixelHeight = (int)(transform.localScale.z * 320);
            _PixelCount = ObjPixelHeight * ObjPixelWidth;
        }

        protected override bool TryHitPos2UV(Vector2 hitPos, out Vector2 uv)
        {
            uv = Vector2.zero;
            return false;
        }
    }
}