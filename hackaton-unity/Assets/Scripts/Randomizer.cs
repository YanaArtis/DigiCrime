using UnityEngine;
using System.Collections;

public class Randomizer
{
	ArrayList array = null;
	int minValue = -1;
	int maxValue = 1;	//не включительно!
    int lastValue = -1;

	public Randomizer(int count)
	{
		this.minValue = 0;
		this.maxValue = count;
		Reset();
	}

	public Randomizer(int minValue, int maxValue)
	{
		this.minValue = minValue;
		this.maxValue = maxValue;
		Reset();
	}
	
	public void Reset()
	{
		array = new ArrayList();
		if (minValue < maxValue) for (int i = minValue; i < maxValue; i++) array.Add(i);
		else array.Add(0);
	}
	
    int getRandom()
    {
        int n = Random.Range(0, array.Count);
        int res = (int)array[ n ];

        array.RemoveAt(n);
        if (array.Count < 1) Reset();

        return res;
    }

	public int GetRandom()
	{
        int n = getRandom();
        if (n == lastValue) n = getRandom();
        lastValue = n;
        return n;
	}
}
