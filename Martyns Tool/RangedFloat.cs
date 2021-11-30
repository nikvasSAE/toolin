using System;
using UnityEngine;

namespace MartynTool
{
    [Serializable]
    public struct RangedFloat
    {
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;

        public float MinValue
        {
            get => minValue;
            private set => minValue = value;
        }

        public float MaxValue {
            get => maxValue;
            private set => maxValue = value;
        }
    }
}