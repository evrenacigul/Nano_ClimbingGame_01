using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Objects;
using Nano.Managers;
using Nano.Extensions;
using Nano.Animators;

namespace Nano.Handlers
{
    public class BoneStretchHandler : MonoBehaviour
    {
        public LimbObject limbObject;
        public Transform target;
        public List<Transform> bones;
        public PlayerAnimator animator;

        public string stretchAnimationName;
        public int stretchAnimationLayer;
        public int stretchAnimationFrameCount;

        [Range(1f, 5f)]
        public float scaleModifier = 1f;

        [Range(0f, 10f)]
        public float maxDistance = 1f;

        private void Start()
        {
            //EventManager.Instance.OnHandlingLimb.AddListener(OnHandlingLimb);

            if (!animator) animator = GetComponentInParent<PlayerAnimator>();
        }

        private void OnHandlingLimb(LimbObject obj)
        {
            if (obj != limbObject) return;

            var startPos = limbObject.transform.parent.position;
            var endPos = target.position;
            //target.LookAt((endPos - limbObject.transform.position) * 2);
            //target.rotation = Quaternion.Euler(Vector3.RotateTowards(startPos, endPos, 0.1f, 0.1f));
            var distance = Vector3.Distance(startPos, endPos);
            
            //var remapped = distance.Remap(0, maxDistance, 1, scaleModifier);
            //var remapped = distance.Remap(0, maxDistance, 0, stretchAnimationFrameCount);
            var remapped = distance.Remap(0, maxDistance, 0, 1);

            Debug.Log("Remapped weight = " + remapped.ToString());

            animator.PlayAnimation(stretchAnimationLayer, stretchAnimationName);
            animator.SetWeight(stretchAnimationLayer, remapped);
            //foreach(Transform bone in bones)
            //{
            //    var scale = bone.localScale;
            //    scale.y = remapped;
            //    bone.localScale = scale;
            //}
        }
    }
}