using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nano.Animators
{
    public class PlayerAnimator : AnimatorBase
    {
        Dictionary<string, UnityAction> actions;

        protected override void Awake()
        {
            base.Awake();

            actions = new();
        }

        public void SubscribeEvent(string eventName, UnityAction unityAction)
        {
            if(actions.ContainsKey(eventName))
                actions[eventName] += unityAction;
            else
                actions.Add(eventName, unityAction);
        }

        public void UnsubscribeEvent(string eventName, UnityAction unityAction)
        {
            if(actions.ContainsKey(eventName))
                actions[eventName] -= unityAction;
        }

        public void OnAnimationEvent(string eventName)
        {
            if (actions.ContainsKey(eventName))
                actions[eventName]?.Invoke();
        }
    }
}