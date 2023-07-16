using UnityEngine;

[DefaultExecutionOrder(-50)]
public class DoorRoomLocationManager : LocationManager
{
	public static DoorRoomLocationManager Instance;

	public Transform DoorPosition;

	public Animator SkyAnimator;

	[SerializeField]
	private GameObject chainDoorObject;

	[SerializeField]
	private GameObject mysticShopObject;

	public override FollowerLocation Location
	{
		get
		{
			return FollowerLocation.DoorRoom;
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

	protected override void Start()
	{
		base.Start();
		chainDoorObject.SetActive(!DataManager.Instance.DeathCatBeaten || !DataManager.Instance.OnboardedMysticShop);
		mysticShopObject.SetActive(DataManager.Instance.DeathCatBeaten);
	}

	protected override void Update()
	{
		base.Update();
		SkyAnimator.SetBool("BloodMoon", FollowerBrainStats.IsBloodMoon);
	}

	protected override Vector3 GetStartPosition(FollowerLocation prevLocation)
	{
		return Instance.DoorPosition.position;
	}

	public override Vector3 GetExitPosition(FollowerLocation destLocation)
	{
		return Instance.DoorPosition.position;
	}
}
