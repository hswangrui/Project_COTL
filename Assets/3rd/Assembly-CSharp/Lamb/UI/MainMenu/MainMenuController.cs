using System.Collections;
using Beffio.Dithering;
using DG.Tweening;
using FMOD.Studio;
using MMTools;
using src.Managers;
using src.UINavigator;
using Unify;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Lamb.UI.MainMenu
{
	public class MainMenuController : MonoBehaviour
	{
		public Animator StartTextAnimator;

		public Image RedFlash;

		private bool EngagementStarted;

		private const string kIntroStateName = "Intro";

		[SerializeField]
		private Animator _animator;

		[FormerlySerializedAs("_mainMenu")]
		[SerializeField]
		private UIMainMenuController _uiMainMenu;

		private Coroutine cIntroSequence;

		[SerializeField]
		private Stylizer _stylizer;

		[SerializeField]
		private CanvasGroup _canvasGroupPRC;

		[SerializeField]
		private Image _darkModeInvertImage;

		private Coroutine cAttractMode;

		public EventInstance loopedSound;

		private void Start()
		{
			AudioManager.Instance.StopCurrentAtmos();
			AudioManager.Instance.PauseActiveLoops();
			AudioManager.Instance.PlayMusic("event:/music/menu/menu_title");
			if (cIntroSequence != null)
			{
				StopCoroutine(cIntroSequence);
			}
			if (!CheatConsole.ForceAutoAttractMode)
			{
				cIntroSequence = StartCoroutine(DoIntroSequence());
			}
			if (CheatConsole.IN_DEMO)
			{
				CheatConsole.DemoBeginTime = 0f;
				AttractMode();
			}
			_canvasGroupPRC.alpha = 0f;
			_canvasGroupPRC.DOFade(1f, 1.5f).SetUpdate(true);
			DeviceLightingManager.PulseAllLighting(new Color(0.7f, 0.65f, 0.1f, 1f), new Color(1f, 0.7f, 0.4f, 1f), 1f, new KeyCode[0]);


        }

		public void DoIntro()
		{
			StartCoroutine(DoIntroSequence());
		}

		public void AttractMode()
		{
			if (cAttractMode != null)
			{
				StopCoroutine(cAttractMode);
			}
			cAttractMode = StartCoroutine(DoAttactMode());
		}

		private IEnumerator DoAttactMode()
		{
			CheatConsole.ForceResetTimeSinceLastKeyPress();
			Debug.Log("CheatConsole.ForceAutoAttractMode: " + CheatConsole.ForceAutoAttractMode);
			bool Waiting = !CheatConsole.ForceAutoAttractMode;
			while (Waiting)
			{
			//	Debug.Log("CheatConsole.TimeSinceLastKeyPress : " + CheatConsole.TimeSinceLastKeyPress);
				if (CheatConsole.TimeSinceLastKeyPress > 20f)
				{
					Waiting = false;
				}
				yield return null;
			}
			Debug.Log("SKIP THE WAIT?!");
			if (CheatConsole.ForceAutoAttractMode)
			{
				while (MMTransition.IsPlaying)
				{
					yield return null;
				}
			}
			CheatConsole.ForceAutoAttractMode = false;
			if (cIntroSequence != null)
			{
				StopCoroutine(cIntroSequence);
			}
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			bool lockInput = true;
			instance2.LockNavigation = true;
			instance.LockInput = lockInput;
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", PlayVideo);
		}

		private void PlayVideo()
		{
			AudioManager.Instance.StopCurrentAtmos();
			AudioManager.Instance.StopCurrentMusic();
			AudioManager.Instance.PauseActiveLoops();
			MMVideoPlayer.Play("Trailer", VideoComplete, MMVideoPlayer.Options.ENABLE, MMVideoPlayer.Options.DISABLE, false);
			loopedSound = AudioManager.Instance.CreateLoop("event:/music/trailer/trailer_video", true);
			MMTransition.ResumePlay();
		}

		private void VideoComplete()
		{
			MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", delegate
			{
				AudioManager.Instance.StopLoop(loopedSound);
				MMVideoPlayer.Hide();
				Debug.Log("VIDEO COMPLETE!");
			});
		}

		private IEnumerator DoIntroSequence()
		{
			while (true)
			{
				if (!EngagementStarted && SessionManager.instance.State == SessionManager.SessionState.Loading)
				{
					DeviceLightingManager.TransitionLighting(Color.red, new Color(0.7f, 0.65f, 0.1f, 1f), 2f, Ease.OutBounce);
					EngagementStarted = true;
					StartTextAnimator.SetTrigger("Active");
					if (SettingsManager.Settings != null && SettingsManager.Settings.Accessibility.FlashingLights)
					{
						RedFlash.color = new Color(RedFlash.color.r, RedFlash.color.g, RedFlash.color.b, 1f);
						RedFlash.DOFade(0f, 0.5f).SetUpdate(true);
					}
					UIManager.PlayAudio("event:/ui/Sermon Speech Bubble");
					UIManager.PlayAudio("event:/sermon/select_upgrade");
				}
				if (SessionManager.instance.HasStarted)
				{
					break;
				}
				yield return null;
			}
			if (UnifyManager.platform != UnifyManager.Platform.Standalone)
			{
				Singleton<SettingsManager>.Instance.LoadAndApply(true);
				Singleton<PersistenceManager>.Instance.Load();
			}
			MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact, false, true, this);
			UIManager.PlayAudio("event:/ui/start_game");
			StartTextAnimator.SetTrigger("End");
			StartCoroutine(_uiMainMenu.PreloadMetadata());
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			bool lockInput = true;
			instance2.LockNavigation = true;
			instance.LockInput = lockInput;
			float lerp = 1f;
			DOTween.To(() => lerp, delegate(float x)
			{
				lerp = x;
			}, 0f, 3f).SetEase(Ease.OutBack).SetDelay(1f)
				.OnUpdate(delegate
				{
					_stylizer.LerpPalette = lerp;
				});
			yield return _animator.YieldForAnimation("Intro");
			if (CheatConsole.IN_DEMO)
			{
				CheatConsole.DemoBeginTime = 0f;
				AttractMode();
			}
			

        }

		public void StartInput()
		{
			MonoSingleton<UINavigatorNew>.Instance.LockInput = (MonoSingleton<UINavigatorNew>.Instance.LockNavigation = false);
			_uiMainMenu.MainMenu.Show(true);
		}
	}
}
