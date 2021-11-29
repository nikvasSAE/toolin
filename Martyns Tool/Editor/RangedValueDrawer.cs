using UnityEngine;
using UnityEditor;

namespace Martyn
{
    [CustomPropertyDrawer(typeof(RangedInt), true)]
    [CustomPropertyDrawer(typeof(RangedFloat), true)]
    public class RangedValueDrawer : PropertyDrawer
    {
        private float _minValue;
        private float _maxValue;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty minProp = property.FindPropertyRelative("minValue");
            SerializedProperty maxProp = property.FindPropertyRelative("maxValue");

            switch (minProp.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    SliderGUI("integer", "F0", minProp, maxProp, position);
                    break;
                }
                case SerializedPropertyType.Float:
                {
                    SliderGUI("float", "F2", minProp, maxProp, position);
                    break;
                }
            }
        }
        
        private void SliderGUI(string numType, string decimals, SerializedProperty minProp, SerializedProperty maxProp, Rect position)
        {
            switch (numType)
            {
                case "integer":
                    _minValue = minProp.intValue;
                    _maxValue = maxProp.intValue;
                    break;
                case "float":
                    _minValue = minProp.floatValue;
                    _maxValue = maxProp.floatValue;
                    break;
            }
            
            float rangeMin = 0;
            float rangeMax = 1;

            MinMaxRangeAttribute[] ranges = (MinMaxRangeAttribute[]) fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
            if (ranges.Length > 0)
            {
                rangeMin = ranges[0].Min;
                rangeMax = ranges[0].Max;
            }

            const float rangeBoundsLabelWidth = 40f;

            Rect rangeBoundsLabel1Rect = new Rect(position)
            {
                width = rangeBoundsLabelWidth
            };
            GUI.Label(rangeBoundsLabel1Rect, new GUIContent(_minValue.ToString(decimals)));
            position.xMin += rangeBoundsLabelWidth;

            Rect rangeBounds2Rect = new Rect(position);
            rangeBounds2Rect.xMin = rangeBounds2Rect.xMax - rangeBoundsLabelWidth;
            GUI.Label(rangeBounds2Rect, new GUIContent(_maxValue.ToString(decimals)));
            position.xMax -= rangeBoundsLabelWidth;

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, ref _minValue, ref _maxValue, rangeMin, rangeMax);
            if (EditorGUI.EndChangeCheck())
            {
                switch (numType)
                {
                    case "integer":
                        minProp.intValue = (int) _minValue;
                        maxProp.intValue = (int) _maxValue;
                        break;
                    case "float":
                        minProp.floatValue = _minValue;
                        maxProp.floatValue = _maxValue;
                        break;
                }
            }
            EditorGUI.EndProperty();
        }
    }
}