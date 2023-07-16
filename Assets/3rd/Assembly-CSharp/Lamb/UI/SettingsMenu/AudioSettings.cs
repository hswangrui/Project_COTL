using UnityEngine;

namespace Lamb.UI.SettingsMenu
{
	public class AudioSettings : UISubmenuBase
	{
		[Header("Audio Settings")]
		[SerializeField]
		private MMSlider _masterVolumeSlider;

		[SerializeField]
		private MMSlider _musicVolumeSlider;

		[SerializeField]
		private MMSlider _sfxVolumeSlider;

		[SerializeField]
		private MMSlider _voVolumeSlider;

		private void Start()
		{
			if (SettingsManager.Settings != null)
			{
				Configure(SettingsManager.Settings.Audio);
				_masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
				_sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
				_musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
				_voVolumeSlider.onValueChanged.AddListener(OnVOVolumeChanged);
			}
		}

		public void Configure(SettingsData.AudioSettings audioSettings)
		{
			_masterVolumeSlider.value = audioSettings.MasterVolume * 100f;
			_sfxVolumeSlider.value = audioSettings.SFXVolume * 100f;
			_musicVolumeSlider.value = audioSettings.MusicVolume * 100f;
			_voVolumeSlider.value = audioSettings.VOVolume * 100f;
		}

		public void Reset()
		{
			SettingsManager.Settings.Audio = new SettingsData.AudioSettings();
		}

		private void OnMasterVolumeChanged(float volume)
		{
			volume /= 100f;
			Debug.Log(string.Format("AudioSettings - Master Volume changed to {0}", volume).Colour(Color.yellow));
			AudioManager.Instance.SetMasterBusVolume(volume);
			SettingsManager.Settings.Audio.MasterVolume = volume;
		}

		private void OnSFXVolumeChanged(float volume)
		{
			volume /= 100f;
			Debug.Log(string.Format("AudioSettings - SFX Volume changed to {0}", volume).Colour(Color.yellow));
			AudioManager.Instance.SetSFXBusVolume(volume);
			SettingsManager.Settings.Audio.SFXVolume = volume;
		}

		private void OnMusicVolumeChanged(float volume)
		{
			volume /= 100f;
			Debug.Log(string.Format("AudioSettings - Music Volume changed to {0}", volume).Colour(Color.yellow));
			AudioManager.Instance.SetMusicBusVolume(volume);
			SettingsManager.Settings.Audio.MusicVolume = volume;
		}

		private void OnVOVolumeChanged(float volume)
		{
			volume /= 100f;
			Debug.Log(string.Format("AudioSettings - VO Volume changed to {0}", volume).Colour(Color.yellow));
			AudioManager.Instance.SetVOBusVolume(volume);
			SettingsManager.Settings.Audio.VOVolume = volume;
		}
	}
}
