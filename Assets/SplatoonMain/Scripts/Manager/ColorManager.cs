using System;
using System.Collections.Generic;
using Services;
using TMPro;

namespace Splatoon
{
    public class ColorManager : Service, IColorManager
    {
        public List<TextMeshProUGUI> Texts;

        public List<Paintable> _Paintables = new();
        public long _PixelSum = 0;
        private long[] _ColorCounts = new long[Utility.PaintColorCount];
        private Dictionary<PaintColor, int> _DrawCountDict = new();

        protected override void Awake()
        {
            base.Awake();
            foreach (PaintColor color in Utility.PaintColorEnumerator)
            {
                _DrawCountDict.Add(color,0);
            }
        }

        private void Update()
        {
            ResetDrawCountDict();
            for (int i = 0; i < Utility.PaintColorCount; i++)
            {
                _ColorCounts[i] = 0;
            }

            foreach (var paintable in _Paintables)
            {
                paintable.LogicUpdate();
                foreach (PaintColor color in Utility.PaintColorEnumerator)
                {
                    _DrawCountDict[color] += paintable.GetDrawCount(color);
                }
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

        private void ResetDrawCountDict()
        {
            foreach (PaintColor color in Utility.PaintColorEnumerator)
            {
                _DrawCountDict[color] = 0;
            }
        }

        public void RegisterPaintable(Paintable obj)
        {
            _Paintables.Add(obj);
            _PixelSum += obj.GetPixelCount();
        }

        /// <summary>
        /// 放在LateUpdate中，获取某个颜色在该帧的涂色数量
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public int GetDrawCount(PaintColor color) => _DrawCountDict[color];

        public long GetPixelCount() => _PixelSum;
    }
}