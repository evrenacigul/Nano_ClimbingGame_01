using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Utilities;
using Nano.Controllers;
using Nano.ShopSystem;
using Facebook.Unity;

namespace Nano.Managers
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        [SerializeField]
        int targetFrameRate = 30;

        [SerializeField]
        int maxEnemyCountInScene = 4;

        int currentEnemyCount = 0;

        public int GetMaxEnemy { get { return maxEnemyCountInScene; } }
        public bool IsMaxEnemy { get { return currentEnemyCount >= maxEnemyCountInScene; } }

        private GameStates gameState;

        public GameStates GetGameState { get { return gameState; } }

        public bool isDebugMode = false;

        public int GetTargetFPS { get { return targetFrameRate; } }

        void Start()
        {
            Application.targetFrameRate = targetFrameRate;

            EventManager.Instance.OnInputTouchStart.AddListener(OnTouchStart);

            EventManager.Instance.OnEnemySpawned.AddListener(OnEnemySpawned);
            EventManager.Instance.OnEnemyDie.AddListener(OnEnemyDied);

            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            while(!FB.IsInitialized)
            {
                yield return new WaitForFixedUpdate();
            }

            SetGameState(GameStates.Load);

            yield return new WaitForFixedUpdate();

            SetGameState(GameStates.Ready);
        }

        public void SetGameState(GameStates state)
        {
            gameState = state;

            EventManager.Instance.OnGameStateChanged?.Invoke(state);

            switch (state)
            {
                case GameStates.Load:
                case GameStates.Ready:
                    TimeController.Instance.StopSlowMotion();
                    currentEnemyCount = 0;
                    break;
            }
        }

        public void OnEnemySpawned()
        {
            currentEnemyCount++;
        }

        public void OnEnemyDied(object[] args)
        {
            currentEnemyCount--;
        }

        void OnTouchStart(Vector2 pos)
        {
            if(gameState == GameStates.Ready && !UIController.Instance.isDebugMenuOpened)
            {
                SetGameState(GameStates.Play);
            }
        }

    }

    public enum GameStates
    {
        Load,
        Ready,
        Play,
        Win,
        Fail
    }
}