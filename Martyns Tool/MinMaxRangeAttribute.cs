using System;
using UnityEngine;

namespace MartynTool
{
    public class MinMaxRangeAttribute : Attribute
    {
        internal MinMaxRangeAttribute(float min, float max)
            {
                Min = min;
                Max = max;
            }
        public float Min { get; }
        public float Max { get; }
    }
}