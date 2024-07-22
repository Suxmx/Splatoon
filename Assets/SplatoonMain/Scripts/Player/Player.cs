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

        [Header("DEBUG")] [SerializeField] private GameObject _Template;

        private void Awake()
        {
            _Rigid = GetComponent<Rigidbody>();
            _Painter = GetComponentInChildren<Painter>();
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
                _Speed = 4;
            }
        }

        /// <summary>
        /// 查看是否能升级或者变长
        /// </summary>
        private void CheckUpgrade()
        {
            float drawPercent = _Painter.GetDrawCountSum() * 1.0f / _ColorManager.GetPixelCount();
            Debug.Log(drawPercent);
            if (_Level < upgradeExp.Length && drawPercent >= upgradeExp[_Level])
            {
                _Level++;
                OnUpgrade();
                Debug.Log("Upgrade to Level" + _Level);
            }
        }

        private void OnUpgrade()
        {
            AddChild();
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