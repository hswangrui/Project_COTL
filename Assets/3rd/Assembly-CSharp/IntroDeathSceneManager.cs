using System.Collections;
using MMTools;
using Spine.Unity;
using Unify;
using UnityEngine;

public class IntroDeathSceneManager : BaseMonoBehaviour
{
	public IntroDeathSceneMusicController musicController;

	public Vector3 ActivateOffset;

	public float ActivateDistance;

	public Interaction_SimpleConversation Conversation;

	public Interaction_SimpleConversation ConversationResponse;

	public SkeletonAnimation DeathSpine;

	public GameObject DeathSpineCamera;

	public SkeletonAnimation DeathCat_0;

	public SkeletonAnimation DeathCat_1;

	public SimpleSetCamera SimpleSetCamera;

	public CameraFollowTarget Camera;

	public GameObject PlayerPrefab;

	public Material PlayerMaterial;

	public Transform StartPlayerPosition;

	public bool HideHUD;

	public BiomeConstantsVolume biomeVolume;

	private bool triggered;

	private float playerStartingMaterialValue_0;

	private float playerStartingMaterialValue_1;

	private bool wasPlayed;

	private GameObject Player;

	private void Start()
	{
		Conversation.enabled = false;
		ConversationResponse.enabled = false;
		StartCoroutine(PlaceAndPositionPlayer());
		StartCoroutine(WaitForPlayer());
	}

	private IEnumerator PlaceAndPositionPlayer()
	{
		if (Player == null)
		{
			Player = Object.Instantiate(PlayerPrefab, GameObject.FindGameObjectWithTag("Unit Layer").transform, true);
		}
		StateMachine state = Player.GetComponent<StateMachine>();
		state.facingAngle = 85f;
		if (StartPlayerPosition != null)
		{
			Player.transform.position = StartPlayerPosition.position;
		}
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew(false, true);
		GameManager.GetInstance().CamFollowTarget.SetOffset(new Vector3(0f, 0f, -0.2f));
		GameManager.GetInstance().CamFollowTarget.CurrentOffset = new Vector3(0f, 0f, -0.2f);
		GameManager.GetInstance().CameraSetZoom(5f);
		GameObject playerCameraBone = GameObject.FindWithTag("Player Camera Bone");
		Camera.SnapTo(playerCameraBone.transform.position);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		SimpleSpineAnimator componentInChildren = Player.GetComponentInChildren<SimpleSpineAnimator>();
		componentInChildren.Animate("intro/kneel-wake", 0, false);
		componentInChildren.AddAnimate("intro/idle-up", 0, true, 0f);
		yield return new WaitForSeconds(4.5f);
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().AddToCamera(playerCameraBone);
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private IEnumerator WaitForPlayer()
	{
		while ((Player = GameObject.FindWithTag("Player")) == null)
		{
			yield return null;
		}
		while (Vector3.Distance(Player.transform.position, base.transform.position + ActivateOffset) > ActivateDistance)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		SimpleSetCamera.Play();
		DeathCat_0.AnimationState.SetAnimation(0, "snarling", true);
		DeathCat_1.AnimationState.SetAnimation(0, "snarling", true);
		AudioManager.Instance.PlayOneShot("event:/dialogue/death_cat_dogs/death_cat_dogs", DeathCat_0.gameObject);
		AudioManager.Instance.PlayOneShot("event:/dialogue/death_cat_dogs/death_cat_dogs", DeathCat_1.gameObject);
		DeathSpine.AnimationState.SetAnimation(0, "crouch-down", false);
		DeathSpine.AnimationState.AddAnimation(0, "idle-crouched", true, 0f);
		Object.FindObjectOfType<PlayerPrisonerController>().GoToAndStop(DeathSpine.transform.position + Vector3.down * 2f, StateMachine.State.InActive);
		yield return new WaitForSeconds(1f);
		Conversation.enabled = true;
		Conversation.Play();
	}

	public void Respond()
	{
		AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("DEAL_WITH_THE_DEVIL"));
		Debug.Log("RESPOND!");
		ConversationResponse.enabled = true;
		ConversationResponse.Play();
		LetterBox.Show(true);
	}

	public void GiveCrown()
	{
		StartCoroutine(GiveCrownRoutine());
	}

	private IEnumerator GiveCrownRoutine()
	{
		yield return new WaitForEndOfFrame();
		DataManager.Instance.HadInitialDeathCatConversation = true;
		Player.GetComponent<StateMachine>().CURRENT_STATE = StateMachine.State.InActive;
		LetterBox.Show(true);
		SimpleSetCamera.Reset();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(DeathSpineCamera, 10f);
		DeathSpine.AnimationState.SetAnimation(0, "give-life", false);
		musicController.PlaySpawnCrown();
		float Progress = 0f;
		float Duration = 3.8333335f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			yield return null;
		}
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", PlayVideo);
	}

	private void PlayVideo()
	{
		biomeVolume.manualExitAndDeactive();
		musicController.StopAll();
		MMVideoPlayer.Play("Intro", VideoComplete, MMVideoPlayer.Options.ENABLE, MMVideoPlayer.Options.DISABLE, false);
		AudioManager.Instance.PlayOneShot("event:/music/intro/intro_video");
		Object.FindObjectOfType<IntroManager>().DisableBoth();
		MMTransition.ResumePlay();
	}

	private void VideoComplete()
	{
		if (!wasPlayed)
		{
			MMTransition.IsPlaying = false;
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", ChangeRoom);
			wasPlayed = true;
		}
	}

	private void ChangeRoom()
	{
		if (!triggered)
		{
			triggered = true;
			MMTransition.IsPlaying = true;
			MMVideoPlayer.Hide();
			PlayerMaterial.SetFloat("_CloudOverlayIntensity", playerStartingMaterialValue_0);
			PlayerMaterial.SetFloat("_ShadowIntensity", playerStartingMaterialValue_1);
			Object.FindObjectOfType<IntroManager>().ToggleGameScene();
			MMTransition.ResumePlay();
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, ActivateDistance, Color.green);
	}
}
