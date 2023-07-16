using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using src.UI.Prompts;
using UnityEngine;

public class Interaction_WeaponItem : Interaction
{
	public enum Types
	{
		Weapon,
		Curse
	}

	public EquipmentType TypeOfWeapon;

	public WeaponShopKeeperManager ShopKeeper;

	private int WeaponLevel;

	private int Cost;

	private Canvas canvas;

	[SerializeField]
	public SpriteRenderer IconSpriteRenderer;

	public static Action<bool> OnHighlightWeapon;

	public static Action<bool> OnHighlightCurse;

	private bool Activated;

	public Types Type;

	private float WobbleTimer;

	private UIWeaponPickupPromptController _weaponPickupUI;

	private string BuyString;

	public void Init(EquipmentType _TypeOfWeapon, int _WeaponLevel, int _Cost, Types _type)
	{
		TypeOfWeapon = _TypeOfWeapon;
		WeaponLevel = _WeaponLevel;
		Cost = _Cost;
		Type = _type;
		IconSpriteRenderer.sprite = GetIcon();
	}

	private void Start()
	{
		WobbleTimer = UnityEngine.Random.Range(0, 360);
		ActivateDistance = 2f;
		UpdateLocalisation();
	}

	private Sprite GetIcon()
	{
		return EquipmentManager.GetEquipmentData(TypeOfWeapon).WorldSprite;
	}

	private new void Update()
	{
		IconSpriteRenderer.transform.localPosition = new Vector3(0f, 0f, Mathf.Sin(WobbleTimer += Time.deltaTime * 2f) * 0.1f - 0.75f);
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
		if (_weaponPickupUI == null)
		{
			_weaponPickupUI = MonoSingleton<UIManager>.Instance.WeaponPickPromptControllerTemplate.Instantiate();
			UIWeaponPickupPromptController weaponPickupUI = _weaponPickupUI;
			weaponPickupUI.OnHidden = (Action)Delegate.Combine(weaponPickupUI.OnHidden, (Action)delegate
			{
				_weaponPickupUI = null;
			});
		}
		_weaponPickupUI.Show(TypeOfWeapon, damage, speed, WeaponLevel);
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
		BuyString = ScriptLocalization.Interactions.Buy;
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(20) >= Cost - 1)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void GetLabel()
	{
		string text = string.Format(BuyString, EquipmentManager.GetEquipmentData(TypeOfWeapon).GetLocalisedTitle() + " " + WeaponLevel.ToNumeral());
		base.Label = text + " | " + GetAffordColor() + FontImageNames.GetIconByType(InventoryItem.ITEM_TYPE.BLACK_GOLD) + " " + Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) + " / " + Cost + "</color>";
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Inventory.GetItemQuantity(20) >= Cost - 1 && !Activated)
		{
			Activated = true;
			for (int i = 0; i < Cost; i++)
			{
				if (i < 10)
				{
					Inventory.GetItemByType(20);
					AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
					ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.gameObject.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
				}
				Inventory.ChangeItemQuantity(20, -1);
			}
			CameraManager.shakeCamera(0.3f, UnityEngine.Random.Range(0, 360));
			switch (Type)
			{
			case Types.Weapon:
				StartCoroutine(PlayerShowWeaponRoutine());
				break;
			case Types.Curse:
				StartCoroutine(PlayerShowCurseRoutine());
				break;
			}
			return;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
		MonoSingleton<Indicator>.Instance.PlayShake();
		if (ShopKeeper.CantAffordBark != null)
		{
			if (ShopKeeper.NormalBark != null)
			{
				ShopKeeper.NormalBark.SetActive(false);
			}
			ShopKeeper.CantAffordBark.SetActive(true);
		}
	}

	private IEnumerator PlayerShowWeaponRoutine()
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.down * 0.5f, base.gameObject);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		if (DataManager.Instance.CurrentWeapon != EquipmentType.None)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), state.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-2f, 2.5f), 270f);
			obj.GetComponent<Interaction_WeaponPickUp>().SetWeapon(DataManager.Instance.CurrentWeapon, PlayerFarming.Instance.playerWeapon.CurrentWeaponLevel, Interaction_WeaponPickUp.Types.Weapon);
		}
		PlayerFarming.Instance.playerWeapon.SetWeapon(TypeOfWeapon, WeaponLevel);
		IconSpriteRenderer.enabled = false;
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
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator PlayerShowCurseRoutine()
	{
		if (base.transform.position.x < state.transform.position.x)
		{
			PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.right * 1.25f, base.gameObject);
		}
		else
		{
			PlayerFarming.Instance.GoToAndStop(base.transform.position + Vector3.left * 1.25f, base.gameObject);
		}
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		FaithAmmo.Reload();
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
		yield return new WaitForSeconds(0.3f);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (DataManager.Instance.CurrentCurse != EquipmentType.None)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), state.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-2f, 2.5f), 270f);
			obj.GetComponent<Interaction_WeaponPickUp>().SetWeapon(DataManager.Instance.CurrentCurse, DataManager.Instance.CurrentCurseLevel, Interaction_WeaponPickUp.Types.Curse);
		}
		PlayerFarming.Instance.playerSpells.SetSpell(TypeOfWeapon, WeaponLevel);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
