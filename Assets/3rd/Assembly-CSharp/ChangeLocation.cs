using I2.Loc;
using MMTools;
using UnityEngine;

public class ChangeLocation : BaseMonoBehaviour
{
	public string Scene = "Hub";

	[TermsPopup("")]
	public string LocationName = "";

	private bool Activated;

	private void OnTriggerEnter2D(Collider2D Collision)
	{
		if (!Activated && Collision.gameObject.tag == "Player")
		{
			SimulationManager.Pause();
			Collision.gameObject.GetComponent<StateMachine>().CURRENT_STATE = StateMachine.State.InActive;
			Activated = true;
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, Scene, 1f, LocationName, FadeSave);
		}
	}

	private void FadeSave()
	{
		SaveAndLoad.Save();
	}
}
