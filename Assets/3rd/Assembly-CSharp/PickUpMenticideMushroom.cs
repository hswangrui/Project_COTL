public class PickUpMenticideMushroom : PickUp
{
	public override void PickMeUp()
	{
		base.PickMeUp();
		if (!DataManager.Instance.CollectedMenticide)
		{
			DataManager.Instance.CollectedMenticide = true;
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.MenticideMushroom);
		}
	}
}
