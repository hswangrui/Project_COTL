using Unify.Standalone;
using UnityEngine;

namespace Unify
{
	public class VideoPlayer : MonoBehaviour
	{
		public delegate void VideoEventDelegate();

		public VideoEventDelegate videoPreparedDelegate;

		public VideoEventDelegate videoFinishedDelegate;

		public GameObject Target;

		public static VideoPlayer Create(GameObject go = null, GameObject target = null)
		{
			if (go == null)
			{
				go = Object.Instantiate(Resources.Load<GameObject>("prefabs/Video"));
			}
			Unify.Standalone.VideoPlayer videoPlayer = go.AddComponent<Unify.Standalone.VideoPlayer>();
			videoPlayer.Target = target;
			return videoPlayer;
		}

		public virtual void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		public virtual bool Load(string name)
		{
			Logger.Log("VIDEO: Load: " + name);
			return false;
		}

		public virtual void Play()
		{
		}

		public virtual void Pause()
		{
		}

		public virtual void Stop()
		{
		}

		public virtual bool isPlaying()
		{
			return false;
		}

		public virtual bool isPaused()
		{
			return false;
		}

		public virtual void SetVolume(float volume)
		{
		}

		public virtual void OnVideoPrepared()
		{
			Logger.Log("VIDEO: Prepared.");
			if (videoPreparedDelegate != null)
			{
				videoPreparedDelegate();
			}
		}

		public virtual void OnVideoFinished()
		{
			Logger.Log("VIDEO: FINISHED!");
			if (videoFinishedDelegate != null)
			{
				videoFinishedDelegate();
			}
		}
	}
}
