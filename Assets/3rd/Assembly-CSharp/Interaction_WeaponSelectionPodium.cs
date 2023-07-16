using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using MMBiomeGeneration;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using src.UI.Prompts;
using UnityEngine;

public class Interaction_WeaponSelectionPodium : Interaction
{
	public enum Types
	{
		Random,
		Weapon,
		Curse,
		Relic
	}

	[Serializable]
	public class WeaponIcons
	{
		public TarotCards.Card Weapon;

		public Sprite Sprite;
	}

	protected int WeaponLevel;

	[SerializeField]
	public SpriteRenderer IconSpriteRenderer;

	[SerializeField]
	protected SpriteRenderer LockedIcon;

	[SerializeField]
	protected Interaction_WeaponSelectionPodium[] otherWeaponOptions;

	[SerializeField]
	protected PlayerFleeceManager.FleeceType[] onlyTurnOffOtherPodiumsWhileWearingFleeceType;

	public ParticleSystem particleEffect;

	public GameObject podiumOn;

	public GameObject podiumOff;

	protected Canvas canvas;

	[SerializeField]
	public Animator AvailableGoop;

	[SerializeField]
	private NewWeaponEffect NewWeaponEffect;

	public static Action<bool> OnHighlightWeapon;

	public static Action<bool> OnHighlightCurse;

	public GameObject Lighting;

	public int LevelIncreaseAmount = 1;

	public Material WeaponMaterial;

	public Material CurseMaterial;

	private LayerMask collisionMask;

	public SpriteRenderer weaponBetterIcon;

	public Sprite weaponUp;

	public Sprite weaponDown;

	[SerializeField]
	private GameObject ps_relicsBlessed;

	[SerializeField]
	private GameObject ps_relicsDammed;

	public Types Type;

	public bool RemoveIfNotFirstLayer;

	protected bool WeaponTaken;

	private const int rerollLevelDecrease = 1;

	private bool initialDungeonEnter;

	private bool activated;

	public GameObject VFXRerollWeapon;

	public GameObject VFXRerollCurse;

	private UIWeaponPickupPromptController _weaponPickupUI;

	private EquipmentType PrevEquipment = EquipmentType.None;

	public static EquipmentType LastCurseSelected;

	private float WobbleTimer;

	private string sLabel;

	private bool AllowResummonWeapon;

	private int ResummonCost = 50;

	public bool ReadyToOpenDoor;

	public static List<Interaction_WeaponSelectionPodium> Podiums = new List<Interaction_WeaponSelectionPodium>();

	public static Action OnTutorialShown;

	public EquipmentType TypeOfWeapon { get; protected set; } = EquipmentType.None;


	public RelicType TypeOfRelic { get; protected set; }

	public bool RelicStartCharged { get; set; } = true;


