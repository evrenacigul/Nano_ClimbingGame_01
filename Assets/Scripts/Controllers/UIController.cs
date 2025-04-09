using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Nano.Managers;
using Nano.Utilities;
using Nano.Obstacles;
using Nano.Objects;
using Nano.ShopSystem;
using TMPro;

namespace Nano.Controllers
{
    public class UIController : SingletonMonoBehaviour<UIController>
    {
        [Header("Sliders")]
        [SerializeField]
        Slider staminaSlider;

        [SerializeField]
        Slider progressSlider;

        [SerializeField]
        GameObject winScreen;

        [SerializeField]
        GameObject failScreen;

        [SerializeField]
        GameObject debugScreen;

        [SerializeField]
        TMP_Text totalTimeWin;

        [SerializeField]
        TMP_Text totalTimeFail;

        [SerializeField]
        TMP_Text killCountWin;

        [SerializeField]
        TMP_Text killCountFail;

        [SerializeField]
        TMP_Text levelText;

        [SerializeField]
        TMP_Text onJumpPointText;

        //[SerializeField]
        //TMP_Text totalCashRightTop;

        [SerializeField]
        TMP_Text shakyRockTutText;

        [SerializeField]
        TMP_Text enemyFightTutText;

        [SerializeField]
        TMP_Text exhaustedTutText;

        [SerializeField]
        GameObject[] stars;

        [SerializeField]
        GameObject tapToKick;

        [SerializeField]
        GameObject tapToJump;

        [SerializeField]
        GameObject tapHandObj;

        [SerializeField]
        GameObject fpsObj;

        [SerializeField]
        Button restartButton;

        [SerializeField]
        Image staminaFillImage;

        [SerializeField]
        Image staminaEnergyImage;

        [SerializeField]
        Color staminaDepletedColor = Color.red;

        Color staminaDefaultColor;

        Vector3 staminaDefaultScale;
        Vector3 staminaEnergyDefaultScale;
        Vector3 winDefaultScale;
        Vector3 failDefaultScale;

        PlayerStats playerStats;

        public bool isDebugMenuOpened = false;

        bool exhaustedTutDone = false;
        bool keepOnYourEyeTutDone = false;
        bool enemyFightTutDone = false;
        //bool shakyRockTutDone = false;

        private void Start()
        {
            EventManager.Instance.OnProgressUpdate.AddListener(OnProgressUpdate);
            EventManager.Instance.OnStaminaUpdate.AddListener(OnStaminaUpdate);

            EventManager.Instance.OnPlayerGrabbed.AddListener(OnPlayerGrabbed);
            EventManager.Instance.OnPlayerCantJump.AddListener(OnPlayerCantJump);
            EventManager.Instance.OnPlayerResting.AddListener(OnPlayerResting);

            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);

            EventManager.Instance.OnEnemyCatchPlayer.AddListener(OnEnemyCatchPlayer);
            EventManager.Instance.OnEnemyReleasePlayer.AddListener(OnEnemyReleasePlayer);
            
            if (!staminaSlider) staminaSlider = GameObject.Find("Stamina Bar").GetComponent<Slider>();
            if (!progressSlider) progressSlider = GameObject.Find("Progression Bar").GetComponent<Slider>();

            staminaDefaultScale = staminaSlider.transform.localScale;
            staminaEnergyDefaultScale = staminaEnergyImage.transform.localScale;

            staminaDefaultColor = staminaFillImage.color;

            winDefaultScale = winScreen.transform.localScale;
            failDefaultScale = failScreen.transform.localScale;

            if (GameManager.Instance.isDebugMode)
                fpsObj.SetActive(true);
            else
                fpsObj.SetActive(false);

            Tutorial(isDone:true);

            //shakyRockTutDone = PlayerPrefs.GetInt("shakyRockTutDone", 0) == 0 ? false : true;
            //enemyFightTutDone = PlayerPrefs.GetInt("enemyFightTutDone", 0) == 0 ? false : true;
            //exhaustedTutDone = PlayerPrefs.GetInt("exhaustedTutDone", 0) == 0 ? false : true;
        }

        private void Tutorial(bool isDone)
        {
            //shakyRockTutDone = isDone;
            enemyFightTutDone = isDone;
            exhaustedTutDone = isDone;
            keepOnYourEyeTutDone = isDone;
        }

