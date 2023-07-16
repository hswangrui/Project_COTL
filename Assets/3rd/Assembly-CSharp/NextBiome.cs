using I2.Loc;
using MMTools;
using UnityEngine;

public class NextBiome : BaseMonoBehaviour
{
	public string SceneName = "";

	[TermsPopup("")]
	public string PlaceName = "";

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			collision.gameObject.GetComponent<StateMachine>().CURRENT_STATE = StateMachine.State.InActive;
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, SceneName, 1f, PlaceName, null);
		}
	}
}
