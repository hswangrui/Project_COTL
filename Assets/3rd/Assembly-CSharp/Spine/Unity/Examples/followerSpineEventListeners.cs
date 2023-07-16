using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace Spine.Unity.Examples
{
	[Serializable]
	public class followerSpineEventListeners
	{
		[HideInInspector]
		public string _pitchParameter;

		[HideInInspector]
		public string _vibratoParameter;

		[HideInInspector]
		public float _pitchValue;

		[HideInInspector]
		public float _vibratoValue;

		[HideInInspector]
		public float _followerID;

		public SkeletonAnimation skeletonAnimation;

		[SpineEvent("", "skeletonAnimation", true, true, false)]
		public string eventName;

		public SoundConstants.SoundEventType soundEventType = SoundConstants.SoundEventType.OneShotAtPosition;

		[EventRef]
		public string soundPath = string.Empty;

		public bool isVoice = true;

		public bool UseCallBack;

		public UnityEvent callBack;

		public EventData eventData;

		public void Start()
		{
			if (skeletonAnimation == null)
			{
				Debug.Log("Skeleton Animation = null, For Event: " + eventData.Name);
				return;
			}
			skeletonAnimation.Initialize(false);
			if (!skeletonAnimation.valid)
			{
				Debug.Log("Skeleton Animation not valid, For Event: " + eventData.Name);
				return;
			}
			eventData = skeletonAnimation.Skeleton.Data.FindEvent(eventName);
			skeletonAnimation.AnimationState.Event += HandleAnimationStateEvent;
		}

		private void OnDisable()
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
		}

		private void OnDestroy()
		{
			skeletonAnimation.AnimationState.Event -= HandleAnimationStateEvent;
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
				AudioManager.Instance.PlayOneShotAndSetParametersValue(soundPath, _pitchParameter, _pitchValue, _vibratoParameter, _vibratoValue, skeletonAnimation.transform, (int)_followerID);
				break;
			case SoundConstants.SoundEventType.OneShotAtPosition:
				AudioManager.Instance.PlayOneShotAndSetParametersValue(soundPath, _pitchParameter, _pitchValue, _vibratoParameter, _vibratoValue, skeletonAnimation.transform, (int)_followerID);
				break;
			case SoundConstants.SoundEventType.OneShotAttached:
				AudioManager.Instance.PlayOneShotAndSetParametersValue(soundPath, _pitchParameter, _pitchValue, _vibratoParameter, _vibratoValue, skeletonAnimation.transform, (int)_followerID);
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
