using I2.Loc;
using UnityEngine;

public class Interaction_Teleporter : Interaction
{
	public ParticleSystem particleSystem;

	public static Interaction_Teleporter Instance;

	public bool IsHome;

	private string sReturnToBase;

	private void Start()
	{
		ActivateDistance = 1f;
		particleSystem.Stop();
		HoldToInteract = true;
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sReturnToBase = ScriptLocalization.Interactions.ReturnToBase;
	}

	public override void GetLabel()
	{
		base.Label = (IsHome ? "" : sReturnToBase);
	}

	public override void OnEnableInteraction()
	{
		Instance = this;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		GameManager.ToShip();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!IsHome && base.Label != "" && collision.gameObject.tag == "Player")
		{
			particleSystem.Play();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (base.Label != "" && collision.gameObject.tag == "Player")
		{
			particleSystem.Stop();
		}
	}
}
