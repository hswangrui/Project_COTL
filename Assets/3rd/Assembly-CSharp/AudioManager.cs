using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using MMRoomGeneration;
using UnityEngine;
using WebSocketSharp;

public class AudioManager : BaseMonoBehaviour
{
	private static AudioManager _instance;

	[SerializeField]
	private Transform listener;

	[SerializeField]
	[Range(0f, 1f)]
	private float listenerPosBetweenCameraAndTarget;

	private EventInstance invalidEventInstance;

	private bool fmodBanksLoaded;

	private EventInstance currentMusicInstance;

	private string queuedMusicPath = string.Empty;

	private EventInstance AtmosInstance;

	private EventInstance masterBusVolumeSnapshot;

	private EventInstance sfxBusVolumeSnapshot;

	private EventInstance musicBusVolumeSnapshot;

	private EventInstance voBusVolumeSnapshot;

	[SerializeField]
	private GameManager gameManager;

	private bool SetFilter;

	private List<EventInstance> InstanceList = new List<EventInstance>();

	private List<EventInstance> activeLoops = new List<EventInstance>();

	public string playerFootstepOverride = string.Empty;

	public string footstepOverride = string.Empty;

	public static AudioManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (Object.Instantiate(Resources.Load("MMAudio/AudioManager")) as GameObject).GetComponent<AudioManager>();
			}
			return _instance;
		}
	}

	public GameObject Listener
	{
		get
		{
			return listener.gameObject;
		}
	}

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		if (base.transform.parent != null)
		{
			base.transform.SetParent(null);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private IEnumerator Start()
	{
		while (!RuntimeManager.HaveAllBanksLoaded)
		{
			yield return 0;
			Debug.Log("FMOD All Banks not loaded");
		}
		gameManager = GameManager.GetInstance();
		fmodBanksLoaded = true;
		if (queuedMusicPath != string.Empty)
		{
			PlayMusic(queuedMusicPath);
			queuedMusicPath = string.Empty;
		}
		masterBusVolumeSnapshot = RuntimeManager.CreateInstance("snapshot:/master_bus");
		masterBusVolumeSnapshot.start();
		sfxBusVolumeSnapshot = RuntimeManager.CreateInstance("snapshot:/sfx_bus");
		sfxBusVolumeSnapshot.start();
		musicBusVolumeSnapshot = RuntimeManager.CreateInstance("snapshot:/music_bus");
		musicBusVolumeSnapshot.start();
		voBusVolumeSnapshot = RuntimeManager.CreateInstance("snapshot:/vo_bus");
		voBusVolumeSnapshot.start();
		if (SettingsManager.Settings != null)
		{
			SetMasterBusVolume(SettingsManager.Settings.Audio.MasterVolume);
			SetMusicBusVolume(SettingsManager.Settings.Audio.MusicVolume);
			SetSFXBusVolume(SettingsManager.Settings.Audio.SFXVolume);
			SetVOBusVolume(SettingsManager.Settings.Audio.VOVolume);
		}
	}

	private void Update()
	{
		if (!(GameManager.GetInstance() == null) && GameManager.GetInstance().CamFollowTarget != null)
		{
			listener.position = Vector3.Lerp(GameManager.GetInstance().CamFollowTarget.transform.position, GameManager.GetInstance().CamFollowTarget.targetPosition, listenerPosBetweenCameraAndTarget);
		}
	}

	public void SetMusicFilter(string SoundParam, float value)
	{
		RuntimeManager.StudioSystem.setParameterByName(SoundParam, value);
	}

	public void SetMusicPitch(float value)
	{
		currentMusicInstance.setPitch(value);
	}

	public void ToggleFilter(string SoundParam, bool toggle)
	{
		if (toggle)
		{
			RuntimeManager.StudioSystem.setParameterByName(SoundParam, 1f);
		}
		else
		{
			RuntimeManager.StudioSystem.setParameterByName(SoundParam, 0f);
		}
	}

	public void ToggleFilter(string SoundParam, bool toggle, float delay)
	{
		StartCoroutine(ToggleFilterDelay(SoundParam, toggle, delay));
	}

	private IEnumerator ToggleFilterDelay(string SoundParam, bool toggle, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		ToggleFilter(SoundParam, toggle);
	}

	public void SetGameManager(GameManager gm)
	{
		gameManager = gm;
	}

	public bool IsSoundPathValid(string soundPath)
	{
		if (string.IsNullOrEmpty(soundPath))
		{
			return false;
		}
		if (!fmodBanksLoaded)
		{
			Debug.LogWarning("FMOD cannot play event '" + soundPath + "' because banks have not loaded yet");
			return false;
		}
        EventDescription _event;
        RuntimeManager.StudioSystem.getEvent(soundPath, out _event);
        if (_event.isValid())
        {
            return true;
        }
        Debug.LogWarning("FMOD cannot find event '" + soundPath + "'");
		return false;
	}

	public bool CurrentEventIsPlayingPath(EventInstance currentInstance, string soundPath)
	{
		if (currentInstance.isValid())
		{
			PLAYBACK_STATE state;
			currentInstance.getPlaybackState(out state);
			if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
			{
				return false;
			}
			EventDescription description;
			currentInstance.getDescription(out description);
			if (description.isValid())
			{
				string path;
				description.getPath(out path);
				if (path == soundPath)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void PlayMusic(string soundPath, bool StartMusic = true)
	{
		if (!fmodBanksLoaded)
		{
			queuedMusicPath = soundPath;
		}
		else
		{
			if (!IsSoundPathValid(soundPath))
			{
				return;
			}
			if (currentMusicInstance.isValid())
			{
				if (CurrentEventIsPlayingPath(currentMusicInstance, soundPath))
				{
					Debug.Log("PK: " + soundPath + " is already playing");
					return;
				}
				StopCurrentMusic();
			}
			currentMusicInstance = CreateLoop(soundPath, StartMusic, false);
		}
	}

	public void PlayLoop(EventInstance instance)
	{
		if (instance.isValid())
		{
			instance.start();
		}
		else
		{
			Debug.Log("Couldn't start instance");
		}
	}

	public void StartMusic()
	{
		if (currentMusicInstance.isValid())
		{
			currentMusicInstance.start();
		}
		else
		{
			Debug.Log("Couldn't start music");
		}
	}

	public void PlayCurrentAtmos()
	{
		if (AtmosInstance.isValid())
		{
			AtmosInstance.start();
		}
	}

	public void PlayAtmos(string soundPath)
	{
		if (!fmodBanksLoaded)
		{
			queuedMusicPath = soundPath;
		}
		else
		{
			if (!IsSoundPathValid(soundPath))
			{
				return;
			}
			if (AtmosInstance.isValid())
			{
				if (CurrentEventIsPlayingPath(AtmosInstance, soundPath))
				{
					return;
				}
				StopCurrentAtmos();
			}
			AtmosInstance = CreateLoop(soundPath, true, false);
		}
	}

	public void AdjustAtmosParameter(string parameter, float value)
	{
		if (AtmosInstance.isValid())
		{
			SetEventInstanceParameter(AtmosInstance, parameter, value);
		}
		else
		{
			Debug.Log("AtmosInstance not valid");
		}
	}

	public void StopCurrentAtmos(bool AllowFadeOut = true)
	{
		if (AtmosInstance.isValid())
		{
			if (AllowFadeOut)
			{
				AtmosInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}
			else
			{
				AtmosInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			}
		}
	}

	public void SetMusicRoomID(SoundConstants.RoomID roomID)
	{
		if (currentMusicInstance.isValid())
		{
			if (roomID == SoundConstants.RoomID.NoMusic)
			{
				StopCurrentMusic();
			}
			else
			{
				SetEventInstanceParameter(currentMusicInstance, SoundParams.RoomID, (float)roomID);
			}
		}
	}

	public void SetMusicRoomID(int roomID, string Parameter)
	{
		if (currentMusicInstance.isValid())
		{
			if (roomID == 9999)
			{
				StopCurrentMusic();
			}
			else
			{
				SetEventInstanceParameter(currentMusicInstance, Parameter, roomID);
			}
		}
		else
		{
			Debug.Log("currentMusicInstance is not valid");
		}
	}

	public void SetMusicBaseID(SoundConstants.BaseID baseID)
	{
		if (currentMusicInstance.isValid())
		{
			if (baseID == SoundConstants.BaseID.NoMusic)
			{
				StopLoop(currentMusicInstance);
			}
			else
			{
				SetEventInstanceParameter(currentMusicInstance, SoundParams.BaseID, (float)baseID);
			}
		}
	}

	public void SetFollowersDance(float value)
	{
		SetEventInstanceParameter(currentMusicInstance, SoundParams.FollowersDance, value);
	}

	public void SetFollowersSing(float value)
	{
		SetEventInstanceParameter(currentMusicInstance, SoundParams.FollowersSing, value);
	}

	public void SetMusicPsychedelic(float value)
	{
		SetEventInstanceParameter(currentMusicInstance, SoundParams.Psychedelic, value);
	}

	public void StopCurrentMusic()
	{
		if (currentMusicInstance.isValid())
		{
			StopLoop(currentMusicInstance);
		}
	}

	public void SetMusicCombatState(bool active = true)
	{
		//Debug.Log("SetMusicCombatState º¯ÊýÐÞ¸Ä ¹Ø±Õ ");
		RuntimeManager.StudioSystem.setParameterByName(SoundParams.Combat, active ? 1f : 0f);
	}

	public void PlayOneShot(string soundPath)
	{
		if (IsSoundPathValid(soundPath))
		{
			RuntimeManager.PlayOneShot(soundPath);
		}
	}

	public void PlayOneShotDelayed(string soundPath, float delay, Transform t)
	{
		if (IsSoundPathValid(soundPath))
		{
			StartCoroutine(OneShotDelayedTransform(soundPath, delay, t));
		}
	}

	public void PlayOneShotDelayed(string soundPath, float delay)
	{
		if (IsSoundPathValid(soundPath))
		{
			StartCoroutine(OneShotDelayed(soundPath, delay));
		}
	}

	private IEnumerator OneShotDelayedTransform(string soundPath, float delay, Transform t = null)
	{
		yield return new WaitForSecondsRealtime(delay);
		RuntimeManager.PlayOneShot(soundPath, t.position);
	}

	private IEnumerator OneShotDelayed(string soundPath, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		RuntimeManager.PlayOneShot(soundPath);
	}

	public void PlayOneShot(string soundPath, Vector3 pos)
	{
		if (IsSoundPathValid(soundPath))
		{
			RuntimeManager.PlayOneShot(soundPath, pos);
		}
	}

	public EventInstance PlayOneShotWithInstance(string soundPath)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return invalidEventInstance;
		}
		EventInstance result = RuntimeManager.CreateInstance(soundPath);
		result.start();
		return result;
	}

	public void PlayOneShot(string soundPath, GameObject go)
	{
		if (IsSoundPathValid(soundPath))
		{
			RuntimeManager.PlayOneShotAttached(soundPath, go);
		}
	}

	public EventInstance CreateLoop(string soundPath, bool playLoop = false, bool addToActiveLoops = true)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return invalidEventInstance;
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
		if (addToActiveLoops)
		{
			activeLoops.Add(eventInstance);
		}
		if (eventInstance.isValid())
		{
			eventInstance.set3DAttributes(base.gameObject.transform.To3DAttributes());
		}
		if (playLoop)
		{
			eventInstance.start();
		}
		return eventInstance;
	}

	public void PauseActiveLoops()
	{
		foreach (EventInstance activeLoop in activeLoops)
		{
			activeLoop.setPaused(true);
		}
	}

	public void StopActiveLoops()
	{
		foreach (EventInstance activeLoop in activeLoops)
		{
			activeLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			activeLoop.release();
		}
	}

	public void ResumeActiveLoops()
	{
		foreach (EventInstance activeLoop in activeLoops)
		{
			activeLoop.setPaused(false);
		}
	}

	public EventInstance CreateLoop(string soundPath, GameObject go, bool playLoop = false, bool addToActiveLoops = true)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return invalidEventInstance;
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
		if (addToActiveLoops)
		{
			activeLoops.Add(eventInstance);
		}
		if (eventInstance.isValid())
		{
			eventInstance.set3DAttributes(go.transform.To3DAttributes());
		}
		RuntimeManager.AttachInstanceToGameObject(eventInstance, go.transform, (Rigidbody)null);
		if (playLoop)
		{
			eventInstance.start();
		}
		return eventInstance;
	}

	public void StopLoop(EventInstance instance)
	{
		if (instance.isValid())
		{
			if (activeLoops.Contains(instance))
			{
				activeLoops.Remove(instance);
			}
			instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			instance.release();
		}
	}

	public void PlayOneShotAndSetParameterValue(string soundPath, string parameterName, float value, Transform tf = null)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return;
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
		if (eventInstance.isValid())
		{
			eventInstance.setParameterByName(parameterName, value);
			if (tf != null)
			{
				eventInstance.set3DAttributes(tf.To3DAttributes());
			}
			eventInstance.start();
			eventInstance.release();
		}
	}

	public void PlayOneShotAndSetParametersValue(string soundPath, string parameterName, float value, string parameterName2, float value2, Transform tf = null, int followerID = -1)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return;
		}
		if ((float)followerID != -1f && FollowerManager.GetSpecialFollowerFallback(followerID) != null)
		{
			soundPath = FollowerManager.GetSpecialFollowerFallback(followerID);
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
		if (eventInstance.isValid())
		{
			eventInstance.setParameterByName(parameterName, value);
			eventInstance.setParameterByName(parameterName2, value2);
			eventInstance.setVolume(1f);
			if (tf != null)
			{
				eventInstance.set3DAttributes(tf.To3DAttributes());
			}
			eventInstance.start();
			eventInstance.release();
		}
	}

	public void PlayOneShotAndSetParametersValue(string soundPath, string parameterName, float value, string parameterName2, float value2, Transform tf = null)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return;
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
		if (eventInstance.isValid())
		{
			eventInstance.setParameterByName(parameterName, value);
			eventInstance.setParameterByName(parameterName2, value2);
			eventInstance.setVolume(1f);
			if (tf != null)
			{
				eventInstance.set3DAttributes(tf.To3DAttributes());
			}
			eventInstance.start();
			eventInstance.release();
		}
	}

	public void PlayOneShotAndSetParametersValue(string soundPath, string parameterName, float value, string parameterName2, float value2, string parameterName3, float value3, Transform tf = null)
	{
		if (!IsSoundPathValid(soundPath))
		{
			return;
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(soundPath);
		if (eventInstance.isValid())
		{
			eventInstance.setParameterByName(parameterName, value);
			eventInstance.setParameterByName(parameterName2, value2);
			eventInstance.setParameterByName(parameterName3, value3);
			eventInstance.setVolume(1f);
			if (tf != null)
			{
				eventInstance.set3DAttributes(tf.To3DAttributes());
			}
			eventInstance.start();
			eventInstance.release();
		}
	}

	public void SetEventInstanceParameter(EventInstance eventInstance, string name, float value)
	{
		if (!eventInstance.isValid())
		{
			Debug.Log("Event Instance not valid");
		}
		else
		{
			eventInstance.setParameterByName(name, value);
		}
	}

	public void PlayFootstep(Vector3 pos)
	{
		if (!footstepOverride.IsNullOrEmpty())
		{
			PlayOneShot(footstepOverride, pos);
			return;
		}
		string soundPath = "event:/material/footstep_grass";
		switch (GenerateRoom.GetGroundTypeFromPosition(pos))
		{
		case GroundType.Grass:
			soundPath = "event:/material/footstep_grass";
			break;
		case GroundType.Bush:
			soundPath = "event:/material/footstep_bush";
			break;
		case GroundType.Hard:
			soundPath = "event:/material/footstep_hard";
			break;
		case GroundType.Sand:
			soundPath = "event:/material/footstep_sand";
			break;
		case GroundType.Snow:
			soundPath = "event:/material/footstep_snow";
			break;
		case GroundType.Water:
			soundPath = "event:/material/footstep_water";
			break;
		case GroundType.Woodland:
			soundPath = "event:/material/footstep_woodland";
			break;
		}
		PlayOneShot(soundPath, pos);
	}

	public string GetGroundType(GroundType groundType)
	{
		switch (groundType)
		{
		case GroundType.Grass:
			return "event:/player/footstep_grass";
		case GroundType.Bush:
			return "event:/player/footstep_bush";
		case GroundType.Hard:
			return "event:/player/footstep_hard";
		case GroundType.Sand:
			return "event:/player/footstep_sand";
		case GroundType.Snow:
			return "event:/player/footstep_snow";
		case GroundType.Water:
			return "event:/player/footstep_water";
		case GroundType.Woodland:
			return "event:/player/footstep_woodland";
		default:
			return "";
		}
	}

	public void PlayFootstepPlayer(Vector3 pos)
	{
		if (PlayerFarming.Location == FollowerLocation.Base)
		{
			playerFootstepOverride = GetGroundType(PathTileManager.Instance.GetTileSoundAtPosition(pos));
		}
		if (!playerFootstepOverride.IsNullOrEmpty())
		{
			PlayOneShot(playerFootstepOverride, pos);
			return;
		}
		string soundPath = "event:/player/footstep_grass";
		switch (GenerateRoom.GetGroundTypeFromPosition(pos))
		{
		case GroundType.Grass:
			soundPath = "event:/player/footstep_grass";
			break;
		case GroundType.Bush:
			soundPath = "event:/player/footstep_bush";
			break;
		case GroundType.Hard:
			soundPath = "event:/player/footstep_hard";
			break;
		case GroundType.Sand:
			soundPath = "event:/player/footstep_sand";
			break;
		case GroundType.Snow:
			soundPath = "event:/player/footstep_snow";
			break;
		case GroundType.Water:
			soundPath = "event:/player/footstep_water";
			break;
		case GroundType.Woodland:
			soundPath = "event:/player/footstep_woodland";
			break;
		}
		PlayOneShot(soundPath, pos);
	}

	public void SetMasterBusVolume(float volume)
	{
		if (masterBusVolumeSnapshot.isValid())
		masterBusVolumeSnapshot.setParameterByName(SoundParams.MasterBusVolume, volume);
	}

	public void SetMusicBusVolume(float volume)
	{
        if (musicBusVolumeSnapshot.isValid())
            musicBusVolumeSnapshot.setParameterByName(SoundParams.MusicBusVolume, volume);
	}

	public void SetSFXBusVolume(float volume)
	{
        if (sfxBusVolumeSnapshot.isValid())
            sfxBusVolumeSnapshot.setParameterByName(SoundParams.SFXBusVolume, volume);
	}

	public void SetVOBusVolume(float volume)
	{
        if (voBusVolumeSnapshot.isValid())
            voBusVolumeSnapshot.setParameterByName(SoundParams.VOBusVolume, volume);
	}

	private void OnDisable()
	{
		if (_instance == this)
		{
			StopCurrentMusic();
		}
	}
}
