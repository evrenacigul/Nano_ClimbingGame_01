using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Controllers;
using DitzelGames.FastIK;

namespace Nano.Handlers
{
    public class EnemyCatchHandler : MonoBehaviour
    {
        [SerializeField]
        FastIKFabric rightFootIK;

        [SerializeField]
        FastIKFabric leftFootIK;

        [SerializeField]
        Transform rightFootIKTarget;

        [SerializeField]
        Transform leftFootIKTarget;

        EnemyController currentEnemy;

        PlayerController playerController;

        EnemyCatchFoot catchedFoot;

        bool isPlayerHitting = false;
        bool isCatched = false;

        private void Start()
        {
            EventManager.Instance.OnEnemyCatchPlayer.AddListener(OnEnemyCatchPlayer);
            EventManager.Instance.OnEnemyDie.AddListener(OnEnemyDied);
            EventManager.Instance.OnInputTouchStart.AddListener(OnInputTouchStart);
            EventManager.Instance.OnPlayerDie.AddListener(OnPlayerDie);
        }

        public void SetPlayer(PlayerController player, Transform rightFootTarget, Transform leftFootTarget,
            FastIKFabric rightIK, FastIKFabric leftIK)
        {
            playerController = player;
            
            rightFootIKTarget = rightFootTarget;
            leftFootIKTarget = leftFootTarget;

            rightFootIK = rightIK;
            leftFootIK = leftIK;
        }

        private void OnEnemyCatchPlayer(object [] args)
        {
            currentEnemy = (EnemyController)args[0];
            catchedFoot = (EnemyCatchFoot)args[1];

            isCatched = true;

            StartCoroutine(HitEnemy());
        }

        private void OnInputTouchStart(Vector2 pos)
        {
            if (!isCatched) return;

            isPlayerHitting = true;
        }

        private void OnPlayerDie()
        {
            isCatched = false;
        }

        private void OnEnemyDied(object[] args)
        {
            if(currentEnemy == (EnemyController)args[0])
            {
                isCatched = false;
                isPlayerHitting = false;
                EventManager.Instance.OnEnemyReleasePlayer?.Invoke();
                TimeController.Instance.StopSlowMotion();
            }
        }

        IEnumerator HitEnemy()
        {
            TimeController.Instance.StartSlowMotion(0.3f);
            Transform selectedFootIKTarget;
            
            FastIKFabric playerIK;
            FastIKFabric enemyIK;

            if (catchedFoot == EnemyCatchFoot.Left)
            {
                selectedFootIKTarget = leftFootIKTarget;
                playerIK = leftFootIK;
                //enemyIK = currentEnemy.rightHandIK;
            }
            else
            {
                selectedFootIKTarget = rightFootIKTarget;
                playerIK = rightFootIK;
                //enemyIK = currentEnemy.leftHandIK;
            }
            var playerPos = playerController.transform.position;
            playerPos.y += 1f;

            if (Vector2.Distance(currentEnemy.leftHandTarget.position, playerPos) <
                Vector2.Distance(currentEnemy.rightHandTarget.position, playerPos))
            {
                enemyIK = currentEnemy.leftHandIK;
            }
            else
            {
                enemyIK = currentEnemy.rightHandIK;
            }

            playerIK.gameObject.SetActive(true);
            //enemyIK.gameObject.SetActive(true);
            enemyIK.enabled = true;

            playerIK.Target = selectedFootIKTarget;
            //enemyIK.Target = enemyIK.transform;

            while (isCatched)
            {
                if (!LeanTween.isTweening(selectedFootIKTarget.gameObject) && isPlayerHitting)
                {
                    LeanTween.delayedCall(0.05f, () => { currentEnemy.GetHit(); CameraShakeHandler.Instance.Shake(); });
                    LeanTween.move(selectedFootIKTarget.gameObject, currentEnemy.transform.position, 0.05f).setEaseInOutCirc()

                        .setOnComplete(() =>
                        {
                            
                            EventManager.Instance.OnPlayerHitEnemy?.Invoke(selectedFootIKTarget.position);
                            isPlayerHitting = false;
                        }).setLoopPingPong(1);
                }

                if (!LeanTween.isTweening(enemyIK.Target.gameObject))
                {
                    LeanTween.move(enemyIK.Target.gameObject, playerPos, 0.2f).setEaseInOutCirc()
                        .setLoopPingPong(1)
                        .setOnComplete(() => 
                    {
                        EventManager.Instance.OnEnemyHitPlayer?.Invoke(selectedFootIKTarget.position, currentEnemy.staminaPerHit);
                    });
                }

                yield return new WaitForSeconds(0.01f / TimeController.Instance.timeScaleDifference);
            }

            yield return null;
        }
    }

    public enum EnemyCatchFoot
    {
        Right,
        Left
    }
}