using System.Collections;
using MMTools;
using UnityEngine;

public class Interaction_TempleBossRitual : Interaction
{
	public RoomSwapManager RoomSwapManager;

	private bool Activating;

	public override void GetLabel()
	{
		base.Label = (Activating ? "" : "Perform spooky boopy ritual!");
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StartCoroutine(InteractionRoutine());
		StartCoroutine(CentrePlayer());
	}

	private IEnumerator CentrePlayer()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		Vector3 StartPosition = base.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				state.transform.position = Vector3.Lerp(StartPosition, base.transform.position + Vector3.down, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator InteractionRoutine()
	{
		state.facingAngle = 90f;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("sermons/sermon-loop", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		float Progress = 0f;
		float Duration = 3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			GameManager.GetInstance().CameraSetZoom(8f - 3f * Progress / Duration);
			CameraManager.shakeCamera(0.1f + 0.6f * (Progress / Duration));
			yield return null;
		}
		PlayerFarming.Instance.simpleSpineAnimator.Animate("teleport-out", 0, false);
		yield return new WaitForSeconds(1.1666666f);
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.2f, "", ChangeRoom);
	}

	private void ChangeRoom()
	{
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (allBrain.CurrentTask.Type == FollowerTaskType.AssistRitual)
			{
				allBrain.FollowingPlayer = false;
				allBrain.CompleteCurrentTask();
				allBrain.DesiredLocation = FollowerLocation.Base;
			}
		}
		RoomSwapManager.ToggleChurch();
		CameraManager.shakeCamera(0.5f);
		GameManager.GetInstance().StartCoroutine(PlayerStopAnimationRoutine());
	}

	private IEnumerator PlayerStopAnimationRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		PlayerFarming.Instance.simpleSpineAnimator.Animate("teleport-in", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(1.1f);
		GameManager.GetInstance().OnConversationEnd();
	}
}
