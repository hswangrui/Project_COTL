using I2.Loc;
using UnityEngine;

public class DungeonDoor : BaseMonoBehaviour
{
	public string BiomeName0 = "Game Biome 0";

	[TermsPopup("")]
	public string PlaceName0 = "";

	public string BiomeName = "Game Biome 1";

	[TermsPopup("")]
	public string PlaceName = "";

	public GameObject WorldMap;

	private bool Activated;

	private StateMachine state;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!Activated && collision.gameObject.tag == "Player")
		{
			state = collision.gameObject.GetComponent<StateMachine>();
			state.CURRENT_STATE = StateMachine.State.InActive;
			Activated = true;
		}
	}

	private void CancelCallback()
	{
		Activated = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		state.transform.position = base.transform.position + Vector3.down * 3f;
	}
}
