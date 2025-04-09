using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nano.Managers
{
    public class InputManager : MonoBehaviour
    {
        private void Update()
        {
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);

            if (GameManager.Instance.GetGameState != GameStates.Play &&
                EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    EventManager.Instance.OnInputTouchStart?.Invoke(touch.position);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    EventManager.Instance.OnInputTouchStationary?.Invoke(touch.position);
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    EventManager.Instance.OnInputTouchEnd?.Invoke(touch.position);
                    break;
            }
        }
    }
}