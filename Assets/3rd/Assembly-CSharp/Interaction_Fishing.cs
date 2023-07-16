using System;
using System.Collections;
using System.Collections.Generic;
using EasyCurvedLine;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using Unify;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_Fishing : Interaction
{
	[Serializable]
	public class FishType
	{
		public InventoryItem.ITEM_TYPE Type;

		public int Quantity = 2;

		public Vector2 Scale;

		[Range(0f, 1f)]
		public float Probability;

		public int Difficulty = 1;
	}

	public delegate void FishEvent();

	[SerializeField]
	private Structure structure;

	private Structures_FishingSpot fishingSpot;

	[Space]
	[SerializeField]
	private FishType[] fishTypes;

	[SerializeField]
	private Vector2 fishSpawnAmount;

	[SerializeField]
	private Vector2 fishSpawnRandomTime;

	[SerializeField]
	private Transform fishGroup;

	[SerializeField]
	private Fishable fishPrefab;

	[Space]
	[SerializeField]
	private Transform fishingHook;

	[SerializeField]
	private Transform fishingLine;

	[SerializeField]
	private Transform playerPosition;

	[SerializeField]
	private BoxCollider2D boundsCollider;

	[SerializeField]
	private float castingStrengthIncrement;

	[SerializeField]
	private AnimationCurve heightCurve;

	[SerializeField]
	private float height;

	[SerializeField]
	private float castDuration;

	[SerializeField]
	private AnimationCurve castDurationCurve;

	[SerializeField]
	private Vector2 minMaxCastDistance;

	[SerializeField]
	private float reelingSmooth;

	[SerializeField]
	private float reelSlerp;

	[SerializeField]
	private float reelHorizontalMoveSpeed;

	[SerializeField]
	private float reelingMaxSpeed;

	[SerializeField]
	private float reelingShoreOffset;

	[SerializeField]
	private float amountIncreaseOverTime = 0.01f;

	[SerializeField]
	private float amountDecreaseOverTime = 0.01f;

	private List<Fishable> fishables = new List<Fishable>();

	private bool isWithinDistance;

	private bool isFishing;

	private float castingStrength;

	private float maxReelingSpeed;

	private Vector3 hookLandPosition;

	private Vector3 hookVelocity;

	private float reelingHorizontalOffset;

	private float hookReelLerpSpeed = 2f;

	private UIFishingOverlayController _fishingOverlayControllerUI;

	[SerializeField]
	private GameObject splashObj;

	public Fishable currentFish;

	private float zPosition = 0.6f;

	private float fishSpawnTimer;

	private Vector3 playerDirection;

	private Vector3 playerRightDirection;

	public UnityEvent CallBackCaught;

	public UnityEvent CallBackFail;

	public Action OnCatchFish;

	public Action OnFishEscaped;

	public GameObject FishingParticles;

	private EventInstance CastLoopedSound;

	private EventInstance LoopedSound;

	private const float minTimeBetweenHookedDirectionChange = 1.5f;

	private const float maxTimeBetweenHookedDirectionChange = 2.5f;

	private float hookDirectionChangeTimer;

	private float hookDirectionSpeed = 1f;

	private int hookDirection = 1;

	private const int maxFish = 20;

	private bool startedCastLoop;

	private bool changedState;

	private int r;

	private bool Activated;

	public float ReelDistance;

	private bool startedLoop;

	public CurvedLinePoint curvedLinePoint;

	public Structures_FishingSpot FishingSpot
	{
		get
		{
			if (fishingSpot == null)
			{
				fishingSpot = structure.Brain as Structures_FishingSpot;
			}
			return fishingSpot;
		}
	}

	public Transform FishingHook
	{
		get
		{
			return fishingHook;
		}
	}

	public float ReelingMaxSpeed
	{
		get
		{
			return reelingMaxSpeed;
		}
	}

	public bool FishChasing { get; set; }

	public float HookVelocity
	{
		get
		{
			return hookVelocity.magnitude;
		}
	}

	public Vector3 HookedFishFleePosition { get; private set; }

	public Vector3 PlayerPosition
	{
		get
		{
			return playerPosition.position + playerDirection * reelingShoreOffset;
		}
	}

	public float ReelLerped { get; private set; }

	public float ReeledAmount { get; private set; } = 0.5f;


	private bool RitualActive
	{
		get
		{
			return FollowerBrainStats.IsFishing;
		}
	}

	public event FishEvent OnCasted;

	private void Start()
	{
		IncreaseChanceOfFishSkin();
		if (RitualActive)
		{
			fishSpawnAmount *= 2f;
			for (int i = 0; i < fishTypes.Length; i++)
			{
				if (IsGoodFishType(fishTypes[i].Type))
				{
					fishTypes[i].Probability *= 2.5f;
				}
			}
			if (FishingParticles != null)
			{
				FishingParticles.SetActive(true);
			}
		}
		else if (FishingParticles != null)
		{
			FishingParticles.SetActive(false);
		}
		for (int j = 0; j < fishTypes.Length; j++)
		{
			if ((fishTypes[j].Type == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN && DataManager.GetRandomLockedSkin() == "") || (fishTypes[j].Type == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION && DataManager.GetRandomLockedDecoration() == StructureBrain.TYPES.NONE) || (fishTypes[j].Type == InventoryItem.ITEM_TYPE.RELIC && (DataManager.Instance.PlayerFoundRelics.Contains(RelicType.FillUpFervour) || !DataManager.Instance.OnboardedRelics)))
			{
				if (!DataManager.Instance.RatooFishing_FISH_CRAB)
				{
					fishTypes[j].Type = InventoryItem.ITEM_TYPE.FISH_CRAB;
				}
				else if (!DataManager.Instance.RatooFishing_FISH_LOBSTER)
				{
					fishTypes[j].Type = InventoryItem.ITEM_TYPE.FISH_LOBSTER;
				}
				else if (!DataManager.Instance.RatooFishing_FISH_OCTOPUS)
				{
					fishTypes[j].Type = InventoryItem.ITEM_TYPE.FISH_OCTOPUS;
				}
				else if (!DataManager.Instance.RatooFishing_FISH_SQUID)
				{
					fishTypes[j].Type = InventoryItem.ITEM_TYPE.FISH_SQUID;
				}
				else
				{
					fishTypes[j].Type = InventoryItem.ITEM_TYPE.FISH_SWORDFISH;
				}
			}
		}
		state.CURRENT_STATE = StateMachine.State.Idle;
		fishSpawnTimer = Time.time + UnityEngine.Random.Range(fishSpawnRandomTime.x, fishSpawnRandomTime.y);
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		if (structure.Brain != null)
		{
			SpawnAlreadySpawnedFish();
		}
	}

	private void OnBrainAssigned()
	{
		SpawnAlreadySpawnedFish();
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void SpawnAlreadySpawnedFish()
	{
		int num = (int)UnityEngine.Random.Range(fishSpawnAmount.x, fishSpawnAmount.y + 1f);
		int count = FishingSpot.SpawnedFish.Count;
		num = Mathf.Clamp(num - count, 0, 20);
		foreach (FishType item in FishingSpot.SpawnedFish)
		{
			if (fishables.Count < 20)
			{
				SpawnFish(item, false);
			}
		}
		if (num > 0 && fishables.Count < 20)
		{
			SpawnFish(num, true);
		}
	}

	private void IncreaseChanceOfFishSkin()
	{
		if (DataManager.GetFollowerSkinUnlocked("Fish"))
		{
			return;
		}
		FishType[] array = fishTypes;
		foreach (FishType fishType in array)
		{
			if (fishType.Type == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)
			{
				fishType.Probability = 0.25f;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Activated && InputManager.Gameplay.GetCancelFishingButtonDown() && _fishingOverlayControllerUI != null)
		{
			StopAllCoroutines();
			_fishingOverlayControllerUI.StopAllCoroutines();
			AudioManager.Instance.StopLoop(LoopedSound);
			foreach (Fishable fishable in fishables)
			{
				if (fishable != null)
				{
					fishable.state = StateMachine.State.Idle;
				}
			}
			isFishing = false;
			state.CURRENT_STATE = StateMachine.State.Idle;
			GameManager instance = GameManager.GetInstance();
			if ((object)instance != null)
			{
				instance.OnConversationEnd();
			}
			ReeledAmount = 0.5f;
			fishingHook.gameObject.SetActive(false);
			fishingLine.gameObject.SetActive(false);
			isWithinDistance = false;
			base.HasChanged = true;
			Activated = false;
			GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
			{
				_fishingOverlayControllerUI.Hide();
				MonoSingleton<Indicator>.Instance.Reset();
			}));
		}
		if ((bool)PlayerFarming.Instance)
		{
			if (Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < ActivateDistance)
			{
				if (!isWithinDistance && state.CURRENT_STATE != 0)
				{
					GameManager.GetInstance().AddToCamera(LockPosition.gameObject);
					isWithinDistance = true;
				}
			}
			else if (isWithinDistance)
			{
				GameManager.GetInstance().RemoveFromCamera(LockPosition.gameObject);
				isWithinDistance = false;
			}
		}
		if (Time.time > fishSpawnTimer && (float)fishables.Count < fishSpawnAmount.y)
		{
			SpawnFish(1, true);
			fishSpawnTimer = Time.time + UnityEngine.Random.Range(fishSpawnRandomTime.x, fishSpawnRandomTime.y);
		}
		bool flag = false;
		flag = SettingsManager.Settings.Accessibility.AutoFish || InputManager.Gameplay.GetInteractButtonDown();
		if (state.CURRENT_STATE == StateMachine.State.Aiming && flag)
		{
			state.CURRENT_STATE = StateMachine.State.Charging;
		}
		if (state.CURRENT_STATE == StateMachine.State.Charging)
		{
			bool flag2 = false;
			if (!SettingsManager.Settings.Accessibility.AutoFish)
			{
				flag = InputManager.Gameplay.GetInteractButtonHeld();
				flag2 = InputManager.Gameplay.GetInteractButtonUp();
			}
			if (flag)
			{
				if (!startedCastLoop)
				{
					startedCastLoop = true;
					CastLoopedSound = AudioManager.Instance.CreateLoop("event:/ui/hold_button_loop", base.gameObject, true);
				}
				CastLoopedSound.setParameterByName("hold_time", castingStrength);
				castingStrength = Mathf.Clamp(castingStrength + castingStrengthIncrement * Time.deltaTime, 0f, 1f);
				_fishingOverlayControllerUI.UpdateCastingStrength(castingStrength);
				if (!changedState)
				{
					_fishingOverlayControllerUI.CastingButtonDown(true);
					changedState = true;
				}
				if (SettingsManager.Settings.Accessibility.AutoFish)
				{
					Vector3 vector = PlayerFarming.Instance.FishingLineBone.transform.position + Vector3.down * Mathf.Lerp(minMaxCastDistance.x, minMaxCastDistance.y, castingStrength);
					foreach (Fishable fishable2 in fishables)
					{
						if (vector.y < fishable2.transform.position.y)
						{
							CastLine();
						}
					}
				}
				if (castingStrength >= 1f)
				{
					CastLine();
				}
			}
			else if (flag2)
			{
				CastLine();
				if (changedState)
				{
					_fishingOverlayControllerUI.CastingButtonDown(false);
					changedState = false;
				}
			}
		}
		if (state.CURRENT_STATE == StateMachine.State.Reeling)
		{
			if (InputManager.Gameplay.GetInteractButtonHeld())
			{
				maxReelingSpeed = reelingMaxSpeed;
			}
			else
			{
				maxReelingSpeed = Mathf.Clamp(maxReelingSpeed - reelSlerp * Time.deltaTime, 0f, reelingMaxSpeed);
			}
			float horizontalAxis = InputManager.Gameplay.GetHorizontalAxis();
			if (Mathf.Abs(horizontalAxis) > 0.1f)
			{
				reelingHorizontalOffset = Mathf.Clamp(reelingHorizontalOffset + horizontalAxis * reelHorizontalMoveSpeed * Time.deltaTime, -1f, 1f);
			}
			Reel();
		}
		if (state.CURRENT_STATE != StateMachine.State.Attacking)
		{
			return;
		}
		if (r >= UnityEngine.Random.Range(50, 100))
		{
			if (splashObj != null)
			{
				UnityEngine.Object.Instantiate(splashObj, fishingHook.transform.position - Vector3.back * 0.1f, Quaternion.identity);
			}
			AudioManager.Instance.PlayOneShot("event:/fishing/splash", fishingHook.transform.position);
			r = 0;
		}
		else
		{
			r++;
		}
		ReeledAmount += (_fishingOverlayControllerUI.IsNeedleWithinSection() ? amountIncreaseOverTime : amountDecreaseOverTime) * Time.deltaTime;
		_fishingOverlayControllerUI.UpdateReelBar(ReeledAmount);
		if (ReeledAmount <= 0f)
		{
			currentFish.Spooked();
			NoCatch();
		}
		else if (ReeledAmount >= 1f)
		{
			FishCaught();
		}
		ReelLerped = Mathf.Lerp(ReelLerped, ReeledAmount, hookReelLerpSpeed * Time.deltaTime);
		Vector3 vector2 = Vector3.Lerp(HookedFishFleePosition, PlayerFarming.Instance.Spine.transform.position, ReelLerped);
		reelingHorizontalOffset += hookDirectionSpeed * (float)hookDirection * Time.deltaTime;
		if (Time.time > hookDirectionChangeTimer)
		{
			hookDirection *= -1;
			hookDirectionChangeTimer = Time.time + UnityEngine.Random.Range(1.5f, 2.5f);
		}
		fishingHook.transform.position = new Vector3(vector2.x + reelingHorizontalOffset, vector2.y, zPosition);
	}

	public override void GetLabel()
	{
		if (state.CURRENT_STATE == StateMachine.State.Idle)
		{
			if (fishables.Count > 0)
			{
				base.Label = ScriptLocalization.Interactions.Fish;
				if (!Interactable)
				{
					Interactable = true;
					base.HasChanged = true;
				}
			}
			else
			{
				base.Label = ScriptLocalization.Interactions.NoFish;
				Interactable = false;
			}
		}
		else
		{
			base.Label = "";
			Interactable = false;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		PlayerFarming.Instance.GoToAndStop(base.transform.position, base.gameObject, false, false, delegate
		{
			base.gameObject.SetActive(true);
			if (!isFishing)
			{
				StartCoroutine(BeganFishingIE());
			}
		});
	}

	private bool IsGoodFishType(InventoryItem.ITEM_TYPE fishType)
	{
		if (fishType != InventoryItem.ITEM_TYPE.FISH_SQUID && fishType != InventoryItem.ITEM_TYPE.FISH_SWORDFISH && fishType != InventoryItem.ITEM_TYPE.FISH_OCTOPUS)
		{
			return fishType == InventoryItem.ITEM_TYPE.FISH_LOBSTER;
		}
		return true;
	}

	private void Reel()
	{
		Vector3 vector = Vector3.SmoothDamp(fishingHook.transform.position, PlayerFarming.Instance.FishingLineBone.transform.position + playerRightDirection * reelingHorizontalOffset, ref hookVelocity, reelingSmooth, maxReelingSpeed);
		fishingHook.transform.position = new Vector3(vector.x, vector.y, zPosition);
		ReelDistance = Vector3.Distance(fishingHook.transform.position, PlayerFarming.Instance.transform.position);
		if (Vector3.Distance(fishingHook.transform.position, PlayerFarming.Instance.transform.position) < 2.1f)
		{
			NoCatch();
		}
	}

	private void CastLine()
	{
		AudioManager.Instance.PlayOneShot("event:/ui/hold_activate", PlayerFarming.Instance.gameObject.transform.position);
		AudioManager.Instance.StopLoop(CastLoopedSound);
		StartCoroutine(CastLineIE());
	}

	private IEnumerator CastLineIE()
	{
		fishingHook.gameObject.SetActive(true);
		isFishing = true;
		state.CURRENT_STATE = StateMachine.State.Casting;
		GetLabel();
		fishingHook.transform.position = PlayerFarming.Instance.FishingLineBone.transform.position;
		fishingHook.gameObject.SetActive(true);
		fishingLine.gameObject.SetActive(true);
		float num = Vector3.Dot(Vector3.left, PlayerFarming.Instance.transform.position - LockPosition.position);
		float num2 = Vector3.Dot(Vector3.right, PlayerFarming.Instance.transform.position - LockPosition.position);
		float num3 = Vector3.Dot(Vector3.up, PlayerFarming.Instance.transform.position - LockPosition.position);
		float num4 = Vector3.Dot(Vector3.down, PlayerFarming.Instance.transform.position - LockPosition.position);
		float a = Mathf.Max(num, num2);
		float b = Mathf.Max(num3, num4);
		float num5 = Mathf.Max(a, b);
		if (num5 == num)
		{
			playerDirection = Vector3.right;
		}
		else if (num5 == num2)
		{
			playerDirection = Vector3.left;
		}
		else if (num5 == num3)
		{
			playerDirection = Vector3.down;
		}
		else if (num5 == num4)
		{
			playerDirection = Vector3.up;
		}
		float degree = Utils.GetAngle(Vector3.zero, playerDirection) + 90f;
		playerRightDirection = Utils.DegreeToVector2(degree);
		Vector3 fromPosition = PlayerFarming.Instance.FishingLineBone.transform.position;
		hookLandPosition = PlayerFarming.Instance.FishingLineBone.transform.position + playerDirection * Mathf.Lerp(minMaxCastDistance.x, minMaxCastDistance.y, castingStrength);
		AudioManager.Instance.PlayOneShot("event:/fishing/cast_rod", PlayerFarming.Instance.gameObject.transform.position);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "Fishing/fishing-start", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "Fishing/fishing-loop", true, 0f);
		float t = 0f;
		while (t < castDuration)
		{
			float time = t / castDuration;
			Vector3 position = Vector3.Lerp(fromPosition, new Vector3(hookLandPosition.x, hookLandPosition.y, zPosition), castDurationCurve.Evaluate(time));
			position.z = heightCurve.Evaluate(time) * height;
			fishingHook.transform.position = position;
			t += Time.deltaTime;
			yield return null;
		}
		if (splashObj != null)
		{
			UnityEngine.Object.Instantiate(splashObj, fishingHook.transform.position - Vector3.back * 0.1f, Quaternion.identity);
		}
		AudioManager.Instance.PlayOneShot("event:/fishing/splash", fishingHook.transform.position);
		GameManager.GetInstance().OnConversationNext(fishingHook.gameObject, 12f);
		state.CURRENT_STATE = StateMachine.State.Reeling;
		_fishingOverlayControllerUI.SetState(StateMachine.State.Idle);
		yield return new WaitForSeconds(1.5f);
		FishEvent onCasted = this.OnCasted;
		if (onCasted != null)
		{
			onCasted();
		}
	}

	public bool IsClosestFish(Fishable fish)
	{
		Fishable fishable = null;
		foreach (Fishable fishable2 in fishables)
		{
			if (fishable2 != null && (fishable == null || Vector3.Distance(fishable2.transform.position, fishingHook.transform.position) < Vector3.Distance(fishable.transform.position, fishingHook.transform.position)))
			{
				fishable = fishable2;
			}
		}
		if (fishable != null)
		{
			return fishable == fish;
		}
		return false;
	}

	public void FishOn(Fishable currentFish)
	{
		Debug.Log("Fish on");
		if (!startedLoop)
		{
			startedLoop = true;
			LoopedSound = AudioManager.Instance.CreateLoop("event:/fishing/caught_something_loop", PlayerFarming.Instance.gameObject, true);
		}
		AudioManager.Instance.PlayOneShot("event:/fishing/caught_something_alert", PlayerFarming.Instance.transform.position);
		_fishingOverlayControllerUI.SetState(StateMachine.State.Attacking);
		_fishingOverlayControllerUI.SetReelingDifficulty(currentFish.FishType.Difficulty - 1);
		state.CURRENT_STATE = StateMachine.State.Attacking;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "Fishing/fishing-reel", true);
		ReelLerped = ReeledAmount;
		float num = Vector3.Distance(currentFish.transform.position, playerPosition.position);
		Vector3 normalized = (currentFish.transform.position - playerPosition.position).normalized;
		HookedFishFleePosition = currentFish.transform.position + normalized * num;
		fishingHook.gameObject.SetActive(false);
		this.currentFish = currentFish;
	}

	private void FishCaught()
	{
		StartCoroutine(FishCaughtIE());
	}

	private IEnumerator FishCaughtIE()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
		MMVibrate.StopRumble();
		startedLoop = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		_fishingOverlayControllerUI.Hide();
		isFishing = false;
		while (Mathf.Abs(ReelLerped - ReeledAmount) > 0.1f)
		{
			ReelLerped = Mathf.Lerp(ReelLerped, ReeledAmount, hookReelLerpSpeed * Time.deltaTime);
			Vector3 vector = Vector3.Lerp(HookedFishFleePosition, PlayerFarming.Instance.FishingLineBone.transform.position, ReelLerped);
			fishingHook.transform.position = new Vector3(vector.x, vector.y, zPosition);
			yield return null;
		}
		fishingHook.gameObject.SetActive(false);
		currentFish.gameObject.SetActive(false);
		fishingLine.gameObject.SetActive(false);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "Fishing/fishing-catch", true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
		AudioManager.Instance.PlayOneShot("event:/fishing/catch_fish", PlayerFarming.Instance.gameObject.transform.position);
		FishingSpot.FishCaught(currentFish.FishType);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.TimedAction(2.4f, null, "reactions/react-happy");
		AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
		if (!DataManager.Instance.GetVariable(DataManager.Variables.ShoreFishFished))
		{
			if ((currentFish.ItemType == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN || DataManager.Instance.FishCaughtTotal >= 4) && !DataManager.GetFollowerSkinUnlocked("Fish"))
			{
				FoundItemPickUp component = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN, 1, PlayerFarming.Instance.transform.position).GetComponent<FoundItemPickUp>();
				component.FollowerSkinForceSelection = true;
				component.SkinToForce = "Fish";
			}
			else if ((currentFish.ItemType == InventoryItem.ITEM_TYPE.RELIC || DataManager.Instance.FishCaughtTotal >= 20) && !DataManager.Instance.PlayerFoundRelics.Contains(RelicType.FillUpFervour) && DataManager.Instance.OnboardedRelics)
			{
				bool waiting = true;
				GameObject speaker = RelicCustomTarget.Create(currentFish.transform.position, PlayerFarming.Instance.transform.position, 1f, RelicType.FillUpFervour, delegate
				{
					waiting = false;
				});
				GameManager.GetInstance().OnConversationNew();
				GameManager.GetInstance().OnConversationNext(speaker, 6f);
				while (waiting)
				{
					yield return null;
				}
			}
			else
			{
				InventoryItem.ITEM_TYPE iTEM_TYPE = currentFish.ItemType;
				int quantity = currentFish.Quantity;
				if (iTEM_TYPE == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN && DataManager.GetFollowerSkinUnlocked("Fish"))
				{
					iTEM_TYPE = InventoryItem.ITEM_TYPE.FISH_BIG;
				}
				else
				{
					switch (iTEM_TYPE)
					{
					case InventoryItem.ITEM_TYPE.RELIC:
						iTEM_TYPE = InventoryItem.ITEM_TYPE.FISH_BIG;
						break;
					case InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION:
						quantity = 1;
						break;
					}
				}
				InventoryItem.Spawn(iTEM_TYPE, quantity, PlayerFarming.Instance.transform.position);
				if (iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_SMALL || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_BLOWFISH || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_CRAB || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_SQUID || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_BIG || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_LOBSTER || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_OCTOPUS || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH_SWORDFISH || iTEM_TYPE == InventoryItem.ITEM_TYPE.FISH)
				{
					if (!DataManager.Instance.FishCaught.Contains(currentFish.ItemType))
					{
						DataManager.Instance.FishCaught.Add(currentFish.ItemType);
					}
					checkAchievements();
				}
			}
		}
		else
		{
			InventoryItem.Spawn(InventoryItem.ITEM_TYPE.FISH_BIG, 2, PlayerFarming.Instance.transform.position + Vector3.right);
			if (!DataManager.Instance.FishCaught.Contains(InventoryItem.ITEM_TYPE.FISH_BIG))
			{
				DataManager.Instance.FishCaught.Add(InventoryItem.ITEM_TYPE.FISH_BIG);
				checkAchievements();
			}
		}
		if (RitualActive)
		{
			for (int i = 0; i < UnityEngine.Random.Range(1, 3); i++)
			{
				FishType randomFishType = GetRandomFishType();
				if (randomFishType.Type != InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION && randomFishType.Type != InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)
				{
					InventoryItem.Spawn(randomFishType.Type, 1, PlayerFarming.Instance.transform.position + Vector3.right);
				}
				if (!DataManager.Instance.FishCaught.Contains(randomFishType.Type))
				{
					DataManager.Instance.FishCaught.Add(randomFishType.Type);
					checkAchievements();
				}
			}
		}
		UnityEvent callBackCaught = CallBackCaught;
		if (callBackCaught != null)
		{
			callBackCaught.Invoke();
		}
		yield return new WaitForSeconds(1f);
		ReeledAmount = 0.5f;
		fishables.Remove(currentFish);
		UnityEngine.Object.Destroy(currentFish);
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		MonoSingleton<Indicator>.Instance.Reset();
		isWithinDistance = false;
		base.HasChanged = true;
		Activated = false;
		Action onCatchFish = OnCatchFish;
		if (onCatchFish != null)
		{
			onCatchFish();
		}
		DataManager.Instance.FishCaughtTotal++;
	}

	public void checkAchievements()
	{
		Debug.Log("Achievement check, collected fish types" + DataManager.Instance.FishCaught.Count);
		if (DataManager.Instance.FishCaught.Count >= 9)
		{
			Debug.Log("Achievement unlocked, collected all fish types");
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("FISH_ALL_TYPES"));
		}
	}

	private void NoCatch()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
		startedLoop = false;
		AudioManager.Instance.PlayOneShot("event:/fishing/fishing_failure", PlayerFarming.Instance.gameObject.transform.position);
		UnityEvent callBackFail = CallBackFail;
		if (callBackFail != null)
		{
			callBackFail.Invoke();
		}
		Action onFishEscaped = OnFishEscaped;
		if (onFishEscaped != null)
		{
			onFishEscaped();
		}
		isFishing = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager instance = GameManager.GetInstance();
		if ((object)instance != null)
		{
			instance.OnConversationEnd();
		}
		ReeledAmount = 0.5f;
		fishingHook.gameObject.SetActive(false);
		fishingLine.gameObject.SetActive(false);
		_fishingOverlayControllerUI.Hide();
		isWithinDistance = false;
		base.HasChanged = true;
		Activated = false;
		GameManager.GetInstance().StartCoroutine(FrameDelay(delegate
		{
			MonoSingleton<Indicator>.Instance.Reset();
		}));
	}

	private IEnumerator FrameDelay(Action callback)
	{
		yield return new WaitForSeconds(0.1f);
		callback();
	}

	private void SpawnFish(int amount, bool addFishSpawned)
	{
		if (!FishingSpot.CanSpawnFish && !RitualActive)
		{
			return;
		}
		amount = Mathf.Clamp(amount, 0, 20);
		for (int i = 0; i < amount; i++)
		{
			FishType randomFishType = GetRandomFishType();
			if (fishables.Count < 20)
			{
				SpawnFish(randomFishType, addFishSpawned);
				if (randomFishType.Type == InventoryItem.ITEM_TYPE.FOUND_ITEM_DECORATION || randomFishType.Type == InventoryItem.ITEM_TYPE.FOUND_ITEM_FOLLOWERSKIN)
				{
					break;
				}
				continue;
			}
			break;
		}
	}

	private Fishable SpawnFish(FishType fishType, bool addFishSpawned)
	{
		Bounds bounds = boundsCollider.bounds;
		Fishable fishable = UnityEngine.Object.Instantiate(position: new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y), fishGroup.position.z), original: fishPrefab, rotation: Quaternion.identity, parent: fishGroup);
		fishable.FadeIn();
		fishable.Configure(fishType, this);
		fishables.Add(fishable);
		if (addFishSpawned)
		{
			FishingSpot.AddFishSpawned(fishType);
		}
		return fishable;
	}

	private FishType GetRandomFishType()
	{
		FishType fishType = null;
		fishTypes.Shuffle();
		while (fishType == null)
		{
			FishType[] array = fishTypes;
			foreach (FishType fishType2 in array)
			{
				float num = UnityEngine.Random.Range(0f, 1f);
				if (fishType2.Probability >= num)
				{
					fishType = fishType2;
					break;
				}
			}
		}
		return fishType;
	}

	private IEnumerator BeganFishingIE()
	{
		Activated = true;
		if (PlayerFarming.Instance != null)
		{
			curvedLinePoint.lockToGameObject = PlayerFarming.Instance.FishingLineBone;
		}
		isFishing = true;
		hookVelocity = Vector3.zero;
		playerPosition.position = PlayerFarming.Instance.transform.position;
		GameManager instance = GameManager.GetInstance();
		if ((object)instance != null)
		{
			instance.OnConversationNew();
		}
		reelingHorizontalOffset = 0f;
		yield return new WaitForEndOfFrame();
		castingStrength = 0f;
		state.CURRENT_STATE = StateMachine.State.Aiming;
		PlayerFarming.Instance.TimedAction(float.MaxValue, null, "Fishing/fishing-loop");
		if (_fishingOverlayControllerUI == null)
		{
			_fishingOverlayControllerUI = GameObjectExtensions.Instantiate(MonoSingleton<UIManager>.Instance.FishingOverlayControllerTemplate, GameObject.FindWithTag("Canvas").transform);
		}
		_fishingOverlayControllerUI.UpdateCastingStrength(0f);
		_fishingOverlayControllerUI.Show(playerPosition.gameObject);
		_fishingOverlayControllerUI.SetState(StateMachine.State.Casting);
		startedCastLoop = false;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		AudioManager.Instance.StopLoop(CastLoopedSound);
		AudioManager.Instance.StopLoop(LoopedSound);
	}

	private new void OnDestroy()
	{
		AudioManager.Instance.StopLoop(CastLoopedSound);
		AudioManager.Instance.StopLoop(LoopedSound);
	}

	public override void IndicateHighlighted()
	{
	}

	public override void EndIndicateHighlighted()
	{
	}
}
