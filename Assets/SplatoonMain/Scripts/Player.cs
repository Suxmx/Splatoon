using System;
using UnityEngine;

namespace Splatoon
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _RotateSpeed = 120f;
        [SerializeField] private float _Speed = 4f;

        private Painter _Painter;
        private Rigidbody _Rigid;

        private void Awake()
        {
            _Painter = GetComponent<Painter>();
            _Rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            UpdateRotation();
            UpdateVelocity();
        }

        private void UpdateRotation()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            transform.Rotate(Vector3.up, _RotateSpeed * horizontal * Time.deltaTime);
        }

        private void UpdateVelocity()
        {
            // float vertical = Input.GetAxisRaw("Vertical");
            _Rigid.velocity = (transform.rotation) * Quaternion.AngleAxis(-90, Vector3.up) *
                              new Vector3(1 * _Speed, 0, 0);
        }
    }
}