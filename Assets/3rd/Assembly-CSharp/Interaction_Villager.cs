public class Interaction_Villager : Interaction
{
	private WorshipperInfoManager character;

	private void Start()
	{
		character = GetComponent<WorshipperInfoManager>();
	}

	private new void Update()
	{
		if (base.Label == "" && character.v_i != null)
		{
			base.Label = character.v_i.Name + " (" + character.v_i.Age + ")";
		}
	}
}
