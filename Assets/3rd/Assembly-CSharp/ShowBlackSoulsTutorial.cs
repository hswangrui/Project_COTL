using UnityEngine;

public class ShowBlackSoulsTutorial : BaseMonoBehaviour
{
	public void Play()
	{
		if (!DataManager.Instance.BlackSoulsEnabled)
		{
			Object.FindObjectOfType<HUD_BlackSoul>().DoTutorial();
		}
	}
}
