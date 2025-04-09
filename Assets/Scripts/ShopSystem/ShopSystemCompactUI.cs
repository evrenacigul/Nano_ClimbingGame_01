using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using Nano.Utilities;

namespace Nano.ShopSystem
{
    public class ShopSystemCompactUI : SingletonMonoBehaviour<ShopSystemCompactUI>
    {
        [SerializeField]
        GameObject _shopWindow;

        [SerializeField]
        TMP_Text _scoreText;

        [SerializeField]
        List<ShopItem> _buyableItems;

        ShopItem lastSelectedItem;
        
        private void Start()
        {
            //
        }

        public void OpenShopWindow()
        {
            if (!_shopWindow) return;

            _shopWindow.SetActive(true);

            RefreshItems();
        }

        public void CloseShopWindow()
        {
            if (!_shopWindow) return;

            _shopWindow.SetActive(false);
        }

        public void BuyItemButton(ShopItem item)
        {
            lastSelectedItem = item;
            ShopSystemManager.Instance.OnSelectItem(item.GetItemSO);
            ShopSystemManager.Instance.OnBuySelectedItem();
            RefreshItems();
        }

        public void RefreshItems()
        {
            foreach(ShopItem item in _buyableItems)
            {
                item.Refresh();
            }
        }

        public void RefreshScoreText(int points)
        {
            _scoreText.text = points.ToString();
        }
    }
}