        private void OnGameStateChanged(GameStates state)
        {
            var checkIfTutorialLevel = false;

            switch (state)
            {
                case GameStates.Load:
                    //OnStaminaUpdate(1f, 0f, 0f);
                    //OnProgressUpdate(0f);

                    winScreen?.SetActive(false);
                    failScreen?.SetActive(false);
                    stars[0].SetActive(false);
                    stars[1].SetActive(false);
                    stars[2].SetActive(false);
                    staminaSlider.gameObject.SetActive(true);
                    progressSlider.gameObject.SetActive(true);

                    if (isDebugMenuOpened) ToggleDebugScreen();
                    break;

                case GameStates.Ready:
                    levelText.text = "Level " + (LevelController.Instance.GetCurrentLevelID + 1).ToString();
                    OnStaminaUpdate(1f, 0f, 0f);
                    OnProgressUpdate(0f);

                    ToggleAnimateTaps(true, false);

                    checkIfTutorialLevel = LevelController.Instance.IsTutorialLevel;

                    if (checkIfTutorialLevel)
                        Tutorial(false);

                    ShopSystemCompactUI.Instance.OpenShopWindow();
                    break;

                case GameStates.Play:
                    ShopSystemCompactUI.Instance.CloseShopWindow();
                    ToggleAnimateTaps(false, false);
                    break;

                case GameStates.Win:
                    ToggleAnimateTaps(false, false);

                    if(checkIfTutorialLevel)
                        Tutorial(true);

                    LeanTween.delayedCall(1.5f, () =>
                    {
                        EventManager.Instance.OnWinScreenAppears?.Invoke();

                        var timeController = TimeController.Instance;
                        
                        timeController.StopSlowMotion();

                        winScreen.transform.localScale = winDefaultScale;

                        winScreen.transform.localScale = Vector3.zero;

                        winScreen.SetActive(true);
                        staminaSlider.gameObject.SetActive(false);
                        progressSlider.gameObject.SetActive(false);

                        killCountWin.text = playerStats.winStats.killCount.ToString();

                        string timeText = timeController.GetLevelFinishMinutes().ToString("00");
                        timeText += ":" + timeController.GetLevelFinishSeconds().ToString("00");

                        totalTimeWin.text = timeText;

                        LeanTween.scale(winScreen, winDefaultScale, 1.2f).setEaseOutElastic().setOnComplete(() => 
                        {
                            StarsAnimation(LevelController.Instance.GetCurrentLevelObject, timeController.GetTimeFinishRawSeconds);
                        });
                    });
                    break;

                case GameStates.Fail:
                    ToggleAnimateTaps(false, false);

                    LeanTween.delayedCall(0.2f, () =>
                    {
                        EventManager.Instance.OnFailScreenAppears?.Invoke();

                        var timeController = TimeController.Instance;

                        timeController.StopSlowMotion();

                        failScreen.transform.localScale = failDefaultScale;

                        failScreen.transform.localScale = Vector3.zero;

                        failScreen.SetActive(true);
                        staminaSlider.gameObject.SetActive(false);
                        progressSlider.gameObject.SetActive(false);

                        killCountFail.text = playerStats.winStats.killCount.ToString();

                        string timeText = timeController.GetLevelFinishMinutes().ToString("00");
                        timeText += ":" + timeController.GetLevelFinishSeconds().ToString("00");

                        totalTimeFail.text = timeText;

                        LeanTween.scale(failScreen, failDefaultScale, 1.2f).setEaseOutElastic().setOnComplete(() =>
                        {
                            
                        });

                    });

                    break;
            }
        }

        private void StarsAnimation(LevelObject currentLevel, float totalSeconds)
        {
            Debug.Log("Total finish seconds = " + totalSeconds);

            bool star1 = currentLevel.CheckFirstStar(totalSeconds);
            bool star2 = currentLevel.CheckSecondStar(totalSeconds);
            bool star3 = currentLevel.CheckThirdStar(totalSeconds);
            var defaultScales = new Vector3[stars.Length];
            for(int i = 0; i < stars.Length; i++)
            {
                defaultScales[i] = stars[i].transform.localScale;
            }

            if (star1)
            {
                stars[0].transform.localScale = Vector3.zero;
                stars[0].SetActive(true);
                ShopSystemManager.Instance.UpdateScore(currentLevel.maxSeconds - totalSeconds);

                LeanTween.scale(stars[0], defaultScales[0], 0.25f).setEaseOutBounce().setOnComplete(() => 
                {
                    EventManager.Instance.OnStarAppears?.Invoke();

                    if (star2)
                    {
                        stars[1].transform.localScale = Vector3.zero;
                        stars[1].SetActive(true);
                        LeanTween.scale(stars[1], defaultScales[1], 0.25f).setEaseOutBounce().setOnComplete(() =>
                        {
                            EventManager.Instance.OnStarAppears?.Invoke();

                            if (star3)
                            {
                                stars[2].transform.localScale = Vector3.zero;
                                stars[2].SetActive(true);
                                LeanTween.scale(stars[2], defaultScales[2], 0.25f).setEaseOutBounce().setOnComplete(()=> 
                                {
                                    EventManager.Instance.OnStarAppears?.Invoke();
                                });
                            }
                        });
                    }
                });
            }
        }

