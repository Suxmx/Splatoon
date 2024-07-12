using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Splatoon
{
    public class DynamicTextureObject : MonoBehaviour
    {
        //public Texture test_texture;
        public Material m_mat;

        [Range(16, 8192)] public int computePixelLength = 2048;

        //public Texture _mainTexture;
        public ComputeShader Cs;
        private ComputeBuffer _Cb;
        private ComputeBuffer _CountBuffer;
        private int _KernelIndex;
        private Queue<DrawOnceInfo> _DrawInfoQueue = new Queue<DrawOnceInfo>();

        public struct PixelInfo
        {
            public Vector4 MainColor;
            public int ColorType;
        }

        private PixelInfo[] _PixelArray;
        private int[] _ColorCntBuffer = new int[5];

        private readonly Vector4[] _PaintColors = new Vector4[]
        {
            new Vector4(1, 1, 1, 1),
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 0, 1, 1),
            new Vector4(1, 1, 0, 1)
        };

        private bool _InitializeComplete = false;

        // Shader Property
        private static readonly int TokenHeight = Shader.PropertyToID("tokenHeight");
        private static readonly int TokenWidth = Shader.PropertyToID("tokenWidth");
        private static readonly int TokenTexture = Shader.PropertyToID("tokenTexture");
        private static readonly int TokenOffset = Shader.PropertyToID("tokenOffset");
        private static readonly int TokenScale = Shader.PropertyToID("tokenScale");
        private static readonly int InputWidth = Shader.PropertyToID("inputWidth");
        private static readonly int InputHeight = Shader.PropertyToID("inputHeight");
        private static readonly int PixelArray = Shader.PropertyToID("pixelArray");
        private static readonly int CurColorType = Shader.PropertyToID("curColorType");
        private static readonly int ColorCntBuffer = Shader.PropertyToID("colorCntBuffer");

        private void OnEnable()
        {
            MeshRenderer m_meshrender = transform.GetComponent<MeshRenderer>();
            if (m_meshrender != null)
            {
                m_mat = m_meshrender.material;
            }

            if (Cs == null || m_mat == null)
            {
                _InitializeComplete = false;
                return;
            }
            else
            {
                _InitializeComplete = true;
            }


            int kernelCount = computePixelLength * computePixelLength;
            _PixelArray = new PixelInfo[kernelCount];
            //设置初始颜色
            for (int i = 0; i < _PixelArray.Length; i++)
                _PixelArray[i].ColorType = (int)PaintColor.Red;
            _ColorCntBuffer[(int)PaintColor.Red] = kernelCount;
            
            _Cb = new ComputeBuffer(kernelCount, Marshal.SizeOf(typeof(PixelInfo)));
            _Cb.SetData(_PixelArray);
            
            _CountBuffer = new ComputeBuffer(5, sizeof(int), ComputeBufferType.Raw);
            _CountBuffer.SetData(_ColorCntBuffer);

            ComputeBuffer colorConstants = new ComputeBuffer(5, sizeof(float) * 4);
            colorConstants.SetData(_PaintColors);
            _KernelIndex = Cs.FindKernel("CSMain");
            //m_cs.SetTexture(kernelIndex, "inputTexture", _mainTexture);
            //将ComputeBuffer设置给材质球，以供shader使用
            m_mat.SetBuffer(PixelArray, _Cb);
            m_mat.SetInt(InputWidth, computePixelLength);
            m_mat.SetInt(InputHeight, computePixelLength);
            
            
            
            Cs.SetInt(InputWidth, computePixelLength);
            Cs.SetInt(InputHeight, computePixelLength);
            //设置颜色常量
            Cs.SetBuffer(_KernelIndex,"paintColor",colorConstants);
            Cs.SetBuffer(_KernelIndex, PixelArray, _Cb);
            Cs.SetBuffer(_KernelIndex, ColorCntBuffer, _CountBuffer);
            //AddDrawInfo(new Vector4(0,0,0,0), test_texture);
        }

        public void AddDrawInfo(DrawOnceInfo m_DrawOnceInfo)
        {
            _DrawInfoQueue.Enqueue(m_DrawOnceInfo);
        }

        void ComputeOnce(DrawOnceInfo _drawInfo)
        {
            Cs.SetInt(TokenWidth, _drawInfo.texture.width);
            Cs.SetInt(TokenHeight, _drawInfo.texture.height);

            Cs.SetTexture(_KernelIndex, TokenTexture, _drawInfo.texture);
            Vector4 pixelAddress = new Vector4(_drawInfo.hitUV.x * computePixelLength - (_drawInfo.texture.width) / 2,
                _drawInfo.hitUV.y * computePixelLength - (_drawInfo.texture.height) / 2, 0, 0);
            Debug.Log(pixelAddress);
            Cs.SetVector(TokenOffset, pixelAddress);
            Cs.SetFloat(TokenScale, _drawInfo.textureScale);
            Cs.SetInt(InputWidth, computePixelLength);
            Cs.SetInt(InputHeight, computePixelLength);
            Cs.SetInt(CurColorType, (int)_drawInfo.color);
            Cs.Dispatch(_KernelIndex, computePixelLength / 8, computePixelLength / 8, 1);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_InitializeComplete || _DrawInfoQueue.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _DrawInfoQueue.Count; i++)
            {
                var temp = _DrawInfoQueue.Dequeue();
                ComputeOnce(temp);
            }

            
            
            // int redCnt =Cs.ge;
        }

        public int[] GetScores()
        {
            _CountBuffer.GetData(_ColorCntBuffer);
            return _ColorCntBuffer;
        }

        public int GetPixelsCount()
        {
            return computePixelLength * computePixelLength;
        }

        private void OnDestroy()
        {
            _Cb?.Release();
        }
    }


    public class DrawOnceInfo
    {
        public Vector4 hitUV;
        public Texture texture;
        public float textureScale;
        public PaintColor color;
    }
}