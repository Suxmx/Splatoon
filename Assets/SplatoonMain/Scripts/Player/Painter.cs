using System;
using PaintIn3D;
using Services;
using UnityEngine;

namespace Splatoon
{
    public class Painter : MonoBehaviour
    {
        public Texture DrawTexture;
        public PaintColor CurColor = PaintColor.Red;
        private ColorManager _ColorManager;
        private long _DrawCountSum = 0;
        private float _Scale = 1f;

        //Paint in 3D
        private CwPaintDecal _PaintDecal;

        private void Awake()
        {
            _PaintDecal = GetComponent<CwPaintDecal>();
        }

        private void Start()
        {
            _ColorManager = ServiceLocator.Get<ColorManager>();
        }

        /// <summary>
        /// 现在仅用于统计升级数据
        /// </summary>
        private void FixedUpdate()
        {
            Ray ray = new Ray(transform.position, Vector2.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                var paintable = hit.collider.GetComponent<Paintable>();
                if (paintable is null) return;
                DrawData drawData = new DrawData();
                drawData.Color = CurColor;
                drawData.UV = hit.textureCoord;
                drawData.Position = new Vector2(hit.point.x, hit.point.z);
                drawData.Scale = _Scale;
                // drawData.Texture = DrawTexture;
                paintable.AddDrawData(drawData);
            }
        }

        private void LateUpdate()
        {
            _DrawCountSum += _ColorManager.GetUpgradeDrawCount(CurColor);
        }

        public long GetDrawCountSum() => _DrawCountSum;

        public void SetScale(float scale)
        {
            _Scale = scale;
            _PaintDecal.Radius = scale;
        }
    }
}