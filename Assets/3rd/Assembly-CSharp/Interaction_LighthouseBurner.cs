using System.Collections;
using DG.Tweening;
using I2.Loc;
using Spine.Unity;
using Unify;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_LighthouseBurner : Interaction
{
	public enum State
	{
		Off,
		AddIngredients,
		Disabled,
		Full
	}

	public Structure structure;

	[SerializeField]
	private UnityEvent CallBack;

	public HubShoreManager hubShoreManager;

	private int NumCooked;

	private string sAddFuel;

	private string sAddIngredients;

	private string sRecipe;

	private string sCancel;

	private int Fuel;

	private int RequiredFuel = 15;

	public bool LitLighthouse;

	private string _Label;

	public State CurrentState;

	public GameObject BurnerOn;

	public GameObject BurnerOff;

	public GameObject Light;

	public GameObject litLighthouseConvo;

	public DOTweenAnimation burnerAnimation;

	public GameObject Demons;

	public SkeletonAnimation[] worshippers;

	public SkeletonAnimation leader;

	private bool Activating;

	private bool TurnedOn;

	public override void OnEnableInteraction()
	{
		HasSecondaryInteraction = true;
		base.OnEnableInteraction();
		UpdateLocalisation();
		SetImages();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sAddFuel = ScriptLocalization.Interactions.AddFuel;
		sAddIngredients = ScriptLocalization.Interactions.AddIngredients;
	}

	private string GetAffordColor()
	{
		if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.LOG) > 0)
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	public override void GetLabel()
	{
		switch (CurrentState)
		{
		case State.Off:
			if (DataManager.Instance.LighthouseFuel < RequiredFuel)
			{
				base.Label = string.Join(" ", sAddFuel, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.LOG, RequiredFuel));
			}
			else
			{
				Interactable = false;
				base.Label = "";
			}
			break;
		case State.AddIngredients:
			base.Label = sAddIngredients;
			break;
		case State.Disabled:
			base.Label = "";
			break;
		}
	}

	public void SetImages()
	{
		BurnerOn.SetActive(false);
		BurnerOff.SetActive(false);
		Light.SetActive(false);
		GetLabel();
		if (DataManager.Instance.Lighthouse_Lit)
		{
			Light.SetActive(true);
			BurnerOn.SetActive(true);
			Interactable = false;
		}
		else
		{
			BurnerOff.SetActive(true);
			Interactable = true;
		}
	}

	private void TurnOn()
	{
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FIX_LIGHTHOUSE"));
		AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", base.gameObject);
		for (int i = 0; i < worshippers.Length; i++)
		{
			worshippers[i].AnimationState.SetAnimation(0, "prayer_idle", true);
		}
		leader.AnimationState.SetAnimation(0, "animation", true);
		DataManager.Instance.Lighthouse_Lit = true;
		Demons.SetActive(false);
		burnerAnimation.DOPlay();
		litLighthouseConvo.SetActive(true);
		TurnedOn = true;
		hubShoreManager.LighthouseLit = true;
		Activating = false;
		Interactable = false;
		CurrentState = State.Full;
		SetImages();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (CurrentState != State.Full && !Activating)
		{
			if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.LOG) >= RequiredFuel)
			{
				StartCoroutine(AddFuel());
				return;
			}
			AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	public void ShakeBurner()
	{
		base.gameObject.transform.DOShakeScale(1f, new Vector3(1.5f, 1.5f, 1.5f));
		AudioManager.Instance.PlayOneShot("event:/locations/light_house/fireplace_shake", base.gameObject);
	}

	private IEnumerator AddFuel()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 7f);
		Activating = true;
		int count = 0;
		InventoryItem.ITEM_TYPE ItemToDeposit = InventoryItem.ITEM_TYPE.LOG;
		MMVibrate.RumbleContinuous(0.25f, 0.75f);
		while (count < RequiredFuel)
		{
			Inventory.GetItemByType((int)ItemToDeposit);
			AudioManager.Instance.PlayOneShot("event:/cooking/add_food_ingredient", PlayerFarming.Instance.gameObject);
			ResourceCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, ItemToDeposit, null);
			Inventory.ChangeItemQuantity((int)ItemToDeposit, -1);
			count++;
			yield return new WaitForSeconds(0.1f);
		}
		CameraManager.shakeCamera(10f);
		DataManager.Instance.LighthouseFuel = RequiredFuel;
		DataManager.Instance.Lighthouse_Lit = true;
		TurnOn();
		yield return new WaitForSeconds(0.1f);
		MMVibrate.StopRumble();
		Activating = false;
		BurnerOff.SetActive(false);
		BurnerOn.SetActive(true);
		Light.SetActive(true);
		base.enabled = false;
		GameManager.GetInstance().OnConversationEnd();
	}
}
