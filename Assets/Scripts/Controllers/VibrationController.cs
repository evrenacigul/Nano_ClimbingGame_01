using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;
using Nano.Utilities;

namespace Nano.Controllers
{
    public class VibrationController : SingletonMonoBehaviour<VibrationController>
    {
        private void Start()
        {
            Vibration.Init();

            EventManager.Instance.OnPlayerJump.AddListener(OnPlayerJump);
            EventManager.Instance.OnPlayerFirstJump.AddListener(OnPlayerFirstJump);
            EventManager.Instance.OnEnemyHitPlayer.AddListener(OnEnemyHitPlayer);
            EventManager.Instance.OnPlayerDie.AddListener(OnPlayerDie);
            EventManager.Instance.OnPlayerHitEnemy.AddListener(OnPlayerHitEnemy);
        }

        void OnPlayerFirstJump()
        {
            OnPlayerJump();
        }

        void OnPlayerJump()
        {
            Vibration.Vibrate(100);
            Debug.Log("jump vibrate");
        }

        void OnEnemyHitPlayer(Vector3 _, float __)
        {
            Vibration.VibratePop();
            Debug.Log("enemy hit vibrate");
        }

        void OnPlayerDie()
        {
            Vibration.Vibrate(300);
            Debug.Log("player die vibrate");
        }

        void OnPlayerHitEnemy(Vector3 _)
        {
            Vibration.VibratePop();
            Debug.Log("player hit vibrate");
        }
    }
}