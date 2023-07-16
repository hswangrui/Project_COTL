using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PickUp : BaseMonoBehaviour
{
	public delegate void PickUpEvent(PickUp pickUp);

	private const float STOP_SEPERATION_DELAY = 2f;

	private const float START_SEPERATION_DELAY = 0.3f;

	public InventoryItem.ITEM_TYPE type;

	public int quantity = 1;

	public AudioClip PickUpClip;

	private float Scale;

	private float ScaleSpeed;

	private Vector2 SquishScale = Vector2.one;

	private Vector2 SquishScaleSpeed = Vector2.zero;

	private float FacingAngle = -1f;

	public float Speed;

	private float vx;

	private float vy;

	private Collider2D m_Collider;

	private float Timer;

	public bool PlayerDeath;

	public bool MagnetToPlayer = true;

	public bool CanBePickedUp = true;

	public GameObject child;

	private float vz;

	private float childZ;

	public bool CanStopFollowingPlayer = true;

	public bool CanBeStolenByCritter = true;

	public UnityEvent Callback;

	public bool Bounce = true;

	public bool EmitPickUpFX = true;

	[HideInInspector]
	public GameObject Reserved;

	public static List<PickUp> PickUps = new List<PickUp>();

	public bool DestroysAfterDelay = true;

	private float destroyTimer;

	private const float destroyDelay = 60f;

	public bool DisableSeperation;

	private float seperationTimer;

	private float initialSeperationTimer = 0.3f;

	public bool WasMovedByOther;

	public InventoryItemDisplay inventoryItemDisplay;

	private bool Collected;

	public bool AddToChestIfNotCollected;

	public float MagnetDistance = 7f;

	public Vector3 Seperator = Vector3.zero;

	private float SeperationRadius = 0.5f;

	private bool isSeperated;

	private bool SetInitialSpeed;

	public bool AddToInventory = true;

	public bool StolenByCritter { get; private set; }

	public bool Activated { get; private set; }

	public GameObject Player { get; set; }

	public event PickUpEvent OnPickedUp;

	private void Start()
	{
		if (base.transform.position.z == 0f)
		{
			Scale = 0f;
			childZ = -0.5f;
		}
		else
		{
			Scale = 0.8f;
			childZ = base.transform.position.z;
			Vector3 position = base.transform.position;
			position.z = 0f;
			base.transform.position = position;
		}
		vz = UnityEngine.Random.Range(-0.3f, -0.15f);
		FacingAngle = ((FacingAngle == -1f) ? ((float)UnityEngine.Random.Range(0, 360)) : FacingAngle);
		m_Collider = GetComponent<Collider2D>();
	}

	public void SetInitialSpeedAndDiraction(float Speed, float FacingAngle)
	{
		this.Speed = Speed;
		this.FacingAngle = FacingAngle;
	}

	public void SetInitialFacing(float FacingAngle)
	{
		this.FacingAngle = FacingAngle;
	}

	public void SetImage(InventoryItem.ITEM_TYPE Item)
	{
		if (inventoryItemDisplay != null)
		{
			inventoryItemDisplay.SetImage(Item);
		}
	}

	private void OnEnable()
	{
		MagnetDistance = 7f;
		StolenByCritter = false;
		PickUps.Add(this);
		Timer = 0f;
		SquishScale = Vector2.one;
		SquishScaleSpeed = Vector2.zero;
		Activated = false;
		childZ = 0f;
		destroyTimer = 0f;
		seperationTimer = 0f;
		initialSeperationTimer = 0.3f;
		WasMovedByOther = false;
		child.transform.localScale = Vector3.zero;
		Speed = 0f;
		if (Collected)
		{
			if (base.transform.position.z == 0f)
			{
				Scale = 0f;
				childZ = -0.5f;
			}
			else
			{
				Scale = 0.8f;
				childZ = base.transform.position.z;
				Vector3 position = base.transform.position;
				position.z = 0f;
				base.transform.position = position;
			}
			vz = UnityEngine.Random.Range(-0.3f, -0.15f);
			Speed = UnityEngine.Random.Range(5f, 10f);
			FacingAngle = ((FacingAngle == -1f) ? ((float)UnityEngine.Random.Range(0, 360)) : FacingAngle);
			Collected = false;
		}
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Combine(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(OnChestRevealed));
	}

	private void OnDisable()
	{
		PickUps.Remove(this);
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(OnChestRevealed));
	}

	private void OnChestRevealed()
	{
		if (!StolenByCritter)
		{
			MagnetDistance = 100f;
		}
	}

	private void OnRecycle()
	{
		if (AddToChestIfNotCollected && !Collected && PlayerFarming.Location == FollowerLocation.Base && ObjectPool.IsSpawned(base.gameObject))
		{
			List<Structures_CollectedResourceChest> allStructuresOfType = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(PlayerFarming.Location);
			if (allStructuresOfType.Count > 0)
			{
				allStructuresOfType[0].AddItem(type, 1);
			}
		}
	}

	private void OnDestroy()
	{
	}

	public static bool UnreservedExists(InventoryItem.ITEM_TYPE type)
	{
		foreach (PickUp pickUp in PickUps)
		{
			if (pickUp.type == type && pickUp.Reserved == null)
			{
				return true;
			}
		}
		return false;
	}

	public static PickUp UnreservedAnyExists()
	{
		foreach (PickUp pickUp in PickUps)
		{
			if (pickUp.Reserved == null)
			{
				return pickUp;
			}
		}
		return null;
	}

	public static List<PickUp> UnreservedAnyExistsAll()
	{
		List<PickUp> list = new List<PickUp>();
		foreach (PickUp pickUp in PickUps)
		{
			if (pickUp.Reserved == null)
			{
				list.Add(pickUp);
			}
		}
		return list;
	}

	private void Update()
	{
		initialSeperationTimer -= Time.deltaTime;
		if (Timer < 3f && Bounce)
		{
			BounceChild();
		}
		if (PlayerFarming.Location == FollowerLocation.Base)
		{
			destroyTimer += Time.deltaTime;
			if (destroyTimer >= 60f && DestroysAfterDelay)
			{
				OnRecycle();
				base.gameObject.Recycle();
			}
		}
	}

	private void FixedUpdate()
	{
		if (Timer < 3f)
		{
			if (!Bounce)
			{
				Scale += (1f - Scale) / 7f;
				SquishScaleSpeed.x += (1f - SquishScale.x) * 0.2f;
				SquishScale.x += (SquishScaleSpeed.x *= 0.9f);
				SquishScaleSpeed.y += (1f - SquishScale.y) * 0.2f;
				SquishScale.y += (SquishScaleSpeed.y *= 0.9f);
				if (Time.timeScale > 0f)
				{
					child.transform.localScale = new Vector3(Scale * SquishScale.x, Scale * SquishScale.y, Scale);
				}
			}
			else
			{
				Scale += (1f - Scale) / 7f;
				SquishScaleSpeed.x += (1f - SquishScale.x) * 0.2f;
				SquishScale.x += (SquishScaleSpeed.x *= 0.9f);
				SquishScaleSpeed.y += (1f - SquishScale.y) * 0.2f;
				SquishScale.y += (SquishScaleSpeed.y *= 0.9f);
				if (Time.timeScale > 0f)
				{
					child.transform.localScale = new Vector3(Scale * SquishScale.x, Scale * SquishScale.y, Scale);
				}
			}
		}
		if (PlayerDeath)
		{
			Speed += (0f - Speed) / 12f / (Time.fixedUnscaledDeltaTime * 60f);
			vx = Speed * Mathf.Cos(FacingAngle * ((float)Math.PI / 180f));
			vy = Speed * Mathf.Sin(FacingAngle * ((float)Math.PI / 180f));
			base.transform.position = base.transform.position + new Vector3(vx, vy) * Time.fixedUnscaledDeltaTime;
			return;
		}
		if (!DisableSeperation)
		{
			CheckSeperation();
		}
		if (initialSeperationTimer < 0f)
		{
			seperationTimer += Time.fixedDeltaTime;
		}
		if ((Timer += Time.fixedDeltaTime) > 0.3f)
		{
			base.gameObject.layer = LayerMask.NameToLayer("Pick Ups");
		}
		if (Timer > 0.7f)
		{
			if (Player == null)
			{
				if (PlayerFarming.Instance != null)
				{
					Player = PlayerFarming.Instance.gameObject;
				}
			}
			else if (MagnetToPlayer)
			{
				if (Activated && !SetInitialSpeed)
				{
					Speed = -5f;
					SetInitialSpeed = true;
				}
				float num = Vector3.Distance(base.transform.position, Player.transform.position);
				if (num < MagnetDistance)
				{
					if (Speed < 45f)
					{
						Speed += 45f * Time.fixedDeltaTime;
					}
					FacingAngle = Utils.GetAngle(base.transform.position, Player.transform.position);
					if (num < 0.5f && CanBePickedUp)
					{
						CameraManager.shakeCamera(0.2f, Utils.GetAngle(base.transform.position, Player.transform.position));
						PickMeUp();
					}
					if (!Activated)
					{
						IgnoreCollision(true);
					}
					Activated = true;
					childZ = Mathf.Lerp(childZ, -0.5f, 5f * Time.fixedDeltaTime);
					child.transform.localPosition = new Vector3(0f, 0f, childZ);
					if (!CanStopFollowingPlayer)
					{
						MagnetDistance = 2.1474836E+09f;
					}
				}
				else
				{
					Activated = false;
					IgnoreCollision(false);
				}
			}
			else if (CanBePickedUp && Vector3.Distance(base.transform.position, Player.transform.position) < 0.5f)
			{
				CameraManager.shakeCamera(0.2f, Utils.GetAngle(base.transform.position, Player.transform.position));
				PickMeUp();
			}
		}
		if (!PlayerDeath)
		{
			if (!Activated)
			{
				Speed += (0f - Speed) / 12f * GameManager.FixedDeltaTime;
			}
			vx = Speed * Mathf.Cos(FacingAngle * ((float)Math.PI / 180f));
			vy = Speed * Mathf.Sin(FacingAngle * ((float)Math.PI / 180f));
			base.transform.position += new Vector3(vx, vy) * Time.fixedDeltaTime + Seperator * Time.fixedDeltaTime;
		}
	}

	private void CheckSeperation()
	{
		Seperator = Vector2.zero;
		for (int i = 0; i < PickUps.Count; i++)
		{
			if (!(PickUps[i] == null) && !(PickUps[i] == this) && !PickUps[i].DisableSeperation)
			{
				float num = MagnitudeFindDistanceBetween(PickUps[i].transform.position, base.transform.position);
				if (num < SeperationRadius)
				{
					float num2 = Time.fixedDeltaTime * 60f;
					float angleR = Utils.GetAngleR(PickUps[i].transform.position, base.transform.position);
					float num3 = (SeperationRadius - num) / 2f;
					Seperator.x += num3 * Mathf.Cos(angleR) * num2;
					Seperator.y += num3 * Mathf.Sin(angleR) * num2;
					PickUps[i].Seperator.x -= num3 * Mathf.Cos(angleR) * num2;
					PickUps[i].Seperator.y -= num3 * Mathf.Sin(angleR) * num2;
				}
			}
		}
	}

	private void CheckSeperationOptimized()
	{
		Seperator = Vector2.zero;
		for (int i = 0; i < PickUps.Count; i++)
		{
			if (!(PickUps[i] == null) && !(PickUps[i] == this) && !PickUps[i].DisableSeperation)
			{
				float num = MagnitudeFindDistanceBetween(PickUps[i].transform.position, base.transform.position);
				if (num < SeperationRadius)
				{
					float num2 = Time.fixedDeltaTime * 60f;
					float angleR = Utils.GetAngleR(PickUps[i].transform.position, base.transform.position);
					float num3 = (SeperationRadius - num) / 2f;
					float num4 = num3 * Mathf.Cos(angleR) * num2 * 2f;
					float num5 = num3 * Mathf.Sin(angleR) * num2 * 2f;
					Seperator.x += num4;
					Seperator.y += num5;
					PickUps[i].Seperator.x -= num4;
					PickUps[i].Seperator.y -= num5;
					PickUps[i].WasMovedByOther = true;
				}
			}
		}
	}

	private void LateUpdate()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(base.transform.position + Vector3.back * 3f, Vector3.forward, out hitInfo, float.PositiveInfinity))
		{
			if (hitInfo.collider.gameObject.GetComponent<MeshCollider>() != null)
			{
				Vector3 position = base.transform.position;
				position.z = hitInfo.point.z;
				base.transform.position = position;
			}
		}
		else
		{
			Vector3 position2 = base.transform.position;
			position2.z = 0f;
			base.transform.position = position2;
		}
	}

	private void BounceChild()
	{
		if (childZ >= 0f)
		{
			if (vz > 0.08f)
			{
				vz *= -0.4f;
				SquishScale = new Vector2(0.8f, 1.2f);
			}
			else
			{
				vz = 0f;
			}
			childZ = 0f;
		}
		else if (!Activated)
		{
			vz += 0.02f * Time.deltaTime * 60f;
		}
		childZ += vz * Time.deltaTime * 60f;
		child.transform.localPosition = new Vector3(0f, 0f, childZ);
	}

	public void PickMeUpSimpleInventory(GameObject playerGO)
	{
		if (!Collected)
		{
			PlayerSimpleInventory simpleInventory = playerGO.GetComponent<PlayerFarming>().simpleInventory;
			simpleInventory.DropItem();
			simpleInventory.GiveItem(type);
			Collected = true;
			FacingAngle = -1f;
			StartCoroutine(FrameDelay(delegate
			{
				base.gameObject.Recycle();
			}));
		}
	}

	public virtual void PickMeUp()
	{
		if (Collected)
		{
			return;
		}
		AudioManager.Instance.PlayOneShot("event:/player/collect_black_soul", base.gameObject);
		if (EmitPickUpFX)
		{
			BiomeConstants.Instance.EmitPickUpVFX(child.transform.position);
			string[] array = new string[2] { "BloodImpact_Small_0", "BloodImpact_Small_1" };
			int num = UnityEngine.Random.Range(0, array.Length - 1);
			if (array[num] != null)
			{
				BiomeConstants.Instance.EmitBloodImpact(child.transform.position, Quaternion.identity.x, "black", array[num]);
			}
		}
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
		if (AddToInventory)
		{
			Inventory.AddItem((int)type, 1);
		}
		PickedUp();
		Collected = true;
		FacingAngle = -1f;
		StartCoroutine(FrameDelay(delegate
		{
			base.gameObject.Recycle();
		}));
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	public void PickedUp()
	{
		PickUpEvent onPickedUp = this.OnPickedUp;
		if (onPickedUp != null)
		{
			onPickedUp(this);
		}
	}

	public void IgnoreCollision(bool Toggle)
	{
	}

	public void TargetedByCritter()
	{
		StolenByCritter = true;
		MagnetDistance = 7f;
	}

	private float MagnitudeFindDistanceBetween(Vector2 a, Vector2 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		return num * num + num2 * num2;
	}
}
