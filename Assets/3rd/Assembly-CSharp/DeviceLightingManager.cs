using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.Video;

public class DeviceLightingManager : MonoSingleton<DeviceLightingManager>
{
	private class CustomKey
	{
		public KeyCode KeyCode;

		public Color TargetColor;

		public Color PreviousColor;

		public TweenerCore<float, float, FloatOptions> tween;

		public bool IsTransitioning
		{
			get
			{
				if (tween != null)
				{
					return tween.IsPlaying();
				}
				return false;
			}
		}
	}

	private enum EffectType
	{
		None,
		Rain,
		Video,
		Boss
	}

	public bool DEBUGGING;

	public Color DEBUG_COLOR;

	private const uint DEVTYPE_KEYBOARD = 524288u;

	public static KeyCode[] F_KEYS = new KeyCode[12]
	{
		KeyCode.F1,
		KeyCode.F2,
		KeyCode.F3,
		KeyCode.F4,
		KeyCode.F5,
		KeyCode.F6,
		KeyCode.F7,
		KeyCode.F8,
		KeyCode.F9,
		KeyCode.F10,
		KeyCode.F11,
		KeyCode.F12
	};

	public static KeyCode[] NUMPAD_KEYS = new KeyCode[17]
	{
		KeyCode.Keypad0,
		KeyCode.KeypadPeriod,
		KeyCode.Keypad1,
		KeyCode.Keypad2,
		KeyCode.Keypad3,
		KeyCode.KeypadEnter,
		KeyCode.Keypad4,
		KeyCode.Keypad5,
		KeyCode.Keypad6,
		KeyCode.Keypad7,
		KeyCode.Keypad8,
		KeyCode.Keypad9,
		KeyCode.KeypadPlus,
		KeyCode.Numlock,
		KeyCode.KeypadDivide,
		KeyCode.KeypadMultiply,
		KeyCode.KeypadMinus
	};

	private static Color[] TimeOfDayColors = new Color[5]
	{
		new Color(1f, 0.5f, 0f, 1f),
		new Color(1f, 0.5f, 0f, 1f),
		new Color(1f, 0.5f, 0f, 1f),
		new Color(1f, 0.5f, 0.5f, 1f),
		new Color(0.5f, 0f, 1f, 1f)
	};

	private List<CustomKey> customKeys = new List<CustomKey>();

	private VideoPlayer videoPlayer;

	private EffectType currentEffectType;

	private Array keycodes = Enum.GetValues(typeof(KeyCode));

	private float timestamp;

	private float previousFaith;

	private float previousHP;

	private TweenerCore<float, float, FloatOptions> allKeysTween;

