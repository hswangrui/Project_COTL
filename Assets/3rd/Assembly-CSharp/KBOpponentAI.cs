using System.Collections.Generic;
using KnuckleBones;
using UnityEngine;

public class KBOpponentAI
{
	private float _randomStupidity;

	public KBOpponentAI(float stupidity)
	{
		_randomStupidity = stupidity;
	}

	public int Evaluate(List<KBDiceTub> tubs, Dice newDice)
	{
		if (Random.value * 10f > _randomStupidity)
		{
			List<int> list = new List<int>();
			int num = -1;
			while (++num < tubs.Count)
			{
				Debug.Log(num);
				if (tubs[num].Dice.Count < 3)
				{
					list.Add(num);
				}
			}
			int num2 = list[Random.Range(0, list.Count)];
			Debug.Log("BE STUPID! " + num2);
			return num2;
		}
		Debug.Log("BE SMART");
		List<int> list2 = new List<int> { 0, 0, 0 };
		int num3 = -1;
		while (++num3 < tubs.Count)
		{
			KBDiceTub kBDiceTub = tubs[num3];
			if (kBDiceTub.Dice.Count >= 3)
			{
				list2[num3] = int.MinValue;
				continue;
			}
			switch (kBDiceTub.OpponentTub.NumMatchingDice(newDice.Num))
			{
			case 0:
			{
				List<int> list3 = list2;
				int index = num3;
				list3[index] = list3[index];
				break;
			}
			case 1:
				list2[num3]++;
				break;
			case 2:
				list2[num3] += 2;
				break;
			case 3:
				list2[num3] += 5;
				break;
			}
			switch (kBDiceTub.NumMatchingDice(newDice.Num))
			{
			case 0:
			{
				List<int> list3 = list2;
				int index = num3;
				list3[index] = list3[index];
				break;
			}
			case 1:
				list2[num3]++;
				break;
			case 2:
				list2[num3] += 2;
				break;
			case 3:
				list2[num3] += 5;
				break;
			}
		}
		int num4 = int.MinValue;
		num3 = -1;
		while (++num3 < list2.Count)
		{
			if (list2[num3] > num4)
			{
				num4 = list2[num3];
			}
		}
		List<int> list4 = new List<int>();
		num3 = -1;
		while (++num3 < list2.Count)
		{
			if (list2[num3] == num4)
			{
				list4.Add(num3);
			}
		}
		return list4[Random.Range(0, list4.Count)];
	}
}
