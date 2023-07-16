using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using MMTools;
using Spine;
using Spine.Unity;
using src.Extensions;
using src.Managers;
using src.UI;
using src.UI.Overlays.EventOverlay;
using UnityEngine;

public class Interaction_BaseTeleporter : Interaction
{
	public Animator animator;

	private SkeletonAnimation skeletonAnimation;

	private bool Used;

	private bool Activating;

	public GameObject distortionObject;

	public GameObject LightingVolume;

	public Material PlayerStencilRT;

	public Material originalMaterial;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject BlackWhiteOverlay;

	public static Interaction_BaseTeleporter Instance;

	public GoopFade goopFade;

	[SerializeField]
	public GameObject newLocation;

	private string sInteract;

	public static Action OnPlayerTeleportedIn;

	private void Start()
	{
		HasSecondaryInteraction = false;
		ActivateDistance = 2f;
		UpdateLocalisation();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Instance = this;
		distortionObject.gameObject.SetActive(false);
		if (LightingVolume != null)
		{
			LightingVolume.gameObject.SetActive(false);
		}
	}

	private new void OnDestroy()
	{
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
	}

	public override void OnDisableInteraction()
	{
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
		base.OnDisableInteraction();
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		animator.SetBool("isReady", true);
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_on", base.gameObject);
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		animator.SetBool("isReady", false);
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_off", base.gameObject);
	}

	public override void IndicateHighlighted()
	{
	}

	public override void EndIndicateHighlighted()
	{
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sInteract = ScriptLocalization.Interactions.Teleport;
	}

	public override void GetLabel()
	{
		bool flag = false;
	//	flag = TwitchHelpHinder.Active;
		base.Label = ((Activating || !DataManager.Instance.UnlockBaseTeleporter || DataManager.Instance.DiscoveredLocations.Count <= 1 || flag) ? "" : sInteract);
	}

	public override void OnInteract(StateMachine state)
	{
		if (Used)
		{
			return;
		}
		base.OnInteract(state);
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		if (!(skeletonAnimation == null))
		{
			skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
			Used = true;
			Activating = true;
			GameManager.GetInstance().OnConversationNew(false);
			GameManager.GetInstance().OnConversationNext(base.gameObject, 8f);
			PlayerFarming.Instance.GoToAndStop(base.transform.position, null, false, false, delegate
			{
				StartCoroutine(TeleportOut());
			});
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(PlayerFarming.Instance == null))
		{
			newLocation.SetActive(!Activating && DataManager.Instance.UnlockBaseTeleporter && DataManager.Instance.DiscoveredLocations.Count > 1 && DataManager.Instance.DiscoveredLocations.Count > DataManager.Instance.VisitedLocations.Count);
		}
	}

