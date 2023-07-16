using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Meal : Interaction
{
	public bool Burned;

	public ParticleSystem BurnedFlamesParticles;

	public bool SetRotten;

	private bool Activated;

	public SpriteRenderer spriteRenderer;

	public Sprite RottenSprite;

	public FollowerLocation CreateStructureLocation = FollowerLocation.None;

	public bool CreateStructureOnStop;

	public Structure structure;

	private Structures_Meal _StructureInfo;

	public PickUp pickup;

	public static List<Meal> Meals = new List<Meal>();

	[SerializeField]
	private GameObject mealIndicator;

	[SerializeField]
	private GameObject rottenIdicator;

	private bool playedSfx;

	private SkeletonAnimation skeletonAnimation;

	public bool TakenByPlayer;

	private float Timer;

	public StructuresData StructureInfo
	{
		get
		{
			return structure.Structure_Info;
		}
	}

	public Structures_Meal StructureBrain
	{
		get
		{
			if (_StructureInfo == null)
			{
				_StructureInfo = structure.Brain as Structures_Meal;
			}
			return _StructureInfo;
		}
	}

	public bool Rotten
	{
		get
		{
			if (StructureInfo == null)
			{
				return false;
			}
			return StructureInfo.Rotten;
		}
	}

	private void Start()
	{
		if (!CreateStructureOnStop)
		{
			pickup.Speed = 0f;
		}
		else if (Burned)
		{
			BurnedFlamesParticles.gameObject.SetActive(true);
		}
		HasSecondaryInteraction = false;
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Meals.Add(this);
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		if (SetRotten && Outliner != null)
		{
			Outliner.OutlineLayers[2].Add((OutlineTarget == null) ? base.gameObject : OutlineTarget);
		}
	}

	private void OnBrainAssigned()
	{
		if (SetRotten)
		{
			StructureInfo.Rotten = true;
			return;
		}
		if (Burned)
		{
			StructureInfo.Burned = true;
			return;
		}
		CookingData.MealEffect[] mealEffects = CookingData.GetMealEffects(CookingData.GetMealFromStructureType(StructureInfo.Type));
		for (int i = 0; i < mealEffects.Length; i++)
		{
			CookingData.MealEffect mealEffect = mealEffects[i];
			foreach (Follower follower in Follower.Followers)
			{
				if (follower != null && follower.Brain != null && !FollowerManager.FollowerLocked(follower.Brain.Info.ID) && ((mealEffect.MealEffectType == CookingData.MealEffectType.RemovesIllness && follower.Brain.Info.CursedState == Thought.Ill) || (mealEffect.MealEffectType == CookingData.MealEffectType.RemovesDissent && follower.Brain.Info.CursedState == Thought.Dissenter)))
				{
					SetTargetFollower(follower);
					break;
				}
			}
		}
	}

	private void SetTargetFollower(Follower f)
	{
		f.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal, StructureInfo.Type);
		f.Brain.CompleteCurrentTask();
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Meals.Remove(this);
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		StructureManager.OnStructureRemoved = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureRemoved, new StructureManager.StructureChanged(OnStructureRemoved));
		if (SetRotten && Outliner != null)
		{
			Outliner.OutlineLayers[2].Remove((OutlineTarget == null) ? base.gameObject : OutlineTarget);
		}
	}

	private void OnStructureRemoved(StructuresData structure)
	{
		if (StructureInfo != null && structure.ID == StructureInfo.ID)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void GetLabel()
	{
		if (StructureInfo == null)
		{
			base.Label = "";
			return;
		}
		StructuresData structureInfo = StructureInfo;
		if (structureInfo != null && structureInfo.Burned)
		{
			HoldToInteract = false;
			base.Label = ScriptLocalization.Interactions.CleanBurntFood;
			Interactable = true;
			return;
		}
		StructuresData structureInfo2 = StructureInfo;
		if (structureInfo2 != null && structureInfo2.Rotten)
		{
			HoldToInteract = false;
			base.Label = ScriptLocalization.Interactions.CleanRottenFood;
			Interactable = true;
		}
		else if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ability_Eat) && !Activated && !DataManager.Instance.PlayerEaten)
		{
			HoldToInteract = false;
			base.Label = ScriptLocalization.Interactions.Eat;
			Interactable = true;
		}
		else
		{
			HoldToInteract = false;
			base.Label = "";
			Interactable = false;
		}
	}

	private bool MealSafeToEat()
	{
		if (StructureInfo != null)
		{
			switch (StructureInfo.Type)
			{
			case global::StructureBrain.TYPES.MEAL:
			case global::StructureBrain.TYPES.MEAL_MEAT:
			case global::StructureBrain.TYPES.MEAL_GREAT:
			case global::StructureBrain.TYPES.MEAL_GOOD_FISH:
			case global::StructureBrain.TYPES.MEAL_GREAT_FISH:
			case global::StructureBrain.TYPES.MEAL_BAD_FISH:
			case global::StructureBrain.TYPES.MEAL_BERRIES:
			case global::StructureBrain.TYPES.MEAL_MEDIUM_VEG:
			case global::StructureBrain.TYPES.MEAL_BAD_MIXED:
			case global::StructureBrain.TYPES.MEAL_MEDIUM_MIXED:
			case global::StructureBrain.TYPES.MEAL_GREAT_MIXED:
			case global::StructureBrain.TYPES.MEAL_BAD_MEAT:
			case global::StructureBrain.TYPES.MEAL_GREAT_MEAT:
				return true;
			}
		}
		return false;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			if (StructureInfo.Rotten || StructureInfo.Burned)
			{
				base.OnInteract(state);
				StartCoroutine(DoClean());
				Activated = true;
			}
			else if (UpgradeSystem.GetUnlocked(UpgradeSystem.Type.Ability_Eat))
			{
				base.OnInteract(state);
				StartCoroutine(EatRoutine());
				Activated = true;
			}
		}
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		skeletonAnimation.AnimationState.Event -= HandleEvent;
		if (e.Data.Name == "sfxTrigger")
		{
			CameraManager.shakeCamera(0.05f, Utils.GetAngle(PlayerFarming.Instance.gameObject.transform.position, base.transform.position));
			MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact, false, true, GameManager.GetInstance());
			base.transform.DOKill();
			base.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f);
			if (!playedSfx)
			{
				AudioManager.Instance.PlayOneShot("event:/player/sweep", base.transform.position);
				playedSfx = true;
			}
		}
	}

	private IEnumerator DoClean()
	{
		playedSfx = false;
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		skeletonAnimation.AnimationState.Event += HandleEvent;
		Activated = true;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("cleaning", 0, true);
		yield return new WaitForSeconds(14f / 15f);
		float Progress = 0f;
		while (InputManager.Gameplay.GetInteractButtonHeld())
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < 2f * (StructureInfo.GrowthStage / 5f)))
			{
				break;
			}
			StructureInfo.StartingScale = 1f - Progress / (2f * (StructureInfo.GrowthStage / 5f));
			yield return null;
		}
		if (Progress >= 2f * (StructureInfo.GrowthStage / 5f))
		{
			AudioManager.Instance.PlayOneShot("event:/player/weed_pick", base.transform.position);
			base.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(StructureBrain.Remove);
		}
		skeletonAnimation.AnimationState.Event -= HandleEvent;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private new void Update()
	{
		if (StructureInfo != null && StructureInfo.Rotten)
		{
			spriteRenderer.sprite = RottenSprite;
		}
		if (TakenByPlayer)
		{
			return;
		}
		if (StructureInfo != null && StructureInfo.Eaten)
		{
			structure.RemoveStructure();
		}
		else if (!((Timer += Time.deltaTime) < 1f))
		{
			if (CreateStructureOnStop && structure != null)
			{
				structure.CreateStructure(CreateStructureLocation, base.transform.position);
				CreateStructureOnStop = false;
			}
			rottenIdicator.SetActive(StructureInfo != null && StructureInfo.Rotten);
			if (StructureInfo != null && StructureInfo.Burned)
			{
				mealIndicator.SetActive(true);
			}
			else
			{
				mealIndicator.SetActive(StructureInfo == null || !StructureInfo.Rotten);
			}
		}
	}

	private IEnumerator EatRoutine()
	{
		mealIndicator.SetActive(false);
		rottenIdicator.SetActive(false);
		TakenByPlayer = true;
		DataManager.Instance.PlayerEaten = true;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 5f);
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAction0;
		yield return new WaitForSeconds(0.25f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 4f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.CustomAnimation("eat", true);
		AudioManager.Instance.PlayOneShot("event:/dialogue/followers/food_eat", base.gameObject);
		yield return new WaitForSeconds(1.5f);
		CameraManager.shakeCamera(0.3f, UnityEngine.Random.Range(0, 360));
		if (MealSafeToEat())
		{
			AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
			BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big", Time.timeScale == 1f);
			PlayerFarming.Instance.CustomAnimation("eat-react-good", true);
			yield return new WaitForSeconds(2.1666667f);
		}
		else
		{
			PlayerFarming.Instance.CustomAnimation("eat-react-bad", true);
			yield return new WaitForSeconds(7f / 30f);
			AudioManager.Instance.PlayOneShot("event:/dialogue/followers/hold_back_vom", base.gameObject);
			yield return new WaitForSeconds(11f / 15f);
			AudioManager.Instance.PlayOneShot("event:/dialogue/followers/vom", base.gameObject);
			yield return new WaitForSeconds(1.0333333f);
			if (StructureInfo.Type == global::StructureBrain.TYPES.MEAL_FOLLOWER_MEAT)
			{
				AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", base.gameObject);
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", base.gameObject.transform.position);
				BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "red", "burst_big", Time.timeScale == 1f);
			}
			yield return new WaitForSeconds(0.6f);
		}
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(1f);
		HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		if (StructureInfo.Rotten || StructureInfo.Burned)
		{
			AudioManager.Instance.PlayOneShot("event:/player/gethit", base.gameObject);
			component.HP -= 2f;
		}
		else
		{
			switch (StructureInfo.Type)
			{
			case global::StructureBrain.TYPES.MEAL_FOLLOWER_MEAT:
				component.BlackHearts += 4f;
				break;
			case global::StructureBrain.TYPES.MEAL_GRASS:
			case global::StructureBrain.TYPES.MEAL_POOP:
				AudioManager.Instance.PlayOneShot("event:/player/gethit", base.gameObject);
				component.HP -= 1f;
				break;
			case global::StructureBrain.TYPES.MEAL_DEADLY:
				AudioManager.Instance.PlayOneShot("event:/player/gethit", base.gameObject);
				component.HP -= 2f;
				break;
			default:
				component.BlueHearts += 2f;
				break;
			}
		}
		structure.RemoveStructure();
	}
}
