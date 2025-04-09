using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nano.ShopSystem
{
    [CreateAssetMenu(fileName = "NewTab", menuName = "Shop System/Create New Tab")]
    public class TabScriptableObject : ScriptableObject
    {
        public int tabID = 0;
        public string tabName = "Default";
        public string tabDescription = "None";
    }
}