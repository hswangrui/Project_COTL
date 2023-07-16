using System.Collections;
using System.Collections.Generic;
using Beffio.Dithering;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using Lamb.UI.Menus.DoctrineChoicesMenu;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using Spine;
using Spine.Unity;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class DeathCatRoomManager : BaseMonoBehaviour
{
	public enum ConversationTypes
	{
		None,
		Story,
		BeatBoss1,
		BeatBoss2,
		BeatBoss3,
		BeatBoss4,
		GenericDied,
		GenericWon,
		RatauKilled
	}

	public static ConversationTypes ConversationType;

	private static bool Testing;

	public static DeathCatRoomManager Instance;

	private BiomeGenerator biomeGenerator;

	private GenerateRoom generateRoom;

	private GameObject Player;

	public GameObject PlayerPrefab;

	public Transform PlayerPosition;

	public GameObject DeathCatObject;

	public SimpleSetCamera SimpleSetCamera;

	public SkeletonAnimation Spine;

	[TermsPopup("")]
	public string CharacterName = "-";

	[SerializeField]
	private GoopFade goop;

	[SerializeField]
	private Animator animator;

	private SkeletonAnimation skeletonAnimation;

	private List<string> Sounds = new List<string> { "event:/dialogue/death_cat/standard_death" };

	private List<string> Animations = new List<string> { "talk", "talk2", "talk3", "talk-laugh" };

	private Stylizer cameraStylizer;

	private static ConversationTypes TestingResult;

	private List<string> AnimationList = new List<string> { "talk", "talk2", "talk3", "talk-laugh" };

	public ConversationObject ConversationObject;

	public List<ConversationEntry> ConversationEntries;

	private bool Translate;

	public static int Story
	{
		get
		{
			return DataManager.Instance.DeathCatStory;
		}
		set
		{
			DataManager.Instance.DeathCatStory = value;
		}
	}

	public static bool Boss1
	{
		get
		{
			return DataManager.Instance.DeathCatBoss1;
		}
		set
		{
			DataManager.Instance.DeathCatBoss1 = value;
		}
	}

	public static bool Boss2
	{
		get
		{
			return DataManager.Instance.DeathCatBoss2;
		}
		set
		{
			DataManager.Instance.DeathCatBoss2 = value;
		}
	}

	public static bool Boss3
	{
		get
		{
			return DataManager.Instance.DeathCatBoss3;
		}
		set
		{
			DataManager.Instance.DeathCatBoss3 = value;
		}
	}

	public static bool Boss4
	{
		get
		{
			return DataManager.Instance.DeathCatBoss4;
		}
		set
		{
			DataManager.Instance.DeathCatBoss4 = value;
		}
	}

	public static bool RatauKilled
	{
		get
		{
			return DataManager.Instance.DeathCatRatauKilled;
		}
		set
		{
			DataManager.Instance.DeathCatRatauKilled = value;
		}
	}

	public static int Dead
	{
		get
		{
			return DataManager.Instance.DeathCatDead;
		}
		set
		{
			DataManager.Instance.DeathCatDead = value;
		}
	}

	public static int Won
	{
		get
		{
			return DataManager.Instance.DeathCatWon;
		}
		set
		{
			DataManager.Instance.DeathCatWon = value;
		}
	}

	public static ConversationTypes GetConversationType()
	{
		if (Testing)
		{
			Story = 1;
			ConversationType = TestingResult;
			Testing = false;
			return ConversationType;
		}
		ConversationType = ConversationTypes.None;
		if (DataManager.Instance.DeathCatBeaten)
		{
			return ConversationType = ConversationTypes.None;
		}
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) && !Boss1)
		{
			return ConversationType = ConversationTypes.BeatBoss1;
		}
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) && !Boss2)
		{
			return ConversationType = ConversationTypes.BeatBoss2;
		}
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) && !Boss3)
		{
			return ConversationType = ConversationTypes.BeatBoss3;
		}
		if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4) && !Boss4)
		{
			return ConversationType = ConversationTypes.BeatBoss4;
		}
		if (DataManager.Instance.RatauKilled && !RatauKilled)
		{
			return ConversationType = ConversationTypes.RatauKilled;
		}
		if (DataManager.Instance.DeathCatConversationLastRun < DataManager.Instance.dungeonRun + 3)
		{
			switch (UIDeathScreenOverlayController.Result)
			{
			case UIDeathScreenOverlayController.Results.Completed:
				Debug.Log("Completed");
				if (DataManager.Instance.dungeonRun >= 2 && GetConversationCount(ConversationTypes.Story.ToString() + Story) > 0 && UIDeathScreenOverlayController.Result == UIDeathScreenOverlayController.Results.Completed)
				{
					if (Story == 2)
					{
						if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.Special_Bonfire))
						{
							return ConversationType = ConversationTypes.Story;
						}
						return ConversationType = ConversationTypes.None;
					}
					return ConversationType = ConversationTypes.Story;
				}
				return ConversationType = ConversationTypes.None;
			case UIDeathScreenOverlayController.Results.Escaped:
				Debug.Log("ESCAPED");
				return ConversationType = ConversationTypes.None;
			case UIDeathScreenOverlayController.Results.Killed:
				Debug.Log("Killed");
				return ConversationType = ((GetConversationCount(ConversationTypes.GenericDied.ToString() + Dead) > 0) ? ConversationTypes.GenericDied : ConversationTypes.None);
			}
		}
		return ConversationType;
	}

	private void OnEnable()
	{
		Instance = this;
		generateRoom = GetComponent<GenerateRoom>();
		cameraStylizer = Camera.main.gameObject.GetComponent<Stylizer>();
		if (cameraStylizer == null)
		{
			Debug.Log("Camera null");
		}
		if(cameraStylizer)
			cameraStylizer.enabled = true;
		goop.gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		WeatherSystemController.Instance.ExitedBuilding();
        if (cameraStylizer)
            cameraStylizer.enabled = false;
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
	}

	private void Test(ConversationTypes Result = ConversationTypes.Story)
	{
		TestingResult = Result;
		Testing = true;
		Play();
	}

	public static void Play()
	{
		Debug.Log("PLAY!");
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", delegate
		{
			if (Instance == null)
			{
				Instance = Object.FindObjectOfType<DeathCatRoomManager>();
			}
			Time.timeScale = 1f;
			Instance.gameObject.SetActive(true);
			Instance.StartCoroutine(Instance.PlayRoutine());
		});
	}

	public void Init(BiomeGenerator biomeGenerator)
	{
		this.biomeGenerator = biomeGenerator;
	}

	public string GetConversation()
	{
		ConversationTypes conversationType = GetConversationType();
		switch (conversationType)
		{
		case ConversationTypes.Story:
			return "Story" + Story;
		case ConversationTypes.GenericDied:
			return "GenericDied" + Dead;
		case ConversationTypes.GenericWon:
			return "GenericWon" + Won;
		case ConversationTypes.RatauKilled:
			return "Ratau";
		default:
			return conversationType.ToString();
		}
	}

	private static int GetConversationCount(string Entry)
	{
		int num = 0;
		int num2 = -1;
		string text = "DeathCat/" + Entry + "/";
		Debug.Log(text + (num2 + 1));
		while (true)
		{
			int num3 = ++num2;
			if (LocalizationManager.GetTermData(text + num3) == null)
			{
				break;
			}
			num++;
		}
		return num;
	}

	private List<ConversationEntry> GetConversationEntry(string Entry)
	{
		List<ConversationEntry> list = new List<ConversationEntry>();
		string text = "DeathCat/" + Entry + "/";
		int num = -1;
		while (true)
		{
			int num2 = ++num;
			if (LocalizationManager.GetTermData(text + num2) == null)
			{
				break;
			}
			Debug.Log(text + num);
			list.Add(new ConversationEntry(DeathCatObject, text + num, Animations[Random.Range(0, Animations.Count)], "event:/dialogue/death_cat/standard_death"));
			list[list.Count - 1].CharacterName = CharacterName;
			list[list.Count - 1].SkeletonData = Spine;
			list[list.Count - 1].Animation = AnimationList[Random.Range(0, AnimationList.Count)];
		}
		return list;
	}

	public IEnumerator ConversationCompleted()
	{
		Instance.SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNext(PlayerPosition.gameObject, 8f);
		float Delay = 0.5f;
		switch (ConversationType)
		{
		case ConversationTypes.Story:
			Debug.Log("STORY: " + Story);
			switch (Story)
			{
			case 1:
				Delay = 0f;
				yield return StartCoroutine(GiveDoctrine(2));
				break;
			case 2:
				Delay = 0f;
				yield return StartCoroutine(GiveDoctrine(3));
				break;
			case 3:
				DataManager.Instance.OnboardedOfferingChest = true;
				break;
			}
			Story++;
			break;
		case ConversationTypes.GenericWon:
			Won++;
			break;
		case ConversationTypes.GenericDied:
			Dead++;
			break;
		case ConversationTypes.BeatBoss1:
			if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BishopsOfTheOldFaith", Objectives.CustomQuestTypes.FreeDeathCat));
			}
			Boss1 = true;
			break;
		case ConversationTypes.BeatBoss2:
			if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BishopsOfTheOldFaith", Objectives.CustomQuestTypes.FreeDeathCat));
			}
			Boss2 = true;
			break;
		case ConversationTypes.BeatBoss3:
			if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BishopsOfTheOldFaith", Objectives.CustomQuestTypes.FreeDeathCat));
			}
			Boss3 = true;
			break;
		case ConversationTypes.BeatBoss4:
			if (DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) && DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
			{
				ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/BishopsOfTheOldFaith", Objectives.CustomQuestTypes.FreeDeathCat));
			}
			Boss4 = true;
			break;
		case ConversationTypes.RatauKilled:
			RatauKilled = true;
			break;
		}
		DataManager.Instance.DeathCatConversationLastRun = DataManager.Instance.dungeonRun;
		yield return null;
		yield return new WaitForSeconds(Delay);
		Player.GetComponent<StateMachine>().CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		animator.Play("WarpOut");
		PlayerFarming.Instance.simpleSpineAnimator.Animate("warp-out-down", 0, false);
		goop.gameObject.SetActive(true);
		goop.FadeIn(1f, 1.4f);
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(0.5f);
		BiomeConstants.Instance.ChromaticAbberationTween(0.1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		if (GoViaQuote())
		{
			QuoteScreenController.Init(new List<QuoteScreenController.QuoteTypes> { GetQuoteType() }, delegate
			{
				GameManager.ToShip();
			}, delegate
			{
				GameManager.ToShip();
			});
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "QuoteScreen", 5f, "", delegate
			{
				Time.timeScale = 1f;
			});
		}
		else
		{
			GameManager.ToShip();
		}
	}

	private IEnumerator GiveDoctrine(int Level)
	{
		yield return null;
		MMConversation.CURRENT_CONVERSATION = new ConversationObject(null, null, null, new List<DoctrineResponse>
		{
			new DoctrineResponse(SermonCategory.Special, Level, true, null)
		});
		UIDoctrineChoicesMenuController doctrineChoicesInstance = MonoSingleton<UIManager>.Instance.DoctrineChoicesMenuTemplate.Instantiate();
		doctrineChoicesInstance.Show();
		while (doctrineChoicesInstance.gameObject.activeInHierarchy)
		{
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/sermon/select_sermon", PlayerFarming.Instance.gameObject);
		DoctrineUpgradeSystem.UnlockAbility(DoctrineUpgradeSystem.GetSermonReward(SermonCategory.Special, Level, true));
		UITutorialOverlayController TutorialOverlay = null;
		switch (Level)
		{
		case 2:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.FollowerAction))
			{
				TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.FollowerAction);
			}
			ObjectiveManager.Add(new Objectives_Custom("FollowerInteractions/Surveillance", Objectives.CustomQuestTypes.ReadMind), true);
			break;
		case 3:
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Rituals))
			{
				TutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Rituals);
			}
			break;
		}
		while (TutorialOverlay != null)
		{
			yield return null;
		}
	}

	private bool GoViaQuote()
	{
		ConversationTypes conversationType = ConversationType;
		if ((uint)(conversationType - 2) <= 3u)
		{
			return true;
		}
		return false;
	}

	private QuoteScreenController.QuoteTypes GetQuoteType()
	{
		Debug.Log("GET QUOTE TYPE! " + PlayerFarming.Location);
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			return QuoteScreenController.QuoteTypes.QuoteBoss1;
		case FollowerLocation.Dungeon1_2:
			return QuoteScreenController.QuoteTypes.QuoteBoss2;
		case FollowerLocation.Dungeon1_3:
			return QuoteScreenController.QuoteTypes.QuoteBoss3;
		case FollowerLocation.Dungeon1_4:
			return QuoteScreenController.QuoteTypes.QuoteBoss4;
		case FollowerLocation.Dungeon1_5:
			return QuoteScreenController.QuoteTypes.QuoteBoss5;
		default:
			return QuoteScreenController.QuoteTypes.QuoteBoss5;
		}
	}

	private IEnumerator PlayRoutine()
	{
		WeatherSystemController.Instance.EnteredBuilding();
		AudioManager.Instance.PlayOneShot("event:/Stings/church_bell", base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().CachedCamTargets = new List<CameraFollowTarget.Target>();
		Instance.generateRoom.SetColliderAndUpdatePathfinding();
		Instance.biomeGenerator.gameObject.SetActive(false);
		Instance.biomeGenerator.Player.SetActive(false);
		yield return null;
		Camera.main.backgroundColor = Color.white;
		Player = Object.Instantiate(PlayerPrefab, PlayerPosition.position, Quaternion.identity, base.transform);
		GameManager.GetInstance().CameraSnapToPosition(Player.transform.position);
		GameManager.GetInstance().AddToCamera(PlayerFarming.Instance.CameraBone);
		yield return null;
		GameManager.GetInstance().OnConversationNew(false, true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
		StateMachine component = Player.GetComponent<StateMachine>();
		component.facingAngle = 85f;
		component.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return null;
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		}
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_on", base.gameObject);
		animator.SetTrigger("warpIn");
		PlayerFarming.Instance.simpleSpineAnimator.Animate("warp-in-up", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle-up", 0, true, 0f);
		yield return new WaitForSeconds(3f);
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
		Instance.SimpleSetCamera.Play();
		yield return new WaitForSeconds(1f);
		Translate = true;
		ConversationEntries = GetConversationEntry(GetConversation());
		AddFinalConversation();
		ConversationObject = new ConversationObject(ConversationEntries, null, delegate
		{
			StartCoroutine(ConversationCompleted());
		});
		MMConversation.Play(ConversationObject, false, true, true, Translate);
	}

	private void AddFinalConversation()
	{
		if ((ConversationType != ConversationTypes.BeatBoss1 && ConversationType != ConversationTypes.BeatBoss2 && ConversationType != ConversationTypes.BeatBoss3 && ConversationType != ConversationTypes.BeatBoss4) || !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_1) || !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_2) || !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_3) || !DataManager.Instance.BossesCompleted.Contains(FollowerLocation.Dungeon1_4))
		{
			return;
		}
		foreach (ConversationEntry item in GetConversationEntry("BossFinal"))
		{
			ConversationEntries.Add(item);
		}
		Translate = false;
		ConversationEntry conversationEntry = null;
		ConversationEntry conversationEntry2 = null;
		ConversationEntry conversationEntry3 = null;
		foreach (ConversationEntry conversationEntry4 in ConversationEntries)
		{
			if (conversationEntry4.TermToSpeak == "DeathCat/BossFinal/2")
			{
				conversationEntry = conversationEntry4;
				conversationEntry4.TermToSpeak = string.Format(LocalizationManager.GetTranslation(conversationEntry4.TermToSpeak), DataManager.Instance.STATS_FollowersStarvedToDeath);
			}
			else if (conversationEntry4.TermToSpeak == "DeathCat/BossFinal/3")
			{
				conversationEntry2 = conversationEntry4;
				conversationEntry4.TermToSpeak = string.Format(LocalizationManager.GetTranslation(conversationEntry4.TermToSpeak), DataManager.Instance.STATS_Sacrifices);
			}
			else if (conversationEntry4.TermToSpeak == "DeathCat/BossFinal/4")
			{
				conversationEntry3 = conversationEntry4;
				conversationEntry4.TermToSpeak = string.Format(LocalizationManager.GetTranslation(conversationEntry4.TermToSpeak), DataManager.Instance.STATS_Murders);
			}
			else
			{
				conversationEntry4.TermToSpeak = LocalizationManager.GetTranslation(conversationEntry4.TermToSpeak);
			}
		}
		if (conversationEntry != null && DataManager.Instance.STATS_FollowersStarvedToDeath <= 0)
		{
			ConversationEntries.Remove(conversationEntry);
		}
		if (conversationEntry2 != null && DataManager.Instance.STATS_Sacrifices <= 0)
		{
			ConversationEntries.Remove(conversationEntry2);
		}
		if (conversationEntry3 != null && DataManager.Instance.STATS_Murders <= 0)
		{
			ConversationEntries.Remove(conversationEntry3);
		}
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "warp-in-burst_start")
		{
			PlayerFarming.Instance.simpleSpineAnimator.SetColor(Color.black);
			PlayerFarming.Instance.Spine.GetComponent<MeshRenderer>().enabled = true;
		}
		if (e.Data.Name == "warp-in-burst_end")
		{
			PlayerFarming.Instance.simpleSpineAnimator.SetColor(Color.white);
		}
	}
}
