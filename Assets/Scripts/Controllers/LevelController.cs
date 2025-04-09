using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Nano.Managers;
using Nano.Utilities;
using Nano.Objects;

namespace Nano.Controllers
{
    public class LevelController : SingletonMonoBehaviour<LevelController>
    {
        [SerializeField]
        List<Object> levels;
        
        [SerializeField]
        [Range(0, 100)]
        int currentLevelID = 0;

        [SerializeField]
        Object baseLevel;
        
        GameObject currentLevel;

        Transform levelsParent;

        [SerializeField]
        bool initialLoad = true;
        
        public int LevelCount { get; private set; }

        public int GetCurrentLevelID { get { return currentLevelID; } }

        public LevelObject GetCurrentLevelObject { get { return currentLevel.GetComponent<LevelObject>(); } }

        public bool IsTutorialLevel { get { return GetCurrentLevelObject.isTutorialLevel; } }

        private void OnEnable()
        {
            if (!levelsParent) levelsParent = transform;
        }

        protected override void Awake()
        {
            base.Awake();

            if(!levelsParent) levelsParent = transform;

            LevelCount = levels.Count;
        }

        private void Start()
        {
            EventManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStates state)
        {
            if (!Application.isPlaying) return;
            if (levels is null || levels.Count == 0) throw new UnityException("No levels assigned!");

            switch(state)
            {
                case GameStates.Load:
                    var tutorial = PlayerPrefs.GetInt("tutorial_done", 0);
                    currentLevelID = PlayerPrefs.GetInt("currentLevel", 0);
                    if (currentLevelID >= levels.Count)
                    {
                        currentLevelID = 0;
                        PlayerPrefs.SetInt("currentLevel", currentLevelID);
                    }

                    LoadCurrentLevel();

                    if (IsTutorialLevel && tutorial == 1)
                    {
                        LoadNextLevel();
                        break;
                    }

                    if (!initialLoad)
                        LeanTween.delayedCall(0.2f, () => 
                        {
                            GameManager.Instance.SetGameState(GameStates.Ready);
                        });

                    break;

                case GameStates.Ready:
                    initialLoad = false;
                    break;

                case GameStates.Play:

                    break;

                case GameStates.Win:
                    if (IsTutorialLevel)
                    {
                        PlayerPrefs.SetInt("tutorial_done", 1);
                    }

                    break;

                case GameStates.Fail:
                    
                    break;
            }
        }

        private bool CheckLevels()
        {
            if (levels is null || levels.Count == 0) throw new UnityException("Cannot find any level to load");

            return true;
        }

        public void LoadLevelByID(int ID)
        {
            if (!CheckLevels()) return;

            currentLevelID = ID;
            PlayerPrefs.SetInt("currentLevel", ID);
            LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
        }

        public void LoadCurrentLevel()
        {
            if (!CheckLevels()) return;

            if (Application.isPlaying)
            {
                var childCount = transform.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

                //var currentLevel = PlayerPrefs.GetInt("currentLevel", 0);
                //currentLevelID = currentLevel;

                currentLevel = Instantiate(levels[currentLevelID], transform) as GameObject;
            }
            else
            {
            #if UNITY_EDITOR
                var childs = transform.GetComponentsInChildren<Transform>();

                foreach (Transform child in childs)
                {
                    if (child != null && child.parent == transform)
                        DestroyImmediate(child.gameObject);
                }

                var level = PrefabUtility.InstantiatePrefab(levels[currentLevelID]) as GameObject;
                level.transform.SetParent(transform);
            #endif
            }
        }

        public void RestartLevel()
        {
            if (!CheckLevels()) return;
            
            if (Application.isPlaying)
            {
                PlayerPrefs.SetInt("currentLevel", currentLevelID);

                LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
            }
        }

        public void LoadNextLevel()
        {
            if (!CheckLevels()) return;

            if (currentLevelID < levels.Count - 1)
                currentLevelID++;
            else
                currentLevelID = 0;


            if (Application.isPlaying)
            {
                PlayerPrefs.SetInt("currentLevel", currentLevelID);

                LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });   
            }
            else
            {
                LoadCurrentLevel();
            }
        }

        public void LoadPreviousLevel()
        {
            if (!CheckLevels()) return;

            if (currentLevelID == 0)
                currentLevelID = levels.Count - 1;
            else
                currentLevelID--;

            if (Application.isPlaying)
            {
                PlayerPrefs.SetInt("currentLevel", currentLevelID);

                LeanTween.delayedCall(0.1f, () => { GameManager.Instance.SetGameState(GameStates.Load); });
            }
            else
            {
                LoadCurrentLevel();
            }
        }

        //public void CreateLevel()
        //{
            
        //}
    }
}