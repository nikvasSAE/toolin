using MartynTool;
using UnityEngine;
using Random = UnityEngine.Random;

public class ToolExample : MonoBehaviour
{
    [MinMaxRange(0,5)]
    [SerializeField] private RangedFloat testFloat;
    [MinMaxRange(0,5)]
    [SerializeField] private RangedInt testInt;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            float f = Random.Range(testFloat.MinValue, testFloat.MaxValue);
            Debug.Log(f.ToString());
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            int i = Random.Range(testInt.MinValue, testInt.MaxValue);
            Debug.Log(i.ToString());
        }
    }
}
