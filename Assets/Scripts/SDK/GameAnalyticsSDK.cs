using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using Nano.Controllers;
using Nano.Managers;

namespace Nano.SDK
{
    public class GameAnalyticsSDK : MonoBehaviour
    {
        private void Awake()
        {
            GameAnalytics.Initialize();
        }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch (state)
            {
                case GameStates.Play:
                    OnLevelStart();
                    break;

                case GameStates.Win:
                    OnLevelSuccessful();
                    break;

                case GameStates.Fail:
                    OnLevelFailed();
                    break;
            }
        }

        private void OnLevelStart()
        {
            GameAnalytics.NewProgressionEvent(
                    GAProgressionStatus.Start,
                    $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00}");
        }

        private void OnLevelSuccessful()
        {
            GameAnalytics.NewProgressionEvent(
                    GAProgressionStatus.Complete,
                    $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00}");
        }

        private void OnLevelFailed()
        {
            GameAnalytics.NewProgressionEvent(
                    GAProgressionStatus.Fail,
                    $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00}");
        }
    }
}