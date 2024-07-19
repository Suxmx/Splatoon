using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Services;
// using SplatoonOld;
using UnityEngine;

namespace Splatoon
{
    public abstract class Paintable : MonoBehaviour
    {
        //用于compute buffer 与 shader
        public int ObjPixelWidth = 2048;

        public int ObjPixelHeight = 2048;

        // Shader Property
        private static readonly int ObjPixelWidth_ShaderProp = Shader.PropertyToID("objPixelWidth");
        private static readonly int ObjPixelHeight_ShaderProp = Shader.PropertyToID("objPixelHeight");

        private static readonly int PixelArray_ShaderProp = Shader.PropertyToID("pixelArray");

        // Shader Property Update PerFrame
        private static readonly int DrawTexture_ShaderProp = Shader.PropertyToID("drawTexture");
        private static readonly int DrawWidth_ShaderProp = Shader.PropertyToID("drawWidth");
        private static readonly int DrawHeight_ShaderProp = Shader.PropertyToID("drawHeight");
        private static readonly int DrawMiddlePixelAddress_ShaderProp = Shader.PropertyToID("drawMiddlePixelAddress");
        private static readonly int DrawColorType_ShaderProp = Shader.PropertyToID("drawColorType");
        private static readonly int ColorCntBuffer_ShaderProp = Shader.PropertyToID("colorCounts");

        public PaintColor InitColor = PaintColor.White;

        //Components
        protected MeshRenderer _Mr;
        protected Material _Material;
        protected ComputeShader _ComputeShader;

        //Property
        protected Queue<DrawData> DrawDatas = new();
        protected int _PixelCount = 0;
        private int _KernelIndex;

        //Buffers
        private PixelInfo[] _Pixels;
        private ComputeBuffer _PixelBuffer;

        protected int[] _ColorCounts;
        private ComputeBuffer _ColorCountBuffer;

        private void Awake()
        {
            //Get components
            _Mr = GetComponent<MeshRenderer>();
            _Material = _Mr.material;
            //Init Color Counts
            _ColorCounts = new int[Utility.PaintColorCount];

            //Initialize Compute Shader & Material
            var csTemplate = Resources.Load<ComputeShader>("SplatoonCs");
            _ComputeShader = Instantiate(csTemplate);
            InitializeComputeShader();
        }

        private void Start()
        {
            ServiceLocator.Get<ColorManager>().RegisterPaintable(this);
        }

        private void OnDestroy()
        {
            _PixelBuffer?.Release();
            _ColorCountBuffer?.Release();
        }

        private void Update()
        {
            Draw();
        }

        protected virtual void ComputePixelCount()
        {
            _PixelCount = ObjPixelHeight * ObjPixelWidth;
        }

        protected void InitializeComputeShader()
        {
            ComputePixelCount();
            _ColorCounts[(uint)InitColor] = _PixelCount;
            _Pixels = new PixelInfo[_PixelCount];
            var color = Vector4.one;
            for (int i = 0; i < _Pixels.Length; i++)
            {
                _Pixels[i].Color = (uint)InitColor;
            }

            //Set Buffers
            _PixelBuffer = new ComputeBuffer(_PixelCount, Marshal.SizeOf(typeof(PixelInfo)));
            _PixelBuffer.SetData(_Pixels);

            _ColorCountBuffer = new ComputeBuffer(Utility.PaintColorCount, sizeof(int), ComputeBufferType.Raw);
            _ColorCountBuffer.SetData(_ColorCounts);

            //Set Material
            _Material.SetBuffer(PixelArray_ShaderProp, _PixelBuffer);
            _Material.SetInt(ObjPixelWidth_ShaderProp, ObjPixelWidth);
            _Material.SetInt(ObjPixelHeight_ShaderProp, ObjPixelHeight);
            //Set Compute Shader
            _KernelIndex = _ComputeShader.FindKernel("CSMain");
            _ComputeShader.SetInt(ObjPixelWidth_ShaderProp, ObjPixelWidth);
            _ComputeShader.SetInt(ObjPixelHeight_ShaderProp, ObjPixelHeight);
            _ComputeShader.SetBuffer(_KernelIndex, PixelArray_ShaderProp, _PixelBuffer);
            _ComputeShader.SetBuffer(_KernelIndex, ColorCntBuffer_ShaderProp, _ColorCountBuffer);
        }

        protected void Draw()
        {
            while (DrawDatas.Count > 0)
            {
                var drawData = DrawDatas.Dequeue();
                if (!TryHitPos2UV(drawData.Position, out var uv))
                {
                    break;
                }

                // int width = (int)(drawData.Texture.width * drawData.Scale);
                // int height = (int)(drawData.Texture.height * drawData.Scale);
                int width = (int)(32* drawData.Scale);
                int height = (int)(32 * drawData.Scale);
                Vector4 drawMiddlePixelAddress = new Vector4(uv.x * ObjPixelWidth, uv.y * ObjPixelHeight, 0, 0);

                // _ComputeShader.SetTexture(_KernelIndex, DrawTexture_ShaderProp, drawData.Texture);
                _ComputeShader.SetInt(DrawWidth_ShaderProp, width);
                _ComputeShader.SetInt(DrawHeight_ShaderProp, height);
                _ComputeShader.SetInt(DrawColorType_ShaderProp, (int)drawData.Color);
                _ComputeShader.SetVector(DrawMiddlePixelAddress_ShaderProp, drawMiddlePixelAddress);

                _ComputeShader.Dispatch(_KernelIndex, width / 4, height / 4, 1);
            }

            _ColorCountBuffer.GetData(_ColorCounts);
            // for (int i = 0; i < Utility.PaintColorCount; i++)
            // {
            //     Debug.Log($"{(PaintColor)i} : {_ColorCounts[i]}");
            // }
        }

        protected abstract bool TryHitPos2UV(Vector2 hitPos, out Vector2 uv);

        //Interfaces
        public void AddDrawData(DrawData data)
        {
            DrawDatas.Enqueue(data);
        }

        public int GetPixelCount() => _PixelCount;
        public int[] GetColorCounts() => _ColorCounts;
    }
}