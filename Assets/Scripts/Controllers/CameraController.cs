using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;

namespace Nano.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        Vector3 offset;

        [SerializeField]
        float smoothTime = 0.2f;

        [SerializeField]
        float fovZoomIn;

        [SerializeField]
        float fovZoomOut;
        
        GameObject playerObj;

        bool isFollowing = false;

        Camera currentCam;

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            EventManager.Instance.OnEnemyCatchPlayer.AddListener(OnEnemyCatchPlayer);
            EventManager.Instance.OnEnemyReleasePlayer.AddListener(OnEnemyReleasePlayer);

            currentCam = Camera.main;

            fovZoomOut = currentCam.fieldOfView;
        }

        private void OnGameStateChanged(GameStates gameStates)
        {
            switch(gameStates)
            {
                case GameStates.Load:
                    break;

                case GameStates.Ready:
                    playerObj = GameObject.Find("Player");

                    if (playerObj)
                    {
                        SetInitialPosition();

                        isFollowing = true;
                        StartCoroutine(SmoothFollow());
                    }
                    else
                        throw new UnityException("Can't find Player gameObject");
                    break;

                case GameStates.Win:
                case GameStates.Fail:
                    isFollowing = false;
                    StopCoroutine(SmoothFollow());
                    break;
            }
        }

        private void OnEnemyCatchPlayer(object [] args)
        {
            if (!LeanTween.isTweening(gameObject))
            {
                LeanTween.value(fovZoomOut, fovZoomIn, 0.2f).setOnUpdate((float val) =>
                {
                    currentCam.fieldOfView = val;
                });
            }
        }

        private void OnEnemyReleasePlayer()
        {
            if (!LeanTween.isTweening(gameObject))
            {
                LeanTween.value(fovZoomIn, fovZoomOut, 0.2f).setOnUpdate((float val) =>
                {
                    currentCam.fieldOfView = val;
                });
            }
        }

        private void SetInitialPosition()
        {
            var targetPos = playerObj.transform.position;

            targetPos.z = transform.position.z;

            targetPos += offset;

            transform.position = targetPos;

            currentCam.fieldOfView = fovZoomOut;
        }

        private IEnumerator SmoothFollow()
        {
            while (isFollowing)
            {
                var velocity = Vector3.zero;

                if (!playerObj) playerObj = GameObject.Find("Player");
                if (!playerObj) StopCoroutine(SmoothFollow());

                var targetPos = playerObj.transform.position;

                targetPos.z = transform.position.z;

                targetPos += offset;

                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

                yield return null;
            }
        }
    }
}