using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DitzelGames.FastIK;

namespace Nano.Handlers
{
    public class RagdollHandler : MonoBehaviour
    {
        [SerializeField]
        Rigidbody[] ragdollRigidbodies;

        [SerializeField]
        Collider[] ragdollColliders;

        [SerializeField]
        FastIKFabric[] IK_list;

        [SerializeField]
        private Animator animator;

        void Start()
        {
            ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
            ragdollColliders = GetComponentsInChildren<Collider>();
            animator = GetComponent<Animator>();

            //StopRagdoll();
        }

        public void StartRagdoll()
        {
            animator.enabled = false;

            if(ragdollRigidbodies != null)
                foreach (Rigidbody rg in ragdollRigidbodies)
                {
                    rg.useGravity = true;
                    rg.isKinematic = false;
                }

            if(ragdollColliders != null)
                foreach (Collider col in ragdollColliders)
                {
                    col.enabled = true;
                }

            if(IK_list != null)
                foreach (FastIKFabric IK in IK_list)
                {
                    IK.enabled = false;
                }
        }

        public void PushRagdoll(float force, Vector3 direction)
        {
            foreach (Rigidbody rg in ragdollRigidbodies)
            {
                if(rg.name.Contains("Head"))
                    rg.AddForce(direction * force, ForceMode.Force);
            }
        }

        public void StopRagdoll()
        {
            foreach (Rigidbody rg in ragdollRigidbodies)
            {
                if (rg.gameObject.transform.parent != null)
                {
                    rg.isKinematic = true;
                    rg.useGravity = false;
                }
            }
            foreach (Collider col in ragdollColliders)
            {
                if (col.gameObject.transform.parent != null && col != GetComponent<CapsuleCollider>() && col != GetComponent<BoxCollider>())
                    col.enabled = false;
            }
            //foreach (FastIKFabric IK in IK_list)
            //{
            //    IK.enabled = true;
            //}
        }
    }
}