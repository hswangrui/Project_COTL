using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.UI;

public class WeatherSystemController : MonoBehaviour
{
	[Serializable]
	public enum WeatherType
	{
		None,
		Raining,
		Windy,
		Snowing
	}

	[Serializable]
	public enum WeatherStrength
	{
		Light,
		Medium,
		Heavy
	}

	[Serializable]
	public struct ShaderVariable
	{
		public enum variableType
		{
			None,
			Float,
			Texture,
			Vector4
		}

		public variableType VariableType;

		public string ShaderKey;

		public float TargetValue;

		public Vector4 TargetVector;

		public Texture2D TargetTexture;

		public bool SetOnStartOnly;
	}

	[Serializable]
	public class WeatherData
	{
		public WeatherType WeatherType;

		public WeatherStrength WeatherStrength;

		[Range(0f, 1f)]
		public float Chance;

		public float Intensity;

		public float EmissionMultiplier;

		public string ParticleSystemName;

		public Image Overlay;

		[Space]
		public string ShaderKeyword;

		public ShaderVariable[] ShaderVariables;

		[Space]
		public string SoundLoop;

		public float Volume;

		public ParticleSystem ParticleSystem { get; set; }
	}

	public static WeatherSystemController Instance;

	[SerializeField]
	[Range(0f, 1f)]
	private float chanceForWeather;

	[SerializeField]
	private WeatherData[] weatherData;

	private WeatherType currentWeatherType;

	private WeatherStrength currentWeatherStrength;

	private WeatherType previousWeatherType;

	private WeatherStrength previousWeatherStrength;

	private DataManager.LocationWeather locationWeather;

	private WindSystemController windSystem;

	public EventInstance soundLoop;

	public static bool GlobalWeatherOverride = true;

	public bool IsRaining
	{
		get
		{
			return currentWeatherType == WeatherType.Raining;
		}
	}

	public bool IsSnowing
	{
		get
		{
			return currentWeatherType == WeatherType.Snowing;
		}
	}

	public bool IsWindy
	{
		get
		{
			return currentWeatherType == WeatherType.Windy;
		}
	}