	private IEnumerator TeleportOut()
	{
		if (LightingVolume != null)
		{
			LightingVolume.gameObject.SetActive(true);
		}
		PlayerFarming.Instance.transform.DOMove(base.gameObject.transform.position, 0.5f).SetEase(Ease.InOutSine);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		animator.SetTrigger("warpOut");
		PlayerFarming.Instance.simpleSpineAnimator.Animate("warp-out-down", 0, false);
		BlackWhiteOverlay.SetActive(true);
		BiomeConstants.Instance.GoopFadeIn(1f, 1.4f);
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		yield return new WaitForSeconds(1.4f);
		PulseDisplacementObject();
		yield return new WaitForSeconds(0.75f);
		BiomeConstants.Instance.ChromaticAbberationTween(0.1f, 0.6f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		MMTransition.StopCurrentTransition();
		UIWorldMapMenuController uIWorldMapMenuController = MonoSingleton<UIManager>.Instance.ShowWorldMap();
		uIWorldMapMenuController.Show();
		uIWorldMapMenuController.OnCancel = (Action)Delegate.Combine(uIWorldMapMenuController.OnCancel, (Action)delegate
		{
			TeleportIn();
		});
		if (LightingVolume != null)
		{
			LightingVolume.gameObject.SetActive(false);
		}
		if (BlackWhiteOverlay != null)
		{
			BlackWhiteOverlay.SetActive(false);
		}
	}

	public void PulseDisplacementObject()
	{
		if (distortionObject.gameObject.activeSelf)
		{
			distortionObject.transform.localScale = Vector3.zero;
			distortionObject.transform.DORestart();
			distortionObject.transform.DOPlay();
			return;
		}
		distortionObject.SetActive(true);
		distortionObject.transform.localScale = Vector3.zero;
		distortionObject.transform.DOScale(9f, 0.9f).SetEase(Ease.Linear).OnComplete(delegate
		{
			distortionObject.SetActive(false);
		});
	}

	public void TeleportIn()
	{
		StartCoroutine(DoTeleportRoutine());
	}

	private IEnumerator DoTeleportRoutine()
	{
		HUD_Manager.Instance.Hide(true);
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		BlackWhiteOverlay.SetActive(true);
		BiomeConstants.Instance.GoopFadeOut(1f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		Activating = true;
		PlayerFarming.Instance.transform.position = base.transform.position;
		GameManager.GetInstance().CameraSnapToPosition(PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew(false, true);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 8f);
		animator.SetTrigger("warpIn");
		PlayerFarming.Instance.Spine.GetComponent<MeshRenderer>().enabled = false;
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		yield return new WaitForSeconds(1f);
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("warp-in-up", 0, false);
		PlayerFarming.Instance.state.facingAngle = (PlayerFarming.Instance.state.LookAngle = 270f);
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.Spine.GetComponent<MeshRenderer>().enabled = true;
		yield return new WaitForSeconds(2.5f);
		if ((!CheatConsole.IN_DEMO && Onboarding.CurrentPhase != DataManager.OnboardingPhase.Done && !DataManager.Instance.DifficultyChosen) || (!CheatConsole.IN_DEMO && Onboarding.CurrentPhase == DataManager.OnboardingPhase.Done && !DataManager.Instance.DifficultyReminded && DifficultyManager.PrimaryDifficulty >= DifficultyManager.Difficulty.Medium && DataManager.Instance.playerDeathsInARow >= 2))
		{
			UIDifficultySelectorOverlayController uIDifficultySelectorOverlayController = MonoSingleton<UIManager>.Instance.ShowDifficultySelector();
			if (uIDifficultySelectorOverlayController != null)
			{
				uIDifficultySelectorOverlayController.OnHidden = (Action)Delegate.Combine(uIDifficultySelectorOverlayController.OnHidden, (Action)delegate
				{
					if (!DataManager.Instance.DifficultyChosen)
					{
						DataManager.Instance.DifficultyChosen = true;
					}
					else
					{
						DataManager.Instance.DifficultyReminded = true;
					}
				});
				uIDifficultySelectorOverlayController.OnDifficultySelected = (Action<int>)Delegate.Combine(uIDifficultySelectorOverlayController.OnDifficultySelected, (Action<int>)delegate(int difficulty)
				{
					DataManager.Instance.MetaData.Difficulty = difficulty;
					DifficultyManager.ForceDifficulty(DataManager.Instance.MetaData.Difficulty);
				});
			}
			yield return uIDifficultySelectorOverlayController.YieldUntilHidden();
		}
		if (DataManager.Instance.OnboardingFinished && SeasonalEventManager.InitialiseEvents())
		{
			SeasonalEventData activeEvent = SeasonalEventManager.GetActiveEvent();
			if (activeEvent != null)
			{
				UIEventOverlay uIEventOverlay = activeEvent.EventOverlay.Instantiate();
				uIEventOverlay.Show(activeEvent);
				yield return uIEventOverlay.YieldUntilHidden();
			}
		}
		if (DataManager.Instance.DeathCatBeaten && !PersistenceManager.PersistentData.PostGameRevealed)
		{
			UIMenuConfirmationWindow uIMenuConfirmationWindow = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
			uIMenuConfirmationWindow.Show();
			uIMenuConfirmationWindow.Configure(ScriptLocalization.UI_PostGameUnlock.Header, ScriptLocalization.UI_PostGameUnlock.Description, true);
			yield return uIMenuConfirmationWindow.YieldUntilHidden();
			PersistenceManager.PersistentData.PostGameRevealed = true;
			PersistenceManager.Save();
		}
		if (!DataManager.Instance.CompletedSandbox && DungeonSandboxManager.GetCompletedRunCount() >= 120)
		{
			UIMenuConfirmationWindow uIMenuConfirmationWindow2 = MonoSingleton<UIManager>.Instance.ConfirmationWindowTemplate.Instantiate();
			uIMenuConfirmationWindow2.Show();
			uIMenuConfirmationWindow2.Configure(ScriptLocalization.UI_CompletedPurgatory.Header, ScriptLocalization.UI_CompletedPurgatory.Description, true);
			yield return uIMenuConfirmationWindow2.YieldUntilHidden();
			DataManager.Instance.CompletedSandbox = true;
		}
		GameManager.GetInstance().OnConversationEnd();
		Used = false;
		Activating = false;
		skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		if (BlackWhiteOverlay != null)
		{
			BlackWhiteOverlay.SetActive(false);
		}
		Action onPlayerTeleportedIn = OnPlayerTeleportedIn;
		if (onPlayerTeleportedIn != null)
		{
			onPlayerTeleportedIn();
		}
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "warp-in-burst_start")
		{
			if (originalMaterial != null && PlayerStencilRT != null)
			{
				PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
				PlayerFarming.Instance.Spine.CustomMaterialOverride.Add(originalMaterial, PlayerStencilRT);
			}
			PlayerFarming.Instance.Spine.GetComponent<MeshRenderer>().enabled = true;
			PulseDisplacementObject();
		}
		if (e.Data.Name == "warp-in-burst_end")
		{
			PlayerFarming.Instance.Spine.CustomMaterialOverride.Clear();
		}
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
	}

	public void ActivateRoutine()
	{
		StartCoroutine(ActivateIE());
	}

	private IEnumerator ActivateIE()
	{
		Activating = true;
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		AudioManager.Instance.PlayOneShot("event:/ui/map_location_pan", base.gameObject);
		yield return new WaitForSeconds(1f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.SoftImpact);
		CameraManager.instance.ShakeCameraForDuration(0f, 2f, 0.5f);
		animator.SetBool("isReady", true);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.gameObject);
		Activating = false;
		yield return new WaitForSeconds(2.2f);
		animator.SetBool("isReady", false);
		GameManager.GetInstance().OnConversationEnd();
	}
}
