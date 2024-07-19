using System;
using System.Collections.Generic;
using Services;
using TMPro;

namespace Splatoon
{
    public class ColorManager : Service, IColorManager
    {
        public List<TextMeshProUGUI> Texts;
        public int Frame = 0;

        public List<Paintable> _Paintables = new();
        public long _PixelSum = 0;
        private long[] _ColorCounts = new long[Utility.PaintColorCount];

        private void Update()
        {
            for (int i = 0; i < Utility.PaintColorCount; i++)
            {
                _ColorCounts[i] = 0;
            }

            foreach (var paintable in _Paintables)
            {
                var counts = paintable.GetColorCounts();
                for (int i = 0; i < Utility.PaintColorCount; i++)
                {
                    _ColorCounts[i] += counts[i];
                }
            }

            for (int i = 0; i < Utility.PaintColorCount; i++)
            {
                Texts[i].text = $"{(PaintColor)(i)} : {_ColorCounts[i] * 100.0f / _PixelSum:0.0} %";
            }
        }

        private void FixedUpdate()
        {
            Frame++;
        }

        public void RegisterPaintable(Paintable obj)
        {
            _Paintables.Add(obj);
            _PixelSum += obj.GetPixelCount();
        }
    }
}