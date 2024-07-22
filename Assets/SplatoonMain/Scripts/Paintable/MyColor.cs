using System;
using PaintCore;
using UnityEngine;

namespace Splatoon
{
    public class MyColor: MonoBehaviour
    {
        public PaintColor Color;
        private CwColor _CWColor;

        private void Awake()
        {
            _CWColor = GetComponent<CwColor>();
        }


        public int GetDrawCount() => _CWColor.Solid;

        public float GetDrawRatio() => _CWColor.Ratio;
    }
}