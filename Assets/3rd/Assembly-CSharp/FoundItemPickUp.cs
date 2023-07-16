using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using src.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class FoundItemPickUp : Interaction
{
	public UnityEvent CallbackEnd;

	public static List<FoundItemPickUp> FoundItemPickUps = new List<FoundItemPickUp>();

	public GameObject uINewCard;

	public UINewItemOverlayController.TypeOfCard typeOfCard;

	[SpineSkin("", "", true, false, false)]
	public string SkinToForce;

	public bool GiveRecruit;

	public SkeletonAnimation Spine;

	public bool FollowerSkinForceSelection;

	public int WeaponLevel = -1;

	public int DurabilityLevel = -1;

	public TarotCards.Card WeaponModifier = TarotCards.Card.Count;

	public SpriteRenderer itemSprite;

	public StructureBrain.TYPES DecorationType;

	public FollowerLocation Location;

	public int LocationLayer = 1;

	public EquipmentType TypeOfWeapon;

	public RelicType TypeOfRelic;

	public EquipmentType StartingWeapon;

	public EquipmentType TypeOfCurse;

	public EquipmentType StartingCurse;

	public bool SpawnedOldCard;

	public List<WeaponIcons> WeaponSprites = new List<WeaponIcons>();

	private float WeaponOnGroundDamage;

	private float CurrentWeaponDamage;

	private float DamageDifference;

	private string DamageDifferenceString;

	private List<WeaponAttachmentData> weaponAttachments = new List<WeaponAttachmentData>();

	private bool weaponAssigned;

	private float rand;

	private PickUp newCard;

	private GameObject g;

	private InventoryItem.ITEM_TYPE itemType;

	private List<string> FollowerSkinsAvailable = new List<string>();

	private string PickedSkin;

	private Vector3 BookTargetPosition;

	private float Timer;

	private bool spawningMenu = true;

	public float CurrentWeaponDurability { get; set; }

	private void Start()
	{
		if (typeOfCard == UINewItemOverlayController.TypeOfCard.Weapon)
		{
			GetWeapon();
		}
	}

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.PickUp;
		if (!Interactable)
		{
			base.Label = "";
			return;
		}
		switch (typeOfCard)
		{
		case UINewItemOverlayController.TypeOfCard.Curse:
			DamageDifference = CurrentWeaponDamage - WeaponOnGroundDamage;
			if (DamageDifference > 0f)
			{
				DamageDifferenceString = " | DMG: " + WeaponOnGroundDamage + "<color=red> -" + DamageDifference + "</color>";
			}
			else
			{
				DamageDifferenceString = " | DMG: " + WeaponOnGroundDamage + "<color=green>+" + DamageDifference * -1f + "</color>";
			}
			base.Label = ScriptLocalization.Interactions.PickUp + " " + PlayerDetails_Player.GetWeaponCondition(DurabilityLevel) + PlayerDetails_Player.GetWeaponMod(WeaponModifier) + EquipmentManager.GetEquipmentData(TypeOfCurse).GetLocalisedTitle() + " " + PlayerDetails_Player.GetWeaponLevel(WeaponLevel) + DamageDifferenceString;
			break;
		case UINewItemOverlayController.TypeOfCard.Relic:
			base.Label = ScriptLocalization.Interactions.PickUp + " <color=#FFD201>" + RelicData.GetTitleLocalisation(TypeOfRelic);
			break;
		default:
			base.Label = ScriptLocalization.Interactions.PickUp;
			break;
		case UINewItemOverlayController.TypeOfCard.Weapon:
			break;
		}
	}

	public void MagnetToPlayer()
	{
		PickUp component = GetComponent<PickUp>();
		component.MagnetToPlayer = true;
		component.MagnetDistance = 100f;
		component.AddToInventory = false;
		AutomaticallyInteract = true;
		component.Callback.AddListener(delegate
		{
			CameraManager.instance.ShakeCameraForDuration(0.7f, 0.9f, 0.3f);
			SpawnMenu();
		});
	}

	private void GetWeapon()
	{
		if (weaponAssigned)
		{
			return;
		}
		if (!SpawnedOldCard)
		{
			TypeOfWeapon = DataManager.Instance.WeaponPool[UnityEngine.Random.Range(0, DataManager.Instance.WeaponPool.Count)];
			rand = UnityEngine.Random.Range(0, 100);
			if (rand >= 0f && rand <= DataManager.WeaponDurabilityChance[0])
			{
				DurabilityLevel = 0;
			}
			else if (rand > DataManager.WeaponDurabilityChance[0] && rand < DataManager.WeaponDurabilityChance[1])
			{
				DurabilityLevel = 1;
			}
			else if (rand > DataManager.WeaponDurabilityChance[1] && rand < DataManager.WeaponDurabilityChance[2])
			{
				DurabilityLevel = 2;
			}
			else if (rand > DataManager.WeaponDurabilityChance[2])
			{
				DurabilityLevel = 3;
			}
			CurrentWeaponDurability = DataManager.WeaponDurabilityLevels[DurabilityLevel];
		}
		itemSprite.sprite = EquipmentManager.GetEquipmentData(TypeOfWeapon).WorldSprite;
		itemSprite.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
		weaponAssigned = true;
	}

	private void GetCurse()
	{
		if (!SpawnedOldCard)
		{
			for (TypeOfCurse = DataManager.Instance.CursePool[UnityEngine.Random.Range(0, DataManager.Instance.CursePool.Count)]; TypeOfCurse == DataManager.Instance.CurrentCurse; TypeOfCurse = DataManager.Instance.CursePool[UnityEngine.Random.Range(0, DataManager.Instance.CursePool.Count)])
			{
			}
			rand = UnityEngine.Random.Range(0, 100);
			if (rand >= 0f && rand <= DataManager.WeaponDurabilityChance[0])
			{
				DurabilityLevel = 0;
			}
			else if (rand > DataManager.WeaponDurabilityChance[0] && rand < DataManager.WeaponDurabilityChance[1])
			{
				DurabilityLevel = 1;
			}
			else if (rand > DataManager.WeaponDurabilityChance[1] && rand < DataManager.WeaponDurabilityChance[2])
			{
				DurabilityLevel = 2;
			}
			else if (rand > DataManager.WeaponDurabilityChance[2])
			{
				DurabilityLevel = 3;
			}
		}
		else
		{
			TypeOfCurse = StartingCurse;
		}
		itemSprite.sprite = EquipmentManager.GetEquipmentData(TypeOfCurse).WorldSprite;
		itemSprite.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
	}

	private string NumbersToRoman(int number)
	{
		switch (number)
		{
		case 1:
			return "I";
		case 2:
			return "II";
		case 3:
			return "III";
		case 4:
			return "IV";
		case 5:
			return "V";
		case 6:
			return "VI";
		default:
			return "";
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		FoundItemPickUps.Add(this);
		if (typeOfCard == UINewItemOverlayController.TypeOfCard.Curse)
		{
			GetCurse();
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		FoundItemPickUps.Remove(this);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		SpawnMenu();
	}

	public void GetStartingWeapon()
	{
		switch (typeOfCard)
		{
		case UINewItemOverlayController.TypeOfCard.Weapon:
			StartingWeapon = DataManager.Instance.CurrentWeapon;
			break;
		case UINewItemOverlayController.TypeOfCard.Curse:
			StartingCurse = DataManager.Instance.CurrentCurse;
			break;
		}
	}

	public void SetStartingWeapon(EquipmentType _StartingWeapon, EquipmentType _StartingCurse, int _WeaponLevel, int _DurabilityLevel)
	{
		switch (typeOfCard)
		{
		case UINewItemOverlayController.TypeOfCard.Weapon:
			StartingWeapon = _StartingWeapon;
			WeaponLevel = _WeaponLevel;
			DurabilityLevel = _DurabilityLevel;
			GetWeapon();
			break;
		case UINewItemOverlayController.TypeOfCard.Curse:
			StartingCurse = _StartingCurse;
			WeaponLevel = _WeaponLevel;
			DurabilityLevel = _DurabilityLevel;
			break;
		}
	}

	private IEnumerator PickUpRoutine()
	{
		Timer = 0f;
		GameManager.GetInstance().OnConversationNew(true, true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 5f);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		BookTargetPosition = state.transform.position + new Vector3(0f, 0.2f, -1.2f);
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", base.gameObject);
		base.transform.DOMove(BookTargetPosition, 0.2f);
		yield return new WaitForSeconds(1f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SpawnMenu()
	{
		spawningMenu = true;
		GetStartingWeapon();
		BiomeConstants.Instance.EmitPickUpVFX(base.transform.position);
		switch (typeOfCard)
		{
		case UINewItemOverlayController.TypeOfCard.Relic:
		{
			GameManager.GetInstance().OnConversationNew();
			AudioManager.Instance.PlayOneShot("event:/player/new_item_pickup", base.gameObject);
			BiomeConstants.Instance.EmitPickUpVFX(base.transform.position);
			CameraManager.instance.ShakeCameraForDuration(0.7f, 0.9f, 0.3f);
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
			if (TypeOfRelic != 0)
			{
				DataManager.UnlockRelic(TypeOfRelic);
			}
			UIRelicMenuController uIRelicMenuController = MonoSingleton<UIManager>.Instance.RelicMenuTemplate.Instantiate();
			uIRelicMenuController.Show(TypeOfRelic);
			uIRelicMenuController.OnHidden = (Action)Delegate.Combine(uIRelicMenuController.OnHidden, (Action)delegate
			{
				CloseMenuCallback();
				DOTween.To(() => Time.timeScale, delegate(float x)
				{
					Time.timeScale = x;
				}, 1f, 1f).SetUpdate(true);
				GameManager.GetInstance().OnConversationEnd();
			});
			if (TypeOfRelic == RelicType.ProjectileRing)
			{
				DataManager.Instance.FoundRelicAtHubShore = true;
			}
			if (ObjectiveManager.HasCustomObjective(Objectives.TYPES.FIND_RELIC) && ObjectiveManager.GetObjectivesOfType<Objective_FindRelic>()[0].RelicType == TypeOfRelic)
			{
				Objective_FindRelic objective_FindRelic = ObjectiveManager.GetObjectivesOfType<Objective_FindRelic>()[0];
				objective_FindRelic.Complete();
				ObjectiveManager.UpdateObjective(objective_FindRelic);
			}
			if (GameManager.IsDungeon(PlayerFarming.Location))
			{
				PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(TypeOfRelic), true, true);
			}
			MonoSingleton<Indicator>.Instance.text.text = "";
			Interactable = false;
			base.HasChanged = true;
			break;
		}
		case UINewItemOverlayController.TypeOfCard.Decoration:
			itemType = base.gameObject.GetComponent<PickUp>().type;
			if (DecorationType != 0)
			{
				StructuresData.CompleteResearch(DecorationType);
				StructuresData.SetRevealed(DecorationType);
			}
			CreateMenuWeapon();
			break;
		case UINewItemOverlayController.TypeOfCard.Gift:
			itemType = base.gameObject.GetComponent<PickUp>().type;
			base.gameObject.GetComponent<PickUp>().enabled = false;
			if (!DataManager.Instance.FoundItems.Contains(itemType))
			{
				DataManager.Instance.FoundItems.Add(itemType);
				CreateMenuItem();
			}
			else
			{
				StartCoroutine(PickUpRoutine());
				spawningMenu = false;
			}
			Inventory.AddItem((int)itemType, 1);
			break;
		case UINewItemOverlayController.TypeOfCard.Necklace:
			itemType = base.gameObject.GetComponent<PickUp>().type;
			base.gameObject.GetComponent<PickUp>().enabled = false;
			if (!DataManager.Instance.FoundItems.Contains(itemType))
			{
				DataManager.Instance.FoundItems.Add(itemType);
				CreateMenuItem();
			}
			else
			{
				spawningMenu = false;
			}
			Inventory.AddItem((int)itemType, 1);
			UnityEngine.Object.Destroy(base.gameObject);
			break;
		case UINewItemOverlayController.TypeOfCard.FollowerSkin:
			if (!FollowerSkinForceSelection)
			{
				PickedSkin = DataManager.GetRandomLockedSkin();
			}
			else
			{
				PickedSkin = SkinToForce;
			}
			DataManager.SetFollowerSkinUnlocked(PickedSkin);
			if (GiveRecruit)
			{
				FollowerManager.CreateNewRecruit(FollowerLocation.Base, PickedSkin, NotificationCentre.NotificationType.NewRecruit);
			}
			CreateMenuFollowerSkin();
			break;
		case UINewItemOverlayController.TypeOfCard.Weapon:
			Debug.Log(TypeOfWeapon);
			state.GetComponent<PlayerWeapon>().SetWeapon(TypeOfWeapon, WeaponLevel);
			StartCoroutine(PlayerShowWeaponRoutine());
			break;
		case UINewItemOverlayController.TypeOfCard.MapLocation:
			Debug.Log("AA");
			Debug.Log(Location);
			MonoSingleton<UIManager>.Instance.ShowWorldMap().Show(Location);
			UnityEngine.Object.Destroy(base.gameObject);
			break;
		default:
			Debug.Log("Uh oh something went wrong :O");
			break;
		}
		UnityEvent callbackEnd = CallbackEnd;
		if (callbackEnd != null)
		{
			callbackEnd.Invoke();
		}
		if (spawningMenu)
		{
			foreach (Transform item in base.transform)
			{
				item.gameObject.SetActive(false);
			}
		}
		Interactable = false;
		base.Label = " ";
	}

	private IEnumerator PlayerShowWeaponRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().CameraSetTargetZoom(4f);
		float duration = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, EquipmentManager.GetWeaponData(DataManager.Instance.CurrentWeapon).PickupAnimationKey, false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		CloseMenuCallback();
	}

	private IEnumerator PlayerShowCurseRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().CameraSetTargetZoom(4f);
		AudioManager.Instance.PlayOneShot("event:/player/absorb_curse", base.gameObject);
		float duration = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).PickupAnimationKey, false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		CloseMenuCallback();
	}

	private void CreateMenuWeapon()
	{
		AudioManager.Instance.PlayOneShot("event:/player/new_item_pickup", base.gameObject);
		UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
		if (typeOfCard == UINewItemOverlayController.TypeOfCard.Decoration && DecorationType != 0)
		{
			uINewItemOverlayController.pickedBuilding = DecorationType;
			uINewItemOverlayController.Show(typeOfCard, base.transform.position, false);
		}
		else
		{
			uINewItemOverlayController.Show(typeOfCard, base.transform.position, true);
		}
		uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, new Action(CloseMenuCallback));
		DOTween.To(() => Time.timeScale, delegate(float x)
		{
			Time.timeScale = x;
		}, 0f, 1f).SetUpdate(true);
	}

	private void CreateMenuItem()
	{
		PlayerFarming.Instance.playerController.speed = 0f;
		UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
		uINewItemOverlayController.Show(typeOfCard, base.transform.position, itemType);
		uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, new Action(GetComponent<PickUp>().PickedUp));
		uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, new Action(CloseMenuCallback));
		DOTween.To(() => Time.timeScale, delegate(float x)
		{
			Time.timeScale = x;
		}, 0f, 1f).SetUpdate(true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CreateMenuFollowerSkin()
	{
		UINewItemOverlayController uINewItemOverlayController = MonoSingleton<UIManager>.Instance.ShowNewItemOverlay();
		uINewItemOverlayController.Show(typeOfCard, base.transform.position, PickedSkin);
		uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, new Action(GetComponent<PickUp>().PickedUp));
		uINewItemOverlayController.OnHidden = (Action)Delegate.Combine(uINewItemOverlayController.OnHidden, new Action(CloseMenuCallback));
		DOTween.To(() => Time.timeScale, delegate(float x)
		{
			Time.timeScale = x;
		}, 0f, 1f).SetUpdate(true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CloseMenuCallback()
	{
		if (!(this != null))
		{
			return;
		}
		switch (typeOfCard)
		{
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		PlayerFarming.Instance.GoToAndStopping = false;
		PickUp component = GetComponent<PickUp>();
		if (component != null)
		{
			component.PickedUp();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