	private void Awake()
	{
		Instance = this;
		windSystem = GetComponent<WindSystemController>();
		if (CheatConsole.IN_DEMO)
		{
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(SetLocationWeather));
		}
		else
		{
			TimeManager.OnNewDayStarted = (Action)Delegate.Combine(TimeManager.OnNewDayStarted, new Action(SetLocationWeather));
		}
	}

	private void Start()
	{
		ParticleSystem[] componentsInChildren = CameraManager.instance.GetComponentsInChildren<ParticleSystem>();
		WeatherData[] array = this.weatherData;
		foreach (WeatherData weatherData in array)
		{
			ShaderVariable[] shaderVariables = weatherData.ShaderVariables;
			for (int j = 0; j < shaderVariables.Length; j++)
			{
				ShaderVariable shaderVariable = shaderVariables[j];
				if (shaderVariable.TargetValue != 0f)
				{
					Shader.SetGlobalFloat(shaderVariable.ShaderKey, shaderVariable.SetOnStartOnly ? shaderVariable.TargetValue : 0f);
				}
				else if (shaderVariable.TargetVector != Vector4.zero)
				{
					Shader.SetGlobalVector(shaderVariable.ShaderKey, shaderVariable.SetOnStartOnly ? shaderVariable.TargetVector : Vector4.zero);
				}
				else if (shaderVariable.TargetTexture != null)
				{
					Shader.SetGlobalTexture(shaderVariable.ShaderKey, shaderVariable.TargetTexture);
				}
			}
			ParticleSystem[] array2 = componentsInChildren;
			foreach (ParticleSystem particleSystem in array2)
			{
				if (weatherData.ParticleSystemName == particleSystem.name)
				{
					weatherData.ParticleSystem = particleSystem;
					ParticleSystem.EmissionModule emission = weatherData.ParticleSystem.emission;
					emission.enabled = false;
					break;
				}
			}
			if (weatherData.WeatherType == WeatherType.Windy)
			{
				windSystem.Initialise(weatherData.ParticleSystem);
			}
		}
		StartCoroutine(WaitForPlayer());
	}

	private IEnumerator WaitForPlayer()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		if (!LoadLocationWeather())
		{
			SetLocationWeather();
		}
	}

	private void OnDestroy()
	{
		if (CheatConsole.IN_DEMO)
		{
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(SetLocationWeather));
		}
		else
		{
			TimeManager.OnNewDayStarted = (Action)Delegate.Remove(TimeManager.OnNewDayStarted, new Action(SetLocationWeather));
		}
		WeatherData[] array = weatherData;
		for (int i = 0; i < array.Length; i++)
		{
			Shader.DisableKeyword(array[i].ShaderKeyword);
		}
		AudioManager.Instance.StopLoop(soundLoop);
	}

	private void Update()
	{
		if (locationWeather != null && TimeManager.TotalElapsedGameTime - locationWeather.StartingTime > (float)locationWeather.Duration && (locationWeather.Location == PlayerFarming.Location || (GlobalWeatherOverride && DataManager.Instance.GlobalWeatherOverride == locationWeather)))
		{
			ClearLocationWeather();
		}
	}

	private bool LoadLocationWeather()
	{
		if (GlobalWeatherOverride)
		{
			locationWeather = DataManager.Instance.GlobalWeatherOverride;
			SetWeather(locationWeather.WeatherType, locationWeather.WeatherStrength, 0f);
			return true;
		}
		foreach (DataManager.LocationWeather item in DataManager.Instance.LocationsWeather)
		{
			if (item.Location == PlayerFarming.Location)
			{
				if (TimeManager.TotalElapsedGameTime - item.StartingTime < (float)item.Duration)
				{
					locationWeather = item;
					SetWeather(item.WeatherType, item.WeatherStrength, 0f);
					return true;
				}
				break;
			}
		}
		return false;
	}

	private void ClearLocationWeather()
	{
		locationWeather = null;
		StopCurrentWeather(3f);
		for (int i = 0; i < DataManager.Instance.LocationsWeather.Count; i++)
		{
			if (DataManager.Instance.LocationsWeather[i].Location == PlayerFarming.Location)
			{
				DataManager.Instance.LocationsWeather.RemoveAt(i);
				break;
			}
		}
		if (GlobalWeatherOverride)
		{
			DataManager.Instance.GlobalWeatherOverride = new DataManager.LocationWeather();
		}
	}

	private WeatherData ChooseRandomWeather()
	{
		if (UnityEngine.Random.value <= chanceForWeather)
		{
			this.weatherData.Shuffle();
			for (int i = 0; i < 100; i++)
			{
				WeatherData[] array = this.weatherData;
				foreach (WeatherData weatherData in array)
				{
					if (UnityEngine.Random.value <= weatherData.Chance)
					{
						return weatherData;
					}
				}
			}
			return null;
		}
		return null;
	}

	private void SetLocationWeather()
	{
		if (currentWeatherType != 0)
		{
			return;
		}
		WeatherData weatherData = ChooseRandomWeather();
		if (weatherData == null)
		{
			return;
		}
		DataManager.LocationWeather locationWeather = new DataManager.LocationWeather();
		locationWeather.Location = PlayerFarming.Location;
		locationWeather.StartingTime = TimeManager.TotalElapsedGameTime;
		locationWeather.Duration = UnityEngine.Random.Range(240, 480);
		locationWeather.WeatherType = weatherData.WeatherType;
		locationWeather.WeatherStrength = weatherData.WeatherStrength;
		this.locationWeather = locationWeather;
		bool flag = false;
		for (int i = 0; i < DataManager.Instance.LocationsWeather.Count; i++)
		{
			if (DataManager.Instance.LocationsWeather[i].Location == PlayerFarming.Location)
			{
				DataManager.Instance.LocationsWeather[i] = locationWeather;
				flag = true;
			}
		}
		if (!flag)
		{
			DataManager.Instance.LocationsWeather.Add(locationWeather);
		}
		if (GlobalWeatherOverride)
		{
			DataManager.Instance.GlobalWeatherOverride = locationWeather;
		}
		SetWeather(weatherData.WeatherType, weatherData.WeatherStrength, 3f);
	}

	public void SetWeather(WeatherType weatherType, WeatherStrength weatherStrength, float transitionDuration)
	{
		if (weatherType == WeatherType.None)
		{
			return;
		}
		if (currentWeatherType != weatherType)
		{
			StopCurrentWeather(transitionDuration - 0.5f);
		}
		currentWeatherType = weatherType;
		currentWeatherStrength = weatherStrength;
		WeatherData currentData = GetWeatherData(weatherType, weatherStrength);
		if (currentData == null)
		{
			return;
		}
		Shader.EnableKeyword(currentData.ShaderKeyword);
		currentData.Overlay.color = new Color(currentData.Overlay.color.r, currentData.Overlay.color.g, currentData.Overlay.color.b, 0f);
		currentData.Overlay.DOFade(0.15f, transitionDuration);
		if (currentData.ParticleSystem != null)
		{
			ParticleSystem.EmissionModule emission = currentData.ParticleSystem.emission;
			emission.rateOverTime = 0f;
			emission.enabled = true;
		}
		float time = 0f;
		DOTween.To(() => time, delegate(float x)
		{
			time = x;
		}, 1f, transitionDuration).OnUpdate(delegate
		{
			float num = Mathf.Lerp(0f, currentData.Intensity, time);
			ShaderVariable[] shaderVariables = currentData.ShaderVariables;
			for (int i = 0; i < shaderVariables.Length; i++)
			{
				ShaderVariable shaderVariable = shaderVariables[i];
				if (shaderVariable.TargetValue != 0f && !shaderVariable.SetOnStartOnly)
				{
					Shader.SetGlobalFloat(shaderVariable.ShaderKey, Mathf.Lerp(0f, shaderVariable.TargetValue, time));
				}
				else if (shaderVariable.TargetVector != Vector4.zero && !shaderVariable.SetOnStartOnly)
				{
					Shader.SetGlobalVector(shaderVariable.ShaderKey, Vector4.Lerp(Vector4.zero, shaderVariable.TargetVector, time));
				}
			}
			if (currentData.ParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission2 = currentData.ParticleSystem.emission;
				emission2.rateOverTime = currentData.EmissionMultiplier * num;
			}
		}).OnComplete(delegate
		{
			soundLoop = AudioManager.Instance.CreateLoop(currentData.SoundLoop, base.gameObject, true);
			soundLoop.setVolume(currentData.Volume);
		});
		if (weatherType == WeatherType.Windy)
		{
			windSystem.StartWind(currentData);
		}
	}

	public void StopCurrentWeather(float transitionDuration)
	{
		previousWeatherType = currentWeatherType;
		previousWeatherStrength = currentWeatherStrength;
		WeatherData currentData = GetWeatherData(currentWeatherType, currentWeatherStrength);
		currentWeatherType = WeatherType.None;
		if (currentData == null)
		{
			return;
		}
		currentData.Overlay.DOFade(0f, transitionDuration);
		float time = 0f;
		DOTween.To(() => time, delegate(float x)
		{
			time = x;
		}, 1f, transitionDuration).OnUpdate(delegate
		{
			float num = Mathf.Lerp(currentData.Intensity, 0f, time);
			ShaderVariable[] shaderVariables = currentData.ShaderVariables;
			for (int i = 0; i < shaderVariables.Length; i++)
			{
				ShaderVariable shaderVariable = shaderVariables[i];
				if (shaderVariable.TargetValue != 0f && !shaderVariable.SetOnStartOnly)
				{
					Shader.SetGlobalFloat(shaderVariable.ShaderKey, Mathf.Lerp(shaderVariable.TargetValue, 0f, time));
				}
				else if (shaderVariable.TargetVector != Vector4.zero && !shaderVariable.SetOnStartOnly)
				{
					Shader.SetGlobalVector(shaderVariable.ShaderKey, Vector4.Lerp(shaderVariable.TargetVector, Vector4.zero, time));
				}
			}
			if (currentData.ParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission2 = currentData.ParticleSystem.emission;
				emission2.rateOverTime = currentData.EmissionMultiplier * num;
			}
			soundLoop.setVolume(Mathf.Lerp(currentData.Volume, 0f, time));
		}).OnComplete(delegate
		{
			AudioManager.Instance.StopLoop(soundLoop);
			Shader.DisableKeyword(currentData.ShaderKeyword);
			if (currentData.ParticleSystem != null)
			{
				ParticleSystem.EmissionModule emission = currentData.ParticleSystem.emission;
				emission.enabled = false;
			}
		});
		if (currentData.WeatherType == WeatherType.Windy)
		{
			windSystem.StopWind();
		}
	}

	public void EnteredBuilding()
	{
		StopCurrentWeather(0f);
	}

	public void ExitedBuilding()
	{
		SetWeather(previousWeatherType, previousWeatherStrength, 0f);
	}

	private WeatherData GetWeatherData(WeatherType weatherType, WeatherStrength weatherStrength)
	{
		WeatherData[] array = this.weatherData;
		foreach (WeatherData weatherData in array)
		{
			if (weatherData.WeatherType == weatherType && weatherData.WeatherStrength == weatherStrength)
			{
				return weatherData;
			}
		}
		return null;
	}
}
