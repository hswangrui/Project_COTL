using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using MMTools;
using UnityEngine;

public class Credits : BaseMonoBehaviour
{
	public static bool GoToMainMenu;

	[SerializeField]
	private RectTransform _canvasRectTransform;

	[SerializeField]
	private float _skipSpeed = 4f;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _contentTransform;

	private EventInstance loopedSound;

	private void Awake()
	{
		_canvasGroup.alpha = 0f;
		if (MonoSingleton<UIManager>.Instance != null)
		{
			MonoSingleton<UIManager>.Instance.ForceBlockMenus = true;
		}
	}

	private IEnumerator Start()
	{
		yield return null;
		bool wait = true;
		float endValue = _contentTransform.rect.height - _canvasRectTransform.rect.height;
		Sequence creditsSequence = DOTween.Sequence();
		creditsSequence.Append(_canvasGroup.DOFade(1f, 1.5f));
		creditsSequence.Append(_contentTransform.DOAnchorPosY(endValue, 41f).SetEase(Ease.InOutSine));
		creditsSequence.Append(_canvasGroup.DOFade(0f, 1.5f));
		creditsSequence.Play();
		creditsSequence.onComplete = (TweenCallback)Delegate.Combine(creditsSequence.onComplete, (TweenCallback)delegate
		{
			wait = false;
		});
		DeviceLightingManager.PlayVideo("Rainbow");
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.StopCurrentMusic();
		loopedSound = AudioManager.Instance.CreateLoop("event:/music/credits/credits", true);
		while (wait)
		{
			yield return null;
			if (InputManager.UI.GetAcceptButtonHeld())
			{
				creditsSequence.timeScale = _skipSpeed;
			}
			else
			{
				creditsSequence.timeScale = 1f;
			}
			loopedSound.setPitch(creditsSequence.timeScale);
		}
		LoadOverworld();
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(loopedSound);
	}

	private void LoadOverworld()
	{
		MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
		if (GoToMainMenu)
		{
			DeviceLightingManager.Reset();
			FollowerManager.Reset();
			SimulationManager.Pause();
			StructureManager.Reset();
			UIDynamicNotificationCenter.Reset();
		//	TwitchManager.Abort();
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", null);
		}
		else
		{
			MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, "Base Biome 1", 1f, "", null);
		}
		GoToMainMenu = false;
	}
}
