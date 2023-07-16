using UnityEngine;

public class DynamicResolutionManager : MonoBehaviour
{
	private float _maxResScale = 1f;

	private static float _minResScale = 1f;

	private static float _targetFPS = 60f;

	private float _intensity = 1f;

	private float _attenuation;

	public static float _fps;

	public static float _previousInterp;

	public static double m_gpuFrameTime;

	private static float _aggressiveness = 5.2f;

	private static bool _active = true;

	private float _previousTime;

	private uint m_frameCount;

	private const uint kNumFrameTimings = 2u;

	private double m_cpuFrameTime;

	private FrameTiming[] frameTimings = new FrameTiming[3];

	private void Start()
	{
		_active = true;
		_fps = _targetFPS;
		Object.DontDestroyOnLoad(base.gameObject);
		_previousTime = Time.realtimeSinceStartup;
	}

	public static void UpdateTargetFPS(float target)
	{
		_targetFPS = target;
	}

	public static void Toggle(bool value)
	{
		_active = value;
		ScalableBufferManager.ResizeBuffers(1f, 1f);
		_previousInterp = 1f;
	}

	private void Update()
	{
		if (_active && _minResScale < 1f)
		{
			float num = ((m_gpuFrameTime == 0.0) ? _fps : (1000f / (float)m_gpuFrameTime)) - (_targetFPS - 0.1f);
			float num2 = 1f;
			if (num < 0f)
			{
				_attenuation += Time.deltaTime * _intensity * Mathf.Abs(num) * 0.25f;
			}
			else
			{
				num2 = 10f;
				_attenuation -= Time.deltaTime * _intensity;
			}
			_attenuation = Mathf.Clamp01(_attenuation);
			float num3 = Mathf.MoveTowards(_previousInterp, Mathf.Lerp(_maxResScale, _minResScale, _attenuation), Time.deltaTime * _aggressiveness * num2);
			if (_previousInterp != num3 && num3 > 0f)
			{
				ScalableBufferManager.ResizeBuffers(num3, num3);
			}
			_previousInterp = num3;
			DetermineFPS();
			DetermineTimings();
		}
	}

	private void DetermineFPS()
	{
		if (float.IsInfinity(_fps))
		{
			_fps = _targetFPS;
		}
		_fps = (_fps + 1f / Mathf.Max(Time.realtimeSinceStartup - _previousTime, 1E-06f)) / 2f;
		_previousTime = Time.realtimeSinceStartup;
	}

	private void DetermineTimings()
	{
		m_frameCount++;
		if (m_frameCount > 2)
		{
			FrameTimingManager.CaptureFrameTimings();
			FrameTimingManager.GetLatestTimings(2u, frameTimings);
			if ((long)frameTimings.Length < 2L)
			{
				Debug.LogFormat("Skipping frame {0}, didn't get enough frame timings.", m_frameCount);
			}
			else
			{
				m_gpuFrameTime = frameTimings[0].gpuFrameTime;
				m_cpuFrameTime = frameTimings[0].cpuFrameTime;
			}
		}
	}
}
