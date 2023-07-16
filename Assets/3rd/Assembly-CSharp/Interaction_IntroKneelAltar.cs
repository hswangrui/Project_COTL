using System;
using System.Collections;
using I2.Loc;
using MMTools;
using Spine;
using Spine.Unity;
using UnityEngine;

public class Interaction_IntroKneelAltar : Interaction
{
	public Interaction_SimpleConversation Conversation;

	public IntroRoomMusicController musicController;

	public GameObject PlayerMoveToPosition;

	private UnitObject unitObject;

	public EnemyBrute Executioner;

	public SkeletonAnimation ExecutionerSpine;

	public SimpleSetCamera SimpleSetCamera;

	public SimpleSetCamera TurnOffSimpleSetCamera;

	private bool Activated;

	private string sInteraction;

	private void Start()
	{
		HoldToInteract = true;
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sInteraction = ScriptLocalization.Interactions_Intro.Kneel;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sInteraction);
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			base.OnInteract(state);
			Activated = true;
			state.CURRENT_STATE = StateMachine.State.InActive;
			StartCoroutine(PanToLeader());
		}
	}

	private IEnumerator PanToLeader()
	{
		SimpleSetCamera.DisableAll();
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddToCamera(Conversation.Entries[0].Speaker);
		PlayerPrisonerController.Instance.Speed = 0f;
		yield return new WaitForSeconds(1f);
		Conversation.enabled = true;
	}

	public void Play()
	{
		SimpleSetCamera.EnableAll();
		StartCoroutine(KneelRoutine());
	}

	private IEnumerator KneelRoutine()
	{
		musicController.PlayExecutionTrack();
		Activated = true;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 8f);
		yield return new WaitForEndOfFrame();
		LetterBox.Show(true);
		yield return new WaitForSeconds(0.5f);
		PlayerPrisonerController component = state.GetComponent<PlayerPrisonerController>();
		unitObject = component.GoToAndStop(PlayerMoveToPosition.transform.position);
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Combine(obj.EndOfPath, new Action(EndOfPath));
	}

	private void Continue()
	{
		StartCoroutine(ContinueRoutine());
	}

	private IEnumerator ContinueRoutine()
	{
		PlayerPrisonerController.Instance.GoToAndStopping = true;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		SimpleSpineAnimator componentInChildren = state.GetComponentInChildren<SimpleSpineAnimator>();
		componentInChildren.Animate("intro/kneel", 0, false);
		componentInChildren.AddAnimate("intro/kneel-loop", 0, true, 0f);
		TurnOffSimpleSetCamera.enabled = false;
		SimpleSetCamera.Play();
		yield return new WaitForSeconds(1f);
		MMVibrate.RumbleContinuous(0.01f, 0.01f);
		Executioner.state.CURRENT_STATE = StateMachine.State.Moving;
		Vector3 StartPosition = Executioner.transform.position;
		Vector3 EndPosition = state.transform.position + Vector3.right * 3f;
		float Progress = 0f;
		float Duration = 2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Executioner.transform.position = Vector3.Lerp(StartPosition, EndPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Executioner.state.CURRENT_STATE = StateMachine.State.Idle;
		MMVibrate.RumbleContinuous(0.02f, 0.02f);
		yield return new WaitForSeconds(0.5f);
		MMVibrate.RumbleContinuous(0.03f, 0.03f);
		Executioner.state.CURRENT_STATE = StateMachine.State.CustomAction0;
		ExecutionerSpine.AnimationState.SetAnimation(0, "execute", false);
		ExecutionerSpine.AnimationState.AddAnimation(0, "idle", true, 0f);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/warning", Executioner.transform.position);
		ExecutionerSpine.AnimationState.Event += ExecuteEvent;
		MMVibrate.RumbleContinuous(0.5f, 0.5f);
		yield return new WaitForSeconds(1.8f);
		AudioManager.Instance.PlayOneShot("event:/sermon/scroll_sermon_menu", AudioManager.Instance.Listener);
		MMVibrate.RumbleContinuous(1f, 1f);
		yield return new WaitForSeconds(0.3f);
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid_large/attack", AudioManager.Instance.Listener);
		AudioManager.Instance.PlayOneShot("event:/weapon/melee_swing_heavy", AudioManager.Instance.Listener);
	}

	private void ExecuteEvent(TrackEntry trackEntry, Spine.Event e)
	{
		string text = e.Data.Name;
		if (text == "execute")
		{
			AudioManager.Instance.PlayOneShot("weapon/metal_heavy", PlayerPrisonerController.Instance.transform.position);
			CameraManager.shakeCamera(0.5f);
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.2f, "", ChangeRoom);
		}
	}

	private void ChangeRoom()
	{
		MMVibrate.StopRumble();
		musicController.StopAll();
		UnityEngine.Object.FindObjectOfType<IntroManager>().ToggleDeathScene();
	}

	private void EndOfPath()
	{
		UnitObject obj = unitObject;
		obj.EndOfPath = (Action)Delegate.Remove(obj.EndOfPath, new Action(EndOfPath));
		Continue();
	}
}
