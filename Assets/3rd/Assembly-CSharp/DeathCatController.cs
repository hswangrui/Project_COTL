using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using MMBiomeGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class DeathCatController : BaseMonoBehaviour
{
	private const int MAX_FOLLOWERS = 12;

	public static DeathCatController Instance;

	private PlayerFarming playerFarming;

	[SerializeField]
	private UnitObject guardian1;

	[SerializeField]
	private GuardianBossIntro guardian1Intro;

	[SerializeField]
	private List<BaseMonoBehaviour> guardian1Components;

	[SerializeField]
	private UnitObject guardian2;

	[SerializeField]
	private List<BaseMonoBehaviour> guardian2Components;

	[SerializeField]
	private DeathCatClone deathCatClone;

	[SerializeField]
	private GameObject deathCatCloneCamera;

	[SerializeField]
	private EnemyDeathCatBoss deathCatBig;

	[Space]
	[SerializeField]
	private Vector2 fervourTimeDropInterval;

	[SerializeField]
	private GameObject followerToSpawn;

	public Interaction_SimpleConversation conversation0;

	public Interaction_SimpleConversation conversation0_B;

	public Interaction_SimpleConversation conversation0_C;

	public Interaction_SimpleConversation conversation1;

	public Interaction_SimpleConversation conversation2;

	public Interaction_SimpleConversation conversation3;

	public Interaction_SimpleConversation conversation4;

	public Interaction_SimpleConversation conversation5;

	public Interaction_SimpleConversation conversation6_A;

	public Interaction_SimpleConversation conversation6_B;

	[SerializeField]
	private GameObject whiteRoom;

	[SerializeField]
	private GameObject redRoom;

	[SerializeField]
	private GameObject transitionRoom;

	[SerializeField]
	private List<GameObject> torches;

	[SerializeField]
	private List<GameObject> eyes;

	[SerializeField]
	private SpriteRenderer blackFade;

	[SerializeField]
	private GameObject deathCatLighting;

	public SkeletonAnimation Chain1Spine;

	public SkeletonAnimation Chain2Spine;

	public SkeletonAnimation Chain3Spine;

	public SkeletonAnimation BChain1Spine;

	public SkeletonAnimation BChain2Spine;

	public SkeletonAnimation BChain3Spine;

	public SkeletonAnimation BChain4Spine;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain1Spine")]
	private string breakAnimation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain1Spine")]
	private string backgroundBreak1Animation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain1Spine")]
	private string backgroundBreak2Animation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain1Spine")]
	private string backgroundBreak3Animation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain1Spine")]
	private string backgroundBreak4Animation;

	[SerializeField]
	[SpineAnimation("", "", true, false, dataField = "Chain1Spine")]
	private string brokenAnimation;

	[Space]
	[SerializeField]
	private GameObject[] eyeBones;

	[SerializeField]
	private EnemyDeathCatEyesManager eyesManager;

	[SerializeField]
	private ParticleSystem transformParticles;

	[Space]
	[SerializeField]
	private GameObject followerChainsParent;

	[SerializeField]
	private GameObject[] cages;

	private CapturedFollowerChain[] followerChains;

	private float timeBetweenFervourDrop;

	private List<FollowerManager.SpawnedFollower> chainedFollowers = new List<FollowerManager.SpawnedFollower>();

	private List<FollowerManager.SpawnedFollower> cagedFollowers = new List<FollowerManager.SpawnedFollower>();

	private int originalFleece;

	[SerializeField]
	private GameObject distortionObject;

	private bool skippable;

	private Interaction_FollowerSpawn followerSpawn;

	private static int count;

	public bool DroppingFervour { get; set; } = true;


	private void Awake()
	{
		if (!DungeonSandboxManager.Active)
		{
			originalFleece = DataManager.Instance.PlayerFleece;
			DataManager.Instance.PlayerFleece = 0;
		}
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.SetSkin();
		}
		Instance = this;
		foreach (BaseMonoBehaviour guardian1Component in guardian1Components)
		{
			if (guardian1Component != null)
			{
				guardian1Component.enabled = false;
			}
		}
		foreach (BaseMonoBehaviour guardian2Component in guardian2Components)
		{
			if (guardian2Component != null)
			{
				guardian2Component.enabled = false;
			}
		}
		guardian1.GetComponent<Health>().OnDie += Guardian1_OnDie;
		redRoom.SetActive(false);
		transitionRoom.SetActive(false);
		whiteRoom.SetActive(true);
		followerChains = followerChainsParent.transform.GetComponentsInChildren<CapturedFollowerChain>();
		InitializeFollowers();
		if (DungeonSandboxManager.Active)
		{
			SpawnFollowersInCage();
		}
		AudioManager.Instance.PlayMusic("event:/music/death_cat_battle/death_cat_battle");
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.StandardRoom);
		if (DungeonSandboxManager.Active && BiomeGenerator.Instance != null)
		{
			BiomeGenerator.Instance.biomeMusicPath = "";
			BiomeGenerator.Instance.biomeAtmosPath = "";
		}
	}

	private void Update()
	{
		if (deathCatBig.enabled && DroppingFervour && GameManager.GetInstance().CurrentTime > timeBetweenFervourDrop)
		{
			for (int i = 0; i < 50; i++)
			{
				CapturedFollowerChain capturedFollowerChain = followerChains[UnityEngine.Random.Range(0, followerChains.Length)];
				if (!capturedFollowerChain.DroppingFervour)
				{
					capturedFollowerChain.DropFervour();
					break;
				}
			}
			timeBetweenFervourDrop = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(fervourTimeDropInterval.x, fervourTimeDropInterval.y);
		}
		if (PlayerFarming.Instance != null && skippable && (InputManager.Gameplay.GetAttackButtonDown() || DungeonSandboxManager.Active))
		{
			StopAllCoroutines();
			StartCoroutine(SkipIntro());
		}
	}

	private void LateUpdate()
	{
		SimulationManager.Pause();
	}

	private void InitializeFollowers()
	{
		List<FollowerBrain> list = new List<FollowerBrain>(FollowerBrain.AllBrains);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (DataManager.Instance.Followers_Dead.Contains(list[num]._directInfoAccess))
			{
				list.RemoveAt(num);
			}
		}
		if (DungeonSandboxManager.Active)
		{
			for (int i = 0; i < 12; i++)
			{
				FollowerInfo info = FollowerInfo.NewCharacter(FollowerLocation.Base);
				list.Add(FollowerBrain.GetOrCreateBrain(info));
			}
		}
		int num2 = Mathf.CeilToInt(Mathf.Min(list.Count, 12) / 2);
		for (int j = 0; j < list.Count && j < 12; j++)
		{
			if (j < num2)
			{
				FollowerManager.SpawnedFollower item = FollowerManager.SpawnCopyFollower(list[j]._directInfoAccess, cages[0].transform.position + new Vector3(Mathf.Lerp(-1.5f, 2f, (float)j / (float)num2), UnityEngine.Random.Range(-0.1f, 0.4f), 0f), base.transform, PlayerFarming.Location);
				cagedFollowers.Add(item);
				item.Follower.transform.parent = cages[0].transform;
				if (j == 3)
				{
					item.Follower.transform.position = cages[0].transform.position + new Vector3(Mathf.Lerp(-1.5f, 2f, (float)j / (float)num2), -0.1f, 0f);
				}
				item.Follower.gameObject.SetActive(false);
			}
			else if (j >= num2)
			{
				FollowerManager.SpawnedFollower item2 = FollowerManager.SpawnCopyFollower(list[j]._directInfoAccess, cages[1].transform.position + new Vector3(Mathf.Lerp(-1.5f, 2f, (float)(j - num2) / (float)num2), UnityEngine.Random.Range(-0.1f, 0.4f), 0f), base.transform, PlayerFarming.Location);
				cagedFollowers.Add(item2);
				item2.Follower.transform.parent = cages[1].transform;
				item2.Follower.transform.localScale = new Vector3(-1f, 1f, 1f);
				item2.Follower.gameObject.SetActive(false);
			}
		}
	}

	private IEnumerator SkipIntro()
	{
		skippable = false;
		LetterBox.Instance.HideSkipPrompt();
		Interaction_FinalBossAltar interaction_FinalBossAltar = UnityEngine.Object.FindObjectOfType<Interaction_FinalBossAltar>();
		interaction_FinalBossAltar.enabled = false;
		if (!DungeonSandboxManager.Active)
		{
			PlayerFarming.Instance.transform.position = interaction_FinalBossAltar.transform.position;
		}
		CameraManager.instance.Stopshake();
		if (MMConversation.CURRENT_CONVERSATION != null)
		{
			MMConversation.CURRENT_CONVERSATION.CallBack = null;
		}
		MMConversation.mmConversation.Close();
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		yield return new WaitForEndOfFrame();
		if (!DungeonSandboxManager.Active)
		{
			SpawnFollowersInCage();
		}
		foreach (BaseMonoBehaviour guardian1Component in guardian1Components)
		{
			if (guardian1Component != null)
			{
				guardian1Component.enabled = true;
			}
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(guardian1.gameObject, 6f);
		ResetPlayerWalking();
		AudioManager.Instance.SetMusicRoomID(3, "deathcat_room_id");
		StartCoroutine(guardian1Intro.PlayRoutine());
	}

	private void SpawnFollowersInCage()
	{
		int num = cagedFollowers.Count / 2;
		for (int i = 0; i < cagedFollowers.Count; i++)
		{
			if (i < num)
			{
				FollowerManager.SpawnedFollower spawnedFollower = cagedFollowers[i];
				spawnedFollower.Follower.gameObject.SetActive(true);
				spawnedFollower.Follower.OverridingEmotions = true;
				spawnedFollower.Follower.AddBodyAnimation("idle", true, 0f);
				spawnedFollower.Follower.SetFaceAnimation("Emotions/emotion-unhappy", true);
			}
		}
		for (int j = num; j < cagedFollowers.Count; j++)
		{
			FollowerManager.SpawnedFollower spawnedFollower2 = cagedFollowers[j];
			spawnedFollower2.Follower.gameObject.SetActive(true);
			spawnedFollower2.Follower.OverridingEmotions = true;
			spawnedFollower2.Follower.AddBodyAnimation("idle", true, 0f);
			spawnedFollower2.Follower.SetFaceAnimation("Emotions/emotion-unhappy", true);
		}
	}

	private void OnDestroy()
	{
		if (guardian1 != null)
		{
			guardian1.GetComponent<Health>().OnDie -= Guardian1_OnDie;
		}
		foreach (FollowerManager.SpawnedFollower chainedFollower in chainedFollowers)
		{
			FollowerManager.CleanUpCopyFollower(chainedFollower);
		}
		foreach (FollowerManager.SpawnedFollower cagedFollower in cagedFollowers)
		{
			FollowerManager.CleanUpCopyFollower(cagedFollower);
		}
		if (!DungeonSandboxManager.Active)
		{
			DataManager.Instance.PlayerFleece = originalFleece;
		}
	}

	public void Play()
	{
		StartCoroutine(IntroIE());
	}

	private IEnumerator IntroIE()
	{
		SimulationManager.Pause();
		skippable = DataManager.Instance.BossesEncountered.Contains(PlayerFarming.Location);
		if (skippable)
		{
			LetterBox.Instance.ShowSkipPrompt();
		}
		conversation0.Play();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		skippable = false;
		LetterBox.Instance.HideSkipPrompt();
		yield return StartCoroutine(SpawnFollowersInCageIE());
	}

	private IEnumerator SpawnFollowersInCageIE()
	{
		GameManager.GetInstance().OnConversationNew();
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNext(cages[0]);
		List<FollowerBrain> list = new List<FollowerBrain>(FollowerBrain.AllBrains);
		if (DungeonSandboxManager.Active)
		{
			for (int k = 0; k < 12; k++)
			{
				FollowerInfo info = FollowerInfo.NewCharacter(FollowerLocation.Base);
				list.Add(FollowerBrain.GetOrCreateBrain(info));
			}
		}
		int half = cagedFollowers.Count / 2;
		for (int j = 0; j < cagedFollowers.Count; j++)
		{
			if (j < half)
			{
				FollowerManager.SpawnedFollower spawnedFollower = cagedFollowers[j];
				spawnedFollower.Follower.gameObject.SetActive(true);
				AudioManager.Instance.PlayOneShot("event:/followers/pop_in", spawnedFollower.Follower.gameObject);
				spawnedFollower.Follower.OverridingEmotions = true;
				spawnedFollower.Follower.SetBodyAnimation("spawn-in", false);
				spawnedFollower.Follower.AddBodyAnimation("Reactions/react-worried" + UnityEngine.Random.Range(1, 3), false, 0f);
				spawnedFollower.Follower.AddBodyAnimation("idle", true, 0f);
				spawnedFollower.Follower.SetFaceAnimation("Emotions/emotion-unhappy", true);
				yield return new WaitForSeconds(0.05f);
			}
		}
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().OnConversationNext(cages[1]);
		for (int j = half; j < cagedFollowers.Count; j++)
		{
			FollowerManager.SpawnedFollower spawnedFollower2 = cagedFollowers[j];
			spawnedFollower2.Follower.gameObject.SetActive(true);
			AudioManager.Instance.PlayOneShot("event:/followers/pop_in", spawnedFollower2.Follower.gameObject);
			spawnedFollower2.Follower.OverridingEmotions = true;
			spawnedFollower2.Follower.SetBodyAnimation("spawn-in", false);
			spawnedFollower2.Follower.AddBodyAnimation("Reactions/react-worried" + UnityEngine.Random.Range(1, 3), false, 0f);
			spawnedFollower2.Follower.AddBodyAnimation("idle", true, 0f);
			spawnedFollower2.Follower.SetFaceAnimation("Emotions/emotion-unhappy", true);
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().OnConversationEnd();
	}

	public void KneelCallback()
	{
		StartCoroutine(IKneelCallback());
	}

	private IEnumerator IKneelCallback()
	{
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
		AudioManager.Instance.PlayOneShot("event:/Stings/church_bell", PlayerFarming.Instance.gameObject);
		AudioManager.Instance.PlayOneShot("event:/player/kneel_sequence", PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "final-boss/kneel", true);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "final-boss/kneel-loop", true, 0f);
		yield return new WaitForSeconds(3f);
		GameManager.GetInstance().OnConversationNext(deathCatClone.gameObject, 14f);
		yield return new WaitForSeconds(14f / 15f);
		MMVibrate.RumbleContinuous(0.5f, 0.75f);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, float.MaxValue);
		deathCatBig.BaseFormSpine.AnimationState.SetAnimation(0, "break-free-crown", false);
		transformParticles.startDelay = 2.6f;
		transformParticles.Play();
		Chain1Spine.AnimationState.SetAnimation(0, breakAnimation, false);
		Chain2Spine.AnimationState.SetAnimation(0, breakAnimation, false);
		Chain3Spine.AnimationState.SetAnimation(0, breakAnimation, false);
		BChain1Spine.AnimationState.SetAnimation(0, backgroundBreak1Animation, false);
		BChain2Spine.AnimationState.SetAnimation(0, backgroundBreak2Animation, false);
		BChain3Spine.AnimationState.SetAnimation(0, backgroundBreak3Animation, false);
		BChain4Spine.AnimationState.SetAnimation(0, backgroundBreak4Animation, false);
		Chain1Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain2Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain3Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain1Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain2Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain3Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain4Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		AudioManager.Instance.SetMusicRoomID(3, "deathcat_room_id");
		yield return new WaitForSeconds(2f);
		deathCatBig.BaseFormSpine.gameObject.SetActive(false);
		deathCatClone.gameObject.SetActive(true);
		transformParticles.Stop();
		deathCatClone.Spine.AnimationState.SetAnimation(0, "transform", false);
		deathCatClone.Spine.AnimationState.AddAnimation(0, "kill-player", true, 0f);
		deathCatClone.Spine.Skeleton.SetSkin("Crown");
		foreach (FollowerManager.SpawnedFollower f in cagedFollowers)
		{
			StartCoroutine(DelayCallback(UnityEngine.Random.Range(0f, 0.5f), delegate
			{
				f.Follower.SetBodyAnimation("Reactions/react-scared-long", false);
				f.Follower.AddBodyAnimation("idle", true, 0f);
			}));
		}
		deathCatClone.transform.DOMove(deathCatClone.transform.position + Vector3.down * 3f, 3f).SetEase(Ease.InOutSine);
		GameManager.GetInstance().OnConversationNext(deathCatCloneCamera.gameObject, 20f);
		yield return new WaitForSeconds(1f);
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "final-boss/die", false);
		AudioManager.Instance.PlayOneShot("event:/dialogue/death_cat/long_laugh", base.gameObject);
		CameraManager.instance.Stopshake();
		MMVibrate.StopRumble();
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 15f, 5f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(6f);
		for (int i = 0; i < DataManager.Instance.Followers_Demons_IDs.Count; i++)
		{
			FollowerInfo infoByID = FollowerInfo.GetInfoByID(DataManager.Instance.Followers_Demons_IDs[i]);
			if (infoByID != null)
			{
				FollowerBrain orCreateBrain = FollowerBrain.GetOrCreateBrain(infoByID);
				if (orCreateBrain != null)
				{
					orCreateBrain.AddThought(Thought.DemonSuccessfulRun);
				}
			}
		}
		DataManager.Instance.Followers_Demons_IDs.Clear();
		DataManager.Instance.Followers_Demons_Types.Clear();
		DataManager.ResetRunData();
		if (DungeonSandboxManager.Active)
		{
			if (!(UIDeathScreenOverlayController.Instance == null))
			{
				yield break;
			}
			UIDeathScreenOverlayController uIDeathScreenOverlayController = MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results.Completed);
			uIDeathScreenOverlayController.Show();
			uIDeathScreenOverlayController.OnShown = (Action)Delegate.Combine(uIDeathScreenOverlayController.OnShown, (Action)delegate
			{
				foreach (FollowerManager.SpawnedFollower cagedFollower in cagedFollowers)
				{
					cagedFollower.Follower.gameObject.SetActive(false);
				}
			});
		}
		else
		{
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Credits", 10f, "", null);
			Credits.GoToMainMenu = true;
		}
	}

	public void RefuseToKneel()
	{
		StartCoroutine(RefuseToKneelRoutine());
	}

	private IEnumerator RefuseToKneelRoutine()
	{
		DataManager.Instance.BossesEncountered.Add(PlayerFarming.Location);
		AudioManager.Instance.PlayOneShot("event:/player/refuse_kneel_sequence", PlayerFarming.Instance.gameObject);
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 6f);
		PlayerFarming.Instance._state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent += OnSpineEvent;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "final-boss/refuse", true);
		string text = PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.PrimaryEquipmentType.ToString();
		if (PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.PrimaryEquipmentType == EquipmentType.Gauntlet)
		{
			text = "Guantlets";
		}
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "final-boss/refuse-" + text, false, 0f);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "final-boss/idle-" + text, true, 0f);
		AudioManager.Instance.PlayOneShot("event:/Stings/church_bell", PlayerFarming.Instance.gameObject);
		yield return new WaitForSeconds(2.6666667f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 5f);
		yield return new WaitForSeconds(3.6666667f);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 11f);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.simpleSpineAnimator.OnSpineEvent -= OnSpineEvent;
		conversation0_C.SetPlayerInactiveOnStart = false;
		conversation0_C.CallOnConversationEnd = false;
		conversation0_C.Play();
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		foreach (BaseMonoBehaviour guardian1Component in guardian1Components)
		{
			if (guardian1Component != null)
			{
				guardian1Component.enabled = true;
			}
		}
		ResetPlayerWalking();
		StartCoroutine(guardian1Intro.PlayRoutine());
	}

	private void OnSpineEvent(string eventname)
	{
		if (eventname == "wings")
		{
			CameraManager.instance.ShakeCameraForDuration(1.2f, 1.5f, 0.5f);
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 8f);
			distortionObject.transform.parent = PlayerFarming.Instance.transform;
			distortionObject.transform.localPosition = Vector3.zero;
			distortionObject.SetActive(true);
			distortionObject.transform.DOScale(9f, 1f).SetEase(Ease.Linear).OnComplete(delegate
			{
				distortionObject.transform.localScale = Vector3.zero;
				distortionObject.SetActive(false);
				distortionObject.transform.parent = base.transform;
			});
		}
	}

	public void SoundTrigger1()
	{
		AudioManager.Instance.PlayOneShot("event:/music/intro/sting_mm_logo");
		HUD_Manager.Instance.Hide(false, 0);
	}

	public void SoundTrigger2()
	{
		InitPlayerWalking();
		AudioManager.Instance.PlayOneShot("event:/music/intro/sting_dd_logo");
	}

	public void SoundTrigger3()
	{
		AudioManager.Instance.PlayOneShot("event:/music/intro/sting_bridge");
	}

	public void InitPlayerWalking()
	{
		if (DataManager.Instance.FinalBossSlowWalk && !DungeonSandboxManager.Active)
		{
			DataManager.Instance.FinalBossSlowWalk = false;
			PlayerFarming.Instance.unitObject.maxSpeed = 0.03f;
			PlayerFarming.Instance.playerController.RunSpeed = 2f;
			PlayerFarming.Instance.playerController.DefaultRunSpeed = 2f;
			PlayerFarming.Instance.simpleSpineAnimator.NorthIdle = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("idle-up-slow");
			PlayerFarming.Instance.simpleSpineAnimator.Idle = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("idle-slow");
			PlayerFarming.Instance.simpleSpineAnimator.DefaultLoop = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("idle-slow");
			PlayerFarming.Instance.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle-slow");
			PlayerFarming.Instance.simpleSpineAnimator.NorthMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-up-slow");
			PlayerFarming.Instance.simpleSpineAnimator.SouthMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-down-slow");
			PlayerFarming.Instance.simpleSpineAnimator.NorthDiagonalMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-up-diagonal-slow");
			PlayerFarming.Instance.simpleSpineAnimator.SouthDiagonalMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-slow");
			PlayerFarming.Instance.simpleSpineAnimator.ForceDirectionalMovement = true;
			PlayerFarming.Instance.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-horizontal-slow");
			PlayerFarming.Instance.simpleSpineAnimator.UpdateIdleAndMoving();
			PlayerFarming.Instance.playerWeapon.enabled = false;
			PlayerFarming.Instance.playerSpells.enabled = false;
			PlayerFarming.Instance.AllowDodging = false;
		}
	}

	private void ResetPlayerWalking()
	{
		PlayerFarming.Instance.unitObject.maxSpeed = 0.09f;
		PlayerFarming.Instance.playerController.RunSpeed = 5.5f;
		PlayerFarming.Instance.playerController.DefaultRunSpeed = 5.5f;
		PlayerFarming.Instance.simpleSpineAnimator.NorthIdle = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("idle-up");
		PlayerFarming.Instance.simpleSpineAnimator.Idle = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("idle");
		PlayerFarming.Instance.simpleSpineAnimator.DefaultLoop = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("idle");
		PlayerFarming.Instance.simpleSpineAnimator.ChangeStateAnimation(StateMachine.State.Idle, "idle");
		PlayerFarming.Instance.simpleSpineAnimator.SetDefault(StateMachine.State.Idle, "idle");
		PlayerFarming.Instance.simpleSpineAnimator.NorthMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-up");
		PlayerFarming.Instance.simpleSpineAnimator.SouthMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-down");
		PlayerFarming.Instance.simpleSpineAnimator.NorthDiagonalMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run-up-diagonal");
		PlayerFarming.Instance.simpleSpineAnimator.SouthDiagonalMoving = PlayerFarming.Instance.simpleSpineAnimator.GetAnimationReference("run");
		PlayerFarming.Instance.simpleSpineAnimator.ForceDirectionalMovement = false;
		PlayerFarming.Instance.simpleSpineAnimator.SetDefault(StateMachine.State.Moving, "run-horizontal");
		PlayerFarming.Instance.simpleSpineAnimator.ResetAnimationsToDefaults();
		PlayerFarming.Instance.simpleSpineAnimator.UpdateIdleAndMoving();
		PlayerFarming.Instance.playerWeapon.enabled = true;
		PlayerFarming.Instance.playerSpells.enabled = true;
		PlayerFarming.Instance.AllowDodging = true;
	}

	private void Guardian1_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		foreach (BaseMonoBehaviour guardian2Component in guardian2Components)
		{
			if (guardian2Component != null)
			{
				guardian2Component.enabled = true;
			}
		}
	}

	public void DeathCatCloneTransform()
	{
		StartCoroutine(DeathCatCloneTransformIE());
	}

	private IEnumerator DeathCatCloneTransformIE()
	{
		conversation2.Play();
		yield return new WaitForEndOfFrame();
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(deathCatClone.gameObject, 14f);
		MMVibrate.RumbleContinuous(0.5f, 0.75f);
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.6f, float.MaxValue);
		deathCatBig.BaseFormSpine.AnimationState.SetAnimation(0, "break-free", false);
		transformParticles.startDelay = 2.6f;
		transformParticles.Play();
		Chain1Spine.AnimationState.SetAnimation(0, breakAnimation, false);
		Chain2Spine.AnimationState.SetAnimation(0, breakAnimation, false);
		Chain3Spine.AnimationState.SetAnimation(0, breakAnimation, false);
		BChain1Spine.AnimationState.SetAnimation(0, backgroundBreak1Animation, false);
		BChain2Spine.AnimationState.SetAnimation(0, backgroundBreak2Animation, false);
		BChain3Spine.AnimationState.SetAnimation(0, backgroundBreak3Animation, false);
		BChain4Spine.AnimationState.SetAnimation(0, backgroundBreak4Animation, false);
		Chain1Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain2Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		Chain3Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain1Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain2Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain3Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		BChain4Spine.AnimationState.AddAnimation(0, brokenAnimation, true, 0f);
		AudioManager.Instance.SetMusicRoomID(3, "deathcat_room_id");
		yield return new WaitForSeconds(3.5f);
		deathCatBig.BaseFormSpine.gameObject.SetActive(false);
		deathCatClone.gameObject.SetActive(true);
		transformParticles.Stop();
		deathCatClone.Spine.AnimationState.SetAnimation(0, "transform", false);
		deathCatClone.Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		foreach (FollowerManager.SpawnedFollower f in cagedFollowers)
		{
			StartCoroutine(DelayCallback(UnityEngine.Random.Range(0f, 0.5f), delegate
			{
				f.Follower.SetBodyAnimation("Reactions/react-scared-long", false);
				f.Follower.AddBodyAnimation("idle", true, 0f);
			}));
		}
		deathCatClone.transform.DOMove(deathCatClone.transform.position + Vector3.down * 3f, 3f).SetEase(Ease.InOutSine);
		GameManager.GetInstance().OnConversationNext(deathCatClone.gameObject, 20f);
		yield return new WaitForSeconds(1f);
		CameraManager.instance.Stopshake();
		MMVibrate.StopRumble();
		yield return new WaitForSeconds(3f);
		HUD_DisplayName.Play(ScriptLocalization.NAMES.DeathNPC, 2, HUD_DisplayName.Positions.Centre, HUD_DisplayName.textBlendMode.DungeonFinal);
		UIBossHUD.Play(deathCatClone.health, ScriptLocalization.NAMES.DeathNPC);
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().AddToCamera(deathCatClone.gameObject);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
		yield return new WaitForSeconds(1f);
		deathCatClone.enabled = true;
	}

	private IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public void StartPhase2()
	{
		guardian1.gameObject.SetActive(false);
		guardian2.gameObject.SetActive(false);
		deathCatClone.gameObject.SetActive(false);
		ResetPlayerWalking();
		PlayerFarming.Instance.transform.position = Vector3.zero;
		UnityEngine.Object.FindObjectOfType<Interaction_FinalBossAltar>().gameObject.SetActive(false);
		GetComponent<Collider2D>().enabled = false;
		DeathCatBigTransform();
	}

	public void DeathCatBigTransform()
	{
		StartCoroutine(DeathCatBigTransformIE());
	}

	private IEnumerator DeathCatBigTransformIE()
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.NoMusic);
		GameManager.GetInstance().OnConversationNext(transitionRoom.gameObject, 24f);
		foreach (GameObject torch in torches)
		{
			torch.SetActive(false);
		}
		foreach (GameObject eye in eyes)
		{
			eye.SetActive(false);
		}
		BlackSoul.Clear();
		PlayerFarming.Instance.GetBlackSoul(Mathf.RoundToInt(FaithAmmo.Total - FaithAmmo.Ammo), false);
		UIBossHUD.Hide();
		PlayerFarming.Instance.Spine.gameObject.SetActive(false);
		PlayerFarming.Instance.transform.position = new Vector3(0f, 0f, 0f);
		PlayerFarming.Instance.LookToObject = deathCatBig.gameObject;
		whiteRoom.SetActive(false);
		transitionRoom.SetActive(true);
		for (int num = Projectile.Projectiles.Count - 1; num >= 0; num--)
		{
			Projectile.Projectiles[num].DestroyProjectile(true);
		}
		BiomeConstants.Instance.SetAmplifyPostEffect(false);
		deathCatBig.Spine.gameObject.SetActive(true);
		deathCatBig.BaseFormSpine.gameObject.SetActive(false);
		deathCatBig.Spine.AnimationState.SetAnimation(0, "animation", true);
		deathCatLighting.SetActive(false);
		yield return new WaitForSeconds(2f);
		for (int j = 0; j < eyes.Count; j += 2)
		{
			eyes[j].SetActive(true);
			eyes[j + 1].SetActive(true);
			MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact, false, true, this);
			AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", PlayerFarming.Instance.gameObject);
			if (j == 0)
			{
				yield return new WaitForSeconds(1f);
			}
			else
			{
				yield return new WaitForSeconds(0.01f);
			}
		}
		yield return new WaitForSeconds(1f);
		for (int j = 0; j < torches.Count; j += 2)
		{
			torches[j].SetActive(true);
			torches[j + 1].SetActive(true);
			MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact, false, true, this);
			AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", PlayerFarming.Instance.gameObject);
			yield return new WaitForSeconds(0.35f);
		}
		yield return new WaitForSeconds(2f);
		MMVibrate.StopRumble();
		deathCatBig.Spine.AnimationState.SetAnimation(0, "roar", false);
		deathCatBig.Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		yield return new WaitForSeconds(1f);
		BiomeConstants.Instance.SetAmplifyPostEffect(true);
		MMVibrate.RumbleContinuous(1.5f, 1.75f);
		CameraManager.instance.ShakeCameraForDuration(1.6f, 2f, 2.5f);
		GameManager.GetInstance().OnConversationNext(deathCatBig.CinematicBone, 18f);
		PlayerFarming.Instance.Spine.gameObject.SetActive(true);
		redRoom.SetActive(true);
		blackFade.DOFade(0f, 0.5f);
		AudioManager.Instance.PlayMusic("event:/music/death_cat_battle/death_cat_battle");
		AudioManager.Instance.SetMusicRoomID(4, "deathcat_room_id");
		for (int l = 0; l < eyes.Count; l++)
		{
			eyes[l].SetActive(false);
		}
		deathCatLighting.SetActive(true);
		if (FollowerBrain.AllBrains.Count > 0 || DungeonSandboxManager.Active)
		{
			yield return new WaitForSeconds(2.5f);
			MMVibrate.StopRumble();
			yield return new WaitForSeconds(1.5f);
			conversation4.Play();
			yield return new WaitForEndOfFrame();
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(transitionRoom.gameObject, 22f);
			deathCatBig.Spine.AnimationState.SetAnimation(0, "summon-followers", false);
			deathCatBig.Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
			int num2 = cagedFollowers.Count;
			MMVibrate.RumbleContinuous(0.35f, 0.5f);
			CameraManager.instance.ShakeCameraForDuration(0.3f, 0.6f, 5f);
			for (int m = 0; m < followerChains.Length; m++)
			{
				if (m < num2)
				{
					StartCoroutine(_003CDeathCatBigTransformIE_003Eg__SpawnChainedFollower_007C85_3(m, UnityEngine.Random.Range(0f, 1f)));
				}
			}
			yield return new WaitForSeconds(5f);
			MMVibrate.StopRumble();
			foreach (FollowerManager.SpawnedFollower chainedFollower in chainedFollowers)
			{
				if (cagedFollowers.Contains(chainedFollower))
				{
					cagedFollowers.Remove(chainedFollower);
				}
			}
		}
		GameManager.GetInstance().OnConversationNext(deathCatBig.CinematicBone, 14f);
		yield return new WaitForSeconds(2f);
		AudioManager.Instance.PlayOneShot("event:/boss/deathcat/grunt", base.gameObject);
		for (int n = 0; n < 3; n++)
		{
			if (n == 1)
			{
				GameManager.GetInstance().AddToCamera(eyesManager.Eyes[n].gameObject);
				GameManager.GetInstance().CameraSetOffset(Vector3.down * 3f);
			}
			eyesManager.Eyes[n].transform.position = eyeBones[n].transform.position;
		}
		for (int j = 0; j < 3; j++)
		{
			eyesManager.Eyes[j].gameObject.SetActive(true);
			eyesManager.Eyes[j].PopParticle.SetActive(true);
			eyesManager.Eyes[j].transform.localScale = Vector3.one * 0.6f;
			Vector3 vector = new Vector3((j - 1) * -3, -5f, (j == 1) ? (-2) : (-1));
			eyesManager.Eyes[j].transform.DOMove(eyesManager.Eyes[j].transform.position + vector, 3.5f - (float)j).SetEase(Ease.InOutSine);
			eyesManager.Eyes[j].transform.DOScale(0.8f, 3.5f).SetEase(Ease.InSine);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			deathCatBig.Spine.Skeleton.SetSkin((j + 1).ToString());
			AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/teleport_away", base.gameObject);
			if (j != 2)
			{
				yield return new WaitForSeconds(1f);
			}
		}
		yield return new WaitForSeconds(2.5f);
		eyesManager.Eyes[0].transform.DOMove(new Vector3(eyesManager.Eyes[0].transform.position.x, eyesManager.Eyes[0].transform.position.y, 2f), 0.5f).SetEase(Ease.InBack).OnComplete(delegate
		{
			eyesManager.Eyes[0].SplashParticle.gameObject.SetActive(true);
			eyesManager.Eyes[0].enabled = true;
			eyesManager.Eyes[0].spawnTrails = true;
			eyesManager.Eyes[0].Spine.gameObject.SetActive(false);
			AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_impact", base.gameObject);
		});
		eyesManager.Eyes[2].transform.DOMove(new Vector3(eyesManager.Eyes[2].transform.position.x, eyesManager.Eyes[2].transform.position.y, 2f), 0.5f).SetEase(Ease.InBack).OnComplete(delegate
		{
			eyesManager.Eyes[2].SplashParticle.gameObject.SetActive(true);
			eyesManager.Eyes[2].enabled = true;
			eyesManager.Eyes[2].spawnTrails = true;
			eyesManager.Eyes[2].Spine.gameObject.SetActive(false);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_impact", base.gameObject);
		});
		yield return new WaitForSeconds(0.5f);
		eyesManager.Eyes[1].transform.DOMove(new Vector3(eyesManager.Eyes[1].transform.position.x, eyesManager.Eyes[1].transform.position.y, 2f), 0.5f).SetEase(Ease.InBack).OnComplete(delegate
		{
			eyesManager.Eyes[1].SplashParticle.gameObject.SetActive(true);
			eyesManager.Eyes[1].enabled = true;
			eyesManager.Eyes[1].spawnTrails = true;
			eyesManager.Eyes[1].Spine.gameObject.SetActive(false);
			CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
			AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_impact", base.gameObject);
		});
		yield return new WaitForSeconds(1.5f);
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().AddToCamera(deathCatBig.gameObject);
		GameManager.GetInstance().CamFollowTarget.MinZoom = 9f;
		GameManager.GetInstance().CamFollowTarget.MaxZoom = 18f;
		UIBossHUD.Play(deathCatBig.health, ScriptLocalization.NAMES.DeathNPC);
		UIBossHUD.Instance.ForceHealthAmount(0f);
		timeBetweenFervourDrop = GameManager.GetInstance().CurrentTime + UnityEngine.Random.Range(fervourTimeDropInterval.x, fervourTimeDropInterval.y);
		Health component = deathCatBig.GetComponent<Health>();
		component.enabled = true;
		component.HP = component.totalHP;
		deathCatBig.enabled = true;
		float time = 0f;
		while (time < 2f)
		{
			float normHealthAmount = time / 2f;
			UIBossHUD instance = UIBossHUD.Instance;
			if ((object)instance != null)
			{
				instance.ForceHealthAmount(normHealthAmount);
			}
			time += Time.deltaTime;
			yield return null;
		}
	}

	public void DeathCatKilled()
	{
		StartCoroutine(DeathCatKilledIE());
	}

	private IEnumerator DeathCatKilledIE()
	{
		HUD_Manager.Instance.Hide(false);
		PlayerFarming.Instance.GoToAndStop(new Vector3(0f, 2f, 0f), base.gameObject);
		AudioManager.Instance.SetMusicRoomID(0, "deathcat_room_id");
		if (!DungeonSandboxManager.Active)
		{
			GameObject Follower = UnityEngine.Object.Instantiate(followerToSpawn, new Vector3(0f, 8f, 0f), Quaternion.identity);
			followerSpawn = Follower.GetComponent<Interaction_FollowerSpawn>();
			followerSpawn.Play("Boss Death Cat", ScriptLocalization.NAMES.DeathNPC);
			followerSpawn.AutomaticallyInteract = false;
			followerSpawn.Interactable = false;
			followerSpawn.DisableOnHighlighted = true;
			followerSpawn.EndIndicateHighlighted();
			DataManager.SetFollowerSkinUnlocked("Boss Death Cat");
			while (LetterBox.IsPlaying)
			{
				yield return null;
			}
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(Follower.gameObject, 4f);
			foreach (ConversationEntry entry in conversation5.Entries)
			{
				entry.Speaker = followerSpawn.Spine.gameObject;
				entry.SkeletonData = followerSpawn.Spine;
				entry.Animation = "unconverted-talk";
				entry.pitchValue = followerSpawn._followerInfo.follower_pitch;
				entry.vibratoValue = followerSpawn._followerInfo.follower_vibrato;
			}
			yield return new WaitForEndOfFrame();
			conversation5.Play();
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().CamFollowTarget.TargetOffset = new Vector3(0f, 0f, 0.3f);
			while (LetterBox.IsPlaying)
			{
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(Follower.gameObject, 4f);
			GameManager.GetInstance().CamFollowTarget.TargetOffset = new Vector3(0f, 0f, 0.3f);
			bool convert = false;
			GameObject g = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/Choice Indicator"), GameObject.FindWithTag("Canvas").transform) as GameObject;
			ChoiceIndicator choice = g.GetComponent<ChoiceIndicator>();
			choice.Offset = new Vector3(0f, -350f);
			choice.Show("Conversation_NPC/DeathCatBossFight/Dead/Spare", "FollowerInteractions/Murder", delegate
			{
				convert = true;
			}, delegate
			{
				convert = false;
			}, Follower.transform.position);
			while (g != null)
			{
				choice.UpdatePosition(Follower.transform.position);
				yield return null;
			}
			UIManager.PlayAudio("event:/ui/heretics_defeated");
			Interaction_SimpleConversation interaction_SimpleConversation = ((!convert) ? conversation6_B : conversation6_A);
			interaction_SimpleConversation.Entries[0].Speaker = followerSpawn.Spine.gameObject;
			interaction_SimpleConversation.Entries[0].SkeletonData = followerSpawn.Spine;
			interaction_SimpleConversation.Entries[0].Animation = "unconverted-talk";
			interaction_SimpleConversation.Entries[0].pitchValue = followerSpawn._followerInfo.follower_pitch;
			interaction_SimpleConversation.Entries[0].vibratoValue = followerSpawn._followerInfo.follower_vibrato;
			interaction_SimpleConversation.Play();
			yield return new WaitForEndOfFrame();
			GameManager.GetInstance().CamFollowTarget.TargetOffset = new Vector3(0f, 0f, 0.3f);
			while (MMConversation.isPlaying)
			{
				yield return null;
			}
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(followerSpawn.gameObject, 6f);
			Vector3 targetPosition = Follower.transform.position + Vector3.right * 1.5f;
			PlayerFarming.Instance.GoToAndStop(targetPosition, followerSpawn.gameObject);
			while (PlayerFarming.Instance.GoToAndStopping)
			{
				yield return null;
			}
			followerSpawn.Spine.GetComponent<SimpleSpineAnimator>().enabled = false;
			if (convert)
			{
				yield return StartCoroutine(followerSpawn.ConvertFollower());
			}
			else
			{
				yield return StartCoroutine(MurderDeathCat());
			}
		}
		else
		{
			GameManager.GetInstance().OnConversationNew();
		}
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		bool waiting = true;
		PlayerFarming.Instance.GoToAndStop(Vector3.zero, null, true, false, delegate
		{
			waiting = false;
			PlayerFarming.Instance.state.facingAngle = 270f;
			PlayerFarming.Instance.state.LookAngle = 270f;
		});
		yield return new WaitForEndOfFrame();
		while (waiting)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		AstarPath.active = null;
		for (int j = 0; j < chainedFollowers.Count; j++)
		{
			GameManager.GetInstance().AddToCamera(chainedFollowers[j].Follower.gameObject);
		}
		yield return new WaitForSeconds(2f);
		for (int i = 0; i < chainedFollowers.Count; i++)
		{
			StartCoroutine(DropFollower(chainedFollowers[i].Follower, i));
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(2f);
		yield return new WaitForSeconds(4f);
		DataManager.Instance.DiedLastRun = false;
		DataManager.Instance.LastRunResults = UIDeathScreenOverlayController.Results.None;
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "sermons/sermon-start-nobook", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "sermons/sermon-loop-nobook", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/Stings/white_eyes", PlayerFarming.Instance.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/standard_jump_spin_float", PlayerFarming.Instance.transform.position);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower", PlayerFarming.Instance.transform.position);
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.MaxZoom, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.MaxZoom = x;
		}, 3f, 10f).SetEase(Ease.InOutSine);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.MinZoom, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.MinZoom = x;
		}, 1f, 10f).SetEase(Ease.InOutSine);
		DOTween.To(() => GameManager.GetInstance().CamFollowTarget.TargetOffset, delegate(Vector3 x)
		{
			GameManager.GetInstance().CamFollowTarget.TargetOffset = x;
		}, Vector3.forward * -1.2f, 10f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(10f);
		SimulationManager.UnPause();
		Action action = delegate
		{
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Credits", 0.5f, "", Save);
		};
		if (DungeonSandboxManager.Active)
		{
			Inventory.AddItem(InventoryItem.ITEM_TYPE.GOD_TEAR, 1);
			UIDeathScreenOverlayController uIDeathScreenOverlayController = MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results.Completed);
			uIDeathScreenOverlayController.Show();
			uIDeathScreenOverlayController.OnShown = (Action)Delegate.Combine(uIDeathScreenOverlayController.OnShown, (Action)delegate
			{
				foreach (FollowerManager.SpawnedFollower chainedFollower in chainedFollowers)
				{
					chainedFollower.Follower.gameObject.SetActive(false);
				}
			});
		}
		else
		{
			QuoteScreenController.Init(new List<QuoteScreenController.QuoteTypes> { QuoteScreenController.QuoteTypes.QuoteBoss5 }, action, action);
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "QuoteScreen", 5f, "", delegate
			{
				Time.timeScale = 1f;
			});
		}
	}

	private void Save()
	{
		DataManager.ResetRunData();
		SaveAndLoad.Save();
	}

	private IEnumerator MurderDeathCat()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "murder", false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/player/speak_to_follower");
		AudioManager.Instance.PlayOneShot("event:/player/murder_follower_sequence");
		followerSpawn.Spine.AnimationState.SetAnimation(1, "murder", false);
		float Duration = followerSpawn.Spine.AnimationState.GetCurrent(1).Animation.Duration;
		GameManager.GetInstance().AddToCamera(followerSpawn.gameObject);
		yield return new WaitForSeconds(0.1f);
		followerSpawn.Spine.CustomMaterialOverride.Clear();
		followerSpawn.Spine.CustomMaterialOverride.Add(followerSpawn.NormalMaterial, followerSpawn.BW_Material);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(PlayerFarming.Instance.originalMaterial, PlayerFarming.Instance.BW_Material);
		HUD_Manager.Instance.ShowBW(0.33f, 0f, 1f);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(followerSpawn.transform.position, new Vector3(0.5f, 0.5f, 1f));
		CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
		yield return new WaitForSeconds(1.6f);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(followerSpawn.transform.position, new Vector3(1f, 1f, 1f));
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.3f);
		PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		followerSpawn.Spine.CustomMaterialOverride.Clear();
		HUD_Manager.Instance.ShowBW(0.33f, 1f, 0f);
		yield return new WaitForSeconds(Duration - 0.1f - 1.7f);
		bool Waiting = true;
		DecorationCustomTarget.Create(followerSpawn.transform.position, PlayerFarming.Instance.transform.position, 0.5f, StructureBrain.TYPES.DECORATION_BOSS_TROPHY_5, delegate
		{
			Waiting = false;
		});
		while (Waiting)
		{
			yield return null;
		}
	}

	private IEnumerator DoSlowMo()
	{
		yield return new WaitForSeconds(1.3333334f);
		GameManager.SetTimeScale(0.2f);
		GameManager.GetInstance().CameraSetZoom(6f);
		yield return new WaitForSeconds(14f / 15f);
		GameManager.SetTimeScale(1f);
		GameManager.GetInstance().CameraResetTargetZoom();
	}

	private IEnumerator DropFollower(Follower follower, int index)
	{
		follower.SetBodyAnimation("FinalBoss/freed", false);
		yield return new WaitForSeconds(0.5f);
		follower.transform.DOMove(new Vector3(follower.transform.position.x, follower.transform.position.y, 0f), 0.5f).SetEase(Ease.InSine);
		yield return new WaitForSeconds(0.5f);
		follower.transform.localScale = Vector3.one;
		follower.State.facingAngle = ((!(PlayerFarming.Instance.transform.position.x > follower.transform.position.x)) ? 180 : 0);
		follower.State.LookAngle = follower.State.facingAngle;
		yield return new WaitForSeconds(1f / 6f);
		follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-cheering");
		bool waiting = true;
		follower.GoTo(GetFollowerPosition(index), delegate
		{
			follower.State.facingAngle = ((!(PlayerFarming.Instance.transform.position.x > follower.transform.position.x)) ? 180 : 0);
			follower.State.LookAngle = follower.State.facingAngle;
			waiting = false;
			count++;
		});
		while (waiting || count < chainedFollowers.Count - 1)
		{
			yield return null;
		}
		follower.SetBodyAnimation("devotion/devotion-start", false);
		follower.AddBodyAnimation("devotion/devotion-collect-loopstart-whiteyes", false, 0f);
		follower.AddBodyAnimation("devotion/devotion-collect-loop-whiteyes", true, 0f);
	}

	private Vector3 GetFollowerPosition(int index)
	{
		float num;
		float f;
		if (chainedFollowers.Count <= 12)
		{
			num = 2f;
			f = (float)index * (360f / (float)chainedFollowers.Count) * ((float)Math.PI / 180f);
			return PlayerFarming.Instance.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		}
		int num2 = 8;
		if (index < num2)
		{
			num = 2f;
			f = (float)index * (360f / (float)Mathf.Min(chainedFollowers.Count, num2)) * ((float)Math.PI / 180f);
		}
		else
		{
			num = 3f;
			f = (float)(index - num2) * (360f / (float)(chainedFollowers.Count - num2)) * ((float)Math.PI / 180f);
		}
		return PlayerFarming.Instance.transform.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
	}

	[CompilerGenerated]
	private IEnumerator _003CDeathCatBigTransformIE_003Eg__SpawnChainedFollower_007C85_3(int index, float delay)
	{
		yield return new WaitForSeconds(delay);
		followerChains[index].Init(cagedFollowers[index]);
		chainedFollowers.Add(cagedFollowers[index]);
	}
}
