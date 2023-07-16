using System.Collections;
using Lamb.UI;
using Rewired;
using Unify.Input;
using UnityEngine;

public static class MMVibrate
{
	public enum HapticTypes
	{
		Selection,
		Success,
		Warning,
		Failure,
		LightImpact,
		MediumImpact,
		HeavyImpact,
		RigidImpact,
		SoftImpact,
		None
	}

	public static long LightDuration = 20L;

	public static long MediumDuration = 40L;

	public static long HeavyDuration = 80L;

	public static long RigidDuration = 20L;

	public static long SoftDuration = 80L;

	public static int LightAmplitude = 40;

	public static int MediumAmplitude = 120;

	public static int HeavyAmplitude = 255;

	public static int RigidAmplitude = 255;

	public static int SoftAmplitude = 40;

	private static bool _vibrationsActive = true;

	private static bool _debugLogActive = false;

	private static bool _hapticsPlayedOnce = false;

	private static float _vibrationIntensity = 1f;

	private static long[] _rigidImpactPattern = new long[2] { 0L, RigidDuration };

	private static int[] _rigidImpactPatternAmplitude = new int[2] { 0, RigidAmplitude };

	private static long[] _softImpactPattern = new long[2] { 0L, SoftDuration };

	private static int[] _softImpactPatternAmplitude = new int[2] { 0, SoftAmplitude };

	private static long[] _lightImpactPattern = new long[2] { 0L, LightDuration };

	private static int[] _lightImpactPatternAmplitude = new int[2] { 0, LightAmplitude };

	private static long[] _mediumImpactPattern = new long[2] { 0L, MediumDuration };

	private static int[] _mediumImpactPatternAmplitude = new int[2] { 0, MediumAmplitude };

	private static long[] _HeavyImpactPattern = new long[2] { 0L, HeavyDuration };

	private static int[] _HeavyImpactPatternAmplitude = new int[2] { 0, HeavyAmplitude };

	private static long[] _successPattern = new long[4] { 0L, LightDuration, LightDuration, HeavyDuration };

	private static int[] _successPatternAmplitude = new int[4] { 0, LightAmplitude, 0, HeavyAmplitude };

	private static long[] _warningPattern = new long[4] { 0L, HeavyDuration, LightDuration, MediumDuration };

	private static int[] _warningPatternAmplitude = new int[4] { 0, HeavyAmplitude, 0, MediumAmplitude };

	private static long[] _failurePattern = new long[8] { 0L, MediumDuration, LightDuration, MediumDuration, LightDuration, HeavyDuration, LightDuration, LightDuration };

	private static int[] _failurePatternAmplitude = new int[8] { 0, MediumAmplitude, 0, MediumAmplitude, 0, HeavyAmplitude, 0, LightAmplitude };

	private static Vector3 _rumbleRigid = new Vector3(0.5f, 1f, 0.08f);

	private static Vector3 _rumbleSoft = new Vector3(1f, 0.03f, 0.1f);

	private static Vector3 _rumbleLight = new Vector3(0.5f, 0.5f, 0.02f);

	private static Vector3 _rumbleMedium = new Vector3(0.8f, 0.8f, 0.04f);

	private static Vector3 _rumbleHeavy = new Vector3(1f, 1f, 0.08f);

	private static Vector3 _rumbleSuccess = new Vector3(1f, 1f, 1f);

	private static Vector3 _rumbleWarning = new Vector3(1f, 1f, 1f);

	private static Vector3 _rumbleFailure = new Vector3(1f, 1f, 1f);

	private static Vector3 _rumbleSelection = new Vector3(1f, 1f, 1f);

	public static bool Rumbling = false;

	public static bool RumblingContinuous = false;

	public static Player player
	{
		get
		{
			return RewiredInputManager.MainPlayer;
		}
		set
		{
			player = value;
		}
	}

