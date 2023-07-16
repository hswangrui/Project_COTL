using UnityEngine;

public class WindSystemController : MonoBehaviour
{
	private ParticleSystem particleSystem;

	private ParticleSystem.VelocityOverLifetimeModule windVelocity;

	private AnimationCurve curveMinX = new AnimationCurve();

	private AnimationCurve curveMaxX = new AnimationCurve();

	private AnimationCurve curveMinY = new AnimationCurve();

	private AnimationCurve curveMaxY = new AnimationCurve();

	private float lastWindSpeed;

	private float lastWindDensity;

	private float windSpeed = 3f;

	private float windDensity = 0.1f;

	private float transitionRate = 3f;

	private Vector2 windDirection = new Vector2(1f, 0.2f);

	private Vector2 lastWindDirection = new Vector2(0f, 0f);

	private Vector2 windTimer = new Vector2(0f, 0f);

	public void Initialise(ParticleSystem particleSystem)
	{
		this.particleSystem = particleSystem;
	}

	public void StartWind(WeatherSystemController.WeatherData weatherData)
	{
		WeatherSystemController.ShaderVariable[] shaderVariables = weatherData.ShaderVariables;
		for (int i = 0; i < shaderVariables.Length; i++)
		{
			WeatherSystemController.ShaderVariable shaderVariable = shaderVariables[i];
			if (shaderVariable.ShaderKey == "_Wind_Density")
			{
				windDensity = shaderVariable.TargetValue;
			}
			else if (shaderVariable.ShaderKey == "_Wind_Speed")
			{
				windSpeed = shaderVariable.TargetValue;
			}
		}
	}

	public void StopWind()
	{
		windDensity = 0.1f;
		windSpeed = 3f;
	}

	private void Update()
	{
		if ((bool)particleSystem)
		{
			UpdateWindSettings();
		}
	}

	private void UpdateWindSettings()
	{
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
		curveMinX.keys = Linear(0f, 1f, lastWindDirection.x * lastWindSpeed * 1.1f);
		curveMaxX.keys = Linear(0f, 1f, lastWindDirection.x * lastWindSpeed * 0.9f);
		curveMinY.keys = Linear(0f, 1f, lastWindDirection.y * lastWindSpeed * 1.1f);
		curveMaxY.keys = Linear(0f, 1f, lastWindDirection.y * lastWindSpeed * 0.9f);
		ParticleSystem.MinMaxCurve x = default(ParticleSystem.MinMaxCurve);
		x.mode = ParticleSystemCurveMode.TwoCurves;
		x.curveMin = curveMinX;
		x.curveMax = curveMaxX;
		x.curveMultiplier = 1f;
		ParticleSystem.MinMaxCurve y = default(ParticleSystem.MinMaxCurve);
		y.mode = ParticleSystemCurveMode.TwoCurves;
		y.curveMin = curveMinY;
		y.curveMax = curveMaxY;
		y.curveMultiplier = 1f;
		velocityOverLifetime.x = x;
		velocityOverLifetime.y = y;
		windVelocity = velocityOverLifetime;
		if (!Mathf.Approximately(lastWindDirection.x, windDirection.x))
		{
			lastWindDirection.x = Mathf.MoveTowards(lastWindDirection.x, windDirection.x, transitionRate * Time.deltaTime);
		}
		if (!Mathf.Approximately(lastWindDirection.y, windDirection.y))
		{
			lastWindDirection.y = Mathf.MoveTowards(lastWindDirection.y, windDirection.y, transitionRate * Time.deltaTime);
		}
		if (!Mathf.Approximately(lastWindSpeed, windSpeed))
		{
			lastWindSpeed = Mathf.MoveTowards(lastWindSpeed, windSpeed, transitionRate * Time.deltaTime);
		}
		if (!Mathf.Approximately(lastWindDensity, windDensity))
		{
			lastWindDensity = Mathf.MoveTowards(lastWindDensity, windDensity, transitionRate * Time.deltaTime);
		}
		windTimer.x += Time.deltaTime * lastWindSpeed * lastWindDirection.x;
		windTimer.y += Time.deltaTime * lastWindSpeed * lastWindDirection.y;
		Shader.SetGlobalVector("_WindTimer", windTimer);
		Shader.SetGlobalFloat("_WindDensity", lastWindDensity);
	}

	private static Keyframe[] Linear(float timeStart, float timeEnd, float value)
	{
		if (timeStart != timeEnd)
		{
			return new Keyframe[2]
			{
				new Keyframe(timeStart, value, 0f, 0f),
				new Keyframe(timeEnd, value, 0f, 0f)
			};
		}
		return new Keyframe[1]
		{
			new Keyframe(timeStart, value)
		};
	}
}
