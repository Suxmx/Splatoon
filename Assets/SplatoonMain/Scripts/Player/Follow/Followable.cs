using System;
using System.Collections.Generic;
using Services;
using UnityEngine;

namespace Splatoon
{
    public class Followable : MonoBehaviour
    {
        private Queue<FrameInfo> _FrameInfos = new();

        private Followable _Parent;
        private Followable _Child;

        private Rigidbody _TargetRigid;
        private Vector3 _Delta = new Vector3(0, 0, -1);
        private ColorManager manager;

        private bool _IsHead => _Parent is null;
        private bool _IsTail => _Child is null;

        public void Initialize(Followable parent, Rigidbody targetRigid)
        {
            _Parent = parent;
            _TargetRigid = targetRigid;
            manager = ServiceLocator.Get<ColorManager>();
        }

        public void SetChild(Followable child)
        {
            _Child = child;
        }

        public bool TryGetFrameInfo(int frame, out FrameInfo ret)
        {
            bool found = false;
            ret = new FrameInfo();
            while (_FrameInfos.Count > 0 && manager.Frame - _FrameInfos.Peek().Frame > frame)
            {
                ret = _FrameInfos.Dequeue();
                found = true;
            }

            return found;
        }

        public void LogicUpdate(Followable parent)
        {
            if (!_IsHead)
            {
                if (!parent.TryGetFrameInfo(10, out var frameInfo))
                {
                    // _TargetRigid.position = parent.transform.position + parent.transform.rotation * _Delta;
                }
                else
                {
                    _TargetRigid.rotation = frameInfo.Rotation;
                    _TargetRigid.position = frameInfo.Position;
                }
            }

            if (!_IsTail)
            {
                _Child.LogicUpdate(this);
            }

            _FrameInfos.Enqueue(new FrameInfo()
            {
                Position = _TargetRigid.position,
                Rotation = _TargetRigid.rotation,
                Time = Time.time,
                Frame = manager.Frame
            });
            Debug.Log($"{gameObject.name}: {_FrameInfos.Count}");
        }
    }
}