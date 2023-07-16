using I2.Loc;
using MMTools;
using UnityEngine;

public class GameOverScreenController : BaseMonoBehaviour
{
	public AudioClip Music;

	public AudioClip Quote;

	public float fadeIn = 1f;

	public AudioSource audioSource;

	private float _UnscaledTime;

	public string QuitToScene = "Main Menu";

	public Animator animator;

	private void Start()
	{
		animator.Play("EndOfGame");
		AmbientMusicController.PlayTrack(Music, fadeIn);
		Debug.Log(LocalizationManager.CurrentLanguage);
		if (LocalizationManager.CurrentLanguage == "English")
		{
			audioSource.clip = Quote;
			audioSource.Play();
		}
		MMTransition.ResumePlay();
	}

	public void Quit()
	{
		Time.timeScale = 1f;
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, QuitToScene, 2f, "", null);
	}

	public void Retry()
	{
		Time.timeScale = 1f;
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Base Biome 1", 2f, "", null);
	}

	private void Clear()
	{
		Time.timeScale = 1f;
		Object.Destroy(base.gameObject);
	}
}
