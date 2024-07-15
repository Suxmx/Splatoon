using UnityEngine;

namespace Splatoon
{
    public static class Utility
    {
        public static int PaintColorCount() => System.Enum.GetValues(typeof(PaintColor)).Length;

        public static Vector4[] ColorConstants = new[]
        {
            // White
            new Vector4(1, 1, 1, 1),
            // Red
            new Vector4(1, 0, 0, 1),
            // Green
            new Vector4(0, 1, 0, 1),
            // Blue
            new Vector4(0, 0, 1, 1),
            // Yellow
            new Vector4(1, 1, 0, 1)
        };
    }
}