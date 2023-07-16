using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Interaction_JellyFishBank : Interaction
{
	public enum States
	{
		Intro,
		AwaitDonation,
		AwaitInvestment,
		Active,
		DonationProcessing,
		DonationProcessed
	}

	public States State;

	private int InvestmentAmount = 350;

	private int DonationAmount = 10;

	private bool Activated;

	private GameObject g;

	public Interaction_SimpleConversation IntroConversation;

	public Interaction_SimpleConversation GiveMoneyConversation;

	public Interaction_SimpleConversation RefuseConversation;

	public SkeletonAnimation ReceiveDonationSpine;

	[SerializeField]
	private bool overrideReward;

	[SerializeField]
	private float rewardNumber = -1f;

	private GameObject Target;

	private bool ShowingIndicator;

	private int cacheDay;

	private void Start()
	{
		if (!DataManager.Instance.MidasBankIntro)
		{
			State = States.Intro;
		}
		else if (DataManager.Instance.midasDonation == null)
		{
			if (!DataManager.Instance.MidasBankUnlocked)
			{
				State = States.AwaitDonation;
			}
			else
			{
				State = States.AwaitInvestment;
			}
		}
		else if (TimeManager.CurrentDay - DataManager.Instance.midasDonation.Day > 2)
		{
			State = States.DonationProcessed;
		}
		else
		{
			State = States.DonationProcessing;
		}
	}

	private void LeaveMenu()
	{
		StartCoroutine(LeaveMenuRoutine());
	}

	private IEnumerator LeaveMenuRoutine()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		Debug.Log("Leave Menu called");
		yield return new WaitForSeconds(1f);
		Object.Destroy(g);
		Activated = false;
		Interactable = true;
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < InvestmentAmount)
		{
			return "<color=red>";
		}
		return "<color=#f4ecd3>";
	}

	private string GetAffordDonationColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GOLD_REFINED) < DonationAmount)
		{
			return "<color=red>";
		}
		return "<color=#f4ecd3>";
	}

	public override void GetLabel()
	{
		switch (State)
		{
		case States.Intro:
			base.Label = ScriptLocalization.Interactions.Look;
			break;
		case States.AwaitDonation:
			base.Label = string.Format(ScriptLocalization.UI_ItemSelector_Context.Give, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.GOLD_REFINED, DonationAmount));
			break;
		case States.AwaitInvestment:
			base.Label = string.Format(ScriptLocalization.UI_ItemSelector_Context.Give, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, InvestmentAmount));
			break;
		case States.DonationProcessing:
		{
			Interactable = false;
			string text = ((!CheckSingleDayLeft()) ? ScriptLocalization.Interactions_Bank.Processing_plural : ScriptLocalization.Interactions_Bank.Processing);
			string text2 = text.Replace("{0}", GetDaysRemaining());
			base.Label = text2;
			break;
		}
		case States.DonationProcessed:
			Interactable = true;
			base.Label = ScriptLocalization.Interactions_Bank.Withdrawl;
			break;
		case States.Active:
			base.Label = ScriptLocalization.Interactions.Buy;
			break;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		switch (State)
		{
		case States.Intro:
			IntroConversation.gameObject.SetActive(true);
			IntroConversation.Callback.AddListener(delegate
			{
				DataManager.Instance.MidasBankIntro = true;
				base.enabled = true;
				Start();
			});
			base.enabled = false;
			break;
		case States.AwaitDonation:
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.GOLD_REFINED) < DonationAmount)
			{
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
			else
			{
				StartCoroutine(UnlockBank());
			}
			break;
		case States.AwaitInvestment:
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < InvestmentAmount)
			{
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
			else
			{
				StartCoroutine(DepositRoutine());
			}
			break;
		case States.DonationProcessing:
			Interactable = false;
			break;
		case States.DonationProcessed:
			Interactable = true;
			GetReward();
			break;
		case States.Active:
			state.CURRENT_STATE = StateMachine.State.InActive;
			break;
		}
	}

	private string GetDaysRemaining()
	{
		return (DataManager.Instance.midasDonation.Day - TimeManager.CurrentDay + 3).ToString();
	}

	private bool CheckSingleDayLeft()
	{
		if (DataManager.Instance.midasDonation.Day - TimeManager.CurrentDay + 3 == 1)
		{
			return true;
		}
		return false;
	}

	private void CheckRewardProcessed()
	{
		if (TimeManager.CurrentDay - DataManager.Instance.midasDonation.Day > 2)
		{
			State = States.DonationProcessed;
		}
		else
		{
			State = States.DonationProcessing;
		}
	}

	public void GetReward()
	{
		StartCoroutine(GetRewardRoutine());
	}

	private IEnumerator GetRewardRoutine()
	{
		ReceiveDonationSpine.gameObject.SetActive(false);
		State = States.Active;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		base.gameObject.transform.DOShakePosition(1f, new Vector3(0.1f, 0f, 0f));
		BiomeConstants.Instance.ShakeCamera();
		Vector3 position = base.transform.position;
		AudioManager.Instance.PlayOneShot("event:/locations/light_house/fireplace_shake", position);
		yield return new WaitForSeconds(2f);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(position);
		if (!overrideReward)
		{
			rewardNumber = Random.Range(1, 10);
		}
		if (rewardNumber <= 2f)
		{
			ConversationEntry conversationEntry = new ConversationEntry(base.gameObject, string.Format(ScriptLocalization.Interactions_Bank.BadReward), "");
			conversationEntry.Speaker = OutlineTarget;
			conversationEntry.CharacterName = "<color=yellow>" + ScriptLocalization.Interactions_Bank.Title + "</color>";
			MMConversation.Play(new ConversationObject(new List<ConversationEntry> { conversationEntry }, null, delegate
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.POOP, Random.Range(5, 10), base.transform.position);
				GameManager.GetInstance().OnConversationEnd();
			}));
		}
		else if (rewardNumber > 2f && rewardNumber <= 5f)
		{
			ConversationEntry conversationEntry2 = new ConversationEntry(base.gameObject, string.Format(ScriptLocalization.Interactions_Bank.OkayReward), "");
			conversationEntry2.Speaker = OutlineTarget;
			conversationEntry2.CharacterName = "<color=yellow>" + ScriptLocalization.Interactions_Bank.Title + "</color>";
			int reward3 = Random.Range(350, 500);
			ConversationEntry item = new ConversationEntry(base.gameObject, string.Format(reward3 + "<sprite name=\"icon_blackgold\">" + InventoryItem.LocalizedName(InventoryItem.ITEM_TYPE.BLACK_GOLD)), "");
			MMConversation.Play(new ConversationObject(new List<ConversationEntry> { conversationEntry2, item }, null, delegate
			{
				GiveCoins(reward3, InventoryItem.ITEM_TYPE.BLACK_GOLD);
				GameManager.GetInstance().OnConversationEnd();
			}));
		}
		else if (rewardNumber > 5f)
		{
			ConversationEntry conversationEntry3 = new ConversationEntry(base.gameObject, string.Format(ScriptLocalization.Interactions_Bank.GoodReward), "");
			conversationEntry3.Speaker = OutlineTarget;
			conversationEntry3.CharacterName = "<color=yellow>" + ScriptLocalization.Interactions_Bank.Title + "</color>";
			int reward2 = Random.Range(500, 700);
			ConversationEntry item2 = new ConversationEntry(base.gameObject, string.Format(reward2 + "<sprite name=\"icon_blackgold\">" + InventoryItem.LocalizedName(InventoryItem.ITEM_TYPE.BLACK_GOLD)), "");
			MMConversation.Play(new ConversationObject(new List<ConversationEntry> { conversationEntry3, item2 }, null, delegate
			{
				GiveCoins(reward2, InventoryItem.ITEM_TYPE.BLACK_GOLD);
				GameManager.GetInstance().OnConversationEnd();
			}));
		}
		else if (rewardNumber > 9f)
		{
			ConversationEntry conversationEntry4 = new ConversationEntry(base.gameObject, string.Format(ScriptLocalization.Interactions_Bank.GreatReward), "");
			conversationEntry4.Speaker = OutlineTarget;
			conversationEntry4.CharacterName = "<color=yellow>" + ScriptLocalization.Interactions_Bank.Title + "</color>";
			int reward = Random.Range(700, 1000);
			ConversationEntry item3 = new ConversationEntry(base.gameObject, string.Format(reward + "<sprite name=\"icon_blackgold\">" + InventoryItem.LocalizedName(InventoryItem.ITEM_TYPE.BLACK_GOLD)), "");
			MMConversation.Play(new ConversationObject(new List<ConversationEntry> { conversationEntry4, item3 }, null, delegate
			{
				GiveCoins(reward, InventoryItem.ITEM_TYPE.BLACK_GOLD);
				GameManager.GetInstance().OnConversationEnd();
			}));
		}
		DataManager.Instance.midasDonation = null;
		Start();
		Debug.Log("Gave Reward!");
	}

	private void GiveCoins(int quantity, InventoryItem.ITEM_TYPE item)
	{
		StartCoroutine(GiveCoinsRoutine(quantity, item));
	}

	private IEnumerator GiveCoinsRoutine(int quantity, InventoryItem.ITEM_TYPE item)
	{
		float num = 1f;
		float increment = num / 20f;
		for (int i = 0; i < 20; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
			ResourceCustomTarget.Create(PlayerFarming.Instance.CameraBone.gameObject, base.transform.position, item, null);
			yield return new WaitForSeconds(increment);
		}
		yield return new WaitForSeconds(0.5f);
		Inventory.ChangeItemQuantity((int)item, quantity);
	}

	private IEnumerator DepositRoutine()
	{
		State = States.Active;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		Target = base.gameObject;
		yield return new WaitForSeconds(1f);
		float num = 1f;
		float increment = num / 20f;
		for (int i = 0; i < 20; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
			ResourceCustomTarget.Create(Target, PlayerFarming.Instance.CameraBone.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSeconds(increment);
		}
		yield return new WaitForSeconds(0.5f);
		Inventory.ChangeItemQuantity(20, -InvestmentAmount);
		GameManager.GetInstance().OnConversationEnd();
		MidasDonation midasDonation = new MidasDonation();
		midasDonation.Day = TimeManager.CurrentDay;
		midasDonation.InvestmentAmount = 350;
		DataManager.Instance.midasDonation = midasDonation;
		base.HasChanged = true;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		Start();
		Debug.Log("Gave Money!");
		yield return new WaitForSeconds(0.5f);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.3f);
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
	}

	private IEnumerator UnlockBank()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.PayMidas);
		yield return new WaitForSeconds(1f);
		State = States.Active;
		ShowingIndicator = false;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		if (!DataManager.Instance.MidasBankUnlocked)
		{
			ReceiveDonationSpine.gameObject.SetActive(true);
			ReceiveDonationSpine.AnimationState.SetAnimation(0, "enter", false);
			ReceiveDonationSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
			Target = ReceiveDonationSpine.gameObject;
		}
		yield return new WaitForSeconds(1f);
		float num = 1f;
		float increment = num / 20f;
		for (int i = 0; i < 10; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", base.transform.position);
			ResourceCustomTarget.Create(Target, PlayerFarming.Instance.CameraBone.transform.position, InventoryItem.ITEM_TYPE.GOLD_REFINED, null);
			yield return new WaitForSeconds(increment);
		}
		yield return new WaitForSeconds(0.5f);
		Inventory.ChangeItemQuantity(86, -DonationAmount);
		GameManager.GetInstance().OnConversationEnd();
		GiveMoneyConversation.gameObject.SetActive(true);
		GiveMoneyConversation.Callback.AddListener(delegate
		{
			DataManager.Instance.MidasBankIntro = true;
			DataManager.Instance.MidasBankUnlocked = true;
			base.HasChanged = true;
			base.enabled = true;
			GameManager.GetInstance().OnConversationEnd();
			Start();
		});
		base.enabled = false;
		Debug.Log("Gave Money!");
	}

	protected override void Update()
	{
		base.Update();
		States states = State;
		if (states == States.DonationProcessing && cacheDay != TimeManager.CurrentDay)
		{
			base.HasChanged = true;
			Start();
			cacheDay = TimeManager.CurrentDay;
		}
	}
}