        private void ToggleAnimateTaps(bool begin, bool onlyHand)
        {
            if (begin)
            {
                if(!onlyHand)
                    tapToJump.SetActive(true);

                tapHandObj.SetActive(true);

                if (!LeanTween.isTweening(tapHandObj))
                {
                    if(!onlyHand)
                        LeanTween.scale(tapToJump, Vector3.one * 1.2f, 0.3f).setEaseInOutExpo().setLoopPingPong();

                    LeanTween.scale(tapHandObj, Vector3.one * 1.2f, 0.3f).setEaseInOutExpo().setLoopPingPong();
                }
            }
            else
            {
                LeanTween.cancel(tapHandObj);
                LeanTween.cancel(tapToJump);

                tapToJump.SetActive(false);
                tapHandObj.SetActive(false);

                tapToJump.transform.localScale = Vector3.one;
                tapHandObj.transform.localScale = Vector3.one;
            }
        }

        public void ToggleDebugScreen()
        {
            if (!GameManager.Instance.isDebugMode) return;

            isDebugMenuOpened = !isDebugMenuOpened;
            debugScreen.SetActive(isDebugMenuOpened);
        }

        public void SetPlayerStat(PlayerStats stats)
        {
            playerStats = stats;
        }

        private void OnEnemyCatchPlayer(object [] args)
        {
            ToggleAnimateTaps(true, true);

            if (!enemyFightTutDone)
            {
                tapToKick.SetActive(true);
                tapHandObj.SetActive(true);

                LeanTween.scale(tapHandObj.gameObject, Vector3.one * 1.2f, 0.05f).setEaseInCirc().setLoopPingPong()
                    .setOnComplete(() =>
                    {
                        //TimeController.Instance.StopSlowMotion();
                        tapHandObj.SetActive(false);
                    });
                LeanTween.scale(tapToKick.gameObject, Vector3.one * 1.2f, 0.05f).setEaseInCirc().setLoopPingPong()
                    .setOnComplete(() =>
                    {
                        tapToKick.SetActive(false);;
                    });
            }
        }

        private void OnEnemyReleasePlayer()
        {
            if (!enemyFightTutDone)
            {
                LeanTween.cancel(tapToKick);
                LeanTween.cancel(tapHandObj);

                enemyFightTutDone = true;

                tapToKick.SetActive(false);
                tapHandObj.SetActive(false);

                tapToKick.transform.localScale = Vector3.one;
                tapHandObj.transform.localScale = Vector3.one;
            }

            ToggleAnimateTaps(false, true);
        }

