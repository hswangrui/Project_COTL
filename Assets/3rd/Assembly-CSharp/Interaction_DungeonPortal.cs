using I2.Loc;
using UnityEngine;

public class Interaction_DungeonPortal : Interaction
{
	private string sReturnToBase;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sReturnToBase = ScriptLocalization.Interactions.ReturnToBase;
	}

	public override void GetLabel()
	{
		Interactable = true;
		base.Label = sReturnToBase;
	}

	public override void OnInteract(StateMachine state)
	{
		AudioSource[] array = Object.FindObjectsOfType<AudioSource>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
		base.OnInteract(state);
		GameManager.ToShip();
	}
}
