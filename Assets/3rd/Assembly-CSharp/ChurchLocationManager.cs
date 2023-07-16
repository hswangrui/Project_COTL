using UnityEngine;

[DefaultExecutionOrder(-50)]
public class ChurchLocationManager : LocationManager
{
	public static ChurchLocationManager Instance;

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.Church;
		}
	}

	public override Transform UnitLayer
	{
		get
		{
			return base.transform;
		}
	}

	public override Transform StructureLayer
	{
		get
		{
			return base.transform;
		}
	}

	protected override void Awake()
	{
		Instance = this;
		base.Awake();
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		return ChurchFollowerManager.Instance.DoorPosition.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
	}

	public override Vector3 GetExitPosition(FollowerLocation destLocation)
	{
		return ChurchFollowerManager.Instance.DoorPosition.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
	}
}
