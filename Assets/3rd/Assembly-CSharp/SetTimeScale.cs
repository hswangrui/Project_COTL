using UnityEngine;

public class SetTimeScale : BaseMonoBehaviour
{
	public float timescale = 1f;

	public bool UpdateUnscaledTime;

	private void Start()
	{
		Time.timeScale = timescale;
	}

	private void Update()
	{
		if (UpdateUnscaledTime)
		{
			Shader.SetGlobalFloat("_GlobalTimeUnscaled", Time.unscaledTime);
			Shader.SetGlobalFloat("_GlobalTimeUnscaled1", Time.unscaledTime);
		}
	}
}
