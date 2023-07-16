using FMOD.Studio;
using Lamb.UI;
using Lamb.UI.DeathScreen;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReturnToBase : BaseMonoBehaviour
{
	private float Duration = 4f;

	private float Timer;

	public Image ProgressRing;

	private bool Meditating;

	private bool Active;

	private EventInstance loopingSoundInstance;

	private string holdTime = "hold_time";

	public static bool Disabled { get; set; }

	private void Start()
	{
		ProgressRing.fillAmount = 0f;
	}

	private void Update()
	{
		if (Disabled || PlayerFarming.Instance == null || Active || DungeonSandboxManager.Active || PlayerFarming.Location == FollowerLocation.Base || (RespawnRoomManager.Instance != null && RespawnRoomManager.Instance.gameObject.activeSelf))
		{
			return;
		}
		switch (PlayerFarming.Instance.state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
		case StateMachine.State.Moving:
			if (InputManager.Gameplay.GetReturnToBaseButtonHeld() && !PlayerFarming.Instance.GoToAndStopping)
			{
				if (!Meditating)
				{
					loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/ui/hold_button_loop", base.gameObject, true);
				}
				Meditating = true;
				PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Meditate;
				GameManager.GetInstance().CameraSetTargetZoom(15f);
			}
			break;
		case StateMachine.State.Meditate:
			Timer += Time.deltaTime;
			ProgressRing.fillAmount = Timer / Duration;
			if (loopingSoundInstance.isValid())
			{
				loopingSoundInstance.setParameterByName(holdTime, Timer / Duration);
			}
			if (!InputManager.Gameplay.GetReturnToBaseButtonHeld())
			{
				if (Timer > 0.3f)
				{
					PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
				}
			}
			else if (Timer / Duration >= 1f && UIDeathScreenOverlayController.Instance == null)
			{
				AudioManager.Instance.StopLoop(loopingSoundInstance);
				MonoSingleton<UIManager>.Instance.ShowDeathScreenOverlay(UIDeathScreenOverlayController.Results.Escaped);
				AudioManager.Instance.PlayOneShot("event:/pentagram_platform/pentagram_platform_curse");
				AudioManager.Instance.PlayOneShot("event:/ui/heretics_defeated");
				AudioManager.Instance.PlayMusic("event:/music/game_over/game_over");
				Active = true;
			}
			break;
		}
		if (!PlayerFarming.Instance || PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.Meditate)
		{
			return;
		}
		if (Timer > 0f)
		{
			Timer -= Time.deltaTime * 3f;
			if (Timer <= 0f)
			{
				Timer = 0f;
			}
		}
		ProgressRing.fillAmount = Timer / Duration;
		if (Meditating)
		{
			AudioManager.Instance.StopLoop(loopingSoundInstance);
			GameManager.GetInstance().CameraResetTargetZoom();
			Meditating = false;
		}
	}
}
