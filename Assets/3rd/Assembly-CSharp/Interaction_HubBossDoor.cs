using System.Collections;
using I2.Loc;
using MMTools;
using UnityEngine;

public class Interaction_HubBossDoor : Interaction
{
	private string sString;

	public FollowerLocation Location = FollowerLocation.Hub1;

	public string Scene;

	[TermsPopup("")]
	public string LocationName = "";

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.OpenDoor;
	}

	public override void GetLabel()
	{
		base.Label = sString;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (false)
		{
			StartCoroutine(EnterTempleRoutine());
		}
		else
		{
			StartCoroutine(InteractRoutine());
		}
	}

	private IEnumerator EnterTempleRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		yield return new WaitForSecondsRealtime(3f);
		yield return new WaitForSecondsRealtime(1f);
		EnterTemple();
	}

	private void EnterTemple()
	{
		MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, Scene, 1f, LocationName, FadeSave);
	}

	private void FadeSave()
	{
		SaveAndLoad.Save();
	}

	private IEnumerator InteractRoutine()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		yield return null;
		state.CURRENT_STATE = StateMachine.State.Idle;
	}
}
