using MMTools;
using UnityEngine;

public class SimplePlayVideo : BaseMonoBehaviour
{
	public string VideoFile = "Intro";

	public void Play()
	{
		Time.timeScale = 0f;
		Object.FindObjectOfType<IntroManager>().DisableBoth();
		MMVideoPlayer.Play(VideoFile, Continue, MMVideoPlayer.Options.DISABLE, MMVideoPlayer.Options.DISABLE);
	}

	public void Continue()
	{
		Time.timeScale = 1f;
		Object.FindObjectOfType<IntroManager>().ToggleGameScene();
	}
}
