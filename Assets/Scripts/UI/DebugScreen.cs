using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nano.Controllers;
using Nano.Managers;
using TMPro;

namespace Nano.UI
{
    public class DebugScreen : MonoBehaviour
    {
        [SerializeField]
        GameObject levelSelectObj;

        private void Start()
        {
            LeanTween.delayedCall(0.1f, () =>
            {
                if (!GameManager.Instance.isDebugMode) return;

                InstantiateLevelButtons();

                gameObject.SetActive(false);
            });
        }

        private void InstantiateLevelButtons()
        {
            for(int i = 0; i < LevelController.Instance.LevelCount; i++)
            {
                var levelObj = Instantiate(levelSelectObj, levelSelectObj.transform.parent);
                levelObj.SetActive(true);

                var levelText = levelObj.GetComponentInChildren<TMP_Text>();

                levelText.text = (i + 1).ToString();
                levelObj.name = "Level Button " + i;

                var debugButton = levelObj.AddComponent<DebugLevelButton>();
                debugButton.Init(i);
            }
        }

        //void InstantiateLevelByID(int ID)
        //{
        //    LevelController.Instance.LoadLevelByID(ID);
        //}
    }
}