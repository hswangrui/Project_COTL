using MMBiomeGeneration;

public class TutorialRatauSetUpRemainingDungeon : BaseMonoBehaviour
{
	public void Play()
	{
		BiomeGenerator.Instance.DoFirstArrivalRoutine = true;
		BiomeGenerator.Instance.ShowDisplayName = false;
		PlayerFarming.Instance.health.GodMode = Health.CheatMode.Demigod;
		DataManager.Instance.InTutorial = false;
	}

	private void OnEnable()
	{
		AudioManager.Instance.PlayOneShot("event:/Stings/tarot_room", base.transform.position);
	}
}
