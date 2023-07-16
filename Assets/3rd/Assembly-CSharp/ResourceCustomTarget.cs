using System;
using DG.Tweening;
using MMRoomGeneration;
using UnityEngine;

public class ResourceCustomTarget : BaseMonoBehaviour, IPoolListener
{
	private const float MinimumCollectDistance = 1f;

	private const float MinimumCollectDuration = 0.33f;

	private const float Acceleration = 12f;

	private const float MaxSpeed = 15f;

	public InventoryItemDisplay inventoryItemDisplay;

	private GameObject Target;

	private Transform ResourceTransform;

	private float Delay = 0.5f;

	private Vector3 TargetPosition;

	private float Speed = -7f;

	private float ResourceTargetDistance;

	private float Angle;

	private float AngleTimer;

	private float TargetAngle;

	public TrailRenderer Trail;

	private float ImageZ;

	private float ImageZSpeed;

	private float TurnSpeed = 7f;

	private Vector3 ZPosition;

	private Action Callback;

	private static GameObject resourceCustomTargetPrefab;

	private bool UseDeltaTime = true;

	private float lifetime;

	private string SfxToPlay;

	private bool init;

	public GameObject createdObject;

	private float DeltaTime
	{
		get
		{
			if (!UseDeltaTime)
			{
				return Time.unscaledDeltaTime;
			}
			return Time.deltaTime;
		}
	}

	public void Init(GameObject Target, InventoryItem.ITEM_TYPE Item, Action Callback)
	{
		this.Target = Target;
		Angle = UnityEngine.Random.Range(0, 360);
		inventoryItemDisplay.SetImage(Item);
		this.Callback = Callback;
		base.transform.localScale = Vector3.one * 1.5f;
		base.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
		ResourceTransform = base.transform;
	}

