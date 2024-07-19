using System;
using System.Collections.Generic;
using Services;
using TMPro;
using UnityEngine;

namespace SplatoonOld
{
    public class ColorManager : Service, IColorManager
    {
        public List<TextMeshProUGUI> Texts;


        public List<PaintableObject> _Paintables = new();
        public long _PixelSum = 0;
        private long[] _ColorCounts = new long[Utility.PaintColorCount()];

        private void Update()
        {
            for (int i = 0; i < Utility.PaintColorCount(); i++)
            {
                _ColorCounts[i] = 0;
            }

            foreach (var paintable in _Paintables)
            {
                var counts = paintable.GetColorCounts();
                for (int i = 0; i < Utility.PaintColorCount(); i++)
                {
                    _ColorCounts[i] += counts[i];
                }
            }

            for (int i = 0; i < Utility.PaintColorCount(); i++)
            {
                Texts[i].text = $"{(PaintColor)(i)} : {_ColorCounts[i] * 100.0f / _PixelSum} %";
            }
        }

        public void RegisterPaintable(PaintableObject obj)
        {
            _Paintables.Add(obj);
            _PixelSum += obj.GetPixelCount();
        }
    }
}