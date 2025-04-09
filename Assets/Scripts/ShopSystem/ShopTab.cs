using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Nano.ShopSystem
{
    public class ShopTab : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _tabText;

        [SerializeField]
        private Button _tabButton;

        [SerializeField]
        private Image _tabImage;

        [SerializeField]
        private int _tabID;

        [SerializeField]
        private TabScriptableObject _tabObjectSO;

        public void SetTabTag(string text)
        {
            _tabText.text = text;
            gameObject.name = text;
        }

        public void SetObjectSO(TabScriptableObject tabObjSO)
        {
            _tabObjectSO = tabObjSO;
            SetTabTag(_tabObjectSO.tabName);
            
            //if(!_tabObjectSO.tabImage.texture)
            //{
            //    _tabImage.sprite = _tabObjectSO.tabImage;
            //}

            _tabID = _tabObjectSO.tabID;

            _tabButton.onClick.AddListener(() => ShopSystemManager.Instance.OnSelectTab(_tabObjectSO));
        }
    }
}