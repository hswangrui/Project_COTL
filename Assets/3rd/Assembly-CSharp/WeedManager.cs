using System;
using System.Collections.Generic;
using UnityEngine;

public class WeedManager : MonoBehaviour
{
	[Serializable]
	public struct Weed
	{
		public List<GameObject> Weeds;
	}

	public static List<WeedManager> WeedManagers = new List<WeedManager>();

	public List<Weed> WeedTypes = new List<Weed>();

	public GameObject ChosenWeed;

	public Interaction_Weed Interaction_Weed;

	private int weedTypeChosen = -1;

	private int growthStageOffset;

	public int WeedTypeChosen
	{
		get
		{
			return weedTypeChosen;
		}
		set
		{
			weedTypeChosen = Mathf.Clamp(value, 0, WeedTypes.Count - 1);
			UpdateWeedGrowth();
		}
	}

	public int GrowthStageOffset
	{
		get
		{
			return growthStageOffset;
		}
		set
		{
			growthStageOffset = Mathf.Clamp(value, 0, WeedTypes[WeedTypeChosen].Weeds.Count - 1);
			UpdateWeedGrowth();
		}
	}

	private void OnEnable()
	{
		OnBrainAssigned();
		TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
		WeedManagers.Add(this);
	}

	private void OnNewDayStarted()
	{
		UpdateWeedGrowth();
	}

	private void OnDisable()
	{
		TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(OnNewDayStarted));
		WeedManagers.Remove(this);
	}

	private void Hide()
	{
		HideAll();
	}

	public static void HideAll()
	{
		foreach (WeedManager weedManager in WeedManagers)
		{
			if ((bool)weedManager.ChosenWeed)
			{
				weedManager.ChosenWeed.SetActive(false);
			}
		}
	}

	private void Show()
	{
		ShowAll();
	}

	public static void ShowAll()
	{
		foreach (WeedManager weedManager in WeedManagers)
		{
			if ((bool)weedManager.ChosenWeed)
			{
				weedManager.ChosenWeed.SetActive(true);
			}
		}
	}

	private void OnBrainAssigned()
	{
		UpdateWeedType();
	}

	private void UpdateWeedType()
	{
		UpdateWeedGrowth();
	}

	private void UpdateWeedGrowth()
	{
		if (WeedTypeChosen != -1)
		{
			if (ChosenWeed != null)
			{
				ObjectPool.Recycle(ChosenWeed);
			}
			ChosenWeed = ObjectPool.Spawn(WeedTypes[WeedTypeChosen].Weeds[Mathf.Clamp(Mathf.FloorToInt((float)TimeManager.CurrentDay / 2f) + growthStageOffset, 0, WeedTypes[WeedTypeChosen].Weeds.Count - 1)], base.transform);
		}
	}
}
