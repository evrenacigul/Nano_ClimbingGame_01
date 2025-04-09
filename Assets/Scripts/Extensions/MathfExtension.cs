using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nano.Extensions
{
    public static class MathfExtension
    {
        public static float Remap(this float value, float from1, float from2, float to1, float to2)
        {
            //return (value - from1) / (to1 - from1) * (to2 - from2) + from2;

            float t = Mathf.InverseLerp(from1, from2, value);
            return Mathf.Lerp(to1, to2, t);
        }
    }
}