	public override void Start()
	{
		base.Start();
		Initialise();
		videoPlayer = GetComponentInChildren<VideoPlayer>();
		HealthPlayer.OnDamaged += UpdateHealth;
		HealthPlayer.OnHPUpdated += OnHeal;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(OnLocationSet));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void OnDestroy()
	{
		HealthPlayer.OnDamaged -= UpdateHealth;
		HealthPlayer.OnHPUpdated -= OnHeal;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnLocationSet));
		CultFaithManager.OnPulse = (Action)Delegate.Remove(CultFaithManager.OnPulse, new Action(PulseLowFaithBar));
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhaseStarted));
	}

	private void Initialise()
	{
	}

	private void Update()
	{
		if (currentEffectType == EffectType.Video)
		{
			UpdateVideo();
		}
		if (PlayerFarming.Location == FollowerLocation.None)
		{
			return;
		}
		if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			if (currentEffectType == EffectType.Boss)
			{
				UpdateTrickleRedEffect(F_KEYS);
			}
			return;
		}
		if (PlayerFarming.Location == FollowerLocation.Church)
		{
			UpdateTrickleRedEffect();
			return;
		}
		if (WeatherSystemController.Instance.IsRaining && currentEffectType == EffectType.None)
		{
			List<KeyCode> list = new List<KeyCode>();
			list.AddRange(F_KEYS);
			list.AddRange(NUMPAD_KEYS);
			TransitionLighting(TimeOfDayColors[(int)TimeManager.CurrentPhase], Color.grey, 1f, list.ToArray());
			currentEffectType = EffectType.Rain;
		}
		else if (!WeatherSystemController.Instance.IsRaining && currentEffectType == EffectType.Rain)
		{
			List<KeyCode> list2 = new List<KeyCode>();
			list2.AddRange(F_KEYS);
			list2.AddRange(NUMPAD_KEYS);
			TransitionLighting(Color.grey, TimeOfDayColors[(int)TimeManager.CurrentPhase], 1f, list2.ToArray());
			currentEffectType = EffectType.None;
		}
		if (currentEffectType == EffectType.Rain)
		{
			UpdateRainEffect();
		}
		if (Mathf.Abs(CultFaithManager.CultFaithNormalised - previousFaith) > 0.05f && CultFaithManager.CultFaithNormalised > 0.1f)
		{
			UpdateFaith();
		}
	}

	private void OnLocationSet()
	{
		if (PlayerFarming.Location == FollowerLocation.IntroDungeon)
		{
			SetIntroLayout();
		}
		else if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			SetDungeonLayout();
		}
		else if (PlayerFarming.Location == FollowerLocation.Church)
		{
			SetTempleLayout();
		}
		else
		{
			SetBaseLayout();
		}
	}

	public static void UpdateLocation()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.OnLocationSet();
		}
	}

	public void SetDungeonLayout()
	{
		StopAll();
		if (currentEffectType != EffectType.Boss)
		{
			currentEffectType = EffectType.None;
		}
		CultFaithManager.OnPulse = (Action)Delegate.Remove(CultFaithManager.OnPulse, new Action(PulseLowFaithBar));
		BiomeGenerator.OnBiomeChangeRoom += OnBiomeChangeRoom;
		UpdateHealth(null);
		StartCoroutine(WaitForSeconds(0.5f, delegate
		{
			UpdateHealth(null);
		}));
	}

	public void SetBaseLayout()
	{
		StopAll();
		currentEffectType = EffectType.None;
		OnNewPhaseStarted();
		UpdateFaith();
		StartCoroutine(UpdateXPBar());
		CultFaithManager.OnPulse = (Action)Delegate.Combine(CultFaithManager.OnPulse, new Action(PulseLowFaithBar));
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
	}

	public static void ForceBaseLayout()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.SetBaseLayout();
		}
	}

	public void SetTempleLayout()
	{
		StopAll();
		CultFaithManager.OnPulse = (Action)Delegate.Remove(CultFaithManager.OnPulse, new Action(PulseLowFaithBar));
	}

	public static void ForceTempleLayout()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.SetTempleLayout();
		}
	}

	public void SetIntroLayout()
	{
		StopAll();
		SetCustomKey(KeyCode.W, Color.black, Color.red, 5f);
		SetCustomKey(KeyCode.A, Color.black, Color.red, 5f);
		SetCustomKey(KeyCode.S, Color.black, Color.red, 5f);
		SetCustomKey(KeyCode.D, Color.black, Color.red, 5f);
	}

	private void UpdateHealth(HealthPlayer Target)
	{
		if (!(PlayerFarming.Instance == null))
		{
			int num = F_KEYS.Length;
			int num2 = (int)PlayerFarming.Instance.health.totalHP;
			float t = PlayerFarming.Instance.health.HP / (float)num2;
			int num3 = Mathf.CeilToInt(Mathf.Lerp(0f, num, t));
			for (int i = 0; i < num; i++)
			{
				SetKeyColor(F_KEYS[i], (i <= num3) ? Color.red : Color.black);
			}
		}
	}

	private void OnHeal(HealthPlayer Target)
	{
		if (previousHP < Target.HP)
		{
			StopAll();
			StartCoroutine(WaitForEndOfFrame(delegate
			{
				UpdateLocation();
			}));
		}
		previousHP = Target.HP;
	}

	private void OnBiomeChangeRoom()
	{
		if ((bool)BiomeGenerator.Instance.GetComponentInChildren<MiniBossController>())
		{
			currentEffectType = EffectType.Boss;
			TransitionLighting(Color.black, Color.black, 0f, F_KEYS);
		}
		else if (currentEffectType == EffectType.Boss)
		{
			currentEffectType = EffectType.None;
		}
	}

	private void UpdateFaith()
	{
		if (!DataManager.Instance.ShowCultFaith)
		{
			return;
		}
		for (int i = 0; i < NUMPAD_KEYS.Length; i++)
		{
			SetKeyColor(NUMPAD_KEYS[i], Color.black);
		}
		float cultFaithNormalised = CultFaithManager.CultFaithNormalised;
		Color color = StaticColors.ColorForThreshold(cultFaithNormalised);
		if (cultFaithNormalised >= 0.1f)
		{
			if (previousFaith < 0.1f)
			{
				SetCustomKey(KeyCode.Keypad0, Color.white, color, 2f);
				SetCustomKey(KeyCode.KeypadPeriod, Color.white, color, 2f);
			}
			else
			{
				SetKeyColor(KeyCode.Keypad0, color);
				SetKeyColor(KeyCode.KeypadPeriod, color);
			}
		}
		if (cultFaithNormalised >= 0.3f)
		{
			if (previousFaith < 0.3f)
			{
				SetCustomKey(KeyCode.Keypad1, Color.white, color, 2f);
				SetCustomKey(KeyCode.Keypad2, Color.white, color, 2f);
				SetCustomKey(KeyCode.Keypad3, Color.white, color, 2f);
				SetCustomKey(KeyCode.KeypadEnter, Color.white, color, 2f);
			}
			else
			{
				SetKeyColor(KeyCode.Keypad1, color);
				SetKeyColor(KeyCode.Keypad2, color);
				SetKeyColor(KeyCode.Keypad3, color);
				SetKeyColor(KeyCode.KeypadEnter, color);
			}
		}
		if (cultFaithNormalised >= 0.5f)
		{
			if (previousFaith < 0.5f)
			{
				SetCustomKey(KeyCode.Keypad4, Color.white, color, 2f);
				SetCustomKey(KeyCode.Keypad5, Color.white, color, 2f);
				SetCustomKey(KeyCode.Keypad6, Color.white, color, 2f);
			}
			else
			{
				SetKeyColor(KeyCode.Keypad4, color);
				SetKeyColor(KeyCode.Keypad5, color);
				SetKeyColor(KeyCode.Keypad6, color);
			}
		}
		if (cultFaithNormalised >= 0.7f)
		{
			if (previousFaith < 0.7f)
			{
				SetCustomKey(KeyCode.Keypad7, Color.white, color, 2f);
				SetCustomKey(KeyCode.Keypad8, Color.white, color, 2f);
				SetCustomKey(KeyCode.Keypad9, Color.white, color, 2f);
				SetCustomKey(KeyCode.KeypadPlus, Color.white, color, 2f);
			}
			else
			{
				SetKeyColor(KeyCode.Keypad7, color);
				SetKeyColor(KeyCode.Keypad8, color);
				SetKeyColor(KeyCode.Keypad9, color);
				SetKeyColor(KeyCode.KeypadPlus, color);
			}
		}
		if (cultFaithNormalised >= 0.95f)
		{
			if (previousFaith < 0.95f)
			{
				SetCustomKey(KeyCode.Numlock, Color.white, color, 2f);
				SetCustomKey(KeyCode.KeypadDivide, Color.white, color, 2f);
				SetCustomKey(KeyCode.KeypadMultiply, Color.white, color, 2f);
				SetCustomKey(KeyCode.KeypadMinus, Color.white, color, 2f);
			}
			else
			{
				SetKeyColor(KeyCode.Numlock, color);
				SetKeyColor(KeyCode.KeypadDivide, color);
				SetKeyColor(KeyCode.KeypadMultiply, color);
				SetKeyColor(KeyCode.KeypadMinus, color);
			}
		}
		previousFaith = cultFaithNormalised;
	}

	private void PulseLowFaithBar()
	{
		if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			return;
		}
		KeyCode[] nUMPAD_KEYS = NUMPAD_KEYS;
		foreach (KeyCode keycode in nUMPAD_KEYS)
		{
			if (!GetCustomKey(keycode).IsTransitioning)
			{
				SetCustomKey(keycode, Color.red, new Color(0.1f, 0f, 0f, 1f), 0.5f, Ease.OutBounce);
			}
		}
	}

	private void OnNewPhaseStarted()
	{
		if (!GameManager.IsDungeon(PlayerFarming.Location) && currentEffectType == EffectType.None)
		{
			List<KeyCode> list = new List<KeyCode>();
			if (DataManager.Instance.ShowCultFaith)
			{
				list.AddRange(NUMPAD_KEYS);
			}
			if (DataManager.Instance.HasBuiltShrine1)
			{
				list.AddRange(F_KEYS);
			}
			Color previousColor = TimeOfDayColors[(int)Mathf.Repeat((float)(TimeManager.CurrentPhase - 1), 5f)];
			Color targetColor = TimeOfDayColors[(int)TimeManager.CurrentPhase];
			TransitionLighting(previousColor, targetColor, 1f, list.ToArray());
		}
	}

	private IEnumerator UpdateXPBar()
	{
		if (!DataManager.Instance.HasBuiltShrine1)
		{
			yield break;
		}
		while (true)
		{
			if (UpgradeSystem.AbilityPoints > 0)
			{
				for (int i = 0; i < F_KEYS.Length; i++)
				{
					if (GetCustomKey(F_KEYS[i]).PreviousColor == Color.red)
					{
						SetCustomKey(F_KEYS[i], Color.white, Color.red, 0.2f);
					}
					else
					{
						SetCustomKey(F_KEYS[i], Color.red, Color.white, 0.2f);
					}
					yield return new WaitForSecondsRealtime(0.04f);
				}
			}
			else
			{
				int targetXP = DataManager.GetTargetXP(Mathf.Min(DataManager.Instance.Level, Mathf.Max(DataManager.TargetXP.Count - 1, 0)));
				float t = (float)DataManager.Instance.XP / (float)targetXP;
				int num = Mathf.CeilToInt(Mathf.Lerp(0f, F_KEYS.Length, t));
				for (int j = 0; j < F_KEYS.Length; j++)
				{
					SetKeyColor(F_KEYS[j], (j <= num) ? Color.white : Color.black);
				}
			}
			yield return null;
		}
	}

	public void SetRain()
	{
		currentEffectType = EffectType.Rain;
	}

	private void UpdateRainEffect()
	{
		if (Time.unscaledTime > timestamp)
		{
			timestamp = Time.unscaledTime + UnityEngine.Random.Range(0.00025f, 0.0005f);
			KeyCode keyCode;
			do
			{
				keyCode = (KeyCode)UnityEngine.Random.Range(0, keycodes.Length / 2);
			}
			while (NUMPAD_KEYS.Contains(keyCode) || F_KEYS.Contains(keyCode) || GetCustomKey(keyCode).IsTransitioning);
			SetCustomKey(keyCode, Color.blue, Color.grey, 0.5f);
		}
	}

	public static void TransitionLighting(Color previousColor, Color targetColor, float transitionDuration, Ease ease = Ease.Linear)
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			TransitionLighting(previousColor, targetColor, transitionDuration, new KeyCode[0], ease);
		}
	}

	public static void TransitionLighting(Color previousColor, Color targetColor, float transitionDuration, KeyCode[] excludedKeys, Ease ease = Ease.Linear)
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.allKeysTween.Kill();
			float time = 0f;
			MonoSingleton<DeviceLightingManager>.Instance.allKeysTween = DOTween.To(() => time, delegate(float x)
			{
				time = x;
			}, 1f, transitionDuration).OnUpdate(delegate
			{
				Color.Lerp(previousColor, targetColor, time);
			}).SetEase(ease)
				.SetUpdate(true);
		}
	}

	public static void PulseAllLighting(Color previousColor, Color targetColor, float transitionDuration, KeyCode[] excludedKeys)
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.StartCoroutine(MonoSingleton<DeviceLightingManager>.Instance.PulseAllLightingIE(previousColor, targetColor, transitionDuration, excludedKeys));
		}
	}

	private IEnumerator PulseAllLightingIE(Color previousColor, Color targetColor, float transitionDuration, KeyCode[] excludedKeys)
	{
		Color currentColor = previousColor;
		while (true)
		{
			if (allKeysTween == null || !allKeysTween.IsPlaying())
			{
				if (currentColor == previousColor)
				{
					TransitionLighting(targetColor, previousColor, transitionDuration, excludedKeys);
					currentColor = targetColor;
				}
				else
				{
					TransitionLighting(previousColor, targetColor, transitionDuration, excludedKeys);
					currentColor = previousColor;
				}
			}
			yield return null;
		}
	}

	public static void StopAll()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			TweenerCore<float, float, FloatOptions> tweenerCore = MonoSingleton<DeviceLightingManager>.Instance.allKeysTween;
			if (tweenerCore != null)
			{
				tweenerCore.Kill();
			}
			ClearAllCustomKeys();
			MonoSingleton<DeviceLightingManager>.Instance.StopAllCoroutines();
		}
	}

	private void SetKeyColor(KeyCode keycode, Color color)
	{
		GetCustomKey(keycode);
	}

	private CustomKey GetCustomKey(KeyCode keycode)
	{
		foreach (CustomKey customKey2 in customKeys)
		{
			if (customKey2.KeyCode == keycode)
			{
				return customKey2;
			}
		}
		CustomKey customKey = new CustomKey
		{
			KeyCode = keycode
		};
		customKeys.Add(customKey);
		return customKey;
	}

	private void SetCustomKey(KeyCode keycode, Color targetColor, float transitionDuration, Ease ease = Ease.Linear)
	{
		CustomKey key = GetCustomKey(keycode);
		key.TargetColor = targetColor;
		if (transitionDuration == 0f)
		{
			SetKeyColor(key.KeyCode, key.TargetColor);
			return;
		}
		float time = 0f;
		key.tween = DOTween.To(() => time, delegate(float x)
		{
			time = x;
		}, 1f, transitionDuration).OnUpdate(delegate
		{
			SetKeyColor(key.KeyCode, Color.Lerp(key.PreviousColor, key.TargetColor, time));
		}).SetEase(ease)
			.SetUpdate(true);
	}

	private void SetCustomKey(KeyCode keycode, Color previousColor, Color targetColor, float transitionDuration, Ease ease = Ease.Linear)
	{
		GetCustomKey(keycode).PreviousColor = previousColor;
		SetCustomKey(keycode, targetColor, transitionDuration, ease);
	}

	public static void ClearAllCustomKeys()
	{
		if (MonoSingleton<DeviceLightingManager>.Instance == null)
		{
			return;
		}
		foreach (CustomKey customKey in MonoSingleton<DeviceLightingManager>.Instance.customKeys)
		{
			customKey.tween.Kill();
		}
	}

	public static void PlayVideo(string fileName)
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null) && !(MonoSingleton<DeviceLightingManager>.Instance.videoPlayer == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.videoPlayer.clip = Resources.Load<VideoClip>("Keyboard Lighting Assets/" + fileName);
			MonoSingleton<DeviceLightingManager>.Instance.videoPlayer.Play();
			MonoSingleton<DeviceLightingManager>.Instance.currentEffectType = EffectType.Video;
		}
	}

	public static void PlayVideo()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null) && !(MonoSingleton<DeviceLightingManager>.Instance.videoPlayer == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.currentEffectType = EffectType.Video;
		}
	}

	public static void StopVideo()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			MonoSingleton<DeviceLightingManager>.Instance.videoPlayer.Stop();
			MonoSingleton<DeviceLightingManager>.Instance.currentEffectType = EffectType.None;
		}
	}

	private void UpdateVideo()
	{
		StartCoroutine(WaitForEndOfFrame(delegate
		{
			Texture2D texture2D = new Texture2D(210, 70, TextureFormat.RGB24, false);
			RenderTexture.active = (videoPlayer.isPlaying ? videoPlayer.targetTexture : RenderTexture.active);
			texture2D.ReadPixels(new Rect(0f, 0f, 210f, 70f), 0, 0, false);
			texture2D.Apply(false, true);
			UnityEngine.Object.Destroy(texture2D);
		}));
	}

	public static void FlashColor(Color color, float fadeOut = 0.5f)
	{
		if (GameManager.IsDungeon(PlayerFarming.Location))
		{
			TransitionLighting(color, Color.black, fadeOut, F_KEYS);
			return;
		}
		if (PlayerFarming.Location == FollowerLocation.Church)
		{
			TransitionLighting(color, Color.black, fadeOut);
			return;
		}
		List<KeyCode> list = new List<KeyCode>();
		if (DataManager.Instance.ShowCultFaith)
		{
			list.AddRange(NUMPAD_KEYS);
		}
		if (DataManager.Instance.HasBuiltShrine1)
		{
			list.AddRange(F_KEYS);
		}
		TransitionLighting(color, TimeOfDayColors[(int)TimeManager.CurrentPhase], fadeOut, list.ToArray());
	}

	private void UpdateTrickleRedEffect(KeyCode[] excludedKeys)
	{
		if (Time.unscaledTime > timestamp)
		{
			timestamp = Time.unscaledTime + UnityEngine.Random.Range(2.5E-05f, 5E-05f);
			KeyCode keyCode;
			do
			{
				keyCode = (KeyCode)UnityEngine.Random.Range(0, keycodes.Length);
			}
			while (excludedKeys.Contains(keyCode) || GetCustomKey(keyCode).IsTransitioning);
			SetCustomKey(keyCode, Color.red, Color.black, 0.5f);
		}
	}

	private void UpdateTrickleRedEffect()
	{
		UpdateTrickleRedEffect(new KeyCode[0]);
	}

	private IEnumerator WaitForEndOfFrame(Action callback)
	{
		yield return new WaitForEndOfFrame();
		if (callback != null)
		{
			callback();
		}
	}

	private IEnumerator WaitForSeconds(float seconds, Action callback)
	{
		yield return new WaitForSeconds(seconds);
		if (callback != null)
		{
			callback();
		}
	}

	private void OnApplicationQuit()
	{
	}

	public static void Reset()
	{
		if (!(MonoSingleton<DeviceLightingManager>.Instance == null))
		{
			StopAll();
			MonoSingleton<DeviceLightingManager>.Instance.currentEffectType = EffectType.None;
		}
	}
}
