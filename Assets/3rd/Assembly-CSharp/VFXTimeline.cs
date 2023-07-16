using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class VFXTimeline : VFXObject, INotificationReceiver
{
	[SerializeField]
	private PlayableDirector _playableDirector;

	public override void Init()
	{
		if (!base.Initialized)
		{
			if (!_playableDirector)
			{
				_playableDirector = GetComponent<PlayableDirector>();
			}
			_playableDirector.playOnAwake = false;
		}
		base.Init();
	}

	public override void PlayVFX(float addEmissionDelay = 0f)
	{
		if (base.Playing)
		{
			_playableDirector.Stop();
		}
		_playableDirector.stopped -= OnPlayableDirectorStopped;
		_playableDirector.stopped += OnPlayableDirectorStopped;
		base.PlayVFX(addEmissionDelay);
	}

	protected override void Emit()
	{
		_playableDirector.Play();
		base.Emit();
	}

	private void OnPlayableDirectorStopped(PlayableDirector director)
	{
		_playableDirector.stopped -= OnPlayableDirectorStopped;
		base.CancelVFX();
	}

	public override void StopVFX()
	{
		_playableDirector.Stop();
		TriggerStopEvent();
	}

	public void OnNotify(Playable origin, INotification notification, object context)
	{
	}

	private void OnDestroy()
	{
		if (_playableDirector != null)
		{
			_playableDirector.stopped -= OnPlayableDirectorStopped;
		}
	}
}
