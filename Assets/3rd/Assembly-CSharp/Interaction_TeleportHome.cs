using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using MMTools;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Interaction_TeleportHome : Interaction
{
	public bool ResurrectionRoom;

	public RunResults RunResults;

	private bool Activating;

	public Transform PlayerPosition;

	public bool Debug_WarpIn;

	private SkeletonAnimation skeletonAnimation;

	public Animator animator;

	public GoopFade goopFade;

	public bool GoViaQuoteScreen;

	private string sReturnToBase;

	private string sSummonWorkers;

	public bool CanSummonWorkers;

	public static Action<Interaction_TeleportHome> PlayerActivatingStart;

	public static Action<Interaction_TeleportHome> PlayerActivatingEnd;

	public bool DoDeathScreen = true;

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sReturnToBase = ScriptLocalization.Interactions.ReturnToBase;
		sSummonWorkers = "Summon Workers - Add in loc";
	}

	public override void GetLabel()
	{
		base.Label = (Activating ? "" : sReturnToBase);
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		animator.SetBool("isEnabled", true);
	}

	private new void OnDestroy()
	{
		base.OnDestroy();
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		if (skeletonAnimation != null)
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}
		base.OnDisableInteraction();
		animator.SetBool("isEnabled", false);
	}

	public override void OnBecomeCurrent()
	{
		base.OnBecomeCurrent();
		animator.SetBool("isReady", true);
		if (!base.gameObject.activeSelf)
		{
			AudioManager.Instance.PlayOneShot("pentagram_platform/pentagram_platform_start", base.gameObject);
		}
	}

	private IEnumerator PlaySoundAfterOneSecond()
	{
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_on", base.gameObject);
	}

	public override void OnBecomeNotCurrent()
	{
		base.OnBecomeNotCurrent();
		animator.SetBool("isReady", false);
		AudioManager.Instance.PlayOneShot("pentagram_platform/pentagram_platform_end", base.gameObject);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Activating)
		{
			return;
		}
		if (skeletonAnimation == null)
		{
			skeletonAnimation = PlayerFarming.Instance.Spine;
		}
		if (skeletonAnimation == null)
		{
			return;
		}
		skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		if (!(PlayerFarming.Instance != null) || PlayerFarming.Instance.GoToAndStopping)
		{
			return;
		}
		if (!Debug_WarpIn)
		{
			PlayerFarming.Instance.GoToAndStop(PlayerPosition.position, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoTeleportOut());
			});
		}
		else
		{
			PlayerFarming.Instance.GoToAndStop(PlayerPosition.position, base.gameObject, false, false, delegate
			{
				StartCoroutine(DoTeleportIn());
			});
		}
	}

	private IEnumerator DoTeleportOut()
	{
		HUD_Manager.Instance.Hide(false);
		Activating = true;
		Action<Interaction_TeleportHome> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(this);
		}
		PlayerFarming.Instance.transform.DOMove(PlayerPosition.position, 0.25f);
		GameManager.GetInstance().OnConversationNext(PlayerPosition.gameObject, 8f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
		animator.SetTrigger("warpOut");
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("warp-out-down", 0, false, 0f);
		goopFade.FadeIn(1f, 1.4f);
		BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
		yield return new WaitForSeconds(3f);
		BiomeConstants.Instance.ChromaticAbberationTween(0.1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		if (DoDeathScreen)
		{
			if (UIDeathScreenOverlayController.Instance == null)
			{
				MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay((!ResurrectionRoom) ? UIDeathScreenOverlayController.Results.Completed : UIDeathScreenOverlayController.Results.Killed).Show();
				if (ResurrectionRoom)
				{
					RespawnRoomManager.Instance.ResetPathFinding();
				}
			}
		}
		else
		{
			CompleteDoTeleportOut();
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

	private void CompleteDoTeleportOut()
	{
		if (GoViaQuoteScreen)
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
		Activating = false;
		Action<Interaction_TeleportHome> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
		skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
	}

	private IEnumerator DoTeleportIn()
	{
		Activating = true;
		Action<Interaction_TeleportHome> playerActivatingStart = PlayerActivatingStart;
		if (playerActivatingStart != null)
		{
			playerActivatingStart(this);
		}
		GameManager.GetInstance().OnConversationNext(PlayerPosition.gameObject, 8f);
		animator.SetTrigger("warpIn");
		PlayerFarming.Instance.Spine.GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds(1f);
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.simpleSpineAnimator.Animate("warp-in-up", 0, false);
		yield return new WaitForSeconds(3f);
		GameManager.GetInstance().OnConversationEnd();
		Activating = false;
		Action<Interaction_TeleportHome> playerActivatingEnd = PlayerActivatingEnd;
		if (playerActivatingEnd != null)
		{
			playerActivatingEnd(this);
		}
		skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
	}

	private void HandleAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
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

	private void Start()
	{
		UpdateLocalisation();
	}

	public void Play()
	{
	}
}
