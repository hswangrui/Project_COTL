using System.Runtime.CompilerServices;
using UnityEngine;

public class IntRange
{
	[CompilerGenerated]
	private readonly int _003CMin_003Ek__BackingField;

	[CompilerGenerated]
	private readonly int _003CMax_003Ek__BackingField;

	public int Min
	{
		[CompilerGenerated]
		get
		{
			return _003CMin_003Ek__BackingField;
		}
	}

	public int Max
	{
		[CompilerGenerated]
		get
		{
			return _003CMax_003Ek__BackingField;
		}
	}

	public IntRange(int min, int max)
	{
		_003CMin_003Ek__BackingField = min;
		_003CMax_003Ek__BackingField = max;
	}

	public int Random()
	{
		if (Min == Max)
		{
			return Min;
		}
		return UnityEngine.Random.Range(Min, Max + 1);
	}

	public int Random(int seed)
	{
		if (Min == Max)
		{
			return Min;
		}
		UnityEngine.Random.InitState(seed);
		return UnityEngine.Random.Range(Min, Max + 1);
	}

	public override string ToString()
	{
		if (Min == Max)
		{
			return Min.ToString();
		}
		return string.Join(" ", Min, "-", Max);
	}
}