	public void Init(GameObject Target, Sprite sprite, Action Callback)
	{
		this.Target = Target;
		Angle = UnityEngine.Random.Range(0, 360);
		inventoryItemDisplay.SetImage(sprite);
		this.Callback = Callback;
		base.transform.localScale = Vector3.one * 1.5f;
		base.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).SetUpdate(true);
		ResourceTransform = base.transform;
	}

	private void OnEnable()
	{
		lifetime = 0f;
	}

	public static void CreatePool(int count)
	{
		if (resourceCustomTargetPrefab == null)
		{
			resourceCustomTargetPrefab = Resources.Load("Prefabs/Resources/ResourceCustomTarget") as GameObject;
		}
		ObjectPool.CreatePool(resourceCustomTargetPrefab, count);
	}

	public static void Create(GameObject Target, Vector3 position, InventoryItem.ITEM_TYPE Item, Action Callback, Transform parent, bool UseDeltaTime = true)
	{
		if (resourceCustomTargetPrefab == null)
		{
			resourceCustomTargetPrefab = Resources.Load("Prefabs/Resources/ResourceCustomTarget") as GameObject;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(resourceCustomTargetPrefab, parent, true);
		gameObject.transform.position = position;
		gameObject.transform.eulerAngles = Vector3.zero;
		ResourceCustomTarget component = gameObject.GetComponent<ResourceCustomTarget>();
		component.createdObject = gameObject;
		component.Init(Target, Item, Callback);
		component.UseDeltaTime = UseDeltaTime;
		gameObject = BiomeConstants.Instance.HitFX_Blocked.Spawn();
		gameObject.transform.parent = parent;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.position = position;
		component.init = true;
	}

	public static void Create(GameObject Target, Vector3 position, InventoryItem.ITEM_TYPE Item, Action Callback, bool UseDeltaTime = true)
	{
		Transform transform = null;
		if (RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null)
		{
			transform = RoomManager.Instance.CurrentRoomPrefab.transform;
		}
		else if (GameObject.FindGameObjectWithTag("Unit Layer") != null)
		{
			transform = GameObject.FindGameObjectWithTag("Unit Layer").transform;
		}
		else if (GenerateRoom.Instance != null)
		{
			transform = GenerateRoom.Instance.transform;
		}
		if (resourceCustomTargetPrefab == null)
		{
			resourceCustomTargetPrefab = Resources.Load("Prefabs/Resources/ResourceCustomTarget") as GameObject;
		}
		GameObject gameObject = ObjectPool.Spawn(resourceCustomTargetPrefab, transform);
		gameObject.transform.localPosition = transform.position;
		gameObject.transform.position = position;
		gameObject.transform.eulerAngles = Vector3.zero;
		ResourceCustomTarget component = gameObject.GetComponent<ResourceCustomTarget>();
		component.createdObject = gameObject;
		component.Init(Target, Item, Callback);
		component.UseDeltaTime = UseDeltaTime;
		gameObject = BiomeConstants.Instance.HitFX_Blocked.Spawn();
		gameObject.transform.parent = transform;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.position = position;
		component.init = true;
	}

	public static void Create(GameObject Target, Vector3 position, Sprite sprite, Action Callback)
	{
		Transform transform = null;
		if (RoomManager.Instance != null && RoomManager.Instance.CurrentRoomPrefab.transform != null)
		{
			transform = RoomManager.Instance.CurrentRoomPrefab.transform;
		}
		else if (GameObject.FindGameObjectWithTag("Unit Layer") != null)
		{
			transform = GameObject.FindGameObjectWithTag("Unit Layer").transform;
		}
		else if (GenerateRoom.Instance != null)
		{
			transform = GenerateRoom.Instance.transform;
		}
		if (resourceCustomTargetPrefab == null)
		{
			resourceCustomTargetPrefab = Resources.Load("Prefabs/Resources/ResourceCustomTarget") as GameObject;
		}
		GameObject gameObject = ObjectPool.Spawn(resourceCustomTargetPrefab, transform);
		gameObject.transform.localPosition = transform.position;
		gameObject.transform.position = position;
		gameObject.transform.eulerAngles = Vector3.zero;
		ResourceCustomTarget component = gameObject.GetComponent<ResourceCustomTarget>();
		component.createdObject = gameObject;
		component.Init(Target, sprite, Callback);
		gameObject = BiomeConstants.Instance.HitFX_Blocked.Spawn();
		gameObject.transform.parent = transform;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.position = position;
		component.init = true;
	}

	private void Update()
	{
		if (init)
		{
			lifetime += DeltaTime;
			if (Target != null)
			{
				TargetPosition = Target.transform.position;
				ResourceTargetDistance = Vector2.Distance(ResourceTransform.position, Target.transform.position);
			}
			else
			{
				ResourceTargetDistance = Vector2.Distance(ResourceTransform.position, TargetPosition);
			}
			MoveToTarget(TargetPosition);
			if (ResourceTargetDistance < 1f && lifetime > 0.33f)
			{
				CollectMe();
			}
		}
	}

	private void MoveToTarget(Vector3 targetPosition)
	{
		if (Speed > 0f)
		{
			ZPosition = inventoryItemDisplay.gameObject.transform.position;
			ZPosition.z = Mathf.Lerp(ZPosition.z, targetPosition.z - 0.5f, 10f * DeltaTime);
			inventoryItemDisplay.gameObject.transform.position = ZPosition;
		}
		if ((AngleTimer += DeltaTime) > 0.5f)
		{
			TurnSpeed = Mathf.Lerp(TurnSpeed, 1f, 20f * DeltaTime);
		}
		TargetAngle = Utils.GetAngle(ResourceTransform.position, targetPosition);
		Angle += Mathf.Atan2(Mathf.Sin((TargetAngle - Angle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - Angle) * ((float)Math.PI / 180f))) * 57.29578f / TurnSpeed * DeltaTime * 60f;
		if (Speed < 15f)
		{
			Speed += 12f;
		}
		Speed *= DeltaTime;
		float x = Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f));
		float y = Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f));
		Vector3 vector = new Vector3(x, y);
		ResourceTransform.position += vector;
	}

	public void CollectMe()
	{
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", base.gameObject.transform.position);
		GameObject obj = BiomeConstants.Instance.HitFX_Blocked.Spawn();
		obj.transform.position = inventoryItemDisplay.gameObject.transform.position;
		obj.transform.rotation = Quaternion.identity;
		CameraManager.shakeCamera(0.2f, Angle);
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
		ObjectPool.Recycle(createdObject);
	}

	public void OnRecycled()
	{
		init = false;
		Speed = -7f;
		AngleTimer = 0f;
		TurnSpeed = 7f;
		Angle = 0f;
		TargetAngle = 0f;
		Target = null;
	}
}
