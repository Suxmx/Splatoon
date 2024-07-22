using System;
using System.Collections.Generic;
using Services;
using UnityEngine;

namespace Splatoon
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _RotateSpeed = 120f;
        [SerializeField] private float _Speed = 4f;

        private Rigidbody _Rigid;
        private Quaternion _TargetRotation;
        private SnakeBrain _SnakeBrain = new();
        private ColorManager _ColorManager;
        private Painter _Painter;

        private int _Level = 0;

        private readonly float[] upgradeExp = new[] { 0.01f, 0.05f, 0.15f };

        private Transform _TrailCacheParent;
        // private const float _Level01 = 0.05f;
        // private const float _Level12 = 0.15f;
        // private const float _Level23 = 0.30f;

        [Header("DEBUG")] [SerializeField] private int childCount;
        [SerializeField] private GameObject _Template;
        [SerializeField] private List<GameObject> _Children;
        private Material _TrailMaterial;


        private void Awake()
        {
            _Rigid = GetComponent<Rigidbody>();
            _Painter = GetComponent<Painter>();
            _TrailCacheParent = GameObject.Find("TrailCache").transform;
            _TrailMaterial = transform.Find("Trail").GetComponent<TrailRenderer>().material;
        }

        private void Start()
        {
            _ColorManager = ServiceLocator.Get<ColorManager>();
        }

        private void Update()
        {
            UpdateVelocity();
            UpdateRotation();
            CheckUpgrade();

            //For DEBUG
            if (Input.GetKeyDown(KeyCode.T))
            {
                for (int i = 0; i < 10; i++)
                {
                    AddChild();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _Speed = 10;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                _Speed = 8;
            }
        }

        /// <summary>
        /// 查看是否能升级或者变长
        /// </summary>
        private void CheckUpgrade()
        {
            float drawPercent = _Painter.GetDrawCountSum() * 1.0f / _ColorManager.GetPixelCount();
            if (_Level < upgradeExp.Length && drawPercent >= upgradeExp[_Level])
            {
                _Level++;
                OnUpgrade();
                Debug.Log("Upgrade to Level" + _Level);
            }
        }

        private void InitNewTrailRenderer(float width)
        {
            GameObject newTrailObj = new GameObject("Trail");
            newTrailObj.transform.position = transform.position;
            newTrailObj.transform.SetParent(transform);
            newTrailObj.transform.Rotate(Vector3.right,90);
            newTrailObj.transform.localPosition = new Vector3(0, -0.4f, 0);

            var newTrail = newTrailObj.AddComponent<TrailRenderer>();
            newTrail.time = float.MaxValue;
            newTrail.minVertexDistance = 0.2f;
            newTrail.startWidth = width;
            newTrail.endWidth = width;
            newTrail.numCapVertices = 3;
            newTrail.numCornerVertices = 3;
            newTrail.material = _TrailMaterial;
            newTrail.alignment = LineAlignment.TransformZ;
        }

        private void OnUpgrade()
        {
            AddChild();
            //move old trail to trail cache
            var trail = transform.Find("Trail");
            trail.SetParent(_TrailCacheParent);
            trail.GetComponent<TrailRenderer>().emitting = false;
            //create new trail
            InitNewTrailRenderer(1f + (_Level * 0.5f));
            _Painter.SetScale(1f + (_Level * 0.5f));
            // newTrail.material
        }

        private void AddChild()
        {
            var child = Instantiate(_Template);
            _SnakeBrain.AddChild(child.transform);
        }

        private void FixedUpdate()
        {
            _SnakeBrain.RecordHeadFrameInfo(transform.position, transform.rotation);
            _SnakeBrain.UpdateChildren();
        }

        private void UpdateRotation()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 lookDir = new Vector3(horizontal, 0, vertical);
            if (lookDir != Vector3.zero)
            {
                _TargetRotation = Quaternion.LookRotation(lookDir);
            }

            _Rigid.rotation = Quaternion.RotateTowards(_Rigid.rotation, _TargetRotation, 14f);
        }

        private void UpdateVelocity()
        {
            _Rigid.velocity = (_Rigid.rotation) * Quaternion.AngleAxis(-90, Vector3.up) *
                              new Vector3(1 * _Speed, 0, 0);
        }
    }
}