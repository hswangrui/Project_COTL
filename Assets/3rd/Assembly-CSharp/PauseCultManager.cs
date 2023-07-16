using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseCultManager : BaseMonoBehaviour
{
	public Image CultFaithLevel;

	public Image CultHungerLevel;

	public Image CultIllnessLevel;

	public Image CultTiredLevel;

	public TextMeshProUGUI FollowerAmount;

	private float HappinessTotal;

	private float HungerTotal;

	private float IllnessTotal;

	private float TiredTotal;

	private int TotalBrains;

	private float FaithProgress;

	public List<ThoughtData> BadThoughts = new List<ThoughtData>();

	public List<ThoughtData> GoodThoughts = new List<ThoughtData>();

	public List<FollowerThoughtObject> BadThoughtObjects = new List<FollowerThoughtObject>();

	public List<FollowerThoughtObject> GoodThoughtObjects = new List<FollowerThoughtObject>();

	private void Start()
	{
	}

	private void OnEnable()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			HappinessTotal += allBrain.Stats.Happiness;
			HungerTotal += allBrain.Stats.Satiation + (75f - allBrain.Stats.Starvation);
			IllnessTotal += allBrain.Stats.Illness;
			TiredTotal += allBrain.Stats.Rest;
			TotalBrains++;
		}
		FaithProgress = ((TotalBrains <= 0) ? 0f : (HappinessTotal / (100f * (float)TotalBrains)));
		CultFaithLevel.fillAmount = FaithProgress;
		CultFaithLevel.color = ReturnColorBasedOnValue(CultFaithLevel.fillAmount);
		CultHungerLevel.fillAmount = ((TotalBrains <= 0) ? 0f : (HungerTotal / (175f * (float)TotalBrains)));
		CultHungerLevel.color = ReturnColorBasedOnValueHunger(CultHungerLevel.fillAmount);
		CultIllnessLevel.fillAmount = ((TotalBrains <= 0) ? 0f : (IllnessTotal / (100f * (float)TotalBrains) - 1f)) * -1f;
		CultIllnessLevel.color = ReturnColorBasedOnValue(CultIllnessLevel.fillAmount);
		CultTiredLevel.fillAmount = ((TotalBrains <= 0) ? 0f : (TiredTotal / (100f * (float)TotalBrains)));
		CultTiredLevel.color = ReturnColorBasedOnValue(CultTiredLevel.fillAmount);
		FollowerAmount.text = DataManager.Instance.Followers.Count.ToString();
		GoodThoughts = new List<ThoughtData>();
		BadThoughts = new List<ThoughtData>();
		foreach (FollowerInfo follower in DataManager.Instance.Followers)
		{
			Debug.Log("Follower name: " + follower.Name);
			foreach (ThoughtData thought in follower.Thoughts)
			{
				bool flag;
				if (thought.Modifier < 0f)
				{
					flag = false;
					foreach (ThoughtData badThought in BadThoughts)
					{
						if (badThought.ThoughtType == thought.ThoughtType)
						{
							flag = true;
							badThought.TotalCountDisplay += thought.Quantity;
						}
					}
					if (!flag)
					{
						thought.TotalCountDisplay = thought.Quantity;
						BadThoughts.Add(thought);
					}
					continue;
				}
				flag = false;
				foreach (ThoughtData goodThought in GoodThoughts)
				{
					if (goodThought.ThoughtType == thought.ThoughtType)
					{
						flag = true;
						goodThought.TotalCountDisplay += thought.Quantity;
					}
				}
				if (!flag)
				{
					thought.TotalCountDisplay = thought.Quantity;
					GoodThoughts.Add(thought);
				}
			}
		}
		BadThoughts.Sort(SortLowToHigh);
		GoodThoughts.Sort(SortHighToLow);
		for (int i = 0; i < BadThoughtObjects.Count; i++)
		{
			if (i < BadThoughts.Count)
			{
				BadThoughtObjects[i].Init(BadThoughts[i]);
			}
			else
			{
				BadThoughtObjects[i].Init(null);
			}
		}
		for (int i = 0; i < GoodThoughtObjects.Count; i++)
		{
			if (i < GoodThoughts.Count)
			{
				GoodThoughtObjects[i].Init(GoodThoughts[i]);
			}
			else
			{
				GoodThoughtObjects[i].Init(null);
			}
		}
	}

	private Color ReturnColorBasedOnValue(float f)
	{
		if (f >= 0f && (double)f < 0.25)
		{
			return StaticColors.RedColor;
		}
		if ((double)f >= 0.25 && (double)f < 0.5)
		{
			return StaticColors.OrangeColor;
		}
		return StaticColors.GreenColor;
	}

	private Color ReturnColorBasedOnValueHunger(float f)
	{
		if (f >= 0f && (double)f < 0.5)
		{
			return StaticColors.RedColor;
		}
		if ((double)f >= 0.5 && (double)f < 0.75)
		{
			return StaticColors.OrangeColor;
		}
		return StaticColors.GreenColor;
	}

	private static int SortLowToHigh(ThoughtData t1, ThoughtData t2)
	{
		return t1.Modifier.CompareTo(t2.Modifier);
	}

	private static int SortHighToLow(ThoughtData t1, ThoughtData t2)
	{
		return t2.Modifier.CompareTo(t1.Modifier);
	}

	private void Update()
	{
	}
}
