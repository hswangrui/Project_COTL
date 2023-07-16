using System;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;

public class DungeonObjectSelector : BaseMonoBehaviour
{
	[Serializable]
	public class DecorationAndLocation
	{
		public GameObject Decoration;

		public FollowerLocation Location;
	}

	public List<DecorationAndLocation> DecorationsAndLocations = new List<DecorationAndLocation>();

	private void OnEnable()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(OnLoaded));
	}

	private void OnDisable()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnLoaded));
	}

	private void OnLoaded()
	{
		foreach (DecorationAndLocation decorationsAndLocation in DecorationsAndLocations)
		{
			if (BiomeGenerator.Instance == null)
			{
				break;
			}
			if (decorationsAndLocation.Decoration != null && decorationsAndLocation.Location == BiomeGenerator.Instance.DungeonLocation)
			{
				decorationsAndLocation.Decoration.SetActive(true);
				break;
			}
		}
	}

	private void Start()
	{
		foreach (DecorationAndLocation decorationsAndLocation in DecorationsAndLocations)
		{
			if (decorationsAndLocation.Decoration != null)
			{
				decorationsAndLocation.Decoration.SetActive(false);
			}
		}
	}
}
