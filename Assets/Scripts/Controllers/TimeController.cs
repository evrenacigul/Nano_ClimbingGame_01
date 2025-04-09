using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Utilities;
using Nano.Managers;

namespace Nano.Controllers
{
    public class TimeController : SingletonMonoBehaviour<TimeController>
    {
        [SerializeField]
        float timeScaleDefault;

        [SerializeField]
        float fixedDeltaTimeDefault;

        [Range(0.0f, 1.0f)]
        [SerializeField]
        float timeScaleSlow = 0.25f;

        [SerializeField]
        float fixedDeltaTimeSlow;

        float levelStartTime;
        float levelEndTime;
        float timeFinishDifference;

        public float timeScaleDifference { get; private set; }

        public float GetTimeFinishRawSeconds { get { return timeFinishDifference; } }

        void Start()
        {
            timeScaleDefault = Time.timeScale;
            fixedDeltaTimeDefault = Time.fixedDeltaTime;
            fixedDeltaTimeSlow = timeScaleSlow * fixedDeltaTimeDefault;

            timeScaleDifference = timeScaleDefault / timeScaleSlow;

            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStates state)
        {
            switch(state)
            {
                case GameStates.Play:
                    levelStartTime = Time.time;
                    break;
                case GameStates.Fail:
                case GameStates.Win:
                    levelEndTime = Time.time;
                    timeFinishDifference = levelEndTime - levelStartTime;
                    break;
            }
        }

        public float GetLevelFinishSeconds()
        {
            return timeFinishDifference % 60;
        }

        public float GetLevelFinishMinutes()
        {
            return timeFinishDifference / 60;
        }

        public void StartSlowMotion(float lerpTime = 0f)
        {
            if (lerpTime == 0)
            {
                Time.timeScale = timeScaleSlow;
                Time.fixedDeltaTime = fixedDeltaTimeSlow;
            }
            else
            {
                LeanTween.value(timeScaleDefault, timeScaleSlow, lerpTime).setOnUpdate((float val) => 
                {
                    Time.timeScale = val;
                });

                LeanTween.value(fixedDeltaTimeDefault, fixedDeltaTimeSlow, lerpTime).setOnUpdate((float val) =>
                {
                    Time.fixedDeltaTime = val;
                });
            }
        }

        public void StartSlowMotion(float timeScale, float fixedTimeScale = 0.02f, float lerpTime = 0f)
        {
            if (lerpTime == 0)
            {
                Time.timeScale = timeScale;
                Time.fixedDeltaTime = fixedTimeScale;
            }
            else
            {
                LeanTween.value(timeScaleDefault, timeScale, lerpTime).setOnUpdate((float val) =>
                {
                    Time.timeScale = val;
                });

                LeanTween.value(fixedDeltaTimeDefault, fixedTimeScale, lerpTime).setOnUpdate((float val) =>
                {
                    Time.fixedDeltaTime = val;
                });
            }
        }

        public void StopSlowMotion(float lerpTime = 0f)
        {
            LeanTween.cancel(gameObject);

            if (lerpTime == 0)
            {
                Time.timeScale = timeScaleDefault;
                Time.fixedDeltaTime = fixedDeltaTimeDefault;
            }
            else
            {
                LeanTween.value(timeScaleDefault, timeScaleSlow, lerpTime).setOnUpdate((float val) =>
                {
                    Time.timeScale = val;
                });

                LeanTween.value(fixedDeltaTimeDefault, fixedDeltaTimeSlow, lerpTime).setOnUpdate((float val) =>
                {
                    Time.fixedDeltaTime = val;
                });
            }

            Time.timeScale = timeScaleDefault;
            Time.fixedDeltaTime = fixedDeltaTimeDefault;
        }
    }
}