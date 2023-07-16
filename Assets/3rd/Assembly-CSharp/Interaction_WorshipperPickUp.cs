public class Interaction_WorshipperPickUp : Interaction
{
	private WorshipperInfoManager character;

	private Worshipper worshipper;

	private new StateMachine state;

	private void Start()
	{
		character = GetComponent<WorshipperInfoManager>();
		worshipper = GetComponent<Worshipper>();
		state = GetComponent<StateMachine>();
	}

	public override void GetLabel()
	{
		if (character == null)
		{
			base.Label = "";
		}
		else if (character.v_i == null)
		{
			base.Label = "";
		}
		else
		{
			base.Label = character.v_i.Name;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
	}
}
