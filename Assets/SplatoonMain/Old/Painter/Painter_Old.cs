using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SplatoonOld
{
    public class Painter_Old : MonoBehaviour
    {
        //tmp
        public TextMeshProUGUI WhiteText;
        public TextMeshProUGUI RedText;
        public TextMeshProUGUI BlueText;
        public TextMeshProUGUI YellowText;

        public TextMeshProUGUI GreenText;
        

        [Range(0.01f, 3.0f)] public float textureScale = 1.0f;
        public float hitRange = 0.1f;

        public List<Texture> textureGroup = new List<Texture>();

        public PaintColor CurColor = PaintColor.White;
        private DynamicTextureObject _Ground;


        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                int index = (int)CurColor;
                CurColor = index + 1 == Enum.GetValues(typeof(PaintColor)).Length
                    ? (PaintColor)(0)
                    : (PaintColor)(index + 1);
            }

            // if (Input.GetKey(KeyCode.Space))
            // {
            RayCheckOnce();
            // }
            var scores = _Ground.GetScores();
            int cnt = _Ground.GetPixelsCount();
            RedText.text = $"Red : {scores[(int)PaintColor.Red] * 1.0f / cnt * 100} %";
            WhiteText.text = $"White : {scores[(int)PaintColor.White] * 1.0f / cnt * 100} %";
            BlueText.text = $"Blue : {scores[(int)PaintColor.Blue] * 1.0f / cnt * 100} %";
            YellowText.text = $"Yellow : {scores[(int)PaintColor.Yellow] * 1.0f / cnt * 100} %";
            GreenText.text = $"Green : {scores[(int)PaintColor.Green] * 1.0f / cnt * 100} %";
        }


        void RayCheckOnce()
        {
            Ray m_ray = new Ray(transform.position, -transform.up);
            RaycastHit _hit;
            if (Physics.Raycast(m_ray, out _hit, 100.0f))
            {
                DynamicTextureObject hitObject = _hit.collider.gameObject.GetComponent<DynamicTextureObject>();
                if (hitObject != null)
                {
                    _Ground = hitObject;
                    Debug.Log(_hit.collider.gameObject.name + "--point--" + _hit.point + "--textureCoord" +
                    _hit.textureCoord.ToString());
                    DrawOnceInfo tempInfo = new DrawOnceInfo();
                    tempInfo.hitUV = _hit.textureCoord;
                    tempInfo.texture = textureGroup[0];
                    tempInfo.textureScale = textureScale;
                    tempInfo.color = CurColor;
                    hitObject.AddDrawInfo(tempInfo);
                }
            }
        }

        private void OnDrawGizmos()
        {
            //if (_hitPos)
            //{

            //}

            Gizmos.DrawLine(transform.position, transform.up * -10 + transform.position);
        }
    }
}