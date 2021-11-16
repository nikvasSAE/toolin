using UnityEngine;
using UnityEditor;

namespace Martyn
{
    [CustomPropertyDrawer(typeof(RangedInt), true)]
    [CustomPropertyDrawer(typeof(RangedFloat), true)]
    public class RangedValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            var minProp = property.FindPropertyRelative("minValue");
            var maxProp = property.FindPropertyRelative("maxValue");

            switch (minProp.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    float minValue = minProp.intValue;
                    float maxValue = maxProp.intValue;

                    float rangeMin = 0;
                    float rangeMax = 1;

                    var ranges = (MinMaxRangeAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
                    if (ranges.Length > 0)
                    {
                        rangeMin = ranges[0].Min;
                        rangeMax = ranges[0].Max;
                    }

                    const float rangeBoundsLabelWidth = 40f;

                    var rangeBoundsLabel1Rect = new Rect(position)
                    {
                        width = rangeBoundsLabelWidth
                    };
                    GUI.Label(rangeBoundsLabel1Rect, new GUIContent(minValue.ToString("F0")));
                    position.xMin += rangeBoundsLabelWidth;

                    var rangeBounds2Rect = new Rect(position);
                    rangeBounds2Rect.xMin = rangeBounds2Rect.xMax - rangeBoundsLabelWidth;
                    GUI.Label(rangeBounds2Rect, new GUIContent(maxValue.ToString("F0")));
                    position.xMax -= rangeBoundsLabelWidth;

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, rangeMin, rangeMax);
                    if (EditorGUI.EndChangeCheck())
                    {
                        minProp.intValue = (int)minValue;
                        maxProp.intValue = (int)maxValue;
                    }
                    EditorGUI.EndProperty();
                    break;
                }
                case SerializedPropertyType.Float:
                {
                    var minValue = minProp.floatValue;
                    var maxValue = maxProp.floatValue;

                    float rangeMin = 0;
                    float rangeMax = 1;

                    var ranges = (MinMaxRangeAttribute[])fieldInfo.GetCustomAttributes(typeof(MinMaxRangeAttribute), true);
                    if (ranges.Length > 0)
                    {
                        rangeMin = ranges[0].Min;
                        rangeMax = ranges[0].Max;
                    }

                    const float rangeBoundsLabelWidth = 40f;

                    var rangeBoundsLabel1Rect = new Rect(position)
                    {
                        width = rangeBoundsLabelWidth
                    };
                    GUI.Label(rangeBoundsLabel1Rect, new GUIContent(minValue.ToString("F2")));
                    position.xMin += rangeBoundsLabelWidth;

                    var rangeBounds2Rect = new Rect(position);
                    rangeBounds2Rect.xMin = rangeBounds2Rect.xMax - rangeBoundsLabelWidth;
                    GUI.Label(rangeBounds2Rect, new GUIContent(maxValue.ToString("F2")));
                    position.xMax -= rangeBoundsLabelWidth;

                    EditorGUI.BeginChangeCheck();
                    EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, rangeMin, rangeMax);
                    if (EditorGUI.EndChangeCheck())
                    {
                        minProp.floatValue = minValue;
                        maxProp.floatValue = maxValue;
                    }
                    EditorGUI.EndProperty();
                    break;
                }
            }
        }
    }
}