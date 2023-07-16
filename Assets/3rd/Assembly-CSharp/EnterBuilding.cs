using System.Collections;
using MMTools;
using UnityEngine;
using UnityEngine.Events;

public class EnterBuilding : BaseMonoBehaviour
{
	public UnityEvent Trigger;

	private GameObject Player;

	public FollowerLocation Destination;

	public bool HideCanvasConstants;

	public bool ShowCanvasConstants;

	private bool placedPlayer;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (MMConversation.isPlaying)
		{
			MMConversation.mmConversation.FinishCloseFadeTweenByForce();
		}
		if (collision.gameObject.tag == "Player" && !MMConversation.isPlaying && !PlayerFarming.Instance.GoToAndStopping && !LetterBox.IsPlaying)
		{
			placedPlayer = false;
			PlayerFarming.Instance.DropDeadFollower();
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.4f, "", DoTrigger);
			Player = collision.gameObject;
			if (HideCanvasConstants)
			{
				CanvasConstants.instance.Hide();
			}
			if (ShowCanvasConstants)
			{
				CanvasConstants.instance.Show();
			}
			GameManager.GetInstance().StartCoroutine(CheckToEnsurePositioned());
		}
	}

	private IEnumerator CheckToEnsurePositioned()
	{
		yield return new WaitForSeconds(1f);
		if (!placedPlayer)
		{
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.4f, "", DoTrigger);
		}
	}

	private void DoTrigger()
	{
		UnityEvent trigger = Trigger;
		if (trigger != null)
		{
			trigger.Invoke();
		}
		if (LocationManager.LocationManagers.ContainsKey(Destination))
		{
			LocationManager.LocationManagers[Destination].PositionPlayer();
		}
		GameManager.GetInstance().CameraSnapToPosition(PlayerFarming.Instance.transform.position);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		placedPlayer = true;
	}
}
