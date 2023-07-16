using UnityEngine;
using UnityEngine.Serialization;

public class LumberjackLocationManager : LocationManager
{
	[FormerlySerializedAs("UnitLayer")]
	[SerializeField]
	private Transform _unitLayer;

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Lumberjack;
		}
	}

	public override Transform UnitLayer
	{
		get
		{
			return _unitLayer;
		}
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		return Vector3.zero;
	}
}
