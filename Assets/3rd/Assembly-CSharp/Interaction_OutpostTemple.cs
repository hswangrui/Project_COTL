using UnityEngine;

public class Interaction_OutpostTemple : Interaction
{
	public GameObject AssignMenu;

	private string sAssignFollowers;

	private void Start()
	{
		ActivateDistance = 2f;
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sAssignFollowers = "Assign Follower to outpost";
	}

	public override void GetLabel()
	{
		base.Label = sAssignFollowers;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Object.Instantiate(AssignMenu, GameObject.FindGameObjectWithTag("Canvas").transform);
	}
}
