using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nano.Controllers;

namespace Nano.UI
{
    public class DebugLevelButton : MonoBehaviour
    {
        private int levelID;
        private Button button;

        public void Init(int id)
        {
            levelID = id;
            button = GetComponent<Button>();
            button.onClick.AddListener(StartLevel);
        }

        public void StartLevel()
        {
            LevelController.Instance.LoadLevelByID(levelID);
        }
    }
}