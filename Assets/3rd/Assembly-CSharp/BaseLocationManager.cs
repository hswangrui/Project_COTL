using System;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class BaseLocationManager : LocationManager
{
	public static BaseLocationManager Instance;

	[SerializeField]
	private Transform _unitLayer;

	[SerializeField]
	private Transform _structureLayer;

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Base;
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
			return true;
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
		Instance = this;
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Combine(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(ShowCultName));
	}

	private void ShowCultName()
	{
		StructureManager.OnStructuresPlaced = (StructureManager.StructuresPlaced)Delegate.Remove(StructureManager.OnStructuresPlaced, new StructureManager.StructuresPlaced(ShowCultName));
		if (!string.IsNullOrEmpty(DataManager.Instance.CultName))
		{
			HUD_DisplayName.PlayTranslatedText(DataManager.Instance.CultName, 3, HUD_DisplayName.Positions.Centre);
		}
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		if (BiomeBaseManager.Instance == null)
		{
			Debug.LogWarning("BiomeBaseManager.Instance == null ??");
			return Vector3.zero;
		}
		switch (prevLocation)
		{
		case FollowerLocation.Church:
			if (Interaction_Temple.Instance == null)
			{
				return BiomeBaseManager.Instance.PlayerSpawnLocation.transform.position;
			}
			return Interaction_Temple.Instance.ExitPosition.transform.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
		case FollowerLocation.DoorRoom:
			return BiomeBaseManager.Instance.PlayerReturnFromDoorRoomLocation.transform.position;
		case FollowerLocation.Endless:
			return BiomeBaseManager.Instance.PlayerReturnFromEndlessLocation.transform.position;
		default:
			return BiomeBaseManager.Instance.PlayerSpawnLocation.transform.position;
		}
	}

	public override Vector3 GetExitPosition(FollowerLocation destLocation)
	{
		if (destLocation == FollowerLocation.Church)
		{
			return Interaction_Temple.Instance.Entrance.transform.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
		}
		return BiomeBaseManager.Instance.PlayerSpawnLocation.transform.position;
	}

	protected override void PostPlaceStructures()
	{
		StructureManager.PlaceRubble(Location);
	}
}
