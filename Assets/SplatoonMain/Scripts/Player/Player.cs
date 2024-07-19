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

        private Followable _Tail;

        private void Awake()
        {
            _Rigid = GetComponent<Rigidbody>();
            _Children = new();
            _Children.Add(gameObject);
            var followable = gameObject.AddComponent<Followable>();
            followable.Initialize(null, _Rigid);
            _Tail = followable;
        }

        private void Update()
        {
            UpdateVelocity();
            UpdateRotation();
            if (Input.GetKeyDown(KeyCode.T))
            {
                var obj = Instantiate(_Template);
                _Children.Add(obj);
                var followable = obj.AddComponent<Followable>();
                followable.Initialize(_Tail,obj.GetComponent<Rigidbody>());
                _Tail.SetChild(followable);
                _Tail = followable;
            }
        }

        private void FixedUpdate()
        {
           GetComponent<Followable>().LogicUpdate(null);
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