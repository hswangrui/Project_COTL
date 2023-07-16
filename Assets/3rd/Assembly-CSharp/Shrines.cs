using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using UnityEngine;

public class Shrines : Interaction
{
	public enum ShrineType
	{
		NONE,
		BLUE_HEARTS,
		RED_HEART,
		BLACK_HEART,
		CURSE,
		WEAPON,
		BLACK_SOULS,
		TAROT_CARD,
		DAMAGE,
		RELIC
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass33_0
	{
		public Shrines _003C_003E4__this;

		public TarotCards.TarotCard card1;

		public TarotCards.TarotCard card2;

		internal void _003CDrawCardRoutine_003Eb__3(TarotCards.TarotCard card)
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.BackToIdleRoutine(card, 0f));
			DataManager.Instance.PlayerRunTrinkets.Remove(_003CDrawCardRoutine_003Eg__GetOther_007C2(card));
		}

		internal void _003CDrawCardRoutine_003Eb__0()
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.BackToIdleRoutine(card1, 0f));
		}

		internal void _003CDrawCardRoutine_003Eb__1()
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.BackToIdleRoutine(card2, 0f));
		}

		internal TarotCards.TarotCard _003CDrawCardRoutine_003Eg__GetOther_007C2(TarotCards.TarotCard card)
		{
			if (card == card1)
			{
				return card2;
			}
			return card1;
		}
	}

	public ShrineType TypeOfShrine;

	public SpriteRenderer Shrine;

	public Light lightOnShrine;

	public GameObject XPBar;

	public SpriteRenderer RadialProgress;

	public InventoryItemDisplay typeOfReward;

	public Sprite ShrineOn;

	public Sprite ShrineOff;

	public int Cost = -1;

	public bool Used;

	public float TimeUsedShrine = -1f;

	private GameObject Player;

	private bool Activating;

	private float value;

	public float subtractedValue;

	public float dividedValue;

	public float currentGameTime;

	public float RechargeTime;

	private float Timer;

	private PickUp p;

	private TarotCards.TarotCard DrawnCard;

	private void SetTypeOfReward()
	{
		switch (TypeOfShrine)
		{
		case ShrineType.BLUE_HEARTS:
			typeOfReward.SetImage(InventoryItem.ITEM_TYPE.BLUE_HEART);
			break;
		case ShrineType.RED_HEART:
			typeOfReward.SetImage(InventoryItem.ITEM_TYPE.RED_HEART);
			break;
		case ShrineType.BLACK_HEART:
			typeOfReward.SetImage(InventoryItem.ITEM_TYPE.BLACK_HEART);
			break;
		case ShrineType.TAROT_CARD:
			typeOfReward.SetImage(InventoryItem.ITEM_TYPE.TRINKET_CARD);
			break;
		case ShrineType.CURSE:
		case ShrineType.WEAPON:
		case ShrineType.BLACK_SOULS:
			break;
		}
	}

	public int GetCostQuantity()
	{
		if (Cost == -1)
		{
			switch (TypeOfShrine)
			{
			case ShrineType.BLUE_HEARTS:
				return 25;
			case ShrineType.RED_HEART:
				return DataManager.RedHeartShrineCosts[DataManager.Instance.RedHeartShrineLevel];
			case ShrineType.BLACK_HEART:
				return 35;
			case ShrineType.TAROT_CARD:
				if (DataManager.Instance.DeathCatBeaten)
				{
					switch (PlayerFarming.Location)
					{
					case FollowerLocation.Dungeon1_1:
						return 50;
					case FollowerLocation.Dungeon1_2:
						return 55;
					case FollowerLocation.Dungeon1_3:
						return 65;
					case FollowerLocation.Dungeon1_4:
						return 70;
					}
				}
				else
				{
					switch (PlayerFarming.Location)
					{
					case FollowerLocation.Dungeon1_1:
						if (!DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_1))
						{
							return 15;
						}
						return 20;
					case FollowerLocation.Dungeon1_2:
						if (!DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2))
						{
							return 20;
						}
						return 30;
					case FollowerLocation.Dungeon1_3:
						if (!DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3))
						{
							return 30;
						}
						return 45;
					case FollowerLocation.Dungeon1_4:
						if (!DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_4))
						{
							return 40;
						}
						return 50;
					}
				}
				return 15;
			case ShrineType.RELIC:
			{
				int count = DataManager.Instance.BossesCompleted.Count;
				if (count < 2)
				{
					return 25;
				}
				if (count < 3)
				{
					return 50;
				}
				if (count < 4)
				{
					return 75;
				}
				return 100;
			}
			case ShrineType.WEAPON:
				return 25;
			case ShrineType.DAMAGE:
				return 25;
			default:
				return 25;
			}
		}
		return Cost;
	}

	public int GetRechargeTime()
	{
		ShrineType typeOfShrine = TypeOfShrine;
		if (typeOfShrine == ShrineType.RED_HEART)
		{
			return 0;
		}
		return 1000;
	}

	public override void GetLabel()
	{
		int num;
		switch (TypeOfShrine)
		{
		case ShrineType.WEAPON:
			num = 5;
			break;
		case ShrineType.CURSE:
			num = 8;
			break;
		default:
			num = 0;
			break;
		}
		if (DataManager.Instance.Followers.Count < num)
		{
			base.Label = ScriptLocalization.Interactions.Requires + " " + num + " <sprite name=\"icon_Followers\">";
			Interactable = false;
			return;
		}
		if (GetShrineUsedTime() == -1f)
		{
			base.Label = string.Join(": ", ScriptLocalization.Interactions.MakeOffering, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, GetCostQuantity()));
			Interactable = true;
		}
		else
		{
			base.Label = ScriptLocalization.Interactions.Recharging;
			Interactable = false;
		}
		if (TypeOfShrine == ShrineType.TAROT_CARD && TarotCards.DrawRandomCard() == null)
		{
			base.Label = "";
			Interactable = false;
		}
	}

	private void UpdateVisuals()
	{
		if (DataManager.Instance.dungeonRun < 10 && TypeOfShrine == ShrineType.TAROT_CARD)
		{
			base.gameObject.SetActive(false);
		}
		if (TypeOfShrine == ShrineType.RELIC && !DataManager.Instance.OnboardedRelics)
		{
			base.gameObject.SetActive(false);
		}
		if (GetShrineUsedTime() != -1f)
		{
			Shrine.sprite = ShrineOff;
			Shrine.color = new Color(0.5f, 0.5f, 0.5f);
			lightOnShrine.enabled = false;
			XPBar.SetActive(true);
		}
		else
		{
			Shrine.sprite = ShrineOn;
			XPBar.SetActive(false);
			Shrine.color = Color.white;
			lightOnShrine.enabled = true;
		}
	}

	public override void OnEnableInteraction()
	{
		SetTypeOfReward();
		TimeUsedShrine = GetShrineUsedTime();
		RadialProgress.material.SetFloat("_Angle", 90f);
		ActivateDistance = 2f;
		UpdateVisuals();
		base.OnEnableInteraction();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (GetCostQuantity() <= Inventory.GetItemQuantity(20))
		{
			StartCoroutine(GiveGold());
			return;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.transform.position);
		MonoSingleton<Indicator>.Instance.PlayShake();
	}

	private new void Update()
	{
		currentGameTime = TimeManager.TotalElapsedGameTime;
		RechargeTime = GetRechargeTime();
		if (!((Player = GameObject.FindWithTag("Player")) == null) && GetShrineUsedTime() != -1f)
		{
			Used = true;
			if (!XPBar.activeSelf)
			{
				XPBar.SetActive(true);
			}
			value = TimeManager.TotalElapsedGameTime - GetShrineUsedTime();
			subtractedValue = value;
			value /= GetRechargeTime();
			dividedValue = value;
			RadialProgress.material.SetFloat("_Frac", value);
			if (value >= 1f)
			{
				Used = false;
				TimeUsedShrine = -1f;
				SetShrineUsedTime(TimeUsedShrine);
				UpdateVisuals();
				value = 0f;
			}
		}
	}

	private IEnumerator GiveGold()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		Vector3 targetPosition = base.transform.position - new Vector3(0f, 1f, 0f);
		PlayerFarming.Instance.GoToAndStop(targetPosition);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("specials/special-activate-long", 0, false);
		int cost = GetCostQuantity();
		Inventory.ChangeItemQuantity(20, -cost);
		for (int i = 0; i < Mathf.Min(cost, 15); i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", state.gameObject.transform.position);
			ResourceCustomTarget.Create(base.gameObject, state.gameObject.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSeconds(2f / (float)GetCostQuantity());
		}
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("pray", 0, false);
		yield return new WaitForSeconds(2f);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.25f, 0.5f);
		GiveReward();
		SetShrineUsed();
		base.transform.DOShakeScale(0.5f);
	}

	private void GiveReward()
	{
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", base.transform.position);
		Debug.Log("GiveReward!");
		switch (TypeOfShrine)
		{
		case ShrineType.BLUE_HEARTS:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, base.transform.position + Vector3.down, 0f);
			break;
		case ShrineType.RED_HEART:
		{
			HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
			component.totalHP += 1f;
			component.HP = component.totalHP;
			DataManager.Instance.RedHeartShrineLevel++;
			break;
		}
		case ShrineType.BLACK_HEART:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_HEART, 1, base.transform.position + Vector3.down, 0f);
			break;
		case ShrineType.TAROT_CARD:
			Debug.Log("TAROT CARD!");
			StartCoroutine(DrawCardRoutine());
			break;
		case ShrineType.RELIC:
			p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.RELIC, 1, base.transform.position, 0f);
			p.SetInitialSpeedAndDiraction(6f, 270f);
			p.MagnetDistance = 2f;
			p.CanStopFollowingPlayer = false;
			GameManager.GetInstance().CameraResetTargetZoom();
			GameManager.GetInstance().OnConversationEnd();
			break;
		case ShrineType.CURSE:
			p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_CURSE, 1, base.transform.position + Vector3.back, 0f);
			p.SetInitialSpeedAndDiraction(3f, 270f);
			p.MagnetDistance = 2f;
			p.CanStopFollowingPlayer = false;
			break;
		case ShrineType.WEAPON:
			p = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_WEAPON, 1, base.transform.position + Vector3.back, 0f);
			p.SetInitialSpeedAndDiraction(3f, 270f);
			p.MagnetDistance = 2f;
			p.CanStopFollowingPlayer = false;
			break;
		case ShrineType.DAMAGE:
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.TRINKET_CARD, 1, base.transform.position + Vector3.down, 0f).GetComponent<Interaction_TarotCard>().CardOverride = TarotCards.Card.Moon;
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.TRINKET_CARD, 1, base.transform.position + Vector3.down, 0f).GetComponent<Interaction_TarotCard>().CardOverride = TarotCards.Card.Sun;
			break;
		case ShrineType.BLACK_SOULS:
			break;
		}
	}

	private TarotCards.TarotCard GetCard()
	{
		TarotCards.TarotCard tarotCard;
		if (!DataManager.Instance.FirstTarot)
		{
			DataManager.Instance.FirstTarot = true;
			tarotCard = new TarotCards.TarotCard(TarotCards.Card.Lovers1, 0);
		}
		else
		{
			tarotCard = TarotCards.DrawRandomCard();
		}
		if (tarotCard != null)
		{
			DataManager.Instance.PlayerRunTrinkets.Add(tarotCard);
		}
		return tarotCard;
	}

	private IEnumerator DrawCardRoutine()
	{
		_003C_003Ec__DisplayClass33_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass33_0();
		CS_0024_003C_003E8__locals0._003C_003E4__this = this;
		HUD_Manager.Instance.Hide(false, 0);
		GameManager.GetInstance().CameraSetTargetZoom(4f);
		LetterBox.Show(false);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.state.facingAngle = -90f;
		CameraFollowTarget.Instance.DisablePlayerLook = true;
		PlayerFarming instance = PlayerFarming.Instance;
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", base.gameObject);
		instance.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
		instance.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0f);
		yield return new WaitForSeconds(0.1f);
		GameManager.GetInstance().CameraSetTargetZoom(6f);
		CS_0024_003C_003E8__locals0.card1 = GetCard();
		CS_0024_003C_003E8__locals0.card2 = GetCard();
		if (CS_0024_003C_003E8__locals0.card1 != null && CS_0024_003C_003E8__locals0.card2 != null)
		{
			UITarotChoiceOverlayController tarotChoiceOverlayInstance = MonoSingleton<UIManager>.Instance.ShowTarotChoice(CS_0024_003C_003E8__locals0.card1, CS_0024_003C_003E8__locals0.card2);
			UITarotChoiceOverlayController uITarotChoiceOverlayController = tarotChoiceOverlayInstance;
			uITarotChoiceOverlayController.OnTarotCardSelected = (Action<TarotCards.TarotCard>)Delegate.Combine(uITarotChoiceOverlayController.OnTarotCardSelected, (Action<TarotCards.TarotCard>)delegate(TarotCards.TarotCard card)
			{
				CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.BackToIdleRoutine(card, 0f));
				DataManager.Instance.PlayerRunTrinkets.Remove(CS_0024_003C_003E8__locals0._003CDrawCardRoutine_003Eg__GetOther_007C2(card));
			});
			UITarotChoiceOverlayController uITarotChoiceOverlayController2 = tarotChoiceOverlayInstance;
			uITarotChoiceOverlayController2.OnHidden = (Action)Delegate.Combine(uITarotChoiceOverlayController2.OnHidden, (Action)delegate
			{
				tarotChoiceOverlayInstance = null;
			});
		}
		else
		{
			if (CS_0024_003C_003E8__locals0.card1 == null && CS_0024_003C_003E8__locals0.card2 == null)
			{
				yield break;
			}
			if (CS_0024_003C_003E8__locals0.card1 != null)
			{
				UITrinketCards.Play(CS_0024_003C_003E8__locals0.card1, delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.BackToIdleRoutine(CS_0024_003C_003E8__locals0.card1, 0f));
				});
			}
			else if (CS_0024_003C_003E8__locals0.card2 != null)
			{
				UITrinketCards.Play(CS_0024_003C_003E8__locals0.card2, delegate
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.BackToIdleRoutine(CS_0024_003C_003E8__locals0.card2, 0f));
				});
			}
		}
	}

	private void BackToIdle()
	{
		StartCoroutine(BackToIdleRoutine());
	}

	private IEnumerator BackToIdleRoutine(TarotCards.TarotCard card, float delay)
	{
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
		GameManager.GetInstance().CameraResetTargetZoom();
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		yield return null;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		StopAllCoroutines();
		GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine(card, delay));
	}

	private IEnumerator DelayEffectsRoutine(TarotCards.TarotCard card, float delay)
	{
		yield return new WaitForSeconds(0.2f + delay);
		TrinketManager.AddTrinket(card);
	}

	private IEnumerator BackToIdleRoutine()
	{
		Time.timeScale = 1f;
		LetterBox.Hide();
		HUD_Manager.Instance.Show(0);
		AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", base.gameObject);
		GameManager.GetInstance().CameraResetTargetZoom();
		CameraFollowTarget.Instance.DisablePlayerLook = false;
		yield return null;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		PlayerFarming.Instance.SpineUseDeltaTime(true);
		SimpleSetCamera.EnableAll();
		GameManager.GetInstance().StartCoroutine(DelayEffectsRoutine());
	}

	private IEnumerator DelayEffectsRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		TrinketManager.AddTrinket(DrawnCard);
	}

	private void SetShrineUsedTime(float Set)
	{
		foreach (ShrineUsageInfo item in DataManager.Instance.ShrineTimerInfo)
		{
			if (item.TypeOfShrine == TypeOfShrine)
			{
				item.useTime = Set;
			}
		}
	}

	private float GetShrineUsedTime()
	{
		foreach (ShrineUsageInfo item in DataManager.Instance.ShrineTimerInfo)
		{
			if (item.TypeOfShrine == TypeOfShrine)
			{
				return item.useTime;
			}
		}
		return -1f;
	}

	private void SetShrineUsed()
	{
		Used = true;
		bool flag = false;
		TimeUsedShrine = TimeManager.TotalElapsedGameTime;
		Debug.Log(TimeManager.TotalElapsedGameTime);
		UpdateVisuals();
		foreach (ShrineUsageInfo item in DataManager.Instance.ShrineTimerInfo)
		{
			if (item.TypeOfShrine == TypeOfShrine)
			{
				flag = true;
				item.useTime = TimeUsedShrine;
			}
		}
		if (!flag)
		{
			ShrineUsageInfo shrineUsageInfo = new ShrineUsageInfo();
			shrineUsageInfo.TypeOfShrine = TypeOfShrine;
			shrineUsageInfo.useTime = TimeUsedShrine;
			DataManager.Instance.ShrineTimerInfo.Add(shrineUsageInfo);
		}
	}
}
