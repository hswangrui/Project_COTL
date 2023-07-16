using System.Collections;
using DG.Tweening;
using MMTools;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class LoadMainMenu : MonoBehaviour
{
	private AsyncOperationHandle<SceneInstance> s;

	private Scene scene;

	public SkeletonGraphic spine;

	public Material spineMaterial;

	public GameObject Loading;

	public LoadingIcon icon;

	public GameObject ControllerRecommended;

	private void Awake()
	{
	}

	private void Start()
	{
		AudioManager.Instance.enabled = true;
		StartCoroutine(RunSplashScreens());
		spineMaterial = spine.material;
		spine.AnimationState.Event += HandleEvent;
		spine.gameObject.SetActive(false);
		DeviceLightingManager.PlayVideo("Rainbow");
	}

	private IEnumerator RunSplashScreens()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(2f);
		spine.gameObject.SetActive(true);
		AudioManager.Instance.PlayOneShot("event:/music/mm_pre_roll/mm_pre_roll");
		yield return spine.YieldForAnimation("animation");
		DeviceLightingManager.StopVideo();
		DeviceLightingManager.PlayVideo();
		spine.AnimationState.Event -= HandleEvent;
		bool PlayingDevolverSplash = true;
		MMVideoPlayer.Play("Devolver_Animated_Logo_v001_4k_Audio-_HQ_HBR_MP4", delegate
		{
			PlayingDevolverSplash = false;
		}, MMVideoPlayer.Options.DISABLE, MMVideoPlayer.Options.DISABLE);
		AudioManager.Instance.PlayOneShot("event:/music/devolver_pre_roll/DevolverSplash");
		while (PlayingDevolverSplash)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		DeviceLightingManager.StopVideo();
		if (ControllerRecommended != null)
		{
			ControllerRecommended.SetActive(true);
		}
		yield return new WaitForSeconds(3f);
		yield return new WaitForEndOfFrame();
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", null);
	}

	private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (SettingsManager.Settings != null && SettingsManager.Settings.Accessibility.FlashingLights)
		{
			spineMaterial.DOFloat(1f, "_RainbowLerp", 0f);
			spineMaterial.DOFloat(0f, "_RainbowLerp", 1f).SetEase(Ease.InOutQuart);
		}
	}

	private IEnumerator EnableMainMenu()
	{
		while (true)
		{
			if (s.IsDone)
			{
				SceneManager.UnloadSceneAsync(scene);
				SceneManager.SetActiveScene(s.Result.Scene);
			}
			yield return null;
		}
	}
}
