using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nano.ShopSystem
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Shop System/Create New Shop Item")]
    public class ShopItemScriptableObject : ScriptableObject
    {
        public TabScriptableObject tab;
        public string itemName;
        public string itemDescription;
        public int price;
        public bool isBought;
        public int boughtCount;
        public int maxCanBeBought;

        [Header("Modifier Variables")]
        public string modifierName;
        public Material materialChange;
        public float floatValue;
        public int intValue;
    }
}