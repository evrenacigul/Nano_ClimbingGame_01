using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;

namespace Nano.Objects
{
    public class LimbObject : MonoBehaviour
    {
        [SerializeField]
        private Transform limbTarget;

        private void OnMouseDown()
        {
            //Debug.Log("Selected " + name);
        }

        private void OnMouseUp()
        {
            //Debug.Log("Released " + name);
        }

        public Vector3 SetTargetPos(Vector3 targetPos)
        {
            var refVel = Vector3.zero;

            limbTarget.position = Vector3.SmoothDamp(limbTarget.position, targetPos, ref refVel, 0.01f);

            Debug.Log("targetPos = " + targetPos + " | limbPos = " + limbTarget.position);

            return refVel;
        }
    }
}