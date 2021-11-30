using System;
using UnityEngine;

namespace MartynTool
{
    [Serializable]
    public struct RangedInt
    {
        [SerializeField] private int minValue;
        [SerializeField] private int maxValue;

        public int MinValue
        {
            get => minValue;
            private set => minValue = value;
        }

        public int MaxValue {
            get => maxValue;
            private set => maxValue = value;
        }
    }
}