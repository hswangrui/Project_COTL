public class AudioOnCallBack : BaseMonoBehaviour
{
	public void PlayAmbient()
	{
		AmbientMusicController.PlayAmbient(0f);
	}

	public void PlayCombat()
	{
		AmbientMusicController.PlayCombat();
		AudioManager.Instance.SetMusicCombatState();
	}

	public void StopCombat()
	{
		AmbientMusicController.StopCombat();
		AudioManager.Instance.SetMusicCombatState(false);
	}
}
