using System;
using System.Collections;
using System.Collections.Generic;
using Map;
using MMBiomeGeneration;
using MMRoomGeneration;
using UnityEngine;

public class RewardRoomLocationManager : LocationManager
{
	[Serializable]
	private struct ExtraSpawnable
	{
		public List<PlacementRegion.ResourcesAndCount> ResourcesToPlace;

		public float Probability;
	}

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

	[Space]
	[SerializeField]
	private ExtraSpawnable[] extraSpawnables = new ExtraSpawnable[0];

	private PlacementRegion placementRegion;

	private static List<StructuresData> createdStructures = new List<StructuresData>();

	private GenerateRoom room;

	private bool triggered;

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

	protected override void Awake()
	{
		base.Awake();
		ExtraSpawnable[] array = extraSpawnables;
		for (int i = 0; i < array.Length; i++)
		{
			ExtraSpawnable extraSpawnable = array[i];
			for (int j = 0; j < extraSpawnable.ResourcesToPlace.Count; j++)
			{
				int num = 0;
				for (int k = 0; k < extraSpawnable.ResourcesToPlace[j].Count; k++)
				{
					if (UnityEngine.Random.Range(0f, 1f) <= extraSpawnable.Probability)
					{
						num++;
					}
				}
				if (num > 0)
				{
					extraSpawnable.ResourcesToPlace[j].Count = num;
					PlacementRegion obj = placementRegion;
					if ((object)obj != null)
					{
						obj.ResourcesToPlace.Add(extraSpawnable.ResourcesToPlace[j]);
					}
				}
			}
		}
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(OnStructuresPlaced));
	}

	private void OnStructuresPlaced()
	{
		if (!triggered)
		{
			GameManager.GetInstance().StartCoroutine(FrameWait());
		}
		triggered = true;
	}

	private IEnumerator FrameWait()
	{
		while (BiomeGenerator.Instance.CurrentRoom.GameObject == null)
		{
			yield return null;
		}
		while (placementRegion == null && this != null)
		{
			placementRegion = GetComponentInParent<PlacementRegion>();
			yield return null;
		}
		if (!(this == null))
		{
			room = GetComponentInParent<GenerateRoom>();
			StructureManager.PlaceRubble(Location, new List<Structures_PlacementRegion> { placementRegion.structure.Brain as Structures_PlacementRegion });
			MapManager instance = MapManager.Instance;
			instance.OnMapShown = (Action)Delegate.Combine(instance.OnMapShown, new Action(BiomeGenerator_OnBiomeLeftRoom));
			createdStructures = base.StructuresData;
		}
	}

	private void BiomeGenerator_OnBiomeLeftRoom()
	{
		triggered = false;
		MapManager instance = MapManager.Instance;
		instance.OnMapShown = (Action)Delegate.Remove(instance.OnMapShown, new Action(BiomeGenerator_OnBiomeLeftRoom));
		for (int num = createdStructures.Count - 1; num >= 0; num--)
		{
			StructureBrain orCreateBrain = StructureBrain.GetOrCreateBrain(createdStructures[num]);
			orCreateBrain.ForceRemoved = true;
			orCreateBrain.Remove();
		}
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		if (prevLocation == Location)
		{
			return EntranceFromBase.position;
		}
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
}
