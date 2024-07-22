using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Splatoon
{
    public class SnakeBrain
    {
        private LinkedList<FrameInfo> _FrameInfos = new();
        private List<Transform> _Children = new();

        private Queue<Transform> _ChildrenToSpawn = new();

        private const int logicNumPerObj = 5;

        public void RecordHeadFrameInfo(Vector3 position, Quaternion rotation)
        {
            _FrameInfos.AddFirst(new FrameInfo()
            {
                Position = position,
                Rotation = rotation
            });
            while (_FrameInfos.Count > (_Children.Count + _ChildrenToSpawn.Count + 5) * logicNumPerObj)
            {
                _FrameInfos.RemoveLast();
            }
        }

        public void UpdateChildren()
        {
            while (_ChildrenToSpawn.Count > 0)
            {
                var childTrans = _ChildrenToSpawn.Dequeue();
                childTrans.gameObject.SetActive(true);
                _Children.Add(childTrans);
            }

            Assert.IsTrue(_FrameInfos.Count > 0, "No FrameInfo was Recorded");
            var frameInfoEnumerator = _FrameInfos.GetEnumerator();
            FrameInfo currentFrameInfo = frameInfoEnumerator.Current;
            for (int i = 0; i < _Children.Count; i++)
            {
                for (int j = 0; j < logicNumPerObj; j++)
                {
                    if (frameInfoEnumerator.MoveNext()) currentFrameInfo = frameInfoEnumerator.Current;
                }

                _Children[i].position = currentFrameInfo.Position;
                _Children[i].rotation = currentFrameInfo.Rotation;
            }
        }

        public void AddChild(Transform childTrans)
        {
            _ChildrenToSpawn.Enqueue(childTrans);
            childTrans.gameObject.SetActive(false);
        }
    }
}