	public static void SetHapticsActive(bool status)
	{
		Debug.Log("[MMVibrate] Set haptics active : " + status);
		_vibrationsActive = status;
		if (status || player == null)
		{
			return;
		}
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			joystick.StopVibration();
		}
	}

	public static void SetHapticsIntensity(float Intensity)
	{
		Debug.Log("[MMVibrate] Intensity Set to : " + Intensity);
		_vibrationIntensity = Intensity;
		SetHapticsActive(Intensity > 0f);
	}

	public static void Haptic(HapticTypes type, bool defaultToRegularVibrate = false, bool alsoRumble = false, MonoBehaviour coroutineSupport = null, int controllerID = -1)
	{
		if (_vibrationsActive && InputManager.General.InputIsController() && alsoRumble && coroutineSupport != null)
		{
			switch (type)
			{
			case HapticTypes.Selection:
				Rumble(_rumbleLight.x, _rumbleMedium.y, _rumbleLight.z, coroutineSupport, controllerID);
				break;
			case HapticTypes.Success:
				Rumble(_successPattern, _successPatternAmplitude, -1, coroutineSupport, controllerID);
				break;
			case HapticTypes.Warning:
				Rumble(_warningPattern, _warningPatternAmplitude, -1, coroutineSupport, controllerID);
				break;
			case HapticTypes.Failure:
				Rumble(_failurePattern, _failurePatternAmplitude, -1, coroutineSupport, controllerID);
				break;
			case HapticTypes.LightImpact:
				Rumble(_rumbleLight.x, _rumbleLight.y, _rumbleLight.z, coroutineSupport, controllerID);
				break;
			case HapticTypes.MediumImpact:
				Rumble(_rumbleMedium.x, _rumbleMedium.y, _rumbleMedium.z, coroutineSupport, controllerID);
				break;
			case HapticTypes.HeavyImpact:
				Rumble(_rumbleHeavy.x, _rumbleHeavy.y, _rumbleHeavy.z, coroutineSupport, controllerID);
				break;
			case HapticTypes.RigidImpact:
				Rumble(_rumbleRigid.x, _rumbleRigid.y, _rumbleRigid.z, coroutineSupport, controllerID);
				break;
			case HapticTypes.SoftImpact:
				Rumble(_rumbleSoft.x, _rumbleSoft.y, _rumbleSoft.z, coroutineSupport, controllerID);
				break;
			case HapticTypes.None:
				break;
			}
		}
	}

	public static void Rumble(float lowFrequency, float highFrequency, float duration, MonoBehaviour coroutineSupport, int controllerID = -1)
	{
		if (InputManager.General.InputIsController() && coroutineSupport.gameObject.activeInHierarchy)
		{
			coroutineSupport.StartCoroutine(RumbleCoroutine(lowFrequency, highFrequency, duration, controllerID));
		}
	}

	private static IEnumerator RumbleCoroutine(float lowFrequency, float highFrequency, float duration, int controllerID = -1)
	{
		if (player == null)
		{
			yield break;
		}
		Rumbling = true;
		float num = 1f;
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			if (player.controllers.GetLastActiveController() == joystick && joystick.supportsVibration)
			{
				joystick.SetVibration(lowFrequency * _vibrationIntensity * num, highFrequency * _vibrationIntensity * num);
			}
		}
		float startedAt = Time.unscaledTime;
		while (Time.unscaledTime - startedAt < duration)
		{
			yield return null;
		}
		foreach (Joystick joystick2 in player.controllers.Joysticks)
		{
			joystick2.StopVibration();
		}
		Rumbling = false;
	}

	public static void Rumble(long[] pattern, int[] amplitudes, int repeat, MonoBehaviour coroutineSupport, int controllerID = -1)
	{
		if (InputManager.General.InputIsController() && pattern != null && amplitudes != null)
		{
			coroutineSupport.StartCoroutine(RumblePatternCoroutine(pattern, amplitudes, amplitudes, repeat, coroutineSupport, controllerID));
		}
	}

	public static void Rumble(long[] pattern, int[] lowFreqAmplitudes, int[] highFreqAmplitudes, int repeat, MonoBehaviour coroutineSupport, int controllerID = -1)
	{
		if (pattern != null && lowFreqAmplitudes != null && highFreqAmplitudes != null)
		{
			coroutineSupport.StartCoroutine(RumblePatternCoroutine(pattern, lowFreqAmplitudes, highFreqAmplitudes, repeat, coroutineSupport, controllerID));
		}
	}

	private static IEnumerator RumblePatternCoroutine(long[] pattern, int[] lowFreqAmplitudes, int[] highFreqAmplitudes, int repeat, MonoBehaviour coroutineSupport, int controllerID = -1)
	{
		float unscaledTime = Time.unscaledTime;
		float currentTime = unscaledTime;
		int currentIndex = 0;
		while (currentIndex < pattern.Length)
		{
			if (player == null)
			{
				yield break;
			}
			int num = 0;
			float num2 = 0f;
			float num3 = 0f;
			do
			{
				float num4 = pattern[currentIndex];
				float num5 = ((lowFreqAmplitudes.Length > currentIndex) ? ((float)lowFreqAmplitudes[currentIndex] / 255f) : 0f);
				num2 += num5;
				float num6 = ((highFreqAmplitudes.Length > currentIndex) ? ((float)highFreqAmplitudes[currentIndex] / 255f) : 0f);
				num3 += num6;
				currentTime += num4 / 1000f;
				num++;
				currentIndex++;
			}
			while (currentTime < Time.unscaledTime && currentIndex < pattern.Length);
			float num7 = 1f;
			foreach (Joystick joystick in player.controllers.Joysticks)
			{
				if (player.controllers.GetLastActiveController() == joystick && joystick.supportsVibration)
				{
					joystick.SetVibration(num2 / (float)num * _vibrationIntensity * num7, num3 / (float)num * _vibrationIntensity * num7);
				}
			}
			while (currentTime > Time.unscaledTime && currentIndex < pattern.Length)
			{
				yield return null;
			}
		}
		foreach (Joystick joystick2 in player.controllers.Joysticks)
		{
			if (joystick2.supportsVibration)
			{
				joystick2.SetVibration(0f, 0f);
			}
		}
	}

	public static void RumbleContinuous(float lowFrequency, float highFrequency, int controllerID = -1)
	{
		if (MonoSingleton<UIManager>.Instance.IsPaused || !InputManager.General.InputIsController() || player == null)
		{
			return;
		}
		float num = 1f;
		Rumbling = true;
		RumblingContinuous = true;
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			if (player.controllers.GetLastActiveController() == joystick && joystick.supportsVibration)
			{
				joystick.SetVibration(lowFrequency * _vibrationIntensity * num, highFrequency * _vibrationIntensity * num);
			}
		}
	}

	public static void StopRumble()
	{
		Rumbling = false;
		RumblingContinuous = false;
		if (player == null)
		{
			return;
		}
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			joystick.StopVibration();
		}
	}
}
