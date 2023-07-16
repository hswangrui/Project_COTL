using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI;
using Map;
using src.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MMTools
{
	public class MMTransition : MonoBehaviour
	{
		public enum Effect
		{
			BlackFade,
			BlackWipe,
			BlackFadeInOnly,
			WhiteFade,
			GoopFade
		}

		[Serializable]
		public class ImageAndAlpha
		{
			public Image Image;

			private Color color;

			public float Alpha
			{
				set
				{
					color = Image.color;
					color.a = value;
					Image.color = color;
				}
			}
		}

		[Serializable]
		public class TransitionEffects
		{
			public Effect type;

			public AnimationClip animation;
		}

		public enum TransitionType
		{
			ChangeSceneAutoResume,
			ChangeRoomWaitToResume,
			ChangeRoom,
			FadeAndCallBack,
			LoadAndFadeOut
		}

		public static GameObject Instance;

		public static bool ForceShowIcon = false;

		public List<ImageAndAlpha> Images = new List<ImageAndAlpha>();

		public static bool IsPlaying = false;

		private static MMTransition mmTransition;

		private string SceneToLoad;

		private float Duration;

		private Action CallBack;

		private bool WaitForCallback;

		public LoadingIcon loadingIcon;

		public TextMeshProUGUI LoadingText;

		public GoopFade goopFade;

		public Image pentagramImage;

		public List<Sprite> pentagramImages = new List<Sprite>();

		private IEnumerator currentTransition;

		[SerializeField]
		public List<TransitionEffects> Effects = new List<TransitionEffects>();

		private static float Speed;

		public static string NO_SCENE = "-1";

		public string Title;

		private Effect CurrentEffect;

		public AsyncOperationHandle<SceneInstance> asyncLoad;

		private Coroutine cFadeOut;

		private AsyncOperationHandle<SceneInstance> BufferAsyncLoad;

		private Dictionary<string, int> lastMenuObjects;

		public static Action OnTransitionCompelte;

		private void Start()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnEnable()
		{
     
            LoadingText.text = "";
			loadingIcon.gameObject.SetActive(false);
			pentagramImage.enabled = false;
		}

		public static void Play(TransitionType transitionType, Effect effect, string SceneToLoad, float Duration, string Title, Action CallBack)
		{
			if (Instance == null)
			{
				Instance = UnityEngine.Object.Instantiate(Resources.Load("MMTransition/Transition")) as GameObject;
				mmTransition = Instance.GetComponent<MMTransition>();
			}
			else
			{
				Instance.SetActive(true);
			}
			mmTransition.Show(transitionType, effect, SceneToLoad, Duration, Title, CallBack);
		}

		public static void StopCurrentTransition()
		{
			if (!(mmTransition == null))
			{
				if (mmTransition.currentTransition != null)
				{
					mmTransition.StopCoroutine(mmTransition.currentTransition);
					SimulationManager.UnPause();
					IsPlaying = false;
				}
				if (mmTransition.cFadeOut != null)
				{
					mmTransition.StopCoroutine(mmTransition.cFadeOut);
					mmTransition.cFadeOut = null;
				}
			}
		}

		public void Show(TransitionType transitionType, Effect effect, string SceneToLoad, float Duration, string Title, Action CallBack)
		{
			if (IsPlaying)
			{
				if (CallBack != null)
				{
					CallBack();
				}
				return;
			}
			MMVibrate.StopRumble();
			this.SceneToLoad = SceneToLoad;
			this.Duration = Duration;
			this.CallBack = CallBack;
			CurrentEffect = effect;
			this.Title = Title;
			SimulationManager.Pause();
			IsPlaying = true;
			currentTransition = null;
			if (SceneToLoad == "MAIN MENU")
			{
				Debug.Log("MAIN MENU: " + transitionType);
			}
			switch (transitionType)
			{
			case TransitionType.ChangeRoomWaitToResume:
				currentTransition = ChangeRoomWaitToResumeRoutine();
				break;
			case TransitionType.ChangeSceneAutoResume:
				currentTransition = ChangeSceneAutoResumeRoutine();
				break;
			case TransitionType.ChangeRoom:
				currentTransition = ChangeRoomRoutine();
				break;
			case TransitionType.FadeAndCallBack:
				currentTransition = FadeAndCallBackRoutine();
				break;
			case TransitionType.LoadAndFadeOut:
					currentTransition = LoadAndFadeOutRoutine();
				break;
			}
			if (currentTransition != null)
			{
				StartCoroutine(currentTransition);
			}
		}

		private IEnumerator ChangeSceneAutoResumeRoutine()
		{
			if (ForceShowIcon)
			{
				loadingIcon.gameObject.SetActive(true);
			}
			yield return StartCoroutine(FadeInRoutine());
			Time.timeScale = 0f;
			yield return new WaitForSecondsRealtime(0.1f);
			Action callBack = CallBack;
			if (callBack != null)
			{
				callBack();
			}
			yield return new WaitForSecondsRealtime(0.1f);
			if (SceneToLoad != NO_SCENE)
			{
				yield return StartCoroutine(LoadScene());
			}
			if (SceneToLoad == "MAIN MENU")
			{
				Debug.Log("MAIN MENU LOADED");
			}
			ResumePlay();
			Resources.UnloadUnusedAssets();
		}

		private IEnumerator ChangeRoomWaitToResumeRoutine()
		{
			if (PlayerFarming.Instance != null)
			{
				UnityEngine.Object.Destroy(PlayerFarming.Instance.gameObject);
			}
			yield return StartCoroutine(FadeInRoutine());
			yield return new WaitForSecondsRealtime(0.1f);
			loadingIcon.gameObject.SetActive(true);
			pentagramImage.enabled = true;
			pentagramImage.color = new Color(1f, 1f, 1f, 0f);
			pentagramImage.DOFade(1f, 0.5f).SetUpdate(true);
			pentagramImage.sprite = pentagramImages[UnityEngine.Random.Range(0, pentagramImages.Count)];
			int num = UnityEngine.Random.Range(0, 2);
			pentagramImage.transform.localScale = ((num == 1) ? new Vector3(1f, -1f, 1f) : Vector3.one);
			yield return new WaitForSecondsRealtime(0.1f);
			Action callBack = CallBack;
			if (callBack != null)
			{
				callBack();
			}
			yield return new WaitForSecondsRealtime(0.1f);
			if (SceneToLoad != NO_SCENE)
			{
				yield return StartCoroutine(LoadScene());
			}
		}

		private IEnumerator ChangeRoomRoutine()
		{
			StartCoroutine(FadeInRoutine());
			yield return new WaitForSecondsRealtime(0.3f * Duration);
			Time.timeScale = 0f;
			yield return new WaitForSecondsRealtime(0.4f * Duration);
			Action callBack = CallBack;
			if (callBack != null)
			{
				callBack();
			}
		}

		private IEnumerator FadeAndCallBackRoutine()
		{
			yield return FadeInRoutine();
			Action callBack = CallBack;
			if (callBack != null)
			{
				callBack();
			}
		}

		private IEnumerator LoadAndFadeOutRoutine()
		{
			if (SceneToLoad != NO_SCENE)
			{
				loadingIcon.gameObject.SetActive(true);
				pentagramImage.enabled = true;
				pentagramImage.color = new Color(1f, 1f, 1f, 0f);
				pentagramImage.DOFade(1f, 0.5f).SetUpdate(true);
				pentagramImage.sprite = pentagramImages[UnityEngine.Random.Range(0, pentagramImages.Count)];
				int num = UnityEngine.Random.Range(0, 2);
				pentagramImage.transform.localScale = ((num == 1) ? new Vector3(1f, -1f, 1f) : Vector3.one);
				yield return LoadScene();
				yield return FadeOutRoutine();
			}
		}

		private IEnumerator LoadScene()
		{
			foreach (ImageAndAlpha image in Images)
			{
				image.Image.color = ((CurrentEffect == Effect.WhiteFade) ? Color.white : Color.black);
				image.Alpha = 1f;
				yield return null;
			}
			yield return new WaitWhile(() => MonoSingleton<UIManager>.Instance == null);
			switch (SceneManager.GetActiveScene().name)
			{
			case "Main Menu":
				MonoSingleton<UIManager>.Instance.UnloadMainMenuAssets();
				yield return MonoSingleton<UIManager>.Instance.LoadPersistentGameAssets().YieldUntilCompleted();
				break;
			case "Dungeon Ratau Home":
				MonoSingleton<UIManager>.Instance.UnloadKnucklebonesAssets();
				break;
			case "Base Biome 1":
				MonoSingleton<UIManager>.Instance.UnloadBaseAssets();
				break;
			case "Hub-Shore":
				MonoSingleton<UIManager>.Instance.UnloadHubShoreAssets();
				break;
			case "Game Biome Intro":
			case "Dungeon1":
			case "Dungeon2":
			case "Dungeon3":
			case "Dungeon4":
			case "Dungeon Sandbox":
			case "Dungeon Final":
				MonoSingleton<UIManager>.Instance.UnloadDungeonAssets();
				break;
			}
			BufferAsyncLoad = Addressables.LoadSceneAsync("Assets/Scenes/BufferScene.unity");
			while (!BufferAsyncLoad.IsDone)
			{
				UpdateProgress(BufferAsyncLoad.PercentComplete);
				yield return null;
			}
			yield return null;
			MapGenerator.Clear();
			MapConfig.Clear();
			ObjectPool.DestroyAll();
			yield return null;
			yield return StartCoroutine(ObjectPool.PoolReset());
			yield return null;
			Resources.UnloadUnusedAssets();
			yield return null;
			switch (SceneToLoad)
			{
			case "Main Menu":
				yield return MonoSingleton<UIManager>.Instance.LoadMainMenuAssets().YieldUntilCompleted();
				MonoSingleton<UIManager>.Instance.UnloadPersistentGameAssets();
				break;
			case "Dungeon Ratau Home":
				yield return MonoSingleton<UIManager>.Instance.LoadKnucklebonesAssets().YieldUntilCompleted();
				break;
			case "Base Biome 1":
				yield return MonoSingleton<UIManager>.Instance.LoadBaseAssets().YieldUntilCompleted();
				break;
			case "Hub-Shore":
				yield return MonoSingleton<UIManager>.Instance.LoadHubShoreAssets().YieldUntilCompleted();
				break;
			case "Game Biome Intro":
			case "Dungeon1":
			case "Dungeon2":
			case "Dungeon3":
			case "Dungeon4":
			case "Dungeon Sandbox":
			case "Dungeon Final":
				MonoSingleton<UIManager>.Instance.UnloadBaseAssets();
				yield return MonoSingleton<UIManager>.Instance.LoadDungeonAssets().YieldUntilCompleted();
				break;
			default:
				MonoSingleton<UIManager>.Instance.UnloadBaseAssets();
				break;
			}
			asyncLoad = Addressables.LoadSceneAsync("Assets/Scenes/" + SceneToLoad + ".unity");
			UpdateProgress(asyncLoad.PercentComplete);
			while (!asyncLoad.IsDone)
			{
				UpdateProgress(asyncLoad.PercentComplete);
				yield return null;
			}
		}

		public static void ForceStopMMTransition()
		{
			if (SceneManager.GetActiveScene().name != "Main Menu")
			{
				mmTransition.StopAllCoroutines();
			}
		}

		public static void ResumePlay()
		{
			ResumePlay(null);
		}

		public static void ResumePlay(Action andThen)
		{
			if (ForceShowIcon)
			{
				ForceShowIcon = false;
			}
			if (IsPlaying && mmTransition.cFadeOut == null)
			{
				mmTransition.cFadeOut = mmTransition.StartCoroutine(mmTransition.FadeOutRoutine(andThen));
			}
		}

		public IEnumerator FadeInRoutine()
		{
			foreach (ImageAndAlpha image in Images)
			{
				image.Image.color = ((CurrentEffect == Effect.WhiteFade) ? Color.white : Color.black);
			}
			float progress = 0f;
			while (true)
			{
				float num;
				progress = (num = progress + Time.unscaledDeltaTime);
				if (!(num / Duration <= 0.5f))
				{
					break;
				}
				foreach (ImageAndAlpha image2 in Images)
				{
					image2.Alpha = Mathf.SmoothStep(0f, 1f, progress / (Duration * 0.5f));
				}
				yield return null;
			}
		}

		public IEnumerator FadeOutRoutine(Action andThen = null)
		{
			Action onTransitionCompelte = OnTransitionCompelte;
			if (onTransitionCompelte != null)
			{
				onTransitionCompelte();
			}
			yield return new WaitForSecondsRealtime(0.1f);
			mmTransition.loadingIcon.gameObject.SetActive(false);
			LoadingText.text = "";
			pentagramImage.enabled = false;
			float FadeProgress = 0f;
			bool ResetTimeScale = false;
			while (FadeProgress / Duration <= 0.5f)
			{
				FadeProgress += Time.unscaledDeltaTime;
				foreach (ImageAndAlpha image in Images)
				{
					image.Alpha = Mathf.SmoothStep(1f, 0f, FadeProgress / (Duration * 0.5f));
				}
				if (!ResetTimeScale && FadeProgress / Duration > 0.2f)
				{
					ResetTimeScale = true;
					Time.timeScale = 1f;
				}
				yield return null;
			}
			if (!ResetTimeScale)
			{
				Time.timeScale = 1f;
			}
			SimulationManager.UnPause();
			IsPlaying = false;
			Instance.SetActive(false);
			mmTransition.cFadeOut = null;
			if (andThen != null)
			{
				andThen();
			}
		}

		public static void UpdateProgress(float Progress, string ProgressText = "")
		{
			if (!(mmTransition == null))
			{
				mmTransition.loadingIcon.UpdateProgress(Progress);
				if(mmTransition.LoadingText)
					mmTransition.LoadingText.text = ProgressText;
			}
		}
	}
}
