using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nano.Managers;

namespace Nano.Objects
{
    public class FinishAreaObject : MonoBehaviour
    {
        private void OnMouseDown()
        {
            var minPosition = GetComponent<BoxCollider>().bounds.min;
            EventManager.Instance.OnFinishAreaTapped?.Invoke(minPosition);
        }
    }
}