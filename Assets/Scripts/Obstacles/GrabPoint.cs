using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Controllers;

namespace Nano.Obstacles
{
    public class GrabPoint : MonoBehaviour
    {
        public Transform leftHandIKPoint;
        public Transform rightHandIKPoint;
        public Transform leftFootIKPoint;
        public Transform rightFootIKPoint;

        public Transform rocksParent;

        public bool isShaky;
        public float shakyRockDelayToFall = 5f;

        public Outline outline;

        public bool IsGrabbed { get { return isGrabbed; } }

        [SerializeField]
        private bool isGrabbed = false;

        [SerializeField]
        private bool isGrabbedFirst = false;

        private float defaultOutlineWidth;

        private Rigidbody rBody;

        private void Awake()
        {
            SetIKPos(leftHandIKPoint, new Vector3(-0.25f, 0f), 0f);
            SetIKPos(rightHandIKPoint, new Vector3(0.25f, 0f), 0f);

            SetIKPos(leftFootIKPoint, new Vector3(0f, -0.25f), 0f);
            SetIKPos(rightFootIKPoint, new Vector3(0f, -0.25f), 0f);

            defaultOutlineWidth = outline.OutlineWidth;
            outline.OutlineWidth = 0f;

            if (isShaky) return;
            
            var count = rocksParent.childCount;

            var random = Random.Range(0, count);

            for (int i = 0; i < count; i++)
            {
                var child = rocksParent.GetChild(i);

                if (random == i)
                    child.gameObject.SetActive(true);
                else
                    child.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            rBody = gameObject.AddComponent<Rigidbody>();
            rBody.useGravity = false;
            rBody.isKinematic = true;
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance.GetGameState != GameStates.Play || isGrabbed || isGrabbedFirst) return;

            isGrabbedFirst = true;

            EventManager.Instance.OnGrabPointSelected?.Invoke(this);
        }

        private void SetIKPos(Transform ikObj, Vector3 toPos, float time)
        {
            var pos = ikObj.position + toPos;

            LeanTween.move(ikObj.gameObject, pos, time);
        }

        public void CancelJump()
        {
            isGrabbedFirst = false;
        }

        public void Grabbed(PlayerController player)
        {
            SetIKPos(leftHandIKPoint, new Vector3(0.25f, 0f), 0.05f);
            SetIKPos(rightHandIKPoint, new Vector3(-0.25f, 0f), 0.05f);

            SetIKPos(leftFootIKPoint, new Vector3(0f, 0.25f), 0.05f);
            SetIKPos(rightFootIKPoint, new Vector3(0f, 0.25f), 0.05f);

            if (isShaky)
            {
                EventManager.Instance.OnShakyRock?.Invoke(this);

                StartCoroutine(ShakyCountDown(player));
                //LeanTween.delayedCall(shakyRockDelayToFall, () => 
                //{
                //    EventManager.Instance.OnShakyRockFalls?.Invoke(this);
                //    Released();
                //});
            }
        }

        IEnumerator ShakyCountDown(PlayerController player)
        {
            float countDown = shakyRockDelayToFall;
            while(countDown > 0)
            {
                if(player.GetPlayerState == PlayerStates.Playing)
                {
                    countDown -= Time.deltaTime;
                }

                yield return null;
            }
            EventManager.Instance.OnShakyRockFalls?.Invoke(this);
            Released();

            yield return null;
        }

        public void Released()
        {
            isGrabbed = true;

            LeanTween.delayedCall(0.1f, () =>
            {
                if (!gameObject) return;

                rBody.useGravity = true;
                rBody.isKinematic = false;
                rBody.mass = 10f;

                Destroy(gameObject, 5f);
            });
        }

        public void ShowHighlight()
        {
            if (isGrabbed) return;

            //var color = Color.red;
            //color.a = 0.85f;

            if (!LeanTween.isTweening(gameObject))
                LeanTween.value(gameObject, 0f, defaultOutlineWidth, 0.5f).setOnUpdate((float val) => 
                {
                    outline.OutlineWidth = val;
                }).setLoopPingPong();
        }

        public void StopHighlight()
        {
            if (isGrabbed) return;

            if (!LeanTween.isTweening(gameObject)) return;

            LeanTween.cancel(gameObject);
            outline.OutlineWidth = 0f;
        }
    }
}