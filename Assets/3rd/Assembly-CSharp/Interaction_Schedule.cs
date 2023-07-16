using UnityEngine;

public class Interaction_Schedule : Interaction
{
	public GameObject Menu;

	public bool Activated { get; set; }

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			base.OnInteract(state);
			Object.Instantiate(Menu, GameObject.FindWithTag("Canvas").transform).GetComponent<UISchedule>().CallbackClose = CallbackCancel;
			state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
			Activated = true;
		}
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : "Schedule");
	}

	private void CallbackCancel()
	{
		state.CURRENT_STATE = StateMachine.State.Idle;
		Activated = false;
	}
}
