#define DEBUGPAINT

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Services;
using UnityEngine;
using UnityEngine.Serialization;


namespace SplatoonOld
{
    public abstract class PaintableObject : MonoBehaviour
    {
        //用于compute buffer 与 shader
        public int ObjPixelWidth = 2048;
        public int ObjPixelHeight = 2048;

        public PaintColor InitializeColor = PaintColor.White;

        // Shader Property
        private static readonly int ObjPixelWidth_ShaderProp = Shader.PropertyToID("objPixelWidth");
        private static readonly int ObjPixelHeight_ShaderProp = Shader.PropertyToID("objPixelHeight");
        private static readonly int PixelArray_ShaderProp = Shader.PropertyToID("pixelArray");

        private static readonly int ColorConstants_ShaderProp = Shader.PropertyToID("ColorConstants");

        // Shader Property Update PerFrame
        private static readonly int DrawTexture_ShaderProp = Shader.PropertyToID("drawTexture");
        private static readonly int DrawWidth_ShaderProp = Shader.PropertyToID("drawWidth");
        private static readonly int DrawHeight_ShaderProp = Shader.PropertyToID("drawHeight");
        private static readonly int DrawMiddlePixelAddress_ShaderProp = Shader.PropertyToID("drawMiddlePixelAddress");
        private static readonly int DrawColorType_ShaderProp = Shader.PropertyToID("drawColorType");
        private static readonly int ColorCntBuffer_ShaderProp = Shader.PropertyToID("colorCountBuffer");


        //Components
        protected MeshRenderer _Mr;
        protected Material _Material;
        protected ComputeShader _ComputeShader;

        //Property
        protected Queue<DrawData> DrawDatas = new();
        protected int _PixelCount = 0;
        private int _KernelIndex;

        private PixelInfo[] _Pixels;
        private ComputeBuffer _PixelBuffer;

        protected int[] _ColorCounts;
        private ComputeBuffer _ColorCountBuffer;
        private ComputeBuffer _ColorConstantsBuffer;


        private void Awake()
        {
            //Get components
            _Mr = GetComponent<MeshRenderer>();
            _Material = _Mr.material;
            //Init Color Counts
            _ColorCounts = new int[Utility.PaintColorCount()];

            //Initialize Compute Shader & Material

            var csTemplate = Resources.Load<ComputeShader>("SplatoonCs");
            _ComputeShader = Instantiate(csTemplate);
            InitializeComputeShader();
        }

        private void Start()
        {
            var colorManager = ServiceLocator.Get<ColorManager>();
            colorManager.RegisterPaintable(this);
        }

        protected virtual void ComputePixelCount()
        {
            _PixelCount = ObjPixelHeight * ObjPixelWidth;
        }

        protected virtual void InitializeComputeShader()
        {
            ComputePixelCount();
            _ColorCounts[(int)InitializeColor] = _PixelCount;
            _Pixels = new PixelInfo[_PixelCount];
            var color = Vector4.one;
            for (int i = 0; i < _Pixels.Length; i++)
            {
                _Pixels[i].ColorType = (int)InitializeColor;
                _Pixels[i].MainColor = color;
            }

            //Set Buffers
            _PixelBuffer = new ComputeBuffer(_PixelCount, Marshal.SizeOf(typeof(PixelInfo)));
            _PixelBuffer.SetData(_Pixels);

            _ColorCountBuffer = new ComputeBuffer(Utility.PaintColorCount(), sizeof(int), ComputeBufferType.Raw);
            _ColorCountBuffer.SetData(_ColorCounts);

            _ColorConstantsBuffer = new ComputeBuffer(Utility.PaintColorCount(), 4 * sizeof(float));
            _ColorConstantsBuffer.SetData(Utility.ColorConstants);
            //Set Material
            _Material.SetBuffer(PixelArray_ShaderProp, _PixelBuffer);
            _Material.SetInt(ObjPixelWidth_ShaderProp, ObjPixelWidth);
            _Material.SetInt(ObjPixelHeight_ShaderProp, ObjPixelHeight);
            //Set Compute Shader
            _KernelIndex = _ComputeShader.FindKernel("CSMain");
            _ComputeShader.SetInt(ObjPixelWidth_ShaderProp, ObjPixelWidth);
            _ComputeShader.SetInt(ObjPixelHeight_ShaderProp, ObjPixelHeight);
            _ComputeShader.SetBuffer(_KernelIndex, ColorConstants_ShaderProp, _ColorConstantsBuffer);
            _ComputeShader.SetBuffer(_KernelIndex, PixelArray_ShaderProp, _PixelBuffer);
            _ComputeShader.SetBuffer(_KernelIndex, ColorCntBuffer_ShaderProp, _ColorCountBuffer);
        }

        private void OnDestroy()
        {
#if DEBUGPAINT
            _DebugBuffer?.Release();
#endif

            _PixelBuffer?.Release();
            _ColorCountBuffer?.Release();
            _ColorConstantsBuffer?.Release();
        }

#if DEBUGPAINT
        private DebugInfo[] _DebugInfos;
        private ComputeBuffer _DebugBuffer;

        private void SetDebugInfo(int width, int height)
        {
            _DebugInfos = new DebugInfo[width * height];
            _DebugBuffer = new ComputeBuffer(width * height, Marshal.SizeOf(typeof(DebugInfo)));
            _ComputeShader.SetBuffer(_KernelIndex, "debugInfos", _DebugBuffer);
        }

        private void GetDebugInfo()
        {
            _DebugBuffer.GetData(_DebugInfos);
            Debug.Log(_DebugInfos);
        }
#endif

        public void AddDrawData(DrawData data)
        {
            DrawDatas.Enqueue(data);
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

                int width = (int)(drawData.Texture.width * drawData.Scale);
                int height = (int)(drawData.Texture.height * drawData.Scale);
                Vector4 drawMiddlePixelAddress = new Vector4(uv.x * ObjPixelWidth, uv.y * ObjPixelHeight, 0, 0);

                _ComputeShader.SetTexture(_KernelIndex, DrawTexture_ShaderProp, drawData.Texture);
                _ComputeShader.SetInt(DrawWidth_ShaderProp, width);
                _ComputeShader.SetInt(DrawHeight_ShaderProp, height);
                _ComputeShader.SetInt(DrawColorType_ShaderProp, (int)drawData.Color);
                _ComputeShader.SetVector(DrawMiddlePixelAddress_ShaderProp, drawMiddlePixelAddress);
#if DEBUGPAINT
                SetDebugInfo(width, height);
#endif

                _ComputeShader.Dispatch(_KernelIndex, width / 4, height / 4, 1);
#if DEBUGPAINT
                GetDebugInfo();
#endif
            }

            _ColorCountBuffer.GetData(_ColorCounts);
        }

        public int[] GetColorCounts()
        {
            return _ColorCounts;
        }

        public int GetPixelCount()
        {
            return _PixelCount;
        }

        protected abstract bool TryHitPos2UV(Vector2 hitPos, out Vector2 uv);

        private void Update()
        {
            Draw();
        }
    }
}