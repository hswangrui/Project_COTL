using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_FollowerInSpiderWeb : Interaction
{
	private string sBuy;

	private string sRequires;

	public bool ForceSpecificSkin;

	[SpineSkin("", "", true, false, false)]
	public string ForceSkin = "";

	private bool doneCantAffordAnimation;

	public GameObject ShopKeeper;

	public SkeletonAnimation ShopKeeperSpine;

	public SkeletonAnimation followerSpine;

	public SkeletonAnimation portalSpine;

	public ParticleSystem recruitParticles;

	public int Cost;

	public List<int> _Cost = new List<int> { 10, 20, 40, 80 };

	public FollowerInfo _followerInfo;

	private FollowerOutfit _outfit;

	[SerializeField]
	private FollowerInfoManager wim;

	public FollowersToBuy followerToBuy;

	private EventInstance receiveLoop;

	public Action BoughtFollowerCallback;

	public Action FollowerCreated;

	private string skin;

	public Transform playerMovePos;

	public bool IsDungeon;

	public bool free;

	public bool saleOn;

	private float SaleAmount;

	private string saleText;

	private string off;

	public bool Activated;

	private GameObject Player;

	private StateMachine CompanionState;

	public GameObject ConversionBone;

	[SerializeField]
	public GameObject normalBark;

	[SerializeField]
	private GameObject buyBark;

	[SerializeField]
	private GameObject cantAffordBark;

	private void Start()
	{
		_followerInfo = FollowerInfo.NewCharacter(FollowerLocation.Base, skin);
		wim.SetV_I(_followerInfo);
		if (_followerInfo.SkinName == "Giraffe")
		{
			_followerInfo.Name = LocalizationManager.GetTranslation("FollowerNames/Sparkles");
		}
		if (IsDungeon)
		{
			GetComponentInParent<spiderShop>().CheckCount();
		}
		UpdateLocalisation();
		StartCoroutine(CheckSaleRoutine());
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		portalSpine.gameObject.SetActive(false);
		Debug.Log("DataManager.Instance.LastFollowerPurchasedFromSpider: " + DataManager.Instance.LastFollowerPurchasedFromSpider + "  TimeManager.CurrentDay: " + TimeManager.CurrentDay);
		if (DataManager.Instance.LastFollowerPurchasedFromSpider == TimeManager.CurrentDay && !IsDungeon)
		{
			normalBark.SetActive(false);
			base.gameObject.SetActive(false);
		}
	}

	private IEnumerator CheckSaleRoutine()
	{
		SaleAmount = 0f;
		saleOn = false;
		yield return new WaitForEndOfFrame();
		if (BiomeBaseManager.Instance == null)
		{
			saleOn = true;
			SaleAmount = UnityEngine.Random.Range(50, 100);
		}
	}

	public void UpdateFollower()
	{
		if (_followerInfo.StartingCursedState == Thought.OldAge)
		{
			wim.outfit = FollowerOutfitType.Old;
			wim.v_i.Outfit = FollowerOutfitType.Old;
			_followerInfo.Outfit = FollowerOutfitType.Old;
		}
		else if (_followerInfo.StartingCursedState == Thought.Ill)
		{
			followerSpine.AnimationState.SetAnimation(1, "Emotions/emotion-sick", true);
		}
		else if (_followerInfo.StartingCursedState == Thought.BecomeStarving)
		{
			followerSpine.AnimationState.SetAnimation(1, "Emotions/emotion-unhappy", true);
		}
		else if (_followerInfo.StartingCursedState == Thought.Dissenter)
		{
			followerSpine.AnimationState.SetAnimation(1, "Emotions/emotion-dissenter", true);
		}
		wim.SetOutfit();
	}

	private int GetCost()
	{
		int num = followerToBuy.followerCost;
		if (saleOn)
		{
			num = (int)Mathf.Round((float)followerToBuy.followerCost - (float)followerToBuy.followerCost * (SaleAmount / 100f));
		}
		if (free)
		{
			num = 0;
		}
		if (DataManager.Instance.DeathCatBeaten)
		{
			num *= 2;
		}
		return num;
	}

	private IEnumerator WaitForFollowerInfoSet(Action callback)
	{
		while (_followerInfo == null)
		{
			yield return null;
		}
		if (callback != null)
		{
			callback();
		}
	}

	public void SetOld()
	{
		GameManager.GetInstance().StartCoroutine(WaitForFollowerInfoSet(delegate
		{
			_followerInfo.StartingCursedState = Thought.OldAge;
			UpdateFollower();
		}));
	}

	public void SetIll()
	{
		GameManager.GetInstance().StartCoroutine(WaitForFollowerInfoSet(delegate
		{
			_followerInfo.StartingCursedState = Thought.Ill;
			UpdateFollower();
		}));
	}

	public void SetFaithful()
	{
		GameManager.GetInstance().StartCoroutine(WaitForFollowerInfoSet(delegate
		{
			_followerInfo.Traits.Clear();
			_followerInfo.Traits.Add(FollowerTrait.TraitType.Faithful);
			float value = UnityEngine.Random.value;
			int num = 1;
			if (value < 0.1f)
			{
				num = 3;
			}
			else if (value < 0.3f)
			{
				num = 2;
			}
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < 50; j++)
				{
					FollowerTrait.TraitType item = FollowerTrait.GoodTraits[UnityEngine.Random.Range(0, FollowerTrait.GoodTraits.Count)];
					if (!_followerInfo.Traits.Contains(item))
					{
						_followerInfo.Traits.Add(item);
						break;
					}
				}
			}
			_followerInfo.TraitsSet = true;
		}));
	}

	public void SetLevel1()
	{
		_followerInfo.XPLevel = 1;
	}

	public void SetLevel2()
	{
		_followerInfo.XPLevel = 2;
	}

	public void SetLevel3()
	{
		_followerInfo.XPLevel = 3;
	}

	public void SetLevel4()
	{
		_followerInfo.XPLevel = 4;
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sBuy = ScriptLocalization.Interactions.Buy;
		sRequires = ScriptLocalization.Interactions.Requires;
		off = ScriptLocalization.UI_Generic.Off;
	}

	public override void GetLabel()
	{
		string text = string.Empty;
		if (followerToBuy != null)
		{
			switch (_followerInfo.StartingCursedState)
			{
			case Thought.OldAge:
				text = ScriptLocalization.Interactions_FollowerShop.Old;
				break;
			case Thought.Ill:
				text = ScriptLocalization.Interactions_FollowerShop.Ill;
				break;
			}
		}
		if (_followerInfo.Traits.Contains(FollowerTrait.TraitType.Faithful))
		{
			text = ScriptLocalization.Interactions_FollowerShop.Faithful;
		}
		string text2 = _followerInfo.Name;
		if (!string.IsNullOrEmpty(text))
		{
			text2 = string.Join(" ", text, text2);
		}
		if (saleOn)
		{
			if (SaleAmount == 0f)
			{
				saleText = string.Empty;
			}
			else
			{
				saleText = string.Format(off, SaleAmount);
			}
		}
		if (GetCost() == 0)
		{
			saleText = string.Format(off, 100);
		}
		string text3 = string.Empty;
		if (!Activated)
		{
			text3 = string.Format(ScriptLocalization.UI_ItemSelector_Context.Buy, text2, CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, GetCost()));
			if (!string.IsNullOrEmpty(saleText))
			{
				text3 = string.Join(" ", text3, saleText.Colour(Color.yellow));
			}
		}
		base.Label = text3;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Inventory.GetItemQuantity(20) >= GetCost() || CheatConsole.BuildingsFree)
		{
			StartCoroutine(Purchase());
		}
		else
		{
			SpiderAnimationCantAfford();
		}
	}

	private string GetAffordColor()
	{
		if (free)
		{
			return "<color=#f4ecd3>";
		}
		if (Inventory.GetItemQuantity(20) >= GetCost())
		{
			return "<color=#f4ecd3>";
		}
		return "<color=red>";
	}

	private IEnumerator Purchase()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(ConversionBone, 4f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.GoToAndStop(playerMovePos.position, base.transform.gameObject);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/shop/buy", PlayerFarming.Instance.transform.position);
		ShopKeeperSpine.AnimationState.SetAnimation(0, "buy", false);
		ShopKeeperSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
		if (!free && GetCost() != 0)
		{
			for (int i = 0; i < Cost; i++)
			{
				ResourceCustomTarget.Create(ShopKeeper, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null, base.transform);
				yield return new WaitForSeconds(0.1f);
			}
			Inventory.ChangeItemQuantity(20, -GetCost());
		}
		BoughtFollowerCallback();
		StartCoroutine(BoughtFollower());
		Activated = true;
	}

	private new void Update()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null))
		{
			GetCost();
		}
	}

	private void SpiderAnimationBoughtItem()
	{
		buyBark.SetActive(true);
	}

	private void SpiderAnimationCantAfford()
	{
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", PlayerFarming.Instance.transform.position);
		MonoSingleton<Indicator>.Instance.PlayShake();
		cantAffordBark.SetActive(true);
	}

	private IEnumerator BoughtFollower()
	{
		SpiderAnimationBoughtItem();
		Interactable = false;
		HealthPlayer h = state.GetComponent<HealthPlayer>();
		h.untouchable = true;
		state.GetComponentInChildren<SimpleSpineAnimator>();
		GameManager.GetInstance().OnConversationNext(followerSpine.gameObject, 4f);
		float duration = followerSpine.AnimationState.SetAnimation(0, "spider-out", false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/spider_small/stuck", AudioManager.Instance.Listener.transform.position);
		duration = followerSpine.AnimationState.SetAnimation(0, "spider-pop2", false).Animation.Duration;
		CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		yield return new WaitForSeconds(duration);
		AudioManager.Instance.PlayOneShot("event:/followers/rescue", AudioManager.Instance.Listener.transform.position);
		followerSpine.AnimationState.SetAnimation(0, "convert-short", false);
		portalSpine.gameObject.SetActive(true);
		portalSpine.AnimationState.SetAnimation(0, "convert-short", false);
		recruitParticles.Play();
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_start", PlayerFarming.Instance.gameObject);
		receiveLoop = AudioManager.Instance.CreateLoop("event:/player/receive_animation_loop", PlayerFarming.Instance.gameObject, true);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		duration = PlayerFarming.Instance.simpleSpineAnimator.Animate("specials/special-activate-long", 0, true).Animation.Duration;
		CameraManager.shakeCamera(0.5f, UnityEngine.Random.Range(0, 360));
		yield return new WaitForSeconds(duration - 1f);
		AudioManager.Instance.PlayOneShot("event:/player/receive_animation_end", PlayerFarming.Instance.gameObject);
		receiveLoop.stop(STOP_MODE.ALLOWFADEOUT);
		if (BiomeBaseManager.Instance != null && BiomeBaseManager.Instance.SpawnExistingRecruits && DataManager.Instance.Followers_Recruit.Count <= 0)
		{
			BiomeBaseManager.Instance.SpawnExistingRecruits = false;
		}
		if (!IsDungeon)
		{
			Debug.Log("SET DATE OF LAST PURCHASE".Colour(Color.red));
			DataManager.Instance.LastFollowerPurchasedFromSpider = TimeManager.CurrentDay;
		}
		FollowerManager.CreateNewRecruit(_followerInfo, NotificationCentre.NotificationType.NewRecruit);
		DataManager.SetFollowerSkinUnlocked(_followerInfo.SkinName);
		Thought thought = ((UnityEngine.Random.value < 0.7f) ? Thought.GratefulRecued : ((!(UnityEngine.Random.value <= 0.3f)) ? Thought.InstantBelieverRescued : Thought.ResentfulRescued));
		ThoughtData data = FollowerThoughts.GetData(thought);
		data.Init();
		_followerInfo.Thoughts.Add(data);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.75f);
		UnityEngine.Object.Destroy(base.gameObject);
		h.untouchable = false;
	}
}
