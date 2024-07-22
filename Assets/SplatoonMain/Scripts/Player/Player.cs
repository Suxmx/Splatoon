using System;
using System.Collections.Generic;
using UnityEngine;

namespace Splatoon
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _RotateSpeed = 120f;
        [SerializeField] private float _Speed = 4f;

        private Rigidbody _Rigid;
        private Quaternion _TargetRotation;

        [Header("DEBUG")] [SerializeField] private int childCount;
        [SerializeField] private GameObject _Template;
        [SerializeField] private List<GameObject> _Children;
        private SnakeBrain _SnakeBrain = new();

        private void Awake()
        {
            _Rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            UpdateVelocity();
            UpdateRotation();

            if (Input.GetKeyDown(KeyCode.T))
            {
                for (int i = 0; i < 10; i++)
                {
                    var child = Instantiate(_Template);
                    _SnakeBrain.AddChild(child.transform);
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