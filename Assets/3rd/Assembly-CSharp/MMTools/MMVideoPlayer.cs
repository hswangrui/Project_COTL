using System;
using Lamb.UI;
using Unify;
using UnityEngine;
using UnityEngine.Video;

namespace MMTools
{
	public class MMVideoPlayer : MonoBehaviour
	{
		public enum Options
		{
			ENABLE,
			DISABLE
		}

		private static Unify.VideoPlayer unifyVideoPlayer;

		private static bool isPlayingVideo;

		private static bool finished;

		private static bool loaded;

		private static bool unifyVideoIsLooping;

		private static bool unifyVideoPlayerPrepared;

		private static float inputBufferTime = 1f;

		private static float videoPlayStartTime;

		private static UnityEngine.Video.VideoPlayer videoPlayer;

		private static Action Callback;

		public static GameObject Instance;

		private Options Skippable;

		private Options FastForward;

		public bool HideOnCompete;

		private static MMVideoPlayer mmVideoPlayer;

		private bool completed;

		public GameObject controlprompt;

		private void Start()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		public static void Hide()
		{
			if (Instance != null)
			{
				Instance.SetActive(false);
			}
		}

		public static void Play(string _FileName, Action _CallBack, Options Skippable, Options FastForward, bool HideOnCompete = true, bool ForceStreaming = false)
		{
			if (Instance == null)
			{
				Instance = UnityEngine.Object.Instantiate(Resources.Load("MMVideoPlayer/Video Player")) as GameObject;
				mmVideoPlayer = Instance.GetComponent<MMVideoPlayer>();
			}
			else
			{
				Instance.SetActive(true);
			}
			DeviceLightingManager.PlayVideo();
			if (MonoSingleton<UIManager>.Instance != null)
			{
				MonoSingleton<UIManager>.Instance.ForceBlockMenus = true;
			}
			if (mmVideoPlayer.controlprompt != null)
			{
				if (_FileName == "Trailer")
				{
					mmVideoPlayer.controlprompt.SetActive(true);
				}
				else
				{
					mmVideoPlayer.controlprompt.SetActive(false);
				}
			}
			mmVideoPlayer.Skippable = Skippable;
			mmVideoPlayer.FastForward = FastForward;
			mmVideoPlayer.HideOnCompete = HideOnCompete;
			mmVideoPlayer.completed = false;
			videoPlayer = Instance.GetComponent<UnityEngine.Video.VideoPlayer>();
			videoPlayer.source = VideoSource.VideoClip;
			videoPlayer.errorReceived += HandleVideoPlayerError;
			if (ForceStreaming)
			{
				videoPlayer.url = Application.streamingAssetsPath + "/MMVideoPlayer/Videos/" + _FileName + ".mp4";
			}
			else
			{
				videoPlayer.clip = Resources.Load("MMVideoPlayer/Videos/" + _FileName) as VideoClip;
			}
			videoPlayer.Play();
			videoPlayer.loopPointReached += EndReached;
			Callback = _CallBack;
		}

		private static void HandleVideoPlayerError(UnityEngine.Video.VideoPlayer source, string message)
		{
			Debug.Log("MMVideoPlayer - ERROR - " + message);
			EndReached(source);
		}

		private void Update()
		{
			if (videoPlayer != null && videoPlayer.isPlaying)
			{
				if (FastForward == Options.ENABLE)
				{
					if (Input.anyKeyDown)
					{
						videoPlayer.playbackSpeed = 2f;
					}
					if (!Input.anyKey)
					{
						videoPlayer.playbackSpeed = 1f;
					}
				}
				if (!MMTransition.IsPlaying && Skippable == Options.ENABLE && (Input.GetKeyUp(KeyCode.Escape) || InputManager.UI.GetAcceptButtonUp()))
				{
					completed = true;
					EndReached(null);
				}
			}
			if (videoPlayer != null && !completed && videoPlayer.frame > 0 && !videoPlayer.isPlaying)
			{
				completed = true;
				EndReached(videoPlayer);
			}
		}

		private static void EndReached(UnityEngine.Video.VideoPlayer vp)
		{
			if (Callback != null)
			{
				Callback();
			}
			videoPlayer.loopPointReached -= EndReached;
			videoPlayer.errorReceived -= HandleVideoPlayerError;
			DeviceLightingManager.StopVideo();
			if (Instance != null)
			{
				Instance.SetActive(!mmVideoPlayer.HideOnCompete);
			}
			if (MonoSingleton<UIManager>.Instance != null)
			{
				MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
			}
		}

		public static void ForceStopVideo()
		{
			if (videoPlayer != null)
			{
				videoPlayer.loopPointReached -= EndReached;
				videoPlayer.errorReceived -= HandleVideoPlayerError;
				videoPlayer.Stop();
				if (MonoSingleton<UIManager>.Instance != null)
				{
					MonoSingleton<UIManager>.Instance.ForceBlockMenus = false;
				}
				if (Instance != null)
				{
					Instance.SetActive(false);
				}
				UnityEngine.Object.Destroy(Instance);
			}
		}
	}
}
