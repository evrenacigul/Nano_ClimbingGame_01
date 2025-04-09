using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Nano.ShopSystem
{
    public class ShopItem : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private TMP_Text _itemText;

        [SerializeField]
        private TMP_Text _priceText;

        [SerializeField]
        private TMP_Text _boughtCountText;

        [SerializeField]
        private Image _itemImage;

        [SerializeField]
        private ShopItemScriptableObject _itemObjectSO;

        private ShopItemScriptableObject _instancedSO;

        public ShopItemScriptableObject GetItemSO { get { return _instancedSO; } }

        private void Awake()
        {
            _instancedSO = (ShopItemScriptableObject)ScriptableObject.CreateInstance(typeof(ShopItemScriptableObject));
            _instancedSO.boughtCount = _itemObjectSO.boughtCount;
            _instancedSO.floatValue = _itemObjectSO.floatValue;
            _instancedSO.intValue = _itemObjectSO.intValue;
            _instancedSO.isBought = _itemObjectSO.isBought;
            _instancedSO.itemDescription = _itemObjectSO.itemDescription;
            _instancedSO.itemName = _itemObjectSO.itemName;
            _instancedSO.materialChange = _itemObjectSO.materialChange;
            _instancedSO.maxCanBeBought = _itemObjectSO.maxCanBeBought;
            _instancedSO.modifierName = _itemObjectSO.modifierName;
            _instancedSO.price = _itemObjectSO.price;
            _instancedSO.tab = _itemObjectSO.tab;

            SetObjectSO(_instancedSO);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnMouseDown();
        }

        private void OnMouseDown()
        {
            if (!ShopSystemManager.Instance.IsBuyable(_instancedSO)) return;

            if (!LeanTween.isTweening(gameObject))
            {
                LeanTween.scale(gameObject, Vector3.one * 0.8f, 0.15f).setEaseInOutElastic().setLoopPingPong(1);
            }

            ShopSystemCompactUI.Instance.BuyItemButton(this);
        }

        public void Refresh()
        {
            if (!ShopSystemManager.Instance.IsBuyable(_instancedSO))
            {
                var itemColor = _itemImage.color;
                itemColor.a = 0.4f;
                _itemImage.color = itemColor;
            }
            else
            {
                var itemColor = _itemImage.color;
                itemColor.a = 1f;
                _itemImage.color = itemColor;
            }

            _instancedSO.boughtCount = PlayerPrefs.GetInt(_instancedSO.itemName, 0);
            SetBoughtCount(_instancedSO.boughtCount, _instancedSO.maxCanBeBought);
        }

        public void SetPrice(string text)
        {
            _priceText.text = text;
        }

        public void SetItemTag(string text)
        {
            _itemText.text = text;
            gameObject.name = text;
        }

        public void SetBoughtCount(int count, int maxCount)
        {
            if (!_boughtCountText) return;

            _boughtCountText.text = count.ToString() + "/" + maxCount.ToString();
        }

        private void SetObjectSO(ShopItemScriptableObject itemObjSO)
        {
            //_itemObjectSO = itemObjSO;
            SetItemTag(_instancedSO.itemName);
            SetPrice(_instancedSO.price.ToString());

            GetItemSO.boughtCount = PlayerPrefs.GetInt(_instancedSO.itemName, 0);
            SetBoughtCount(_instancedSO.boughtCount, _instancedSO.maxCanBeBought);
        }
    }
}