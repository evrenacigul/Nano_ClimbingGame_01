using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Nano.Utilities;

namespace Nano.ShopSystem
{
    public class ShopSystemUI : SingletonMonoBehaviour<ShopSystemUI>
    {
        [SerializeField]
        GameObject _shopWindow;

        [SerializeField]
        GameObject _tabsParent;

        [SerializeField]
        GameObject _tabInstance;

        [SerializeField]
        GameObject _itemHolderInstance;

        [SerializeField]
        GameObject _itemInstance;

        [SerializeField]
        Button _buyButton;

        [SerializeField]
        TMP_Text _scoreText;

        [SerializeField]
        TMP_Text _itemNameText;

        [SerializeField]
        TMP_Text _itemPrice;

        [SerializeField]
        TMP_Text _itemDescriptionText;

        Button _lastSelectedItemButton;
        Dictionary<int, List<GameObject>> _tabItemList;
        GameObject _lastWindowClosed;

        private void Start()
        {
            _buyButton?.onClick.AddListener(() => ShopSystemManager.Instance.OnBuySelectedItem());

            RefreshBuyPanel(null, false);
        }

        public void OpenShopWindow(GameObject closedWindow)
        {
            if (!_shopWindow) return;

            if(closedWindow)
            {
                _lastWindowClosed = closedWindow;
                _lastWindowClosed.SetActive(false);
            }

            _shopWindow.SetActive(true);
        }

        public void CloseShopWindow()
        {
            if (!_shopWindow) return;

            _shopWindow.SetActive(false);

            if (_lastWindowClosed)
                _lastWindowClosed.SetActive(true);
        }

        public void SelectItemButton(Button itemButton)
        {
            _lastSelectedItemButton = itemButton;
        }

        public void DrawTabs(List<TabScriptableObject> tabList)
        {
            tabList = tabList.OrderBy(tab => tab.tabID).ToList();

            _tabItemList = new Dictionary<int, List<GameObject>>();

            foreach (TabScriptableObject tab in tabList)
            {
                GameObject tabObj = Instantiate(_tabInstance, _tabsParent.transform);
                tabObj.SetActive(true);
                tabObj.GetComponent<ShopTab>().SetObjectSO(tab);

                _tabItemList.Add(tab.tabID, new List<GameObject>());
            }
        }

        public void DrawItems(List<ShopItemScriptableObject> itemList)
        {
            foreach (ShopItemScriptableObject item in itemList)
            {
                GameObject itemObj = Instantiate(_itemInstance, _itemHolderInstance.transform);
                //itemObj.GetComponent<ShopItem>().SetObjectSO(item);

                _tabItemList[item.tab.tabID].Add(itemObj);
            }

            RefreshItems(0, 1);
        }

        public void RefreshItems(int tabID, int lastTabID)
        {
            if (tabID == lastTabID) return;

            if (!_tabItemList.TryGetValue(tabID, out var itemList)) return;

            foreach(GameObject itemObj in itemList)
            {
                itemObj.SetActive(true);
            }

            if (!_tabItemList.TryGetValue(lastTabID, out var lastItemList)) return;

            foreach(GameObject itemObj in lastItemList)
            {
                itemObj.SetActive(false);
            }
        }

        public void RefreshBuyPanel(ShopItemScriptableObject itemSO, bool buttonActive)
        {   
            RefreshBuyButtonActive(buttonActive);

            if(!itemSO)
            {
                _itemNameText.text = "";
                _itemDescriptionText.text = "";
                return;
            }

            _itemNameText.text = itemSO.itemName;

            _itemDescriptionText.text = itemSO.itemDescription;

            _itemPrice.text = itemSO.price.ToString();

            if (itemSO.isBought)
                _itemDescriptionText.text += "/nYOU BOUGHT IT ALREADY!";
        }

        public void RefreshBuyButtonActive(bool buttonActive)
        {
            _buyButton.interactable = buttonActive;
        }

        public void RefreshScoreText(int points)
        {
            _scoreText.text = points.ToString();
        }

        public void RefreshItemButton(bool buttonActive)
        {
            _lastSelectedItemButton.interactable = buttonActive;
        }
    }
}