using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nano.Managers;
using Nano.Extensions;
using Nano.Animators;
using Nano.Objects;
using Nano.Obstacles;
using Nano.Handlers;
using Nano.ShopSystem;
using DitzelGames.FastIK;

namespace Nano.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        PlayerStats stats;

        [Header("IK System")]
        [SerializeField]
        FastIKFabric leftHandIK;

        [SerializeField]
        FastIKFabric leftFootIK;

        [SerializeField]
        FastIKFabric rightHandIK;

        [SerializeField]
        FastIKFabric rightFootIK;

        [SerializeField]
        Transform rightFootTarget;

        [SerializeField]
        Transform leftFootTarget;

        [Header("Misc")]
        [SerializeField]
        Transform headPoint;

        [SerializeField]
        Transform leftHandGrenadeHoldPosition;

        [SerializeField]
        PlayerAnimator animator;

        [SerializeField]
        float jumpCoolDownTime;

        [SerializeField]
        float holdPositionYOfsset;

        [SerializeField]
        List<SkinnedMeshRenderer> skinRenderer;

        [SerializeField]
        Object grenadePrefab;

        [SerializeField]
        int enemyKillCountToThrowGrenade = 10;

        public int hitCountToKillEnemy = 3;

        float initialZPos = 0.25f;
        float finishDistance;

        Vector3 startPos;
        Vector3 finishPos;
        Vector3 currentPos;

        GrabPoint firstGrabPoint;
        GrabPoint lastGrabPoint;

        GameObject finishPosObj;

        StaminaHandler staminaHandler;
        NearestBoulderHighlightHandler nearestBoulderHighlightHandler;
        RagdollHandler ragdollHandler;
        EnemyCatchHandler enemyCatchHandler;

        PlayerStates playerStates = PlayerStates.Idle;

        bool readyToJumpAnother = false;
        bool readyToThrowBomb = false;
        bool isTutorialLevel = false;

        public Transform GetHeadPoint { get { return headPoint; } }

        public PlayerStats GetPlayerStats { get { return stats; } }

        public PlayerStates GetPlayerState { get { return playerStates; } }

        public float GetLeftToFinishDistance 
        { 
            get 
            { 
                return finishPosObj.GetComponent<BoxCollider>().bounds.min.y - transform.position.y; 
            } 
        }

        private void Awake()
        {
            if (!animator) animator = GetComponent<PlayerAnimator>();

            if (!animator) animator = GetComponentInChildren<PlayerAnimator>();

            animator.PlayAnimation("Idle", 0.1f);

            SetActiveIKs(false);

            if (!staminaHandler) staminaHandler = gameObject.GetComponent<StaminaHandler>();
            if (!staminaHandler) staminaHandler = gameObject.AddComponent<StaminaHandler>();

            if (!nearestBoulderHighlightHandler) nearestBoulderHighlightHandler = gameObject.GetComponent<NearestBoulderHighlightHandler>();
            if (!nearestBoulderHighlightHandler) nearestBoulderHighlightHandler = gameObject.AddComponent<NearestBoulderHighlightHandler>();

            if (!ragdollHandler) ragdollHandler = animator.gameObject.GetComponent<RagdollHandler>();
            if (!ragdollHandler) ragdollHandler = animator.gameObject.AddComponent<RagdollHandler>();

            if (!enemyCatchHandler) enemyCatchHandler = gameObject.GetComponent<EnemyCatchHandler>();
            if (!enemyCatchHandler) enemyCatchHandler = gameObject.AddComponent<EnemyCatchHandler>();
        }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            EventManager.Instance.OnGrabPointSelected.AddListener(OnGrabPointSelected);
            EventManager.Instance.OnEnemyDie.AddListener(OnEnemyDied);
            EventManager.Instance.OnFinishAreaTapped.AddListener(OnFinishAreaTapped);
            EventManager.Instance.OnEnemyCatchPlayer.AddListener(OnEnemyCatchPlayer);
            EventManager.Instance.OnEnemyReleasePlayer.AddListener(OnEnemyReleasePlayer);
            EventManager.Instance.OnPlayerDie.AddListener(OnPlayerDie);
            EventManager.Instance.OnShakyRockFalls.AddListener(OnShakyRockFalls);
            EventManager.Instance.OnPlayerThrowGrenade.AddListener(OnPlayerThrowGrenade);

            ShopSystemManager.Instance.OnItemBuy.AddListener(OnItemBuy);
            
            CheckBoughtItems(true);
            initialZPos = transform.position.z;

            staminaHandler.SetStamina(stats.stamina);
            staminaHandler.SetPlayer(this);
            nearestBoulderHighlightHandler.SetPlayer(this, stats);
            enemyCatchHandler.SetPlayer(this, rightFootTarget, leftFootTarget, rightFootIK, leftFootIK);
        }

        private void CheckBoughtItems(bool isInitial)
        {
            ShopSystemManager.Instance.CheckBoughtItems(OnItemBuy, isInitial);
        }

        private void OnItemBuy(ShopItemScriptableObject shopItemSO, bool initial)
        {
            if (shopItemSO.modifierName.Equals("Skin"))
            {
                var lastSkin = PlayerPrefs.GetString("LastSkin", "");

                if (initial && lastSkin.Equals(shopItemSO.itemName))
                {
                    ChangeSkin(shopItemSO.materialChange, shopItemSO.itemName);
                }
                else if (!initial)
                {
                    ChangeSkin(shopItemSO.materialChange, shopItemSO.itemName);
                }
            }
            else if (shopItemSO.modifierName.Equals("Max Stamina"))
            {
                stats.stamina.maxStamina += shopItemSO.floatValue * (shopItemSO.boughtCount == 0 ? 1 : shopItemSO.boughtCount);
                stats.stamina.current = stats.stamina.maxStamina;
            }
            else if (shopItemSO.modifierName.Equals("Stamina Raise"))
            {
                stats.stamina.regenerateDelayTime -= shopItemSO.floatValue * (shopItemSO.boughtCount == 0 ? 1 : shopItemSO.boughtCount);
            }
            else if (shopItemSO.modifierName.Equals("Stamina Jump"))
            {
                stats.stamina.staminaCostPerJump -= shopItemSO.floatValue * (shopItemSO.boughtCount == 0 ? 1 : shopItemSO.boughtCount);
            }
        }

        private void ChangeSkin(Material skin, string lastSkin)
        {
            PlayerPrefs.SetString("LastSkin", lastSkin);

            var sharedMaterials = skinRenderer;

            for (int i = 0; i < sharedMaterials.Count; i++)
            {
                sharedMaterials[i].sharedMaterial = skin;
            }
        }

        private void InitSettings()
        {
            playerStates = PlayerStates.Idle;

            startPos = transform.position;

            finishPosObj = GameObject.Find("Finish Area");
            var finishPosDiff = finishPosObj.GetComponent<BoxCollider>().bounds.min.y;

            finishPos = finishPosObj.transform.position;
            finishPos.y = finishPosDiff;

            finishDistance = finishPos.y - startPos.y;

            //finishDistance = GetLeftToFinishDistance;

            animator.PlayAnimation("Idle", 0.1f);

            animator.SubscribeEvent("OnJump", OnFirstJump);

            var grabPoints = GameObject.FindGameObjectsWithTag("GrabPoint");
            firstGrabPoint = grabPoints.OrderBy(grabObj => grabObj.transform.position.y).First().GetComponent<GrabPoint>();

            lastGrabPoint = firstGrabPoint;

            var pos = transform.position;
            pos.x = firstGrabPoint.transform.position.x;
            transform.position = pos;

            stats.stamina.current = stats.stamina.maxStamina;

            UIController.Instance.SetPlayerStat(stats);
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch (state)
            {
                case GameStates.Load:
                    break;

                case GameStates.Ready:
                    InitSettings();
                    isTutorialLevel = LevelController.Instance.IsTutorialLevel;
                    break;

                case GameStates.Play:
                    animator.PlayAnimation("Jump_Start", 0.1f);
                    playerStates = PlayerStates.Playing;
                    break;

                case GameStates.Win:
                    playerStates = PlayerStates.Win;
                    break;

                case GameStates.Fail:
                    ragdollHandler.StartRagdoll();
                    playerStates = PlayerStates.Lose;
                    break;
            }
        }

        private void OnEnemyCatchPlayer(object[] args)
        {
            staminaHandler.SetResting(false, lastGrabPoint);

            playerStates = PlayerStates.Fighting;
        }

        private void OnEnemyReleasePlayer()
        {
            if(isTutorialLevel)
                readyToJumpAnother = true;

            playerStates = PlayerStates.Playing;

            SetIKTargets(lastGrabPoint);
            staminaHandler.SetResting(true, lastGrabPoint);
        }

        private void OnPlayerDie()
        {
            SetActiveIKs(false);
            animator.enabled = false;

            ragdollHandler.StartRagdoll();
            ragdollHandler.PushRagdoll(1f, Vector3.up);

            GameManager.Instance.SetGameState(GameStates.Fail);
        }

        private void OnFinishAreaTapped(Vector3 pos)
        {
            if (GameManager.Instance.GetGameState != GameStates.Play) return;

            if (!staminaHandler.IsReady()) return;

            if (Mathf.Abs(pos.y - lastGrabPoint.transform.position.y) <= stats.jumpSettings.maxJumpDistance)
            {
                readyToJumpAnother = false;

                animator.PlayAnimation("Jump_Start", 0.2f);

                LeanTween.moveLocalY(gameObject, pos.y, 0.35f).setOnComplete(() =>
                {
                    SetActiveIKs(false);

                    animator.PlayAnimation("Win", 0.1f);

                    GameManager.Instance.SetGameState(GameStates.Win);

                    LeanTween.rotateAround(gameObject, new Vector3(0, 1, 0), 180, 0.2f);
                });

                LeanTween.moveLocalZ(gameObject, transform.position.z + 2f, 0.5f);
            }
        }

        private void OnEnemyDied(object[] args)
        {
            stats.winStats.killCount++;
            var leftDiff = Mathf.Abs(stats.winStats.killCount % enemyKillCountToThrowGrenade);
            if (leftDiff != 0) return;

            if (playerStates == PlayerStates.Playing && readyToJumpAnother)
                EventManager.Instance.OnPlayerThrowGrenade?.Invoke(transform.position);
            else
                readyToThrowBomb = true;
        }

        private void OnFirstJump()
        {
            animator.SlowDown(0.35f);
            
            var holdPos = firstGrabPoint.transform.position.y;
            holdPos += holdPositionYOfsset;
            EventManager.Instance.OnPlayerFirstJump?.Invoke();

            LeanTween.moveLocalY(gameObject, holdPos, 0.2f).setEaseInOutBounce().setOnComplete(()=> 
            {
                SetActiveIKs(true);
                SetIKTargets(firstGrabPoint);

                animator.Continue();

                currentPos = firstGrabPoint.transform.position;
                EventManager.Instance.OnProgressUpdate?.Invoke(currentPos.y.Remap(startPos.y, finishPos.y, 0f, 1f));

                firstGrabPoint.Grabbed(this);

                LeanTween.delayedCall(0.1f, () => 
                {
                    animator.PlayAnimation("Jump_Idle", 0.1f);
                    if(!isTutorialLevel)
                        readyToJumpAnother = true;
                    staminaHandler.SetResting(true, firstGrabPoint);
                    animator.UnsubscribeEvent("OnJump", OnFirstJump);
                });
            });
        }

        private void JumpTarget(GrabPoint grabPoint)
        {
            var targetPos = grabPoint.transform.position;
            targetPos.y += holdPositionYOfsset;
            targetPos.z = initialZPos;

            var direction = targetPos - transform.position;
            direction = direction.normalized;

            void OnJumpOnAirBegin()
            {
                animator.SlowDown(0.2f);

                currentPos = grabPoint.transform.position;
                EventManager.Instance.OnProgressUpdate?.Invoke(currentPos.y.Remap(startPos.y, finishPos.y, 0f, 1f));

                LeanTween.move(gameObject, targetPos, 0.2f).setEaseOutQuad().setOnComplete(()=> 
                {
                    SetActiveIKs(false);
                    animator.Continue();
                });
            }

            void OnJumpOnAir()
            {
                SetActiveIKs(true);
                SetIKTargets(grabPoint);

                grabPoint.Grabbed(this);
            }

            void OnJumpOnAirEnd()
            {
                EventManager.Instance.OnPlayerGrabbed?.Invoke(grabPoint);

                animator.PlayAnimation("Jump_Idle");

                animator.UnsubscribeEvent("Jump_Another_OnAirBegin", OnJumpOnAirBegin);
                animator.UnsubscribeEvent("Jump_Another_OnAirEnd", OnJumpOnAirEnd);
                animator.UnsubscribeEvent("Jump_Another_OnAir", OnJumpOnAir);

                //Wait until cooldown for next jump
                LeanTween.delayedCall(jumpCoolDownTime, () => 
                {
                    SetActiveIKs(true);
                    SetIKTargets(grabPoint);

                    if (!readyToThrowBomb)
                    {
                        readyToJumpAnother = true;
                    }
                    else
                    {
                        readyToThrowBomb = false;
                        EventManager.Instance.OnPlayerThrowGrenade?.Invoke(transform.position);
                    }

                    staminaHandler.SetResting(true, grabPoint);
                });
            }

            animator.SubscribeEvent("Jump_Another_OnAirBegin", OnJumpOnAirBegin);
            animator.SubscribeEvent("Jump_Another_OnAir", OnJumpOnAir);
            animator.SubscribeEvent("Jump_Another_OnAirEnd", OnJumpOnAirEnd);

            animator.SetParameter("Horizontal", direction.x);
            animator.SetParameter("Vertical", direction.y);

            animator.PlayAnimation("Jump_Blend", 0.1f);

            lastGrabPoint?.Released();

            lastGrabPoint = grabPoint;

            staminaHandler.SetResting(false, grabPoint);

            EventManager.Instance.OnPlayerJump?.Invoke();
        }

        private void OnGrabPointSelected(GrabPoint grabPoint)
        {
            //if not isPlaying then cancel jumping
            if(playerStates != PlayerStates.Playing)
            {
                grabPoint.CancelJump();
                return;
            }

            //Cancel jump if already jumping another
            if (!readyToJumpAnother)
            {
                grabPoint.CancelJump();
                return;
            }

            //Check stamina
            if(!staminaHandler.CanJump())
            {
                grabPoint.CancelJump();
                EventManager.Instance.OnPlayerCantJump?.Invoke();
                return;
            }

            //Check if stamina completely depleted
            if(!staminaHandler.IsReady())
            {
                grabPoint.CancelJump();
                EventManager.Instance.OnPlayerCantJump?.Invoke();
                return;
            }

            //Cancel if already hanged on it
            if (grabPoint.IsGrabbed) return;

            var targetPos = grabPoint.transform.position;
            var currentPos = lastGrabPoint.transform.position;

            //Cancel jump if grab point lower than player position
            if (targetPos.y + stats.jumpSettings.maxLowerDistance < currentPos.y)
            {
                grabPoint.CancelJump();
                return;
            }

            //Cancel jump if grab point farther than max allowed distance
            var distance = Vector3.Distance(targetPos, currentPos);
            if (distance > stats.jumpSettings.maxJumpDistance)
            {
                grabPoint.CancelJump();
                return;
            }

            readyToJumpAnother = false;

            JumpTarget(grabPoint);
        }

        private void OnShakyRockFalls(GrabPoint grabPoint)
        {
            if (grabPoint == lastGrabPoint && GameManager.Instance.GetGameState == GameStates.Play)
                EventManager.Instance.OnPlayerDie?.Invoke();
        }

        private void OnPlayerThrowGrenade(Vector3 position)
        {
            playerStates = PlayerStates.GrenadeThrow;
            readyToJumpAnother = false;
            TimeController.Instance.StopSlowMotion();

            var grenade = (Instantiate(grenadePrefab, leftHandGrenadeHoldPosition.transform.position,
                Quaternion.identity, leftHandGrenadeHoldPosition.transform) as GameObject).GetComponent<GrenadeObject>();
            grenade.gameObject.SetActive(false);

            void OnGrenadePrepare()
            {
                Debug.Log("OnGrenadePrepare");
                if (grenade is null) OnGrenadeThrowEnd();

                grenade.gameObject.SetActive(true);
            }

            void OnGrenadeThrow()
            {
                Debug.Log("OnGrenadeThrow");

                if (grenade is null) OnGrenadeThrowEnd();

                grenade.transform.SetParent(null);
                grenade.Throw();
            }

            void OnGrenadeThrowEnd()
            {
                Debug.Log("OnGrenadeThrowEnd");
                //TimeController.Instance.StopSlowMotion();

                animator.PlayAnimation("Jump_Idle");

                animator.UnsubscribeEvent("GrenadeThrowEnd", OnGrenadeThrow);
                animator.UnsubscribeEvent("GrenadeThrow", OnGrenadeThrow);
                animator.UnsubscribeEvent("GrenadePrepare", OnGrenadePrepare);

                playerStates = PlayerStates.Playing;
                leftFootIK.enabled = true;
                leftHandIK.enabled = true;

                readyToJumpAnother = true;
            }

            leftFootIK.enabled = false;
            leftHandIK.enabled = false;

            //TimeController.Instance.StartSlowMotion(0.75f, 0.02f);

            animator.SubscribeEvent("GrenadePrepare", OnGrenadePrepare);
            animator.SubscribeEvent("GrenadeThrow", OnGrenadeThrow);
            animator.SubscribeEvent("GrenadeThrowEnd", OnGrenadeThrowEnd);
            animator.PlayAnimation("GrenadeThrow");
        }

        private void SetIKTargets(GrabPoint grabPoint)
        {
            rightFootIK.Target = grabPoint.rightFootIKPoint;
            rightHandIK.Target = grabPoint.rightHandIKPoint;

            leftFootIK.Target = grabPoint.leftFootIKPoint;
            leftHandIK.Target = grabPoint.leftHandIKPoint;
        }

        private void SetActiveIKs(bool active)
        {
            leftFootIK.enabled = active;
            leftHandIK.enabled = active;
            rightFootIK.enabled = active;
            rightHandIK.enabled = active;
        }
    }

    [System.Serializable]
    public enum PlayerStates
    {
        Idle,
        Playing,
        Jumping,
        Fighting,
        GrenadeThrow,
        Win,
        Lose
    }

    [System.Serializable]
    public class PlayerStats
    {
        public Stamina stamina;
        public WindowStats winStats;
        public JumpSettings jumpSettings;
    }

    [System.Serializable]
    public class WindowStats
    {
        public int killCount;
        public float topReachTime;
        public int extraCash;
    }

    [System.Serializable]
    public class JumpSettings
    {
        public float maxJumpDistance = 2.5f;
        public float maxLowerDistance = 1.5f;
    }
}