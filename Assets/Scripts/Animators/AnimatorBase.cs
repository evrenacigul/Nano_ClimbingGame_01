using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nano.Animators
{
    public abstract class AnimatorBase : MonoBehaviour
    {
        [SerializeField]
        Animator animator;

        float lastAnimSpeed = 1f;

        protected virtual void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!animator) throw new UnityException("Can't find Animator!");
        }

        public virtual void PlayAnimation(int layer, string animName)
        {
            animator.Play(animName, layer);
        }

        public virtual void PlayAnimation(string animName)
        {
            animator.Play(animName);
        }

        public virtual void PlayAnimation(int layer, string animName, float crossFadeTime)
        {
            animator.CrossFade(animName, crossFadeTime, layer);
        }

        public virtual void PlayAnimation(string animName, float crossFadeTime)
        {
            animator.CrossFade(animName, crossFadeTime);
        }

        public virtual void SetWeight(int layer, float weight)
        {
            animator.SetLayerWeight(layer, weight);
        }

        public virtual void Continue()
        {
            animator.speed = lastAnimSpeed;
        }

        public virtual void Pause()
        {
            lastAnimSpeed = animator.speed;
            animator.speed = 0f;
        }

        public virtual void SetParameter<T>(string paramName, T value)
        {
            var valObj = (object)value;
            if(value is float)
            {
                animator.SetFloat(paramName, (float)valObj);
            }
            else if(value is bool)
            {
                animator.SetBool(paramName, (bool)valObj);
            }
            else if (value is int)
            {
                animator.SetInteger(paramName, (int)valObj);
            }
            else throw new UnityException("Unacceptable type of parameter");
        }

        public virtual void SetCurrentSpeed(float speed)
        {
            animator.speed = speed;
        }

        public virtual void SlowDown(float speed = 0.1f)
        {
            lastAnimSpeed = animator.speed;
            animator.speed = speed;
        }

        public virtual bool IsPlaying(int layer = 0)
        {
            return animator.GetAnimatorTransitionInfo(layer).anyState;
        }
    }

    #if UNITY_EDITOR
    public static class AnimatorEnable
    {
        [MenuItem("Nano Tools/Enable IK Animation Edit")]
        public static void EnableIKEdit()
        {
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, "IK_ANIMATION");
        }

        [MenuItem("Nano Tools/Disable IK Animation Edit")]
        public static void DisableIKEdit()
        {
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.Android, "");
        }
    }
    #endif
}