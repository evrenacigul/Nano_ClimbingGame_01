using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Utilities;

namespace Nano.Handlers
{
    public class CameraShakeHandler : SingletonMonoBehaviour<CameraShakeHandler>
    {
        [SerializeField] float shakeMagnitude;
        [SerializeField] float shakeDuration;

        Camera currentCamera;

        bool isShaking;

        void Start()
        {
            if (!currentCamera)
                currentCamera = Camera.main;
        }

        public void Shake()
        {
            if(!isShaking)
                StartCoroutine(ShakeCamera(shakeDuration, shakeMagnitude));
        }

        IEnumerator ShakeCamera(float delay, float magnitude)
        {
            var originPos = currentCamera.transform.position;

            var delayed = 0f;

            isShaking = true;

            while (delayed < delay)
            {
                float x = originPos.x + (Random.Range(-1f, 1f) * magnitude);
                float y = originPos.y + (Random.Range(-1f, 1f) * magnitude);

                currentCamera.transform.position = new Vector3(x, y, originPos.z);

                delayed += Time.deltaTime;

                yield return null;
            }

            isShaking = false;

            currentCamera.transform.position = originPos;
        }
    }
}