        private void OnPlayerGrabbed(GrabPoint grabPoint)
        {
            //if(grabPoint.isShaky)
            //{
            //    if (!shakyRockTutDone)
            //    {
            //        TimeController.Instance.StartSlowMotion(0.1f, 0.02f);
            //        shakyRockTutText.gameObject.SetActive(true);
            //        LeanTween.scale(shakyRockTutText.gameObject, Vector3.one * 1.2f, 0.05f).setEaseInCirc().setLoopPingPong(5)
            //            .setOnComplete(() =>
            //            {
            //                TimeController.Instance.StopSlowMotion();
            //                shakyRockTutDone = true;
            //                shakyRockTutText.gameObject.SetActive(false);

            //                PlayerPrefs.SetInt("shakyRockTutDone", 1);
            //            });
            //    }
            //}

            var playerController = GameObject.Find("Player").GetComponent<PlayerController>();

            Debug.Log("Distance to left finish line = " + playerController.GetLeftToFinishDistance);

            if (!keepOnYourEyeTutDone)
            {
                exhaustedTutText.text = "Keep an eye on your Stamina!";
                //TimeController.Instance.StartSlowMotion(0.1f, 0.02f);
                exhaustedTutText.gameObject.SetActive(true);
                keepOnYourEyeTutDone = true;

                LeanTween.scale(exhaustedTutText.gameObject, Vector3.one * 1.1f, 0.5f).setEaseInOutQuad().setLoopPingPong(4)
                    .setOnComplete(() =>
                    {
                        //TimeController.Instance.StopSlowMotion();
                        exhaustedTutText.gameObject.SetActive(false);

                        //PlayerPrefs.SetInt("exhaustedTutDone", 1);
                    });
                LeanTween.scale(staminaSlider.gameObject, staminaDefaultScale * 1.1f, 0.5f).setEaseInOutQuad().setLoopPingPong(4);

                return;
            }

            if (!exhaustedTutDone && playerController.GetLeftToFinishDistance < 4f)
            {
                if (LeanTween.isTweening(exhaustedTutText.gameObject))
                    LeanTween.cancel(exhaustedTutText.gameObject);
                exhaustedTutText.text = "Out of Stamina! Wait to Regenerate!";
                //TimeController.Instance.StartSlowMotion(0.1f, 0.02f);
                exhaustedTutText.gameObject.SetActive(true);
                LeanTween.scale(exhaustedTutText.gameObject, Vector3.one * 1.1f, 0.5f).setEaseInOutQuad().setLoopPingPong(4)
                    .setOnComplete(() =>
                    {
                        //TimeController.Instance.StopSlowMotion();
                        exhaustedTutDone = true;
                        exhaustedTutText.gameObject.SetActive(false);

                        //PlayerPrefs.SetInt("exhaustedTutDone", 1);
                    });
                LeanTween.scale(staminaSlider.gameObject, staminaDefaultScale * 1.1f, 0.5f).setEaseInOutQuad().setLoopPingPong(4);

                return;
            }

            var instText = Instantiate(onJumpPointText.gameObject, onJumpPointText.transform.parent);
            instText.SetActive(true);
            var rectTransform = instText.GetComponent<RectTransform>();
            rectTransform.position = Camera.main.WorldToScreenPoint(grabPoint.transform.position);
            var textMesh = instText.GetComponent<TMP_Text>();
            var textColor = textMesh.faceColor;
            textColor.a = 0;
            textMesh.faceColor = textColor;

            LeanTween.value(0f, 255f, 0.1f).setOnUpdate((float val) => 
            {
                textColor.a = (byte)val;
                textMesh.faceColor = textColor;
            });
            LeanTween.moveY(rectTransform, rectTransform.rect.y + 50f, 0.3f).setOnComplete(() => 
            {
                playerStats.winStats.extraCash++;

                var currentCash = (int)PlayerPrefs.GetFloat(ShopSystemManager.Instance._scorePrefsKey, 0);
                var totalCash = playerStats.winStats.killCount + playerStats.winStats.extraCash;
                
                ShopSystemCompactUI.Instance.RefreshScoreText(totalCash + currentCash);

                LeanTween.value(255f, 0f, 0.1f).setOnUpdate((float val) =>
                {
                    textColor.a = (byte)val;
                    textMesh.faceColor = textColor;
                }).setOnComplete(()=> { Destroy(instText); });
            });
        }

        private void OnPlayerCantJump()
        {
            if (LeanTween.isTweening(staminaSlider.gameObject) || !exhaustedTutDone) return;

            LeanTween.scale(staminaSlider.gameObject, staminaDefaultScale * 1.2f, 0.2f).setEaseInOutBounce().setLoopPingPong(1);
        }

        private void OnPlayerResting(float delayTime, GrabPoint grabPoint)
        {
            if(LeanTween.isTweening(staminaEnergyImage.gameObject)) return;

            LeanTween.scale(staminaEnergyImage.gameObject, staminaEnergyDefaultScale * 1.3f, delayTime).setLoopPingPong(1);
        }

        private void OnStaminaUpdate(float value, float delayInitialTime, float delayTime)
        {
            //if (LeanTween.isTweening(staminaSlider.gameObject)) return;

            LeanTween.delayedCall(delayInitialTime, () => 
            {
                LeanTween.value(staminaSlider.value, value, delayTime).setOnUpdate((float val) => 
                {
                    staminaSlider.value = val;
                    staminaFillImage.color = Color.Lerp(staminaDepletedColor, staminaDefaultColor, val);
                });
            });
        }

        private void OnProgressUpdate(float value)
        {
            if (LeanTween.isTweening(progressSlider.gameObject)) return;

            LeanTween.value(progressSlider.value, value, 0.4f).setOnUpdate((float val) => { progressSlider.value = val; });
        }
    }
}