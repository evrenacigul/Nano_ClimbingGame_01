using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using AppsFlyerSDK;
using Nano.Managers;
using Nano.Controllers;

namespace Nano.SDK
{
    public class AppsFlyerSDK : MonoBehaviour
    {
        public string devKey;

        public string appID;

        public string UWPAppID;

        public string macOSAppID;

        public bool isDebug;

        private float _levelStartTime;

        private void Awake()
        {
            AppsFlyer.setIsDebug(isDebug);

            #if UNITY_WSA_10_0 && !UNITY_EDITOR
                    AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
            #elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                    AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
            #else
                AppsFlyer.initSDK(devKey, appID, null);
            #endif

            AppsFlyer.startSDK();
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
            _levelStartTime = Time.time;

            var levelParameters = new Dictionary<string, string>
             {
                 { "af_level", LevelController.Instance.GetCurrentLevelID.ToString() },
                 { "Nano_Description", $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00} started event log." },
             };

            AppsFlyer.sendEvent("Nano_LevelStart", levelParameters);
        }

        private void OnLevelSuccessful()
        {
            var levelParameters = new Dictionary<string, string>
             {
                 { "af_level", LevelController.Instance.GetCurrentLevelID.ToString() },
                 { "Nano_Description", $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00} successful event log." },
                 { "Nano_LevelCompletionTime", (Time.time - _levelStartTime).ToString(CultureInfo.InvariantCulture) },
                 { "Nano_LevelSuccessful", "true" },
             };
            AppsFlyer.sendEvent("af_level_achieved", levelParameters);
        }

        private void OnLevelFailed()
        {
            var levelParameters = new Dictionary<string, string>
             {
                 { "af_level", LevelController.Instance.GetCurrentLevelID.ToString() },
                 { "Nano_Description", $"Level_{(LevelController.Instance.GetCurrentLevelID + 1):00} failed event log." },
                 { "Nano_LevelCompletionTime", (Time.time - _levelStartTime).ToString(CultureInfo.InvariantCulture) },
                 { "Nano_LevelSuccessful", "false" },
             };
            AppsFlyer.sendEvent("af_level_achieved", levelParameters);
        }
    }
}