using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class HubLocationManager : LocationManager
{
	[Serializable]
	public class LocationAndEntrance
	{
		public FollowerLocation Location;

		public GameObject Entrance;
	}

	[SerializeField]
	private FollowerLocation _location;

	[SerializeField]
	private Transform _unitLayer;

	[SerializeField]
	private Transform _structureLayer;

	public bool AllowStructures = true;

	public Transform EntranceFromBase;

	public List<LocationAndEntrance> LocationAndEntrances = new List<LocationAndEntrance>();

	public override FollowerLocation Location
	{
		get
		{
			return _location;
		}
	}

	public override Transform UnitLayer
	{
		get
		{
			return _unitLayer;
		}
	}

	public override bool SupportsStructures
	{
		get
		{
			return AllowStructures;
		}
	}

	public override Transform StructureLayer
	{
		get
		{
			return _structureLayer;
		}
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		foreach (LocationAndEntrance locationAndEntrance in LocationAndEntrances)
		{
			if (prevLocation == locationAndEntrance.Location)
			{
				return locationAndEntrance.Entrance.transform.position;
			}
		}
		if (prevLocation == FollowerLocation.None || prevLocation == FollowerLocation.Base || prevLocation == FollowerLocation.HubShore)
		{
			return EntranceFromBase.position;
		}
		if (PlayerFarming.Instance != null)
		{
			return PlayerFarming.Instance.transform.position;
		}
		return EntranceFromBase.position;
	}

	public override Vector3 GetExitPosition(FollowerLocation destLocation)
	{
		foreach (LocationAndEntrance locationAndEntrance in LocationAndEntrances)
		{
			if (destLocation == locationAndEntrance.Location)
			{
				return locationAndEntrance.Entrance.transform.position;
			}
		}
		if ((uint)destLocation <= 1u)
		{
			return EntranceFromBase.position;
		}
		return base.GetExitPosition(destLocation);
	}

	protected override void PostPlaceStructures()
	{
		Debug.Log("PostPlaceStructures() " + Location);
		StructureManager.PlaceRubble(Location);
	}
}
