using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Controllers;
using Nano.Extensions;
using Nano.Obstacles;

namespace Nano.Handlers
{
    public class StaminaHandler : MonoBehaviour
    {
        Stamina stamina;

        GrabPoint currentGrabPoint;

        PlayerController playerController;

        bool isResting = false;
        bool isReadyToJump = false;
        bool isTutorial = false;
        
        private void Start()
        {
            EventManager.Instance.OnPlayerJump.AddListener(OnPlayerJump);
            EventManager.Instance.OnEnemyHitPlayer.AddListener(OnEnemyHitPlayer);
            EventManager.Instance.OnPlayerFirstJump.AddListener(OnPlayerFirstJump);
            EventManager.Instance.OnPlayerGrabbed.AddListener(OnPlayerGrabbed);

            isReadyToJump = true;
        }

        public void SetPlayer(PlayerController player)
        {
            playerController = player;
        }

        public void SetStamina(Stamina getStamina)
        {
            stamina = getStamina;
        }

        private void HandleStamina(float staminaValue)
        {
            stamina.current = Mathf.Clamp(staminaValue, 0f, stamina.maxStamina);

            if (stamina.current == 0)
            {
                isReadyToJump = false;
                object[] args = new object[1];
                args[0] = playerController.GetHeadPoint;
                EventManager.Instance.OnPlayerStaminaDepleted?.Invoke(args);
            }
        }
        
        private void OnPlayerFirstJump()
        {
            isTutorial = LevelController.Instance.IsTutorialLevel;
        }

        private void OnPlayerJump()
        {
            HandleStamina(stamina.current -= stamina.staminaCostPerJump);

            EventManager.Instance.OnStaminaUpdate?.Invoke(GetStaminaRemapped(), 0.1f, 0.1f);
        }

        private void OnPlayerGrabbed(GrabPoint grabPoint)
        {
            if (isTutorial && playerController.GetLeftToFinishDistance < 4f)
            {
                HandleStamina(0f);
                isTutorial = false;
            }
        }

        private void OnEnemyHitPlayer(Vector3 _, float hitPoint = 0)
        {
            if(hitPoint == 0)
                HandleStamina(stamina.current - stamina.staminaCostPerHitByEnemy);
            else if(hitPoint > 0)
                HandleStamina(stamina.current - hitPoint);

            if (stamina.current == 0 && GameManager.Instance.GetGameState == GameStates.Play)
            {
                EventManager.Instance.OnPlayerDie?.Invoke();
            }

            EventManager.Instance.OnStaminaUpdate?.Invoke(GetStaminaRemapped(), 0.1f, 0.1f);
        }

        public void SetResting(bool rest, GrabPoint grabPoint)
        {
            isResting = rest;
            currentGrabPoint = grabPoint;

            if (rest)
                StartCoroutine(RaiseStamina());
            else
                StopCoroutine(RaiseStamina());
        }

        public float GetStaminaRemapped()
        {
            return stamina.current.Remap(0f, stamina.maxStamina, 0f, 1f);
        }

        public bool CanJump()
        {
            return stamina.current == 0f ? false : true && isReadyToJump;
        }

        public bool IsReady()
        {
            return isReadyToJump;
        }

        IEnumerator RaiseStamina()
        {
            while (isResting)
            {
                var staminaRaise = stamina.staminaRaisePerDelayedTime;

                if (!isReadyToJump) staminaRaise = stamina.staminaSlowRaisePerDelayedTime;

                HandleStamina(stamina.current += staminaRaise);

                EventManager.Instance.OnPlayerResting?.Invoke(0.2f, currentGrabPoint);
                EventManager.Instance.OnStaminaUpdate?.Invoke(GetStaminaRemapped(), 0f, stamina.regenerateDelayTime);

                if (stamina.current == stamina.maxStamina)
                {
                    isResting = false;
                    isReadyToJump = true;
                    EventManager.Instance.OnPlayerStaminaFull?.Invoke();
                }

                yield return new WaitForSeconds(stamina.regenerateDelayTime);
            }
        }
    }

    [System.Serializable]
    public class Stamina
    {
        public float current;
        public float maxStamina;
        public float staminaCostPerJump;
        public float staminaRaisePerDelayedTime;
        public float staminaSlowRaisePerDelayedTime;
        public float regenerateDelayTime;
        public float staminaCostPerHitByEnemy;
    }
}