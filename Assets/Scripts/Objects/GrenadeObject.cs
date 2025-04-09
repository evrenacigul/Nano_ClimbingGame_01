using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Controllers;
using Nano.Handlers;

namespace Nano.Objects
{
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
    public class GrenadeObject : MonoBehaviour
    {
        public float impactRadius = 2f;

        public float impactPower = 5f;

        Rigidbody rBody;
        SphereCollider sphCollider;

        bool isThrown = false;
        bool isTriggered = false;

        private void Start()
        {
            rBody = GetComponent<Rigidbody>();
            sphCollider = GetComponent<SphereCollider>();

            rBody.useGravity = false;
            sphCollider.enabled = false;

            LeanTween.delayedCall(gameObject, 15f, () => 
            {
                if (gameObject is not null) Destroy(gameObject);
            });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isThrown || isTriggered || !other.CompareTag("Enemy")) return;

            LeanTween.cancel(gameObject);
            Explode();
            CameraShakeHandler.Instance.Shake();
            isTriggered = true;
        }

        private void Explode()
        {
            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, impactRadius);

            foreach (Collider hit in colliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    object[] grenadeInfo = new object[3];
                    grenadeInfo[0] = impactPower;
                    grenadeInfo[1] = impactRadius;
                    grenadeInfo[2] = transform.position;

                    EnemyController enemyController = hit.GetComponent<EnemyController>();
                    enemyController.GetHit(true, grenadeInfo);
                }
            }

            EventManager.Instance.OnGrenadeExplosion?.Invoke(transform.position);
            Destroy(gameObject);
        }

        public void Throw()
        {
            if(rBody is null)
                rBody = GetComponent<Rigidbody>();

            if (sphCollider is null)
                sphCollider = GetComponent<SphereCollider>();

            rBody.useGravity = true;
            sphCollider.enabled = true;
            isThrown = true;
        }
    }
}