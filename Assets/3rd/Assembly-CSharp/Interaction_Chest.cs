using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Interaction_Chest : Interaction
{
	public enum ChestType
	{
		None,
		Wooden,
		Silver,
		Gold
	}

	public enum State
	{
		Hidden,
		Closed,
		Open
	}

	public delegate void ChestEvent();

	public ChestType TypeOfChest;

	public GameObject CameraBone;

	public ColliderEvents DamageCollider;

	public static Interaction_Chest Instance;

	public bool DropFullHeal;

	public GameObject Shadow;

	public GameObject Lighting;

	public bool RevealOnDistance;

	public bool StartRevealed;

	public List<Health> Enemies = new List<Health>();

	public InventoryItemDisplay Item;

	public GameObject PlayerPosition;

	public SkeletonAnimation Spine;

	[SpineEvent("", "", true, false, false)]
	public string ChestLand = "shake";

	private string sLabel;

	private float Timer;

	public StealthCover[] StealthCovers;

	public GameObject uIBlueprint;

	public GameObject uINewCard;

	public List<RewardsItem> ChestRewards = new List<RewardsItem>();

	public RewardsItem.ChestRewards OverrideChestReward;

	public int OverrideChestRewardQuantity = 1;

	public bool ForceGoodReward;

	public bool BlockWeapons;

	public bool BossChest;

	public bool BigBossChest;

	private float delayTimestamp = -1f;

	private bool active;

	public static ChestEvent OnChestRevealed;

	[Space]
	[SerializeField]
	private string spiderCritter;

	private int SilverChestMaxRandom = 12;

	private int GoldChestMaxRandom = 10;

	private Health EnemyHealth;

	private bool givenReward;

	private float CacheCameraMaxZoom;

	private float CacheCameraMinZoom;

	private float CacheCameraZoomLimiter;

	public bool RevealedGiveReward;

	private FoundItemPickUp p;

	private float coinMultiplier = 1f;

	private InventoryItem.ITEM_TYPE previousPick;

	private InventoryItem.ITEM_TYPE FirstGoldReward;

	public GameObject WeaponPodiumPrefab;

	public int DeathCount;

	private bool InCombat;

	private InventoryItem.ITEM_TYPE Reward;

	private HealthPlayer healthPlayer;

	public static int RevealCount = -1;

	private float randomReward;

	private RewardsItem.ChestRewards pickedReward;

	private float previousChance;

	private bool Loop;

	private TarotCards.TarotCard DrawnCard;

	private float totalProbability;

	private float previousTotal;

	private float multiplyer;

	private float previousRewardChance;

	public State MyState { get; private set; }

	public float Delay { get; set; }

	private void SetTypeOfChest()
	{
		if (TypeOfChest == ChestType.None)
		{
			if (!BossChest)
			{
				if (TrinketManager.HasTrinket(TarotCards.Card.RabbitFoot))
				{
					SilverChestMaxRandom = 7;
					GoldChestMaxRandom = 10;
				}
				else
				{
					SilverChestMaxRandom = 15;
					GoldChestMaxRandom = 30;
				}
				if (DungeonSandboxManager.Active)
				{
					SilverChestMaxRandom = Mathf.RoundToInt(SilverChestMaxRandom * 3);
					GoldChestMaxRandom = Mathf.RoundToInt(GoldChestMaxRandom * 3);
				}
				if (UnityEngine.Random.Range(0, SilverChestMaxRandom) == 5 && DataManager.Instance.dungeonRun > 2)
				{
					TypeOfChest = ChestType.Silver;
				}
				else if (UnityEngine.Random.Range(0, GoldChestMaxRandom) == 5 && DataManager.Instance.dungeonRun > 2)
				{
					TypeOfChest = ChestType.Gold;
				}
				else
				{
					TypeOfChest = ChestType.Wooden;
				}
			}
			else
			{
				TypeOfChest = ChestType.Gold;
			}
		}
		switch (TypeOfChest)
		{
		case ChestType.Gold:
			Spine.skeleton.SetSkin("Gold");
			break;
		case ChestType.Silver:
			Spine.skeleton.SetSkin("Silver");
			break;
		case ChestType.Wooden:
			Spine.skeleton.SetSkin("Wooden");
			break;
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (!active)
		{
			if (!BossChest)
			{
				StartCoroutine(DelayGetEnemies());
			}
			DamageCollider.OnTriggerEnterEvent += OnDamageTriggerEnter;
			DamageCollider.SetActive(false);
		}
		Instance = this;
		StartCoroutine(EnableInteractionDelay());
		active = true;
	}

	private IEnumerator EnableInteractionDelay()
	{
		yield return new WaitForEndOfFrame();
		if (Spine != null)
		{
			Spine.AnimationState.Event += HandleEvent;
		}
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && EnemyHealth.team != Health.Team.PlayerTeam)
		{
			EnemyHealth.DealDamage(2.1474836E+09f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
		TrapSpikes component = GetComponent<TrapSpikes>();
		if (component != null)
		{
			component.DestroySpikes();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (Spine != null)
		{
			Spine.AnimationState.Event -= HandleEvent;
		}
		BiomeGenerator.OnRoomActive -= OnRoomActivate;
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Start()
	{
		RevealedGiveReward = false;
		Enemies.Clear();
		Item.gameObject.SetActive(false);
		UpdateLocalisation();
		if (MyState == State.Hidden)
		{
			Spine.AnimationState.SetAnimation(0, "hidden", true);
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, "closed", true);
		}
		SetTypeOfChest();
		Shadow.SetActive(false);
		Lighting.SetActive(false);
		if (base.transform.parent != null)
		{
			StealthCovers = base.transform.parent.GetComponentsInChildren<StealthCover>();
		}
		if (RevealOnDistance)
		{
			AutomaticallyInteract = true;
			ActivateDistance = 3f;
			Spine.AnimationState.SetAnimation(0, "reveal", true);
			RevealSfx();
			Spine.AnimationState.AddAnimation(0, "closed", true, 0f);
			MyState = State.Closed;
			StartCoroutine(ShowShadow());
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (StartRevealed && !givenReward)
		{
			StartCoroutine(DelayReveal());
		}
	}

	private IEnumerator DelayReveal()
	{
		yield return new WaitForEndOfFrame();
		if (base.gameObject.activeInHierarchy)
		{
			givenReward = true;
			Reveal();
		}
	}

	private IEnumerator DelayGetEnemies()
	{
		yield return new WaitForEndOfFrame();
		GetEnemies();
	}

	public void GetEnemies()
	{
		foreach (Health item in new List<Health>(base.transform.parent.GetComponentsInChildren<Health>()))
		{
			if (item.team == Health.Team.Team2 && !item.InanimateObject)
			{
				Enemies.Add(item);
				item.OnDie += OnSpawnedDie;
			}
		}
		BiomeGenerator.OnRoomActive += OnRoomActivate;
	}

	private void OnRoomActivate()
	{
		if (Enemies.Count > 0)
		{
			CacheCameraMaxZoom = GameManager.GetInstance().CamFollowTarget.MaxZoom;
			CacheCameraMinZoom = GameManager.GetInstance().CamFollowTarget.MinZoom;
			CacheCameraZoomLimiter = GameManager.GetInstance().CamFollowTarget.ZoomLimiter;
			GameManager.GetInstance().CamFollowTarget.MaxZoom = 13f;
			GameManager.GetInstance().CamFollowTarget.MinZoom = 11f;
			GameManager.GetInstance().CamFollowTarget.ZoomLimiter = 5f;
			GameManager.GetInstance().AddToCamera(base.gameObject);
		}
	}

	public void AddEnemy(Health h)
	{
		Enemies.Add(h);
		if ((bool)h)
		{
			h.OnDie += OnSpawnedDie;
		}
	}

	private void QueryRoomWeapon()
	{
		Debug.Log("BiomeGenerator.Instance.CurrentRoom.HasWeapon: " + BiomeGenerator.Instance.CurrentRoom.HasWeapon);
	}

	public void Reveal()
	{
		if (!DataManager.Instance.ShownInventoryTutorial)
		{
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
			PlayerFarming.Instance._state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.gameObject.transform.position);
			CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
			cameraFollowTarget.SetOffset((base.gameObject.transform.position - PlayerFarming.Instance.transform.position) * 0.85f);
			HUD_Manager.Instance.Hide(false, 0);
			StartCoroutine(DelayCallback(2f, delegate
			{
				cameraFollowTarget.SetOffset(Vector3.zero);
			}));
		}
		Debug.Log("Reveal()");
		RevealedGiveReward = true;
		StartCoroutine(DamageColliderRoutine());
		if (!BlockWeapons && BiomeGenerator.Instance != null && BiomeGenerator.Instance.CurrentRoom.HasWeapon)
		{
			Debug.Log("CHEST: Weapon!".Colour(Color.red));
			if (DataManager.Instance.WeaponPool.Count > 2 && DataManager.Instance.CursePool.Count > 2)
			{
				UnityEngine.Object.Instantiate(WeaponPodiumPrefab, base.transform.position - new Vector3(-1f, 0f), Quaternion.identity, base.transform.parent);
				UnityEngine.Object.Instantiate(WeaponPodiumPrefab, base.transform.position - new Vector3(1f, 0f), Quaternion.identity, base.transform.parent);
			}
			else
			{
				UnityEngine.Object.Instantiate(WeaponPodiumPrefab, base.transform.position - new Vector3(0f, 0f), Quaternion.identity, base.transform.parent);
			}
			ChestEvent onChestRevealed = OnChestRevealed;
			if (onChestRevealed != null)
			{
				onChestRevealed();
			}
			MyState = State.Open;
			return;
		}
		Debug.Log("CHEST: No weapon".Colour(Color.red));
		float num = 0.04f * TrinketManager.GetChanceForRelicsMultiplier();
		if (DataManager.Instance.CurrentRelic == RelicType.None)
		{
			num += 0.04f;
		}
		if (!BiomeGenerator.Instance.HasSpawnedRelic && UnityEngine.Random.value < num && DataManager.Instance.OnboardedRelics)
		{
			UnityEngine.Object.Instantiate(WeaponPodiumPrefab, base.transform.position - new Vector3(0f, 0f), Quaternion.identity, base.transform.parent).GetComponent<Interaction_WeaponChoiceChest>().Type = Interaction_WeaponSelectionPodium.Types.Relic;
			BiomeGenerator.Instance.HasSpawnedRelic = true;
			ChestEvent onChestRevealed2 = OnChestRevealed;
			if (onChestRevealed2 != null)
			{
				onChestRevealed2();
			}
			MyState = State.Open;
		}
		else
		{
			Spine.AnimationState.SetAnimation(0, "reveal", true);
			RevealSfx();
			Spine.AnimationState.AddAnimation(0, "closed", true, 0f);
			MyState = State.Closed;
			StartCoroutine(ShowShadow());
			SelectReward();
			MyState = State.Open;
			GameManager.GetInstance().StartCoroutine(GiveRewardDelay());
			Projectile.ClearProjectiles();
		}
	}

	private IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator ShowShadow()
	{
		Shadow.SetActive(true);
		GameObject lighting = Lighting;
		if ((object)lighting != null)
		{
			lighting.SetActive(true);
		}
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Shadow.transform.localScale = new Vector3(3f, 2f, 1f) * (Progress / Duration);
			yield return null;
		}
		Shadow.transform.localScale = new Vector3(3f, 2f, 1f);
	}

	public void RevealBossReward(InventoryItem.ITEM_TYPE ForceItem, Action callback = null)
	{
		StartCoroutine(DamageColliderRoutine());
		StartCoroutine(GiveBossReward(ForceItem, callback));
	}

	private void RevealSfx()
	{
		switch (TypeOfChest)
		{
		case ChestType.Wooden:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_appear", base.gameObject);
			break;
		case ChestType.Gold:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_appear", base.gameObject);
			break;
		case ChestType.Silver:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_appear", base.gameObject);
			break;
		default:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_appear", base.gameObject);
			break;
		}
	}

	private void ChestLandSfx()
	{
		switch (TypeOfChest)
		{
		case ChestType.Wooden:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_land", base.gameObject);
			break;
		case ChestType.Gold:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_land", base.gameObject);
			break;
		case ChestType.Silver:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_land", base.gameObject);
			break;
		default:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_land", base.gameObject);
			break;
		}
	}

	private void ChestOpenSfx()
	{
		switch (TypeOfChest)
		{
		case ChestType.Wooden:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_open", base.gameObject);
			break;
		case ChestType.Gold:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_open", base.gameObject);
			break;
		case ChestType.Silver:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_big_open", base.gameObject);
			break;
		default:
			AudioManager.Instance.PlayOneShot("event:/chests/chest_small_open", base.gameObject);
			break;
		}
	}

	private IEnumerator GiveBossReward(InventoryItem.ITEM_TYPE ForceIncludeItem, Action callback = null)
	{
		RevealedGiveReward = false;
		if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			yield break;
		}
		StartCoroutine(ShowShadow());
		Spine.AnimationState.SetAnimation(0, "reveal", true);
		RevealSfx();
		Spine.AnimationState.AddAnimation(0, "closed", true, 0f);
		MyState = State.Closed;
		yield return new WaitForSeconds(1.1333333f);
		Spine.AnimationState.SetAnimation(0, "open", false);
		ChestOpenSfx();
		Spine.AnimationState.AddAnimation(0, "opened", true, 0f);
		StartCoroutine(GiveBlackSouls());
		if (DropFullHeal)
		{
			yield return StartCoroutine(GiveFullHeal());
		}
		Debug.Log("chest ForceIncludeItem " + ForceIncludeItem);
		if (ForceIncludeItem != 0)
		{
			yield return new WaitForSeconds(0.2f);
			InventoryItem.Spawn(ForceIncludeItem, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(5f, 250f);
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
			Debug.Log("SPAWNED FORCED ITEM!");
		}
		yield return new WaitForSeconds(0.2f);
		List<PickUp> itemsRequired = new List<PickUp>();
		int Rewards2;
		if (!BigBossChest)
		{
			coinMultiplier = 1f;
			Rewards2 = UnityEngine.Random.Range(10, 15) * (int)DungeonModifier.HasPositiveModifier(DungeonPositiveModifier.DoubleGold, 2f, 1f);
			Rewards2 = (int)((float)Rewards2 * coinMultiplier);
			if (DoctrineUpgradeSystem.TrySermonsStillAvailable() && DoctrineUpgradeSystem.TryGetStillDoctrineStone() && DataManager.Instance.GetVariable(DataManager.Variables.FirstDoctrineStone))
			{
				PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.DOCTRINE_STONE, 1, base.transform.position);
				if (pickUp != null)
				{
					pickUp.SetInitialSpeedAndDiraction(5f, 250f);
					AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
					Interaction_DoctrineStone component = pickUp.GetComponent<Interaction_DoctrineStone>();
					if (component != null)
					{
						component.AutomaticallyInteract = true;
					}
				}
			}
		}
		else
		{
			List<RewardsItem.ChestRewards> list = new List<RewardsItem.ChestRewards>();
			if (DataManager.CheckIfThereAreSkinsAvailable())
			{
				list.Add(RewardsItem.ChestRewards.FOLLOWER_SKIN);
			}
			if (DataManager.GetDecorationsAvailableCategory(PlayerFarming.Location))
			{
				list.Add(RewardsItem.ChestRewards.BASE_DECORATION);
			}
			if (list.Count > 0 && ForceIncludeItem != InventoryItem.ITEM_TYPE.GOD_TEAR)
			{
				Reward = RewardsItem.Instance.ReturnItemType(list[UnityEngine.Random.Range(0, list.Count)]);
				PickUp pickUp2 = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
				if (pickUp2 != null)
				{
					p = pickUp2.GetComponent<FoundItemPickUp>();
				}
				if (p != null && pickUp2 != null)
				{
					p.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				}
				if (pickUp2 != null)
				{
					FoundItemPickUp component2 = pickUp2.GetComponent<FoundItemPickUp>();
					if (component2 != null)
					{
						component2.AutomaticallyInteract = true;
					}
				}
			}
			if (!DungeonSandboxManager.Active)
			{
				switch (PlayerFarming.Location)
				{
				case FollowerLocation.Dungeon1_1:
					if (!StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1))
					{
						p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION, 1, base.transform.position).GetComponent<FoundItemPickUp>();
						AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
						p.DecorationType = StructureBrain.TYPES.DECORATION_BOSS_TROPHY_1;
						p.GetComponent<PickUp>().DisableSeperation = true;
						FoundItemPickUp component5 = p.GetComponent<FoundItemPickUp>();
						if (component5 != null)
						{
							component5.AutomaticallyInteract = true;
						}
					}
					else if (ForceIncludeItem == InventoryItem.ITEM_TYPE.GOD_TEAR)
					{
						for (int m = 0; m < 2; m++)
						{
							itemsRequired.Add(InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GOD_TEAR, 1, base.transform.position));
						}
					}
					break;
				case FollowerLocation.Dungeon1_2:
					if (!StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2))
					{
						p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION, 1, base.transform.position).GetComponent<FoundItemPickUp>();
						AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
						p.DecorationType = StructureBrain.TYPES.DECORATION_BOSS_TROPHY_2;
						p.GetComponent<PickUp>().DisableSeperation = true;
						FoundItemPickUp component4 = p.GetComponent<FoundItemPickUp>();
						if (component4 != null)
						{
							component4.AutomaticallyInteract = true;
						}
					}
					else if (ForceIncludeItem == InventoryItem.ITEM_TYPE.GOD_TEAR)
					{
						for (int l = 0; l < 2; l++)
						{
							itemsRequired.Add(InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GOD_TEAR, 1, base.transform.position));
						}
					}
					break;
				case FollowerLocation.Dungeon1_3:
					if (!StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3))
					{
						p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION, 1, base.transform.position).GetComponent<FoundItemPickUp>();
						AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
						p.DecorationType = StructureBrain.TYPES.DECORATION_BOSS_TROPHY_3;
						p.GetComponent<PickUp>().DisableSeperation = true;
						FoundItemPickUp component6 = p.GetComponent<FoundItemPickUp>();
						if (component6 != null)
						{
							component6.AutomaticallyInteract = true;
						}
					}
					else if (ForceIncludeItem == InventoryItem.ITEM_TYPE.GOD_TEAR)
					{
						for (int n = 0; n < 2; n++)
						{
							itemsRequired.Add(InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GOD_TEAR, 1, base.transform.position));
						}
					}
					break;
				case FollowerLocation.Dungeon1_4:
					if (!StructuresData.GetUnlocked(StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4) && !DungeonSandboxManager.Active)
					{
						p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION, 1, base.transform.position).GetComponent<FoundItemPickUp>();
						AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
						p.DecorationType = StructureBrain.TYPES.DECORATION_BOSS_TROPHY_4;
						p.GetComponent<PickUp>().DisableSeperation = true;
						FoundItemPickUp component3 = p.GetComponent<FoundItemPickUp>();
						if (component3 != null)
						{
							component3.AutomaticallyInteract = true;
						}
					}
					else if (ForceIncludeItem == InventoryItem.ITEM_TYPE.GOD_TEAR)
					{
						for (int k = 0; k < 2; k++)
						{
							itemsRequired.Add(InventoryItem.Spawn(InventoryItem.ITEM_TYPE.GOD_TEAR, 1, base.transform.position));
						}
					}
					break;
				}
			}
			if (p != null)
			{
				p.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			Rewards2 = UnityEngine.Random.Range(25, 50) * (int)DungeonModifier.HasPositiveModifier(DungeonPositiveModifier.DoubleGold, 2f, 1f);
			if (DoctrineUpgradeSystem.TrySermonsStillAvailable() && DoctrineUpgradeSystem.TryGetStillDoctrineStone() && DataManager.Instance.GetVariable(DataManager.Variables.FirstDoctrineStone))
			{
				ForceIncludeItem = InventoryItem.ITEM_TYPE.DOCTRINE_STONE;
				PickUp pickUp2 = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.DOCTRINE_STONE, 1, base.transform.position);
				if (pickUp2 != null)
				{
					pickUp2.SetInitialSpeedAndDiraction(5f, 250f);
					AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
					Interaction_DoctrineStone component7 = pickUp2.GetComponent<Interaction_DoctrineStone>();
					if (component7 != null)
					{
						component7.MagnetToPlayer();
					}
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		int j = -1;
		while (true)
		{
			int num = j + 1;
			j = num;
			if (num > Rewards2)
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
			CameraManager.shakeCamera(UnityEngine.Random.Range(0.4f, 0.6f));
			PickUp pickUp3 = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f);
			pickUp3.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			pickUp3.MagnetDistance = 3f;
			pickUp3.CanStopFollowingPlayer = false;
			yield return new WaitForSeconds(0.01f);
		}
		if (DataManager.Instance.BonesEnabled && !DataManager.Instance.DeathCatBeaten)
		{
			j = -1;
			int Bones = UnityEngine.Random.Range(10, 15);
			while (true)
			{
				int num = j + 1;
				j = num;
				if (num > Bones)
				{
					break;
				}
				AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
				CameraManager.shakeCamera(UnityEngine.Random.Range(0.4f, 0.6f));
				PickUp pickUp4 = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, 1, base.transform.position + Vector3.back, 0f);
				if (pickUp4 != null)
				{
					pickUp4.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
					pickUp4.MagnetDistance = 3f;
					pickUp4.CanStopFollowingPlayer = false;
				}
				yield return new WaitForSeconds(0.01f);
			}
		}
		yield return new WaitForSeconds(0.5f);
		Reward = InventoryItem.ITEM_TYPE.BLACK_GOLD;
		if (Reward == InventoryItem.ITEM_TYPE.BLACK_GOLD)
		{
			for (int num2 = 0; num2 < 10; num2++)
			{
				PickUp pickUp5 = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
				pickUp5.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				pickUp5.MagnetDistance = 3f;
				pickUp5.CanStopFollowingPlayer = false;
			}
		}
		else
		{
			PickUp pickUp6 = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
			pickUp6.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			pickUp6.MagnetDistance = 3f;
			pickUp6.CanStopFollowingPlayer = false;
		}
		yield return new WaitForSeconds(2f);
		if (ForceIncludeItem == InventoryItem.ITEM_TYPE.KEY_PIECE)
		{
			while (Interaction_KeyPiece.Instance != null)
			{
				yield return null;
			}
		}
		if (ForceIncludeItem == InventoryItem.ITEM_TYPE.BEHOLDER_EYE)
		{
			while (BeholderEye.Instance != null)
			{
				yield return null;
			}
		}
		if (ForceIncludeItem == InventoryItem.ITEM_TYPE.DOCTRINE_STONE)
		{
			while (Interaction_DoctrineStone.DoctrineStones.Count > 0)
			{
				yield return null;
			}
		}
		while (true)
		{
			int num3 = 0;
			foreach (PickUp item in itemsRequired)
			{
				if (item == null)
				{
					num3++;
				}
			}
			if (num3 >= itemsRequired.Count)
			{
				break;
			}
			yield return null;
		}
		while (p != null)
		{
			yield return null;
		}
		while (Interaction_DivineCrystal.Instance != null)
		{
			yield return null;
		}
		RoomLockController.RoomCompleted(true);
		RevealedGiveReward = true;
		if (callback != null)
		{
			callback();
		}
	}

	private InventoryItem.ITEM_TYPE GetRandomFoodItem()
	{
		if (PlayerFarming.Location == FollowerLocation.Dungeon1_1)
		{
			return CookingData.GetLowQualityFoods()[UnityEngine.Random.Range(0, CookingData.GetLowQualityFoods().Length)];
		}
		return CookingData.GetMediumQualityFoods()[UnityEngine.Random.Range(0, CookingData.GetMediumQualityFoods().Length)];
	}

	private IEnumerator GiveFullHeal()
	{
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if ((float)num < PlayerFarming.Instance.health.totalHP / 2f)
			{
				AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
				CameraManager.shakeCamera(UnityEngine.Random.Range(0.4f, 0.6f));
				PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.RED_HEART, 1, base.transform.position + Vector3.back, 0f);
				pickUp.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
				pickUp.MagnetDistance = 2f;
				pickUp.CanStopFollowingPlayer = false;
				yield return new WaitForSeconds(0.1f);
				continue;
			}
			break;
		}
	}

	private IEnumerator GiveBlackSouls()
	{
		int Rewards = 25;
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num < Rewards)
			{
				AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
				CameraManager.shakeCamera(UnityEngine.Random.Range(0.4f, 0.6f));
				BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(1, base.transform.position + Vector3.back);
				if (blackSoul != null)
				{
					blackSoul.SetAngle(270 + UnityEngine.Random.Range(-90, 90), new Vector2(2f, 4f));
				}
				yield return new WaitForSeconds(0.05f);
				continue;
			}
			break;
		}
	}

	private void HandleEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == ChestLand)
		{
			ChestLandSfx();
			CameraManager.shakeCamera(0.5f);
			if (Spine != null)
			{
				Spine.AnimationState.Event -= HandleEvent;
			}
		}
	}

	private IEnumerator DamageColliderRoutine()
	{
		if (DamageCollider != null)
		{
			DamageCollider.SetActive(true);
		}
		yield return new WaitForSeconds(0.1f);
		if (DamageCollider != null)
		{
			DamageCollider.SetActive(false);
		}
	}

	private int ChestCoinMultiplier()
	{
		switch (TypeOfChest)
		{
		case ChestType.Wooden:
			return 0;
		case ChestType.Silver:
			return 2;
		case ChestType.Gold:
			return 5;
		default:
			return 0;
		}
	}

	private void SpawnGoodReward()
	{
		AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
		Reward = RewardsItem.Instance.ReturnItemType(RewardsItem.Instance.GetGoodReward());
		int num = 100;
		while (Reward == previousPick && --num > 0)
		{
			Reward = RewardsItem.Instance.ReturnItemType(RewardsItem.Instance.GetGoodReward());
		}
		previousPick = Reward;
		if (FirstGoldReward != 0)
		{
			FirstGoldReward = Reward;
		}
		else if (InventoryItem.IsGiftOrNecklace(FirstGoldReward) && InventoryItem.IsGiftOrNecklace(Reward))
		{
			Reward = InventoryItem.ITEM_TYPE.BLACK_GOLD;
		}
		PickUp pickUp;
		if (Reward == InventoryItem.ITEM_TYPE.GOLD_NUGGET)
		{
			int num2 = UnityEngine.Random.Range(20, 30);
			for (int i = 0; i < num2; i++)
			{
				pickUp = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
				pickUp.SetInitialSpeedAndDiraction(5f, UnityEngine.Random.Range(150, 350));
			}
			return;
		}
		if (Reward == InventoryItem.ITEM_TYPE.BLACK_GOLD)
		{
			Reward = (DungeonModifier.HasNeutralModifier(DungeonNeutralModifier.ChestsDropFoodNotGold) ? GetRandomFoodItem() : Reward);
		}
		if (Reward == InventoryItem.ITEM_TYPE.BLACK_GOLD)
		{
			for (int j = 0; j < 10; j++)
			{
				pickUp = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
				pickUp.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
			}
			return;
		}
		if (InventoryItem.IsFood(Reward))
		{
			int num3 = UnityEngine.Random.Range(3, 6);
			for (int k = 0; k < num3; k++)
			{
				pickUp = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
				pickUp.SetInitialSpeedAndDiraction(5f, UnityEngine.Random.Range(150, 350));
			}
			return;
		}
		pickUp = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
		if ((bool)pickUp)
		{
			pickUp.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
		}
		FoundItemPickUp component = pickUp.GetComponent<FoundItemPickUp>();
		if (component != null)
		{
			component.AutomaticallyInteract = true;
		}
		Interaction_DoctrineStone component2 = pickUp.GetComponent<Interaction_DoctrineStone>();
		if (component2 != null)
		{
			component2.MagnetToPlayer();
		}
	}

	private void SpawnBlackSouls()
	{
		BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(Mathf.RoundToInt((float)UnityEngine.Random.Range(8, 12) * TrinketManager.GetBlackSoulsMultiplier()), base.transform.position, false, true);
		if ((bool)blackSoul)
		{
			blackSoul.SetAngle(UnityEngine.Random.Range(0, 360), new Vector2(2f, 4f));
		}
	}

	private IEnumerator GiveRewardDelay(float Delay = 1.0666667f)
	{
		yield return new WaitForSeconds(Delay);
		Spine.AnimationState.SetAnimation(0, "open", false);
		ChestOpenSfx();
		Spine.AnimationState.AddAnimation(0, "opened", true, 0f);
		yield return new WaitForSeconds(0.25f);
		RevealedGiveReward = false;
		int Quantity = 1;
		if (Reward == InventoryItem.ITEM_TYPE.GOLD_NUGGET)
		{
			Quantity = UnityEngine.Random.Range(20, 30);
		}
		if (Reward == InventoryItem.ITEM_TYPE.SEED || Reward == InventoryItem.ITEM_TYPE.SEED_PUMPKIN || Reward == InventoryItem.ITEM_TYPE.SEED_BEETROOT || Reward == InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER)
		{
			Quantity = UnityEngine.Random.Range(3, 6);
		}
		if (InventoryItem.IsFood(Reward))
		{
			Quantity = UnityEngine.Random.Range(3, 6);
		}
		if (BiomeGenerator.Instance != null && Reward == InventoryItem.ITEM_TYPE.BLACK_GOLD)
		{
			switch (PlayerFarming.Location)
			{
			case FollowerLocation.Dungeon1_1:
				Quantity = 3;
				break;
			case FollowerLocation.Dungeon1_2:
				Quantity = 3;
				break;
			case FollowerLocation.Dungeon1_3:
				Quantity = 3;
				break;
			case FollowerLocation.Dungeon1_4:
				Quantity = 3;
				break;
			}
			Quantity *= (int)DungeonModifier.HasPositiveModifier(DungeonPositiveModifier.DoubleGold, 2f, 1f);
			Quantity += ChestCoinMultiplier();
		}
		if (OverrideChestReward != 0)
		{
			Quantity = OverrideChestRewardQuantity;
		}
		Reward = (DungeonModifier.HasNeutralModifier(DungeonNeutralModifier.ChestsDropFoodNotGold) ? GetRandomFoodItem() : Reward);
		if (DungeonModifier.HasNeutralModifier(DungeonNeutralModifier.ChestsDropFoodNotGold))
		{
			Quantity = UnityEngine.Random.Range(2, 4);
		}
		if (RewardsItem.Instance.IsBiomeResource(Reward))
		{
			Quantity = UnityEngine.Random.Range(1, 3) * BiomeGenerator.Instance.GoldToGive;
		}
		while (!base.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		if (DungeonSandboxManager.Active && Reward != InventoryItem.ITEM_TYPE.BLACK_GOLD && pickedReward != RewardsItem.ChestRewards.SPIDERS)
		{
			Reward = InventoryItem.ITEM_TYPE.BLACK_GOLD;
		}
		if (Reward != 0)
		{
			for (int i = 0; i < Quantity; i++)
			{
				PickUp pickUp = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
				if (pickUp != null)
				{
					pickUp.SetInitialSpeedAndDiraction(3f, 270 + UnityEngine.Random.Range(-35, 35));
					pickUp.MagnetDistance = 100f;
					pickUp.CanStopFollowingPlayer = false;
				}
			}
		}
		if (UnityEngine.Random.Range(1, 15) == 3)
		{
			SpawnBlackSouls();
		}
		if (pickedReward == RewardsItem.ChestRewards.SPIDERS && TypeOfChest == ChestType.Wooden)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/generic_negative", base.transform.position);
			for (int j = 0; j < 10; j++)
			{
				ObjectPool.Spawn(spiderCritter, base.transform.position, Quaternion.identity, base.transform.parent, delegate(GameObject obj)
				{
					obj.GetComponent<UnitObject>().DoKnockBack(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0.2f, 0.3f), 0.5f);
					obj.name = "Critter";
				});
			}
		}
		else
		{
			if (TypeOfChest == ChestType.Silver)
			{
				SpawnGoodReward();
				SpawnBlackSouls();
			}
			if (TypeOfChest == ChestType.Gold)
			{
				SpawnGoodReward();
				SpawnGoodReward();
			}
			if (healthPlayer != null)
			{
				float num = (healthPlayer.totalHP - healthPlayer.HP) / 10f * DifficultyManager.GetHealthDropsMultiplier() * (PlayerWeapon.FirstTimeUsingWeapon ? 1.2f : 1f);
				float num2 = UnityEngine.Random.Range(0f, 1f);
				float num3 = UnityEngine.Random.Range(0f, 1f);
				if (num >= num2)
				{
					if (num3 < 0.75f)
					{
						Reward = ((UnityEngine.Random.value < 0.5f) ? InventoryItem.ITEM_TYPE.RED_HEART : InventoryItem.ITEM_TYPE.HALF_HEART);
					}
					else
					{
						Reward = ((UnityEngine.Random.value < 0.5f) ? InventoryItem.ITEM_TYPE.BLUE_HEART : InventoryItem.ITEM_TYPE.HALF_BLUE_HEART);
					}
					PickUp pickUp2 = InventoryItem.Spawn(Reward, 1, base.transform.position + Vector3.back, 0f);
					if (pickUp2 != null)
					{
						pickUp2.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), 270 + UnityEngine.Random.Range(-90, 90));
						pickUp2.MagnetDistance = 2f;
						pickUp2.CanStopFollowingPlayer = false;
					}
				}
			}
		}
		AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.gameObject);
		ChestEvent onChestRevealed = OnChestRevealed;
		if (onChestRevealed != null)
		{
			onChestRevealed();
		}
	}

	public void OnSpawnedDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		Victim.OnDie -= OnSpawnedDie;
		DeathCount++;
	}

	private new void Update()
	{
		if (BossChest)
		{
			return;
		}
		if (MyState == State.Hidden && Enemies.Count > 0)
		{
			if (DeathCount >= Enemies.Count)
			{
				if (Delay != 0f && delayTimestamp == -1f)
				{
					delayTimestamp = GameManager.GetInstance().CurrentTime + Delay;
				}
				if (Delay == 0f || GameManager.GetInstance().CurrentTime > delayTimestamp)
				{
					AudioManager.Instance.SetMusicCombatState(false);
					Reveal();
					RoomLockController.RoomCompleted(true);
					GameManager.GetInstance().CamFollowTarget.MaxZoom = CacheCameraMaxZoom;
					GameManager.GetInstance().CamFollowTarget.MinZoom = CacheCameraMinZoom;
					GameManager.GetInstance().CamFollowTarget.ZoomLimiter = CacheCameraZoomLimiter;
					GameManager.GetInstance().RemoveFromCamera(base.gameObject);
				}
			}
			else
			{
				delayTimestamp = -1f;
			}
			if (DeathCount < Enemies.Count)
			{
				foreach (Health enemy in Enemies)
				{
					if (enemy != null && enemy.state.CURRENT_STATE != 0)
					{
						AudioManager.Instance.SetMusicCombatState();
						break;
					}
				}
			}
		}
		if (MyState == State.Closed)
		{
			Timer += Time.deltaTime;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.OpenChest;
	}

	public override void GetLabel()
	{
		base.Label = ((MyState == State.Closed && Timer > 0.5f) ? sLabel : "");
	}

	public override void OnInteract(StateMachine state)
	{
		if (MyState == State.Closed)
		{
			base.OnInteract(state);
			SelectReward();
			MyState = State.Open;
			GameManager.GetInstance().StartCoroutine(GiveRewardDelay(UnityEngine.Random.Range(0f, 0.3f)));
		}
	}

	private void SelectReward()
	{
		healthPlayer = null;
		if (PlayerFarming.Instance != null)
		{
			healthPlayer = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		}
		if (++DataManager.Instance.ChestRewardCount >= 3)
		{
			DataManager.Instance.ChestRewardCount = 0;
		}
		new List<InventoryItem.ITEM_TYPE>();
		DataManager.Instance.ChestRewardCount = 0;
		if (ChestRewards.Count <= 0)
		{
			return;
		}
		randomReward = UnityEngine.Random.Range(0, 100);
		bool flag = false;
		foreach (RewardsItem chestReward in ChestRewards)
		{
			if (!flag)
			{
				if (randomReward >= previousChance && randomReward <= chestReward.SpawnNumber)
				{
					previousChance = chestReward.SpawnNumber;
					pickedReward = chestReward.chestReward;
					flag = true;
					break;
				}
				continue;
			}
			break;
		}
		UpdateChestRewards();
		if (OverrideChestReward != 0)
		{
			pickedReward = OverrideChestReward;
		}
		if (ForceGoodReward)
		{
			Reward = RewardsItem.Instance.ReturnItemType(RewardsItem.Instance.GetGoodReward());
		}
		else
		{
			Reward = RewardsItem.Instance.ReturnItemType(pickedReward);
		}
		if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			Debug.Log("Chest: Just spawn gold in intro");
			Reward = InventoryItem.ITEM_TYPE.BLACK_GOLD;
		}
	}

	private void BackToIdle()
	{
		Debug.Log("BACKL TO IDLE!");
		Loop = false;
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		StartCoroutine(DelayEffectsRoutine());
	}

	private IEnumerator DelayEffectsRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		TrinketManager.AddTrinket(DrawnCard);
	}

	private void CloseMenuCallback()
	{
		Loop = false;
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		foreach (Health enemy in Enemies)
		{
			if (enemy != null)
			{
				Utils.DrawLine(base.transform.position, enemy.transform.position, Color.yellow);
			}
		}
	}

	public void UpdateChestRewards()
	{
		totalProbability = 0f;
		previousRewardChance = 0f;
		foreach (RewardsItem chestReward in ChestRewards)
		{
			totalProbability += chestReward.ChanceToSpawn;
		}
		foreach (RewardsItem chestReward2 in ChestRewards)
		{
			multiplyer = 100f / totalProbability;
			chestReward2.probabilityChance = chestReward2.ChanceToSpawn * multiplyer;
			chestReward2.SpawnNumber = chestReward2.probabilityChance + previousRewardChance;
			previousRewardChance = chestReward2.SpawnNumber;
		}
	}
}
