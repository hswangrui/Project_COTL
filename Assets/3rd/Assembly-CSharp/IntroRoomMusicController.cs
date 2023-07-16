using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class IntroRoomMusicController : BaseMonoBehaviour
{
	public AmbientMusicController.Track AmbientTrack;

	public AmbientMusicController.Track AmbientNatureTrack;

	[EventRef]
	public string atmosEventPath;

	private EventInstance atmosEventInstance;

	[EventRef]
	public string natureEventPath;

	private EventInstance natureEventInstance;

	public TriggerCanvasGroup MMTrigger;

	public AmbientMusicController.Track MMTrack;

	[EventRef]
	public string mmEventPath;

	public TriggerCanvasGroup DDTrigger;

	public AmbientMusicController.Track DDTrack;

	[EventRef]
	public string ddEventPath;

	public TriggerCanvasGroup BridgeTrigger;

	public AmbientMusicController.Track BridgeTrack;

	[EventRef]
	public string bridgeEventPath;

	public AmbientMusicController.Track BassTrack;

	[EventRef]
	public string bassEventPath;

	private EventInstance bassEventInstance;

	public GameObject BassFadeObject;

	public float MaxDistance = 20f;

	public AmbientMusicController.Track ExecutionTrack;

	[EventRef]
	public string ExecutionEventPath;

	[EventRef]
	public string biomeMusicEventPath;

	private GameObject Player;

	private void Start()
	{
		AudioManager.Instance.PlayOneShot("event:/Stings/church_bell", base.gameObject);
		atmosEventInstance = AudioManager.Instance.CreateLoop(atmosEventPath, true);
		bassEventInstance = AudioManager.Instance.CreateLoop(bassEventPath);
		if (bassEventInstance.isValid())
		{
			bassEventInstance.setParameterByName(SoundParams.Intensity, 0f);
			bassEventInstance.start();
		}
		TriggerCanvasGroup mMTrigger = MMTrigger;
		mMTrigger.OnTriggered = (TriggerCanvasGroup.Triggered)Delegate.Combine(mMTrigger.OnTriggered, new TriggerCanvasGroup.Triggered(MMTriggered));
		TriggerCanvasGroup dDTrigger = DDTrigger;
		dDTrigger.OnTriggered = (TriggerCanvasGroup.Triggered)Delegate.Combine(dDTrigger.OnTriggered, new TriggerCanvasGroup.Triggered(DDTriggered));
		TriggerCanvasGroup bridgeTrigger = BridgeTrigger;
		bridgeTrigger.OnTriggered = (TriggerCanvasGroup.Triggered)Delegate.Combine(bridgeTrigger.OnTriggered, new TriggerCanvasGroup.Triggered(BridgeTriggered));
	}

	private void OnDisable()
	{
		TriggerCanvasGroup mMTrigger = MMTrigger;
		mMTrigger.OnTriggered = (TriggerCanvasGroup.Triggered)Delegate.Remove(mMTrigger.OnTriggered, new TriggerCanvasGroup.Triggered(MMTriggered));
		TriggerCanvasGroup dDTrigger = DDTrigger;
		dDTrigger.OnTriggered = (TriggerCanvasGroup.Triggered)Delegate.Remove(dDTrigger.OnTriggered, new TriggerCanvasGroup.Triggered(DDTriggered));
		TriggerCanvasGroup bridgeTrigger = BridgeTrigger;
		bridgeTrigger.OnTriggered = (TriggerCanvasGroup.Triggered)Delegate.Remove(bridgeTrigger.OnTriggered, new TriggerCanvasGroup.Triggered(BridgeTriggered));
		StopAll();
	}

	private void MMTriggered()
	{
		AudioManager.Instance.PlayOneShot(mmEventPath);
	}

	private void DDTriggered()
	{
		AudioManager.Instance.PlayOneShot(ddEventPath);
	}

	private void BridgeTriggered()
	{
		AudioManager.Instance.PlayOneShot(bridgeEventPath);
	}

	public void PlayExecutionTrack()
	{
		AudioManager.Instance.StopLoop(bassEventInstance);
		AudioManager.Instance.PlayOneShot(ExecutionEventPath);
	}

	public void PlayAmbientNature()
	{
		if (!natureEventInstance.isValid())
		{
			natureEventInstance = AudioManager.Instance.CreateLoop(natureEventPath, true);
		}
	}

	public void PlayCombatMusic()
	{
		AudioManager.Instance.PlayMusic(biomeMusicEventPath);
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.SpecialCombat);
		AudioManager.Instance.SetMusicCombatState();
	}

	private void OnDestroy()
	{
		StopAll();
	}

	public void StopAll()
	{
		AudioManager.Instance.StopLoop(atmosEventInstance);
		AudioManager.Instance.StopLoop(natureEventInstance);
		AudioManager.Instance.StopLoop(bassEventInstance);
	}

	private void Update()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null) && bassEventInstance.isValid())
		{
			float num = Vector3.Distance(Player.transform.position, BassFadeObject.transform.position);
			if (Player.transform.position.y <= BassFadeObject.transform.position.y)
			{
				bassEventInstance.setParameterByName(SoundParams.Intensity, 1f - Mathf.Clamp01(num / MaxDistance));
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (BassFadeObject != null)
		{
			Utils.DrawCircleXY(BassFadeObject.transform.position, MaxDistance, Color.yellow);
		}
	}
}
