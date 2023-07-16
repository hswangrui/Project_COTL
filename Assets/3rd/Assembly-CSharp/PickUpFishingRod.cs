public class PickUpFishingRod : BaseMonoBehaviour
{
	private void start()
	{
		if (CrownAbilities.CrownAbilityUnlocked(CrownAbilities.TYPE.Abilities_GrappleHook))
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		if (CrownAbilities.CrownAbilityUnlocked(CrownAbilities.TYPE.Abilities_GrappleHook))
		{
			base.gameObject.SetActive(false);
		}
	}

	public void PickMeUp()
	{
		if (!CrownAbilities.CrownAbilityUnlocked(CrownAbilities.TYPE.Abilities_GrappleHook))
		{
			CrownAbilities.UnlockAbility(CrownAbilities.TYPE.Abilities_GrappleHook);
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.FishingRod);
			base.gameObject.SetActive(false);
		}
	}
}
