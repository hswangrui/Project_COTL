using I2.Loc;
using MMTools;
using UnityEngine;

public class OutpostTemple : Interaction
{
	[TermsPopup("")]
	public string Place;

	public GameObject UnclaimedObject;

	public GameObject ClaimedObject;

	public BlockingDoor blockingDoor;

	private string sClaim;

	private void Start()
	{
		UpdateLocalisation();
		SetGameObjects();
	}

	private void SetGameObjects()
	{
		if (DataManager.Instance.GetVariable(Place.Remove(0, 6) + "_OutpostTemple"))
		{
			UnclaimedObject.SetActive(false);
			ClaimedObject.SetActive(true);
			if (blockingDoor != null)
			{
				blockingDoor.Open();
			}
		}
		else
		{
			UnclaimedObject.SetActive(true);
			ClaimedObject.SetActive(false);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sClaim = "Claim shrine";
	}

	public override void GetLabel()
	{
		base.Label = (DataManager.Instance.GetVariable(Place.Remove(0, 6) + "_OutpostTemple") ? "" : sClaim);
	}

	public override void OnInteract(StateMachine state)
	{
		if (!DataManager.Instance.GetVariable(Place.Remove(0, 6) + "_OutpostTemple"))
		{
			base.OnInteract(state);
			Fade();
		}
	}

	private void Fade()
	{
		state.CURRENT_STATE = StateMachine.State.InActive;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 3f, "", RevealMap);
	}

	private void RevealMap()
	{
	}

	private void Build()
	{
		UnclaimedObject.SetActive(false);
		ClaimedObject.SetActive(true);
		MMTransition.ResumePlay();
		state.CURRENT_STATE = StateMachine.State.Idle;
		if (blockingDoor != null)
		{
			blockingDoor.Open();
		}
	}
}
