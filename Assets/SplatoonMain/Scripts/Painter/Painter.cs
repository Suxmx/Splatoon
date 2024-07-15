using System;
using UnityEngine;

namespace Splatoon
{
    public class Painter : MonoBehaviour
    {
        public Texture DrawTexture;
        public PaintColor CurColor = PaintColor.Red;

        private void Update()
        {
            Ray ray = new Ray(transform.position, Vector2.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                var paintable = hit.collider.GetComponent<PaintableObject>();
                if (paintable is null) return;
                DrawData drawData = new DrawData();
                drawData.Color = CurColor;
                drawData.Position = new Vector2(hit.point.x, hit.point.z);
                drawData.Scale = 1;
                drawData.Texture = DrawTexture;
                paintable.AddDrawData(drawData);
            }

            if (Input.GetMouseButtonDown(1))
            {
                int index = (int)CurColor;
                CurColor = index + 1 == Enum.GetValues(typeof(PaintColor)).Length
                    ? (PaintColor)(1)
                    : (PaintColor)(index + 1);
            }
        }

        private void FixedUpdate()
        {
        }
    }
}