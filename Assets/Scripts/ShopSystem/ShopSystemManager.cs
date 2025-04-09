using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Nano.Managers;
using Nano.Utilities;

namespace Nano.ShopSystem
{
    public class ShopSystemManager : SingletonMonoBehaviour<ShopSystemManager>
    {
        public UnityEvent<ShopItemScriptableObject, bool> OnItemBuy;

        public string _scorePrefsKey;

        [SerializeField]
        List<TabScriptableObject> _tabList;

        [SerializeField]
        List<ShopItemScriptableObject> _itemList;

        ShopItemScriptableObject _selectedItem;

        float _cumulativeScore;

        int _lastTabID;
        int _currentTabID;

        private void Start()
        {
            ReadScoreFromPrefs();

            //ShopSystemUI.Instance.DrawTabs(_tabList);
            //ShopSystemUI.Instance.DrawItems(_itemList);
        }

        public void CheckBoughtItems(UnityAction<ShopItemScriptableObject, bool> callBack, bool initialCheck)
        {
            foreach(ShopItemScriptableObject item in _itemList)
            {
                item.boughtCount = GetBoughtCount(item);
                callBack(item, initialCheck);
            }
        }

        public void UpdateScore(float score)
        {
            ReadScoreFromPrefs();
            _cumulativeScore += score;
            WriteScoreToPrefs();
        }

        private void ReadScoreFromPrefs()
        {
            _cumulativeScore = PlayerPrefs.GetFloat(_scorePrefsKey, 0);

            ShopSystemCompactUI.Instance.RefreshScoreText((int)_cumulativeScore);
        }

        private void WriteScoreToPrefs()
        {
            PlayerPrefs.SetFloat(_scorePrefsKey, _cumulativeScore);
            PlayerPrefs.Save();

            ShopSystemCompactUI.Instance.RefreshScoreText((int)_cumulativeScore);
        }

        private void LevelFinished()
        {
            ReadScoreFromPrefs();
        }

        public void OnSelectTab(TabScriptableObject tabSO)
        {
            _lastTabID = _currentTabID;

            _currentTabID = tabSO.tabID;

            //ShopSystemUI.Instance.RefreshItems(_currentTabID, _lastTabID);
        }

        public void OnSelectItem(ShopItemScriptableObject itemSO)
        {
            _selectedItem = itemSO;

            bool buttonActive = _cumulativeScore >= _selectedItem.price;

            //ShopSystemUI.Instance.RefreshBuyPanel(_selectedItem, buttonActive);
        }

        public void OnBuySelectedItem()
        {
            if (!_selectedItem) return;

            if (!IsBuyable(_selectedItem)) return;

            _cumulativeScore -= _selectedItem.price;

            var boughtCount = PlayerPrefs.GetInt(_selectedItem.itemName, 0) + 1;
            PlayerPrefs.SetInt(_selectedItem.itemName, boughtCount);
            
            WriteScoreToPrefs();

            Debug.Log("OnItemBuy start");
            OnItemBuy.Invoke(_selectedItem, false);
            //OnSelectItem(_selectedItem);
        }

        public bool IsBuyable(ShopItemScriptableObject item)
        {
            if (item is null) return false;

            var boughtCount = PlayerPrefs.GetInt(item.itemName, 0);

            if (boughtCount >= item.maxCanBeBought)
                return false;

            if (_cumulativeScore < item.price)
                return false;

            return true;
        }

        private int GetBoughtCount(ShopItemScriptableObject item)
        {
            return PlayerPrefs.GetInt(item.itemName, 0);
        }

        public void OnLevelFail()
        {
            LevelFinished();
        }

        public void OnLevelWin()
        {
            LevelFinished();
        }
    }
}