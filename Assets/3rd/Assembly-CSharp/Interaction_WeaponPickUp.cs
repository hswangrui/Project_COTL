using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using src.UI.Prompts;
using UnityEngine;

public class Interaction_WeaponPickUp : Interaction
{
	public enum Types
	{
		Weapon,
		Curse
	}

	public EquipmentType TypeOfWeapon;

	public Types Type;

	public int WeaponLevel = -1;

	public int DurabilityLevel = -1;

	private Canvas canvas;

	private float DamageDifference;

	private string DamageDifferenceString;

	public SpriteRenderer IconSpriteRenderer;

	public SpriteRenderer ShadowSpriteRenderer;

	public SpriteRenderer weaponBetterIcon;

	public Sprite weaponUp;

	public Sprite weaponDown;

	private UIWeaponPickupPromptController _weaponPickupUI;

	private string sLabel;

	private string sRecycle;

	private EquipmentType cacheCurse;

	private EquipmentType cacheWeapon;

	private bool Activated;

	private int RecycleCost = 1;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		IconSpriteRenderer.transform.localScale = Vector3.one * 1.5f;
		canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		UpdateLocalisation();
		HasSecondaryInteraction = true;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.Equip;
		sRecycle = ScriptLocalization.Interactions.Recycle;
	}

	protected override void Update()
	{
		base.Update();
		if (cacheWeapon != DataManager.Instance.CurrentWeapon || cacheCurse != DataManager.Instance.CurrentWeapon)
		{
			CheckWeaponLevel();
		}
	}

	private void CheckWeaponLevel()
	{
		weaponBetterIcon.enabled = false;
		cacheCurse = DataManager.Instance.CurrentCurse;
		cacheCurse = DataManager.Instance.CurrentWeapon;
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
		}
		else if (Type == Types.Weapon)
		{
			cacheCurse = TypeOfWeapon;
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
		}
	}

	private Sprite GetIcon()
	{
		return EquipmentManager.GetEquipmentData(TypeOfWeapon).WorldSprite;
	}

	public void SetWeapon(EquipmentType TypeOfWeapon, int WeaponLevel, Types Type)
	{
		this.TypeOfWeapon = TypeOfWeapon;
		this.WeaponLevel = WeaponLevel;
		this.Type = Type;
		IconSpriteRenderer.sprite = GetIcon();
		CheckWeaponLevel();
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sLabel);
	}

	public override void GetSecondaryLabel()
	{
		base.SecondaryLabel = (Activated ? "" : sRecycle);
	}

	public override void OnInteract(StateMachine state)
	{
		Activated = true;
		base.OnInteract(state);
		weaponBetterIcon.enabled = false;
		switch (Type)
		{
		case Types.Weapon:
			StartCoroutine(PlayerShowWeaponRoutine());
			break;
		case Types.Curse:
			StartCoroutine(PlayerShowCurseRoutine());
			break;
		}
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		Activated = true;
		base.OnSecondaryInteract(state);
		InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, RecycleCost, base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/shop/buy", base.transform.position);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator PlayerShowWeaponRoutine()
	{
		if (DataManager.Instance.CurrentWeapon != EquipmentType.None)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), state.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-2f, 2.5f), Utils.GetAngle(base.transform.position, Vector3.zero));
			obj.GetComponent<Interaction_WeaponPickUp>().SetWeapon(DataManager.Instance.CurrentWeapon, PlayerFarming.Instance.playerWeapon.CurrentWeaponLevel, Type);
		}
		PlayerFarming.Instance.playerWeapon.SetWeapon(TypeOfWeapon, WeaponLevel);
		IconSpriteRenderer.enabled = false;
		ShadowSpriteRenderer.enabled = false;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		AudioManager.Instance.PlayOneShot("event:/player/weapon_equip", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/weapon_unlocked", base.transform.position);
		float duration = PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, EquipmentManager.GetEquipmentData(TypeOfWeapon).PickupAnimationKey, false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator PlayerShowCurseRoutine()
	{
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
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		GameManager.GetInstance().CameraSetTargetZoom(8f);
		AudioManager.Instance.PlayOneShot("event:/player/absorb_curse", base.gameObject);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "Curses/curse-get", false);
		IconSpriteRenderer.transform.DOShakePosition(2f, 0.25f);
		IconSpriteRenderer.transform.DOShakeRotation(2f, new Vector3(0f, 0f, 15f));
		Sequence sequence = DOTween.Sequence();
		sequence.Append(IconSpriteRenderer.transform.DOScale(Vector3.one * 1.25f, 0.2f));
		sequence.Append(IconSpriteRenderer.transform.DOScale(Vector3.one * 0.75f, 0.2f));
		sequence.Play().SetLoops(-1);
		yield return new WaitForSeconds(1.0333333f);
		IconSpriteRenderer.enabled = false;
		ShadowSpriteRenderer.enabled = false;
		yield return new WaitForSeconds(0.3f);
		if (DataManager.Instance.CurrentCurse != EquipmentType.None)
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Resources/WeaponPickUp"), state.transform.position, Quaternion.identity, base.transform.parent) as GameObject;
			obj.GetComponent<PickUp>().SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-2f, 2.5f), Utils.GetAngle(base.transform.position, Vector3.zero));
			obj.GetComponent<Interaction_WeaponPickUp>().SetWeapon(DataManager.Instance.CurrentCurse, DataManager.Instance.CurrentCurseLevel, Type);
		}
		PlayerFarming.Instance.playerSpells.SetSpell(TypeOfWeapon, WeaponLevel);
		GameManager.GetInstance().CameraResetTargetZoom();
		state.CURRENT_STATE = StateMachine.State.Idle;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
		switch (Type)
		{
		case Types.Weapon:
		{
			Action<bool> onHighlightWeapon = Interaction_WeaponSelectionPodium.OnHighlightWeapon;
			if (onHighlightWeapon != null)
			{
				onHighlightWeapon(true);
			}
			break;
		}
		case Types.Curse:
		{
			Action<bool> onHighlightCurse = Interaction_WeaponSelectionPodium.OnHighlightCurse;
			if (onHighlightCurse != null)
			{
				onHighlightCurse(true);
			}
			break;
		}
		}
		float damage = 0f;
		float speed = 0f;
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
		AudioManager.Instance.PlayOneShot("event:/ui/open_menu", PlayerFarming.Instance.transform.position);
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
		switch (Type)
		{
		case Types.Weapon:
		{
			Action<bool> onHighlightWeapon = Interaction_WeaponSelectionPodium.OnHighlightWeapon;
			if (onHighlightWeapon != null)
			{
				onHighlightWeapon(false);
			}
			break;
		}
		case Types.Curse:
		{
			Action<bool> onHighlightCurse = Interaction_WeaponSelectionPodium.OnHighlightCurse;
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
}
