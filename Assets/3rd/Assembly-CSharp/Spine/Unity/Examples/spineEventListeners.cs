using System;
using FMODUnity;
using UnityEngine.Events;

namespace Spine.Unity.Examples
{
	[Serializable]
	public class spineEventListeners
	{
		public SkeletonAnimation skeletonAnimation;

		[SpineEvent("", "skeletonAnimation", true, true, false)]
		public string eventName;

		public SoundConstants.SoundEventType soundEventType;

		[EventRef]
		public string soundPath = string.Empty;

		public UnityEvent callBack;

		public EventData eventData;

		public void Start()
		{
			if (!(skeletonAnimation == null))
			{
				skeletonAnimation.Initialize(false);
				if (skeletonAnimation.valid)
				{
					eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);
					skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
				}
			}
		}

		public void Play()
		{
			if (callBack != null)
			{
				callBack.Invoke();
			}
			switch (soundEventType)
			{
			case SoundConstants.SoundEventType.OneShot2D:
				AudioManager.Instance.PlayOneShot(soundPath);
				break;
			case SoundConstants.SoundEventType.OneShotAtPosition:
				AudioManager.Instance.PlayOneShot(soundPath, skeletonAnimation.transform.position);
				break;
			case SoundConstants.SoundEventType.OneShotAttached:
				AudioManager.Instance.PlayOneShot(soundPath, skeletonAnimation.gameObject);
				break;
			}
		}

		public void HandleAnimationStateEvent(TrackEntry trackEntry, Event e)
		{
			if (eventData == e.Data)
			{
				Play();
			}
		}
	}
}
