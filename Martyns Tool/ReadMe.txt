RANDOM VALUE SLIDER TOOL

#############
HOW TO USE
#############

This tool allows your to get a random value from a min and max value given in the inspector.

Use the [MinMaxRange(minValue, maxValue)] attribute above either a RangedFloat or RangedInt variable to see a slider in the inspector.
You will be able to choose a min and max value in the inspector.

#############
EXAMPLE
#############

using Martyn
{

	public class Weapon : MonoBehaviour
	{
		[MinMaxRange(0, 10)]
		[Serialized] private RangedFloat damageDealt;
	}
}

#############