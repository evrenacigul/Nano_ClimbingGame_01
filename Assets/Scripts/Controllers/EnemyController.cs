using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Animators;
using Nano.Obstacles;
using Nano.Handlers;
using Nano.Objects;
using DitzelGames.FastIK;

namespace Nano.Controllers
{
    public class EnemyController : MonoBehaviour
    {
        public FastIKFabric rightHandIK;

        public FastIKFabric leftHandIK;

        public FastIKFabric headIK;

        public Transform rightHandTarget;

        public Transform leftHandTarget;

        public Transform headIKTarget;

        [SerializeField]
        float playerCatchDistance = 1f;

        [SerializeField]
        float speed = 1f;

        public int hitCount = 3;

        public float staminaPerHit;

        [SerializeField]
        float debugDistanceLeftToPlayer;

        Rigidbody rBody;

        PlayerController playerController;

        CreatureAnimator enemyAnimator;

        RagdollHandler ragdollHandler;

        bool explosionDie = false;
        float grenadeImpactRadius;
        float grenadeImpactPower;
        Vector3 grenadeImpactPosition;

        public EnemyState currentState { get; private set; }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

            playerController = GameObject.Find("Player").GetComponent<PlayerController>();

            if (!enemyAnimator) enemyAnimator = GetComponentInChildren<CreatureAnimator>();

            if (!rBody) rBody = gameObject.GetComponent<Rigidbody>();
            if (!rBody) rBody = gameObject.AddComponent<Rigidbody>();
            rBody.useGravity = false;

            if (!ragdollHandler) ragdollHandler = enemyAnimator.gameObject.GetComponent<RagdollHandler>();
            if (!ragdollHandler) ragdollHandler = enemyAnimator.gameObject.AddComponent<RagdollHandler>();

            SetEnemyState(EnemyState.Follow);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("GrabPoint"))
            {
                GrabPoint grabPoint = other.GetComponent<GrabPoint>();

                if (grabPoint.IsGrabbed)
                {
                    EventManager.Instance.OnRockHitZombie?.Invoke(grabPoint, other.ClosestPointOnBounds(grabPoint.transform.position));
                    SetEnemyState(EnemyState.Die);
                }
            }
            else if(other.CompareTag("Enemy") && other.gameObject != gameObject)
            {
                var distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
                var otherDistanceToPlayer = Vector3.Distance(other.transform.position, playerController.transform.position);

                if(distanceToPlayer > otherDistanceToPlayer)
                    SetEnemyState(EnemyState.Wait);
            }
            else if(other.CompareTag("PlayerObj"))
            {
                if(playerController.GetPlayerState == PlayerStates.Playing)
                    SetEnemyState(EnemyState.Attack);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("Enemy") && currentState == EnemyState.Wait)
            {
                SetEnemyState(EnemyState.Follow);
            }
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch (state)
            {
                case GameStates.Load:
                case GameStates.Ready:
                    //SetEnemyState(EnemyState.Wait);
                    break;
                case GameStates.Play:
                    //SetEnemyState(EnemyState.Follow);
                    break;
                case GameStates.Win:
                case GameStates.Fail:
                    SetEnemyState(EnemyState.Die);
                    break;
            }
        }

        public void GetHit(bool byExplosion = false, object [] grenadeInfo = null)
        {
            if (byExplosion)
            {
                explosionDie = true;
                grenadeImpactPower = (float)grenadeInfo[0];
                grenadeImpactRadius = (float)grenadeInfo[1];
                grenadeImpactPosition = (Vector3)grenadeInfo[2];
                SetEnemyState(EnemyState.Die);
                return;
            }

            hitCount--;

            if(!LeanTween.isTweening(headIKTarget.gameObject))
                LeanTween.move(headIKTarget.gameObject, headIKTarget.position + (headIKTarget.forward * -0.5f), 0.1f).setEaseInOutCirc().setLoopPingPong(1);

            if (hitCount <= 0)
                SetEnemyState(EnemyState.Die);
        }

        public void SetEnemyState(EnemyState enemyState)
        {
            StopAllCoroutines();

            currentState = enemyState;

            switch (enemyState)
            {
                case EnemyState.Follow:
                    StartCoroutine(FollowPlayer());
                    enemyAnimator.PlayAnimation("Crawling", 0.2f);
                    break;

                case EnemyState.Attack:
                    enemyAnimator.PlayAnimation("Wait", 0.2f);
                    rBody.velocity = Vector3.zero;
                    enemyAnimator.Pause();

                    headIK.enabled = true;

                    var playerPos = playerController.transform.position;

                    object[] args = new object[2];
                    args[0] = this;
                    if (playerPos.x < transform.position.x)
                        args[1] = EnemyCatchFoot.Right;
                    else
                        args[1] = EnemyCatchFoot.Left;

                    EventManager.Instance.OnEnemyCatchPlayer?.Invoke(args);
                    break;

                case EnemyState.Die:

                    OnDie();
                    break;

                case EnemyState.Wait:
                    enemyAnimator.PlayAnimation("Wait", 0.2f);
                    rBody.velocity = Vector3.zero;
                    enemyAnimator.Pause();
                    break;
            }
        }

        private float GetDistance()
        {
            debugDistanceLeftToPlayer = Vector2.Distance(transform.position, playerController.transform.position);

            return debugDistanceLeftToPlayer;
        }

        private void OnDie()
        {
            var gameState = GameManager.Instance.GetGameState;

            if (gameState != GameStates.Win && gameState != GameStates.Fail)
            {
                object[] args2 = new object[1];
                args2[0] = this;

                EventManager.Instance.OnEnemyDie?.Invoke(args2);
            }

            enemyAnimator.Pause();
            rBody.velocity = Vector3.zero;

            GetComponent<BoxCollider>().isTrigger = false;

            leftHandIK.enabled = false;
            rightHandIK.enabled = false;
            headIK.enabled = false;

            rBody.useGravity = true;

            ragdollHandler.StartRagdoll();

            if (explosionDie)
                rBody.AddExplosionForce(grenadeImpactPower, grenadeImpactPosition, grenadeImpactRadius, 7.0f);
            else
                ragdollHandler.PushRagdoll(3f, Vector3.up);

            Destroy(gameObject, 6f);
        }

        IEnumerator FollowPlayer()
        {
            while(GetDistance() > playerCatchDistance && currentState == EnemyState.Follow)
            {
                var targetTransform = playerController.transform;
                var targetPos = targetTransform.position;
                targetPos.y += 0.5f;

                var direction = targetPos - transform.position;
                var velocity = direction.normalized * Time.deltaTime * speed;
                var angle = Vector3.Angle(Vector3.up, direction);

                var rotationEuler = new Vector3(0f, 0f, transform.position.x < targetPos.x ? -angle : angle);

                rBody.velocity = velocity;

                enemyAnimator.SetCurrentSpeed(velocity.magnitude);

                transform.rotation = Quaternion.Euler(rotationEuler);

                yield return new WaitForEndOfFrame();
            }

            if (GetDistance() <= playerCatchDistance && playerController.GetPlayerState == PlayerStates.Playing)
                SetEnemyState(EnemyState.Attack);

            rBody.velocity = Vector3.zero;

            yield return null;
        }
    }

    public enum EnemyState
    {
        Wait,
        Follow,
        Attack,
        Die
    }
}