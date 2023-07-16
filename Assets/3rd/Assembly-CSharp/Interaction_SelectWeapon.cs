using System.Collections;
using BlendModes;
using FMOD.Studio;
using I2.Loc;
using Rewired;
using Unify.Input;
using UnityEngine;

public class Interaction_SelectWeapon : Interaction
{
	public enum WeaponState
	{
		Locked,
		Unlockable,
		Unlocked,
		Equipped
	}

	public delegate void WeaponSelectEvent(TarotCards.Card weaponType);

	[SerializeField]
	private TarotCards.Card weaponType;

	[SerializeField]
	private int requiredFollowers;

	[SerializeField]
	private SpriteRenderer lockIcon;

	[SerializeField]
	private InventoryItemDisplay itemDisplay;

	[SerializeField]
	private GameObject unlockableParticle;

	[SerializeField]
	private BlendModeEffect blendMode;

	[SerializeField]
	private int index;

	[SerializeField]
	private WeaponIcons[] weaponIcons;

	private Canvas canvas;

	private string sLabel;

	private bool locked;

	private bool withinDistance;

	private const float equipDuration = 1f;

	private const float holdDuration = 3f;

	private float holdTimer;

	private WeaponState weaponState;

	private Player _RewiredController;

	private EventInstance LoopInstance;

	private bool createdLoop;

	private bool fadingOut;

	private float alphaOffset = 1f;

	[HideInInspector]
	public Player RewiredController
	{
		get
		{
			if (_RewiredController == null)
			{
				_RewiredController = RewiredInputManager.MainPlayer;
			}
			return _RewiredController;
		}
	}

	public static event WeaponSelectEvent OnWeaponSelect;

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(DelayedSet());
	}

	private IEnumerator DelayedSet()
	{
		yield return new WaitForEndOfFrame();
		blendMode.enabled = false;
		ActivateDistance = 2f;
		UpdateLocalisation();
		canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		unlockableParticle.SetActive(false);
		SetWeapon(weaponType);
	}

	private void SetWeapon(TarotCards.Card card)
	{
		WeaponIcons[] array = this.weaponIcons;
		foreach (WeaponIcons weaponIcons in array)
		{
			if (weaponIcons.Weapon == card)
			{
				itemDisplay.SetImage(weaponIcons.Sprite);
				break;
			}
		}
		weaponType = card;
		DataManager.Instance.WeaponSelectionPositions[index] = weaponType;
	}

	protected override void Update()
	{
		base.Update();
		itemDisplay.transform.localPosition = new Vector3(0f, 0f, Mathf.Sin(Time.time) * 0.1f);
		lockIcon.gameObject.SetActive(locked);
		itemDisplay.spriteRenderer.color = (locked ? Color.black : Color.white);
		if (!(PlayerFarming.Instance != null))
		{
			return;
		}
		float num = Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position);
		float t = 1f - (num - 5f) / 10f;
		float num2 = Mathf.Lerp(0f, 1f, t);
		itemDisplay.spriteRenderer.color = new Color(itemDisplay.spriteRenderer.color.r, itemDisplay.spriteRenderer.color.g, itemDisplay.spriteRenderer.color.b, num2 * alphaOffset);
		lockIcon.color = new Color(lockIcon.color.r, lockIcon.color.g, lockIcon.color.b, num2);
		if (HoldProgress > 0f)
		{
			if (!createdLoop)
			{
				LoopInstance = AudioManager.Instance.CreateLoop("event:/player/weapon_unlock_loop", base.gameObject, true);
				LoopInstance.setParameterByName("parameter:/unlock", HoldProgress);
				createdLoop = true;
			}
		}
		else
		{
			AudioManager.Instance.StopLoop(LoopInstance);
			createdLoop = false;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		AudioManager.Instance.StopLoop(LoopInstance);
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
	}

	private IEnumerator FadeOut()
	{
		fadingOut = true;
		Interactable = false;
		yield return new WaitForSeconds(0.5f);
		fadingOut = false;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		holdTimer = 0f;
		AudioManager.Instance.StopLoop(LoopInstance);
		if (!locked)
		{
			TarotCards.Card card = weaponType;
			PlayerFarming.Instance.state.GetComponent<PlayerWeapon>();
			blendMode.enabled = true;
			EndIndicateHighlighted();
			AudioManager.Instance.PlayOneShot("event:/player/weapon_equip", base.gameObject);
			WeaponSelectEvent onWeaponSelect = Interaction_SelectWeapon.OnWeaponSelect;
			if (onWeaponSelect != null)
			{
				onWeaponSelect(card);
			}
		}
	}

	public override void GetLabel()
	{
		base.GetLabel();
		switch (weaponState)
		{
		case WeaponState.Locked:
			sLabel = "";
			break;
		case WeaponState.Unlockable:
			sLabel = "";
			break;
		case WeaponState.Unlocked:
			sLabel = ScriptLocalization.Interactions.Equip;
			break;
		case WeaponState.Equipped:
			sLabel = ScriptLocalization.Interactions.Equipped;
			break;
		}
		base.Label = sLabel;
		Interactable = weaponState == WeaponState.Unlockable || weaponState == WeaponState.Unlocked;
		HoldToInteract = weaponState == WeaponState.Unlockable;
	}
}
