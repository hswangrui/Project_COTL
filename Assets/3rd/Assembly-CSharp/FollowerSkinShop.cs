using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using UnityEngine;

public class FollowerSkinShop : Interaction
{
	public GameObject UIFollowerSkinUnlock;

	private bool Activated;

	private GameObject g;

	public SpriteRenderer Shrine;

	public GameObject Portal;

	public GameObject Symbol;

	public GameObject CoinTarget;

	public GameObject LightingVolumeManager;

	public FollowerLocation location = FollowerLocation.None;

	private List<WorshipperData.SkinAndData> Skins;

	public List<WorshipperData.SkinAndData> SkinsAvailable;

	public bool noSkins;

	private void Start()
	{
		Portal.SetActive(false);
		Symbol.SetActive(false);
		LightingVolumeManager.SetActive(false);
	}

	private void Setup()
	{
		location = PlayerFarming.Location;
		Skins = WorshipperData.Instance.GetSkinsFromFollowerLocation(PlayerFarming.Location);
		CheckSkinAvailability();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		CheckSkinAvailability();
	}

	private void disableBuilding()
	{
		if (Shrine != null)
		{
			Shrine.color = new Color(0.5f, 0.5f, 0.5f, 1f);
		}
	}

	private void CheckSkinAvailability()
	{
		SkinsAvailable.Clear();
		if (Skins == null)
		{
			return;
		}
		foreach (WorshipperData.SkinAndData skin in Skins)
		{
			if (!DataManager.GetFollowerSkinUnlocked(skin.Skin[0].Skin) && !skin.Skin[0].Skin.Contains("Boss") && !DataManager.OnBlackList(skin.Skin[0].Skin))
			{
				SkinsAvailable.Add(skin);
			}
		}
		if (SkinsAvailable.Count <= 0)
		{
			disableBuilding();
		}
	}

	private int GetCost()
	{
		switch (location)
		{
		case FollowerLocation.HubShore:
			return 10;
		case FollowerLocation.Sozo_Cave:
			return 20;
		case FollowerLocation.Dungeon_Decoration_Shop1:
			return 30;
		case FollowerLocation.Dungeon_Location_3:
			return 40;
		default:
			return 10;
		}
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		base.OnSecondaryInteract(state);
		state.CURRENT_STATE = StateMachine.State.InActive;
		HUD_Manager.Instance.Hide(false, 0);
		Time.timeScale = 0f;
		UIFollowerFormsMenuController followerFormsMenuInstance = MonoSingleton<UIManager>.Instance.FollowerFormsMenuTemplate.Instantiate();
		followerFormsMenuInstance.Show();
		UIFollowerFormsMenuController uIFollowerFormsMenuController = followerFormsMenuInstance;
		uIFollowerFormsMenuController.OnHidden = (Action)Delegate.Combine(uIFollowerFormsMenuController.OnHidden, (Action)delegate
		{
			Time.timeScale = 1f;
			base.HasChanged = true;
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			Activated = false;
			Interactable = true;
			HUD_Manager.Instance.Show(0);
			followerFormsMenuInstance = null;
		});
	}

	private string GetAffordColor()
	{
		if (GetCost() > Inventory.GetItemQuantity(20))
		{
			return "<color=red>";
		}
		return "<color=#f4ecd3>";
	}

	public override void GetSecondaryLabel()
	{
		base.GetSecondaryLabel();
		base.SecondaryLabel = ScriptLocalization.UI_FollowerSkinUnlock.UnlockedForms;
	}

	public override void GetLabel()
	{
		if (SkinsAvailable.Count <= 0)
		{
			Interactable = false;
			noSkins = true;
			base.Label = ScriptLocalization.Interactions.SoldOut;
		}
		else
		{
			base.Label = string.Join(" ", ScriptLocalization.UI_FollowerSkinUnlock.BuyForm, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, GetCost()));
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (noSkins)
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
		else if (GetCost() <= Inventory.GetItemQuantity(20) && !Activated)
		{
			AudioManager.Instance.PlayOneShot("event:/shop/buy", base.transform.position);
			Activated = true;
			Interactable = false;
			StartCoroutine(GiveSkin());
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.transform.position);
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator GiveSkin()
	{
		GameManager.GetInstance().OnConversationNew();
		PlayerFarming.Instance.GoToAndStop(base.transform.position - Vector3.up, base.gameObject, true, true, delegate
		{
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "idle", true);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		});
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		LightingVolumeManager.SetActive(true);
		yield return new WaitForSeconds(1f);
		LightingVolumeManager.SetActive(true);
		Symbol.SetActive(true);
		Symbol.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
		Symbol.GetComponent<SpriteRenderer>().DOFade(1f, 0.5f);
		for (int i = 0; i < GetCost(); i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.gameObject);
			ResourceCustomTarget.Create(CoinTarget, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			Inventory.ChangeItemQuantity(20, -1);
			if (GetCost() == 10 || GetCost() == 20)
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.1f));
			}
			else
			{
				yield return new WaitForSeconds(UnityEngine.Random.Range(0.005f, 0.25f));
			}
		}
		CameraManager.instance.ShakeCameraForDuration(0.1f, 1f, 1f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		Portal.SetActive(true);
		Symbol.GetComponent<SpriteRenderer>().DOFade(0f, 1f);
		Portal.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
		Portal.GetComponent<SpriteRenderer>().DOFade(0f, 0.3f);
		Portal.transform.localScale = new Vector3(1f, 0f, 1f);
		Portal.transform.DOScaleY(1f, 0.25f);
		AudioManager.Instance.PlayOneShot("event:/small_portal/open", base.gameObject);
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.gameObject);
		FollowerSkinCustomTarget.Create(base.gameObject.transform.position + new Vector3(0f, 0f, -1f), PlayerFarming.Instance.gameObject.transform.position, 2f, SkinsAvailable[UnityEngine.Random.Range(0, SkinsAvailable.Count)].Skin[0].Skin, PickedUp);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		RumbleManager.Instance.Rumble();
		yield return new WaitForSeconds(0.5f);
		LightingVolumeManager.SetActive(false);
	}

	private void PickedUp()
	{
		SecondaryInteractable = true;
		Activated = false;
		Interactable = true;
		Portal.transform.DOScaleY(0f, 0.33f);
		AudioManager.Instance.PlayOneShot("event:/small_portal/close", base.gameObject);
		Symbol.SetActive(false);
		LightingVolumeManager.SetActive(false);
		GameManager.GetInstance().CameraResetTargetZoom();
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		base.HasChanged = true;
		GameManager.GetInstance().OnConversationEnd();
		CheckSkinAvailability();
	}

	protected override void Update()
	{
		base.Update();
		if (!(PlayerFarming.Instance == null))
		{
			if (location == FollowerLocation.None)
			{
				Setup();
			}
			bool interactable = Interactable;
		}
	}
}
