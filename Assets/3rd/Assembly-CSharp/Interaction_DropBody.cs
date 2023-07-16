using I2.Loc;

public class Interaction_DropBody : Interaction
{
	private bool Activating;

	private string sString;

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.Drop;
	}

	private void Start()
	{
		IgnoreTutorial = true;
		UpdateLocalisation();
		GetLabel();
	}

	public override void GetLabel()
	{
		base.Label = sString;
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating)
		{
			base.OnInteract(state);
			base.enabled = false;
			Activating = true;
		}
	}

	private new void Update()
	{
		if (PlayerFarming.Instance != null)
		{
			base.gameObject.transform.position = PlayerFarming.Instance.transform.position;
			GetLabel();
		}
	}
}
