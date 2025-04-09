using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Facebook.Unity;
using UnityEngine;
using Nano.Controllers;
using Nano.Managers;
using TMPro;

namespace Nano.SDK
{
    public class FacebookSDK : MonoBehaviour
    {
        float _levelStartTime;
        bool _isInitialized = false;

        public TMP_Text testText;

        private void Awake()
        {
            StartCoroutine(DelayedStart());
        }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch(state)
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
            _levelStartTime = Time.time;
            var levelParameters = new Dictionary<string, object>
            {
                { AppEventParameterName.Level, LevelController.Instance.GetCurrentLevelObject.name },
                { AppEventParameterName.Description, $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00} Started event log." },
            };
            FB.LogAppEvent("Nano_LevelStart", LevelController.Instance.GetCurrentLevelID, levelParameters);
        }

        private void OnLevelSuccessful()
        {
            var levelParameters = new Dictionary<string, object>
            {
                { AppEventParameterName.Level, LevelController.Instance.GetCurrentLevelObject.name },
                {
                    AppEventParameterName.Description,
                    $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00} Successful event log."
                },
                {
                    "Nano_LevelCompletionTime", (Time.time - _levelStartTime).ToString(CultureInfo.InvariantCulture)
                },
                { AppEventParameterName.Success, true },
            };
            FB.LogAppEvent(AppEventName.AchievedLevel, LevelController.Instance.GetCurrentLevelID, levelParameters);
        }

        private void OnLevelFailed()
        {
            var levelParameters = new Dictionary<string, object>
            {
                { AppEventParameterName.Level, LevelController.Instance.GetCurrentLevelObject.name },
                { AppEventParameterName.Description, $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00} Failed event log." },
                {
                    "Nano_LevelCompletionTime", (Time.time - _levelStartTime).ToString(CultureInfo.InvariantCulture)
                },
                { AppEventParameterName.Success, false },
            };
            FB.LogAppEvent(AppEventName.AchievedLevel, LevelController.Instance.GetCurrentLevelID, levelParameters);
        }

        IEnumerator DelayedStart()
        {
            yield return null;

            void SetInit()
            {
                _isInitialized = true;
            }

            while (!_isInitialized)
            {
                FB.Init(SetInit);

                testText.text = "Trying FB Initialize";

                yield return new WaitForSeconds(0.5f);
            }

            testText.text = "Init DONE!!";
            
            FB.ActivateApp();
        }
    }
}