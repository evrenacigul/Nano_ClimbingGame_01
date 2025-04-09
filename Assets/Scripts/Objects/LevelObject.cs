using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nano.Objects
{
    public class LevelObject : MonoBehaviour
    {
        public float maxSeconds;
        public bool isTutorialLevel;

        public bool CheckFirstStar(float totalSeconds)
        {
            return (totalSeconds <= maxSeconds);
        }

        public bool CheckSecondStar(float totalSeconds)
        {
            return (totalSeconds < maxSeconds / 2);
        }

        public bool CheckThirdStar(float totalSeconds)
        {
            return (totalSeconds < maxSeconds / 3);
        }
    }
}