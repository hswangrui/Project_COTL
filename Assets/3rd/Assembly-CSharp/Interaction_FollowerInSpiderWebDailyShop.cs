using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_FollowerInSpiderWebDailyShop : Interaction
{
	private string sRequires;

	public bool ForceSpecificSkin;

	[SpineSkin("", "", true, false, false)]
	public string ForceSkin = "";

	private bool doneCantAffordAnimation;

	public GameObject ShopKeeper;

	public SkeletonAnimation ShopKeeperSpine;

	public SkeletonAnimation followerSpine;

	public int Cost;

	public List<int> _Cost = new List<int> { 0, 5, 10, 20, 30 };

	private FollowerInfo _followerInfo;

	private FollowerOutfit _outfit;

	private FollowerInfoManager wim;

	private int CostMax;

	private bool Activated;

	public SimpleBark simpleBarkBeforeCard;

	private StateMachine CompanionState;

	public GameObject ConversionBone;

	private void Start()
	{
		if (DataManager.Instance.LastDayUsedFollowerShop != TimeManager.CurrentDay)
		{
			CreateNewFollower();
		}
		else
		{
			SetExistingFollower();
		}
	}

	public override void OnEnableInteraction()
	{
		if (DataManager.Instance.LastDayUsedFollowerShop != TimeManager.CurrentDay)
		{
			CreateNewFollower();
		}
		else
		{
			SetExistingFollower();
		}
		base.OnEnableInteraction();
	}

	private void SetExistingFollower()
	{
		_followerInfo = DataManager.Instance.FollowerForSale;
		if (_followerInfo != null)
		{
			_outfit = new FollowerOutfit(_followerInfo);
			_outfit.SetOutfit(followerSpine, false);
			SetCost();
			UpdateLocalisation();
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	private void SetCost()
	{
		CostMax = Mathf.Clamp(DataManager.Instance.Followers.Count, 0, 5);
		Cost = _Cost[Mathf.Min(_Cost.Count, CostMax)];
	}

	private void CreateNewFollower()
	{
		DataManager.Instance.LastDayUsedFollowerShop = TimeManager.CurrentDay;
		_followerInfo = FollowerInfo.NewCharacter(PlayerFarming.Location, ForceSpecificSkin ? ForceSkin : "");
		_outfit = new FollowerOutfit(_followerInfo);
		_outfit.SetOutfit(followerSpine, false);
		DataManager.Instance.FollowerForSale = _followerInfo;
		SetCost();
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sRequires = ScriptLocalization.Interactions.Requires;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : (sRequires + " " + Cost + " <sprite name=\"icon_blackgold\">"));
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Inventory.GetItemQuantity(20) >= Cost || CheatConsole.BuildingsFree)
		{
			StartCoroutine(Purchase());
		}
		else
		{
			SpiderAnimationCantAfford();
		}
	}

	private IEnumerator Purchase()
	{
		simpleBarkBeforeCard.Close();
		simpleBarkBeforeCard.enabled = false;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(ConversionBone, 4f);
		GameManager.GetInstance().AddPlayerToCamera();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		for (int i = 0; i < Cost; i++)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", base.transform.position);
			ResourceCustomTarget.Create(ShopKeeper, base.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
			yield return new WaitForSeconds(0.1f);
		}
		Inventory.ChangeItemQuantity(20, -Cost);
		StartCoroutine(BoughtFollower());
		Activated = true;
	}

	private void SpiderAnimationBoughtItem()
	{
		ShopKeeperSpine.AnimationState.SetAnimation(0, "buy", true);
	}

	private void SpiderAnimationCantAfford()
	{
		ShopKeeperSpine.AnimationState.SetAnimation(0, "cant-afford", false);
		ShopKeeperSpine.AnimationState.AddAnimation(0, "animation", true, 1.7f);
	}

	private IEnumerator BoughtFollower()
	{
		SpiderAnimationBoughtItem();
		Interactable = false;
		HealthPlayer h = state.GetComponent<HealthPlayer>();
		h.untouchable = true;
		state.GetComponentInChildren<SimpleSpineAnimator>();
		float duration = followerSpine.AnimationState.SetAnimation(0, "spider-out", false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		duration = followerSpine.AnimationState.SetAnimation(0, "spider-pop2", false).Animation.Duration;
		CameraManager.shakeCamera(0.5f, Random.Range(0, 360));
		yield return new WaitForSeconds(duration);
		FollowerInfo followerInfo = FollowerManager.CreateNewRecruit(FollowerLocation.Base, _followerInfo.SkinName, NotificationCentre.NotificationType.NewRecruit);
		Thought thought = ((Random.value < 0.7f) ? Thought.GratefulRecued : ((!(Random.value <= 0.3f)) ? Thought.InstantBelieverRescued : Thought.ResentfulRescued));
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		followerInfo.Thoughts.Add(data);
		DataManager.Instance.FollowerForSale = null;
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.75f);
		simpleBarkBeforeCard.enabled = true;
		Object.Destroy(base.gameObject);
		h.untouchable = false;
	}
}
