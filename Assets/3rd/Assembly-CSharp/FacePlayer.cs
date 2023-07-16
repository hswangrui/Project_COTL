using UnityEngine;

public class FacePlayer : BaseMonoBehaviour
{
	private StateMachine state;

	private GameObject Player;

	private void Start()
	{
		state = GetComponent<StateMachine>();
	}

	private void Update()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null))
		{
			state.facingAngle = Utils.GetAngle(base.transform.position, Player.transform.position);
		}
	}
}