	private void Awake()
	{
		initialDungeonEnter = GameManager.InitialDungeonEnter;
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Island"));
		collisionMask = (int)collisionMask | (1 << LayerMask.NameToLayer("Obstacles"));
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (ps_relicsBlessed != null)
		{
			ps_relicsBlessed.SetActive(false);
		}
		if (ps_relicsDammed != null)
		{
			ps_relicsDammed.SetActive(false);
		}
		HasSecondaryInteraction = true;
		Podiums.Add(this);
		WobbleTimer = UnityEngine.Random.Range(0, 360);
		if (Type == Types.Random)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				Type = Types.Weapon;
			}
			else
			{
				Type = Types.Curse;
			}
		}
		if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			Type = Types.Weapon;
		}
		if (RemoveIfNotFirstLayer && (GameManager.CurrentDungeonFloor > 1 || !initialDungeonEnter))
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (Type == Types.Curse && !DataManager.Instance.EnabledSpells)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!WeaponTaken && !BiomeGenerator.Instance.CurrentRoom.Completed)
		{
			if (RemoveIfNotFirstLayer && UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_ResummonWeapon))
			{
				AllowResummonWeapon = true;
			}
			BiomeGenerator.OnBiomeChangeRoom += LockDoors;
		}
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Combine(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(Reveal));
		if (!WeaponTaken)
		{
			AvailableGoop.Play("Show");
		}
		else
		{
			AvailableGoop.Play("Hidden");
			particleEffect.Stop();
		}
		weaponBetterIcon.enabled = false;
		if (!WeaponTaken)
		{
			CheckWeaponLevel();
		}
	}

	private void CheckWeaponLevel()
	{
		if (Type == Types.Curse)
		{
			if (DataManager.Instance.CurrentCurseLevel > WeaponLevel)
			{
				weaponBetterIcon.sprite = weaponDown;
				weaponBetterIcon.enabled = true;
			}
			else if (DataManager.Instance.CurrentCurseLevel < WeaponLevel)
			{
				weaponBetterIcon.sprite = weaponUp;
				weaponBetterIcon.enabled = true;
			}
			if (DataManager.Instance.CurrentCurse == EquipmentType.None)
			{
				weaponBetterIcon.enabled = false;
			}
		}
		else if (Type == Types.Weapon)
		{
			if (DataManager.Instance.CurrentWeaponLevel > WeaponLevel)
			{
				weaponBetterIcon.sprite = weaponDown;
				weaponBetterIcon.enabled = true;
			}
			else if (DataManager.Instance.CurrentWeaponLevel < WeaponLevel)
			{
				weaponBetterIcon.sprite = weaponUp;
				weaponBetterIcon.enabled = true;
			}
			if (DataManager.Instance.CurrentWeapon == EquipmentType.None)
			{
				weaponBetterIcon.enabled = false;
			}
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Podiums.Remove(this);
		BiomeGenerator.OnBiomeChangeRoom -= LockDoors;
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(Reveal));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		BiomeGenerator.OnBiomeChangeRoom -= LockDoors;
		Interaction_Chest.OnChestRevealed = (Interaction_Chest.ChestEvent)Delegate.Remove(Interaction_Chest.OnChestRevealed, new Interaction_Chest.ChestEvent(Reveal));
	}

	public void Reveal()
	{
		Debug.Log("Reveal()");
		if (PlayerFarming.Location != FollowerLocation.IntroDungeon)
		{
			base.gameObject.SetActive(true);
			NewWeaponEffect.gameObject.SetActive(true);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (TypeOfWeapon == EquipmentType.None && TypeOfRelic == RelicType.None)
		{
			ActivateDistance = 2f;
			UpdateLocalisation();
			StartCoroutine(SetItem());
			podiumOff.SetActive(false);
		}
	}

	public void ResetRandom(bool ForceShowGoop = false, int ForceLevel = -1)
	{
		IconSpriteRenderer.enabled = true;
		WeaponTaken = false;
		StartCoroutine(SetItem(ForceShowGoop, ForceLevel));
	}

	private IEnumerator SetItem(bool ForceShowGoop = false, int ForceLevel = -1)
	{
		yield return new WaitForEndOfFrame();
		switch (Type)
		{
		case Types.Weapon:
			SetWeapon(ForceLevel);
			break;
		case Types.Curse:
			if (DataManager.Instance.EnabledSpells)
			{
				SetCurse(ForceLevel);
				break;
			}
			Type = Types.Weapon;
			SetWeapon(ForceLevel);
			break;
		case Types.Relic:
			SetRelic();
			break;
		}
		IconSpriteRenderer.sprite = GetIcon();
		LockedIcon.gameObject.SetActive(false);
		canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		if (!ForceShowGoop)
		{
			if (!WeaponTaken)
			{
				AvailableGoop.Play("Show");
			}
			else
			{
				AvailableGoop.Play("Hidden");
			}
		}
		CheckWeaponLevel();
	}

	private void LockDoors()
	{
		if (RemoveIfNotFirstLayer && GameManager.CurrentDungeonFloor <= 1)
		{
			BiomeGenerator.OnBiomeChangeRoom -= LockDoors;
			RoomLockController.CloseAll();
		}
	}

	protected virtual void SetWeapon(int ForceLevel = -1)
	{
		if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			int curse = ((DataManager.Instance.CurrentCurse != EquipmentType.None) ? (DataManager.Instance.CurrentRunCurseLevel + LevelIncreaseAmount - 1) : (DataManager.StartingEquipmentLevel + LevelIncreaseAmount));
			SetCurse(curse);
			return;
		}
		IconSpriteRenderer.material = WeaponMaterial;
		TypeOfWeapon = DataManager.Instance.GetRandomWeaponInPool();
		int num = 100;
		if (PrevEquipment != EquipmentType.None)
		{
			while ((TypeOfWeapon == PrevEquipment || (TrinketManager.HasTrinket(TarotCards.Card.Spider) && EquipmentManager.IsPoisonWeapon(TypeOfWeapon))) && --num > 0)
			{
				TypeOfWeapon = DataManager.Instance.GetRandomWeaponInPool();
			}
		}
		PrevEquipment = TypeOfWeapon;
		WeaponLevel = DataManager.Instance.CurrentRunWeaponLevel + 1;
		if (DataManager.Instance.ForcedStartingWeapon != EquipmentType.None && !DungeonSandboxManager.Active)
		{
			TypeOfWeapon = DataManager.Instance.ForcedStartingWeapon;
			DataManager.Instance.ForcedStartingWeapon = EquipmentType.None;
		}
		if (DataManager.Instance.CurrentWeapon == EquipmentType.None)
		{
			WeaponLevel += DataManager.StartingEquipmentLevel;
		}
		if (ForceLevel == -1)
		{
			DataManager.Instance.CurrentRunWeaponLevel = WeaponLevel;
			WeaponLevel += Mathf.Clamp(LevelIncreaseAmount - 1, 0, LevelIncreaseAmount);
		}
		else
		{
			WeaponLevel = (DataManager.Instance.CurrentRunWeaponLevel = ForceLevel);
		}
	}

	protected virtual void SetRelic()
	{
		RelicData randomRelicData = EquipmentManager.GetRandomRelicData(false);
		SetRelic(randomRelicData.RelicType);
	}

	protected void SetRelic(RelicType relicType)
	{
		if (DataManager.Instance.FirstRelic)
		{
			DataManager.Instance.FirstRelic = false;
			relicType = RelicType.LightningStrike;
		}
		IconSpriteRenderer.material = WeaponMaterial;
		IconSpriteRenderer.transform.localScale = Vector3.one * 0.41f;
		IconSpriteRenderer.transform.parent.localPosition = new Vector3(IconSpriteRenderer.transform.parent.localPosition.x, IconSpriteRenderer.transform.parent.localPosition.y, -1.5f);
		TypeOfRelic = relicType;
		if (TypeOfRelic.ToString().Contains("Blessed"))
		{
			ps_relicsBlessed.SetActive(true);
			if (DataManager.Instance.ForceBlessedRelic)
			{
				DataManager.Instance.ForceBlessedRelic = false;
			}
		}
		else if (TypeOfRelic.ToString().Contains("Dammed"))
		{
			ps_relicsDammed.SetActive(true);
			if (DataManager.Instance.ForceDammedRelic)
			{
				DataManager.Instance.ForceDammedRelic = false;
			}
		}
		if (!DataManager.Instance.SpawnedRelicsThisRun.Contains(TypeOfRelic))
		{
			DataManager.Instance.SpawnedRelicsThisRun.Add(TypeOfRelic);
		}
		Type = Types.Relic;
	}

	protected virtual void SetCurse(int ForceLevel = -1)
	{
		if (PlayerFleeceManager.FleeceSwapsCurseForRelic())
		{
			SetRelic();
			return;
		}
		Type = Types.Curse;
		IconSpriteRenderer.material = CurseMaterial;
		for (int i = 0; i < 100; i++)
		{
			TypeOfWeapon = DataManager.Instance.GetRandomCurseInPool();
			if (TypeOfWeapon != LastCurseSelected)
			{
				break;
			}
		}
		LastCurseSelected = TypeOfWeapon;
		if (PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			WeaponLevel += DataManager.Instance.CurrentRunCurseLevel;
		}
		else
		{
			WeaponLevel = DataManager.Instance.CurrentRunCurseLevel;
		}
		WeaponLevel++;
		if (DataManager.Instance.ForcedStartingCurse != EquipmentType.None && !DungeonSandboxManager.Active)
		{
			TypeOfWeapon = DataManager.Instance.ForcedStartingCurse;
			DataManager.Instance.ForcedStartingCurse = EquipmentType.None;
		}
		if (DataManager.Instance.CurrentCurse == EquipmentType.None)
		{
			WeaponLevel += DataManager.StartingEquipmentLevel;
		}
		if (ForceLevel == -1)
		{
			DataManager.Instance.CurrentRunCurseLevel = WeaponLevel;
			WeaponLevel += Mathf.Clamp(LevelIncreaseAmount - 1, 0, LevelIncreaseAmount);
		}
		else
		{
			WeaponLevel = (DataManager.Instance.CurrentRunWeaponLevel = ForceLevel);
		}
	}

	protected Sprite GetIcon()
	{
		if (TypeOfRelic != 0)
		{
			return EquipmentManager.GetRelicData(TypeOfRelic).WorldSprite;
		}
		return EquipmentManager.GetEquipmentData(TypeOfWeapon).WorldSprite;
	}

	private new void Update()
	{
		if (!WeaponTaken && (bool)IconSpriteRenderer)
		{
			IconSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, Mathf.Sin(WobbleTimer += Time.deltaTime * 2f) * 0.1f);
		}
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
		float damage = 0f;
		float speed = 0f;
		switch (Type)
		{
		case Types.Weapon:
		{
			Action<bool> onHighlightWeapon = OnHighlightWeapon;
			if (onHighlightWeapon != null)
			{
				onHighlightWeapon(true);
			}
			break;
		}
		case Types.Curse:
		{
			Action<bool> onHighlightCurse = OnHighlightCurse;
			if (onHighlightCurse != null)
			{
				onHighlightCurse(true);
			}
			break;
		}
		}
		if (Type == Types.Weapon)
		{
			damage = PlayerFarming.Instance.playerWeapon.GetAverageWeaponDamage(TypeOfWeapon, WeaponLevel);
			speed = PlayerFarming.Instance.playerWeapon.GetWeaponSpeed(TypeOfWeapon);
		}
		AudioManager.Instance.PlayOneShot("event:/ui/open_menu", PlayerFarming.Instance.transform.position);
		if (_weaponPickupUI == null)
		{
			_weaponPickupUI = MonoSingleton<UIManager>.Instance.WeaponPickPromptControllerTemplate.Instantiate();
			UIWeaponPickupPromptController weaponPickupUI = _weaponPickupUI;
			weaponPickupUI.OnHidden = (Action)Delegate.Combine(weaponPickupUI.OnHidden, (Action)delegate
			{
				_weaponPickupUI = null;
			});
		}
		if (Type == Types.Relic)
		{
			_weaponPickupUI.Show(TypeOfRelic);
		}
		else
		{
			_weaponPickupUI.Show(TypeOfWeapon, damage, speed, WeaponLevel);
		}
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
		switch (Type)
		{
		case Types.Weapon:
		{
			Action<bool> onHighlightWeapon = OnHighlightWeapon;
			if (onHighlightWeapon != null)
			{
				onHighlightWeapon(false);
			}
			break;
		}
		case Types.Curse:
		{
			Action<bool> onHighlightCurse = OnHighlightCurse;
			if (onHighlightCurse != null)
			{
				onHighlightCurse(false);
			}
			break;
		}
		}
		if (_weaponPickupUI != null)
		{
			_weaponPickupUI.Hide();
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.Equip;
	}

	public override void GetLabel()
	{
		base.Label = (WeaponTaken ? "" : sLabel);
	}

	public override void GetSecondaryLabel()
	{
		base.SecondaryLabel = ((AllowResummonWeapon && !WeaponTaken) ? (ScriptLocalization.Interactions.Resummon + " " + CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, ResummonCost)) : "");
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		if (!AllowResummonWeapon)
		{
			return;
		}
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < ResummonCost)
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
			UIManager.PlayAudio("event:/ui/negative_feedback");
			return;
		}
		Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -ResummonCost);
		switch (Type)
		{
		case Types.Weapon:
		{
			GameObject obj2 = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), base.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj2.GetComponent<PickUp>().SetInitialSpeedAndDiraction(12f, 270f);
			Interaction_WeaponPickUp component2 = obj2.GetComponent<Interaction_WeaponPickUp>();
			component2.SetWeapon(TypeOfWeapon, WeaponLevel, Interaction_WeaponPickUp.Types.Weapon);
			component2.OnInteraction += delegate
			{
				ReadyToOpenDoor = true;
				if (GameManager.CurrentDungeonFloor <= 1)
				{
					ReadyToOpenDoor = true;
					bool flag = true;
					foreach (Interaction_WeaponSelectionPodium podium in Podiums)
					{
						if (!podium.ReadyToOpenDoor)
						{
							flag = false;
						}
					}
					if (flag)
					{
						RoomLockController.RoomCompleted();
					}
				}
			};
			VFXRerollWeapon.SetActive(true);
			DataManager.Instance.CurrentRunWeaponLevel--;
			AudioManager.Instance.PlayOneShot("event:/temple_key/become_whole", base.transform.position);
			break;
		}
		case Types.Curse:
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), base.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(12f, 270f);
			Interaction_WeaponPickUp component = obj.GetComponent<Interaction_WeaponPickUp>();
			component.SetWeapon(TypeOfWeapon, WeaponLevel, Interaction_WeaponPickUp.Types.Curse);
			component.OnInteraction += delegate
			{
				ReadyToOpenDoor = true;
				if (GameManager.CurrentDungeonFloor <= 1)
				{
					ReadyToOpenDoor = true;
					bool flag2 = true;
					foreach (Interaction_WeaponSelectionPodium podium2 in Podiums)
					{
						if (!podium2.ReadyToOpenDoor)
						{
							flag2 = false;
						}
					}
					if (flag2 || PlayerFleeceManager.FleeceSwapsWeaponForCurse())
					{
						RoomLockController.RoomCompleted();
					}
				}
			};
			VFXRerollCurse.SetActive(true);
			DataManager.Instance.CurrentRunCurseLevel--;
			AudioManager.Instance.PlayOneShot("event:/temple_key/fragment_into_place", base.transform.position);
			break;
		}
		}
		HasSecondaryInteraction = false;
		AllowResummonWeapon = false;
		Vector3 localScale = IconSpriteRenderer.transform.localScale;
		activated = true;
		WeaponTaken = true;
		base.HasChanged = true;
		EndIndicateHighlighted();
		Sequence sequence = DOTween.Sequence();
		sequence.Append(IconSpriteRenderer.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
		sequence.AppendInterval(0.5f);
		sequence.AppendCallback(delegate
		{
			ResetRandom(true, WeaponLevel - 1);
			WeaponTaken = true;
		});
		sequence.Append(IconSpriteRenderer.transform.DOScale(localScale, 0.3f).SetEase(Ease.OutBack));
		sequence.AppendCallback(delegate
		{
			WeaponTaken = false;
			base.HasChanged = true;
			activated = false;
		});
		sequence.Play();
	}

	public override void OnInteract(StateMachine state)
	{
		if (!activated)
		{
			base.OnInteract(state);
			weaponBetterIcon.enabled = false;
			activated = true;
			switch (Type)
			{
			case Types.Weapon:
				StartCoroutine(PlayerShowWeaponRoutine());
				break;
			case Types.Curse:
				StartCoroutine(PlayerShowCurseRoutine());
				break;
			case Types.Relic:
			{
				ps_relicsBlessed.GetComponent<ParticleSystem>().Stop();
				ps_relicsDammed.GetComponent<ParticleSystem>().Stop();
				PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(TypeOfRelic), RelicStartCharged, true);
				Interactable = false;
				Lighting.SetActive(false);
				IconSpriteRenderer.enabled = false;
				weaponBetterIcon.enabled = false;
				podiumOn.SetActive(false);
				podiumOff.SetActive(true);
				particleEffect.Stop();
				AvailableGoop.Play("Hide");
				base.enabled = false;
				if (!RemoveIfNotFirstLayer || GameManager.CurrentDungeonFloor > 1)
				{
					break;
				}
				ReadyToOpenDoor = true;
				bool flag = true;
				foreach (Interaction_WeaponSelectionPodium podium in Podiums)
				{
					if (!podium.ReadyToOpenDoor)
					{
						flag = false;
					}
				}
				if (flag)
				{
					RoomLockController.RoomCompleted();
				}
				break;
			}
			}
		}
		PlayerFleeceManager.FleeceType playerFleece = (PlayerFleeceManager.FleeceType)DataManager.Instance.PlayerFleece;
		if (onlyTurnOffOtherPodiumsWhileWearingFleeceType.Contains(playerFleece) || onlyTurnOffOtherPodiumsWhileWearingFleeceType.Length == 0)
		{
			for (int num = otherWeaponOptions.Length - 1; num >= 0; num--)
			{
				otherWeaponOptions[num].Interactable = false;
				otherWeaponOptions[num].Lighting.SetActive(false);
				otherWeaponOptions[num].IconSpriteRenderer.enabled = false;
				otherWeaponOptions[num].weaponBetterIcon.enabled = false;
				otherWeaponOptions[num].podiumOn.SetActive(false);
				otherWeaponOptions[num].podiumOff.SetActive(true);
				otherWeaponOptions[num].particleEffect.Stop();
				otherWeaponOptions[num].AvailableGoop.Play("Hide");
				otherWeaponOptions[num].enabled = false;
			}
		}
	}

	private IEnumerator PlayerShowWeaponRoutine()
	{
		AvailableGoop.Play("Hide");
		PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.down * 0.5f, base.gameObject);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		DataManager.Instance.CurrentRunWeaponLevel += Mathf.Clamp(LevelIncreaseAmount - 1, 0, LevelIncreaseAmount);
		if (DataManager.Instance.CurrentWeapon != EquipmentType.None)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), state.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-2f, 2.5f), 270f);
			obj.GetComponent<Interaction_WeaponPickUp>().SetWeapon(DataManager.Instance.CurrentWeapon, PlayerFarming.Instance.playerWeapon.CurrentWeaponLevel, Interaction_WeaponPickUp.Types.Weapon);
		}
		if (TypeOfWeapon == EquipmentType.None)
		{
			TypeOfWeapon = EquipmentType.Sword;
		}
		PlayerFarming.Instance.playerWeapon.SetWeapon(TypeOfWeapon, WeaponLevel);
		WeaponTaken = true;
		Lighting.SetActive(false);
		IconSpriteRenderer.enabled = false;
		podiumOn.SetActive(false);
		podiumOff.SetActive(true);
		particleEffect.Stop();
		if (!DataManager.Instance.WeaponPool.Contains(TypeOfWeapon))
		{
			DataManager.Instance.WeaponPool.Add(TypeOfWeapon);
		}
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		AudioManager.Instance.PlayOneShot("event:/player/weapon_equip", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/weapon_unlocked", base.transform.position);
		float duration = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.PickupAnimationKey, false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (RemoveIfNotFirstLayer && GameManager.CurrentDungeonFloor <= 1)
		{
			ReadyToOpenDoor = true;
			bool flag = true;
			foreach (Interaction_WeaponSelectionPodium podium in Podiums)
			{
				if (!podium.ReadyToOpenDoor)
				{
					flag = false;
				}
			}
			if (flag)
			{
				RoomLockController.RoomCompleted();
			}
		}
		if (!UpgradeSystem.GetUnlocked(UpgradeSystem.Type.PUpgrade_HeavyAttacks) || !DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.HeavyAttacks))
		{
			yield break;
		}
		UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.HeavyAttacks);
		uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
		{
			Action onTutorialShown = OnTutorialShown;
			if (onTutorialShown != null)
			{
				onTutorialShown();
			}
		});
	}

	private IEnumerator PlayerShowCurseRoutine()
	{
		AvailableGoop.Play("Hide");
		LayerMask layerMask = (int)default(LayerMask) | (1 << LayerMask.NameToLayer("Obstacles"));
		layerMask = (int)layerMask | (1 << LayerMask.NameToLayer("Island"));
		bool flag = Physics2D.Raycast(base.transform.position, Vector3.right, 1.5f, layerMask);
		bool flag2 = Physics2D.Raycast(base.transform.position, Vector3.left, 1.5f, layerMask);
		if ((base.transform.position.x < state.transform.position.x && !flag) || flag2)
		{
			PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.right * 1.25f, base.gameObject);
		}
		else if ((base.transform.position.x >= state.transform.position.x && !flag2) || flag)
		{
			PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 1.25f, base.gameObject);
		}
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		FaithAmmo.Reload();
		WeaponTaken = true;
		IconSpriteRenderer.transform.DOShakePosition(2f, 0.25f);
		IconSpriteRenderer.transform.DOShakeRotation(2f, new Vector3(0f, 0f, 15f));
		Sequence Sequence = DOTween.Sequence();
		Sequence.Append(IconSpriteRenderer.transform.DOScale(Vector3.one * 1.2f, 0.3f));
		Sequence.Append(IconSpriteRenderer.transform.DOScale(Vector3.one * 0.8f, 0.3f));
		Sequence.Play().SetLoops(-1);
		if (!DataManager.Instance.CursePool.Contains(TypeOfWeapon))
		{
			DataManager.Instance.CursePool.Add(TypeOfWeapon);
		}
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		AudioManager.Instance.PlayOneShot("event:/player/absorb_curse", base.gameObject);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "Curses/curse-get", false);
		yield return new WaitForSeconds(1.0333333f);
		IconSpriteRenderer.enabled = false;
		Sequence.Kill();
		Lighting.SetActive(false);
		podiumOn.SetActive(false);
		podiumOff.SetActive(true);
		particleEffect.Stop();
		yield return new WaitForSeconds(0.3f);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		DataManager.Instance.CurrentRunCurseLevel += Mathf.Clamp(LevelIncreaseAmount - 1, 0, LevelIncreaseAmount);
		if (DataManager.Instance.CurrentCurse != EquipmentType.None)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), state.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-2f, 2.5f), 270f);
			obj.GetComponent<Interaction_WeaponPickUp>().SetWeapon(DataManager.Instance.CurrentCurse, DataManager.Instance.CurrentCurseLevel, Interaction_WeaponPickUp.Types.Curse);
		}
		PlayerFarming.Instance.playerSpells.SetSpell(TypeOfWeapon, WeaponLevel);
		if (!RemoveIfNotFirstLayer || GameManager.CurrentDungeonFloor > 1)
		{
			yield break;
		}
		ReadyToOpenDoor = true;
		bool flag3 = true;
		foreach (Interaction_WeaponSelectionPodium podium in Podiums)
		{
			if (!podium.ReadyToOpenDoor)
			{
				flag3 = false;
			}
		}
		if (flag3 || PlayerFleeceManager.FleeceSwapsWeaponForCurse())
		{
			RoomLockController.RoomCompleted();
		}
	}
}
