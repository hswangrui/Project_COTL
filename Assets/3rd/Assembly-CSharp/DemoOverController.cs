using Lamb.UI;
using MMTools;
using UnityEngine;

public class DemoOverController : MonoBehaviour
{
	private float Timer;

	private void Update()
	{
		Timer += Time.deltaTime;
		if (Timer > 2f && (InputManager.Gameplay.GetInteractButtonDown() || InputManager.Gameplay.GetAttackButtonDown()))
		{
			UIManager.PlayAudio("event:/ui/confirm_selection");
			UIManager.PlayAudio("event:/sermon/Sermon Upgrade Menu Appear");
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 3f, "", null);
		}
	}
}
