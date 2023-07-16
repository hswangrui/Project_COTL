using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Unify.Standalone
{
	public class VideoPlayer : Unify.VideoPlayer
	{
		private UnityEngine.Video.VideoPlayer videoPlayer;

		private AudioSource audioSource;

		private bool m_isPlaying;

		private bool m_isPaused;

		private void Awake()
		{
			Logger.Log("VIDEO:STANDALONE: Start.");
		}

		public override void Destroy()
		{
			Logger.Log("VIDEO:STANDALONE: Clear");
			Object.Destroy(base.gameObject);
			if (videoPlayer != null)
			{
				Object.Destroy(videoPlayer);
			}
			if (audioSource != null)
			{
				Object.Destroy(audioSource);
			}
		}

		public override bool Load(string name)
		{
			Logger.Log("VIDEO:STANDALONE: Load: " + name);
			string url = Application.streamingAssetsPath + "/" + name + ".mp4";
			videoPlayer = base.gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
			audioSource = base.gameObject.AddComponent<AudioSource>();
			videoPlayer.playOnAwake = false;
			videoPlayer.isLooping = false;
			videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
			videoPlayer.prepareCompleted += Prepared;
			videoPlayer.url = url;
			videoPlayer.controlledAudioTrackCount = 1;
			videoPlayer.SetTargetAudioSource(0, audioSource);
			videoPlayer.loopPointReached += EndReached;
			Logger.Log("VideoPlayer: Prepare request.");
			videoPlayer.Prepare();
			return true;
		}

		public void Prepared(UnityEngine.Video.VideoPlayer videoPlayer)
		{
			Logger.Log("VideoPlayer: Prepared.");
			Target = ((Target == null) ? base.gameObject : Target);
			Renderer component = Target.GetComponent<Renderer>();
			component = null;
			if (component != null)
			{
				Logger.Log("VIDEO:SWITCH: Apply to renderer");
				videoPlayer.renderMode = VideoRenderMode.RenderTexture;
				videoPlayer.targetMaterialRenderer = component;
			}
			else
			{
				RawImage component2 = Target.GetComponent<RawImage>();
				if (component2 != null)
				{
					Logger.Log("VIDEO:SWITCH: Apply to RawImage");
					videoPlayer.renderMode = VideoRenderMode.APIOnly;
					component2.texture = videoPlayer.texture;
				}
				else
				{
					Logger.Log("VIDEO:SWITCH: Apply to Main Camera");
					videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
					videoPlayer.targetCamera = Camera.main;
				}
			}
			OnVideoPrepared();
		}

		public void EndReached(UnityEngine.Video.VideoPlayer vp)
		{
			Stop();
			OnVideoFinished();
		}

		public override void Play()
		{
			videoPlayer.Play();
			m_isPaused = false;
			m_isPlaying = true;
		}

		public override void Pause()
		{
			videoPlayer.Pause();
			m_isPaused = true;
		}

		public override void Stop()
		{
			videoPlayer.Stop();
			m_isPlaying = false;
			m_isPaused = false;
		}

		public override void SetVolume(float volume)
		{
			if (audioSource != null)
			{
				audioSource.volume = volume;
			}
		}

		public override bool isPlaying()
		{
			return m_isPlaying;
		}

		public override bool isPaused()
		{
			return m_isPaused;
		}
	}
}
