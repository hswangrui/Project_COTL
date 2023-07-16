using System.Collections;
using System.Collections.Generic;
using Beffio.Dithering;
using Lamb.UI;
using Lamb.UI.Menus.DoctrineChoicesMenu;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using Spine;
using Spine.Unity;
using src.Extensions;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class MysticShopKeeperManager : BaseMonoBehaviour
{
	public static MysticShopKeeperManager Instance;

	private GenerateRoom generateRoom;

	private GameObject Player;

	public GameObject PlayerPrefab;

	public Transform PlayerPosition;

	public SimpleSetCamera SimpleSetCamera;

	public SkeletonAnimation Spine;

	[SerializeField]
	private GoopFade goop;

	[SerializeField]
	private Animator animator;

	private SkeletonAnimation skeletonAnimation;

	private Stylizer cameraStylizer;

	private ConversationObject ConversationObject;

	public List<ConversationEntry> ConversationEntries;

	private bool Translate;

	private void OnEnable()
	{
		Instance = this;
		generateRoom = GetComponent<GenerateRoom>();
		cameraStylizer = Camera.main.gameObject.GetComponent<Stylizer>();
		if (cameraStylizer == null)
		{
			Debug.Log("Camera null");
		}
		cameraStylizer.enabled = true;
		goop.gameObject.SetActive(false);
		AudioManager.Instance.PlayMusic("event:/music/death_cat_battle/death_cat_battle");
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.StandardRoom);
	}

	private void OnDisable()
	{
		WeatherSystemController.Instance.ExitedBuilding();
		cameraStylizer.enabled = false;
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
	}

	public static void Play()
	{
		if (Instance == null)
		{
			Instance = Object.FindObjectOfType<MysticShopKeeperManager>();
		}
		Debug.Log("PLAY!");
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", delegate
		{
			Time.timeScale = 1f;
			Instance.gameObject.SetActive(true);
			Instance.StartCoroutine(Instance.PlayRoutine());
		});
	}

	public IEnumerator ConversationCompleted()
	{
		Instance.SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNext(PlayerPosition.gameObject, 8f);
		float seconds = 0.5f;
		yield return new WaitForSeconds(seconds);
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
		GameManager.ToShip();
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
		BiomeGenerator.Instance.gameObject.SetActive(false);
		BiomeGenerator.Instance.Player.SetActive(false);
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
		DataManager.Instance.ForeshadowedMysticShop = true;
		Translate = true;
		ConversationObject = new ConversationObject(ConversationEntries, null, delegate
		{
			StartCoroutine(ConversationCompleted());
		});
		MMConversation.Play(ConversationObject, false, true, true, Translate);
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
