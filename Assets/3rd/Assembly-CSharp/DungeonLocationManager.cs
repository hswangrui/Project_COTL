using System.Collections;
using MMBiomeGeneration;
using MMRoomGeneration;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class DungeonLocationManager : LocationManager
{
	[SerializeField]
	private Transform _unitLayer;

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
			return true;
		}
	}

	public override Transform StructureLayer
	{
		get
		{
			return _unitLayer;
		}
	}

	public FollowerLocation _location { get; set; } = FollowerLocation.None;


	protected override void Awake()
	{
		if ((bool)BiomeGenerator.Instance)
		{
			_location = BiomeGenerator.Instance.DungeonLocation;
		}
		else
		{
			_location = FollowerLocation.Dungeon1_1;
		}
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		BiomeGenerator.OnBiomeChangeRoom += OnChangeRoom;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BiomeGenerator.OnBiomeChangeRoom -= OnChangeRoom;
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		Vector3 result = Vector3.zero;
		if (PlayerFarming.Instance != null)
		{
			result = PlayerFarming.Instance.transform.position;
		}
		return result;
	}

	public override Vector3 GetExitPosition(FollowerLocation destLocation)
	{
		IslandPiece islandPiece = null;
		foreach (IslandPiece piece in GenerateRoom.Instance.Pieces)
		{
			if (piece.IsDoor)
			{
				islandPiece = piece;
				break;
			}
		}
		if (!(islandPiece != null))
		{
			return PlayerFarming.Instance.transform.position;
		}
		return islandPiece.transform.position;
	}

	private void OnChangeRoom()
	{
		StartCoroutine(PlaceRoomStructuresRoutine());
	}

	private IEnumerator PlaceRoomStructuresRoutine()
	{
		if (SupportsStructures)
		{
			yield return new WaitForEndOfFrame();
			yield return StartCoroutine(PlaceStructures());
			PostPlaceStructures();
		}
	}
}
