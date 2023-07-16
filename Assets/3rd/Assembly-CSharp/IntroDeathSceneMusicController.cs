using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class IntroDeathSceneMusicController : BaseMonoBehaviour
{
	[EventRef]
	public string ambientEventPath;

	private EventInstance ambientEventInstance;

	private float AmbientFadeIn = 12f;

	[EventRef]
	public string bassEventPath;

	private EventInstance bassEventInstance;

	public GameObject BassFadeObject;

	public float MaxDistance = 20f;

	[EventRef]
	public string crownEventPath;

	private GameObject Player;

	private float Timer;

	private void OnEnable()
	{
		AudioManager.Instance.StopCurrentAtmos();
		ambientEventInstance = AudioManager.Instance.CreateLoop(ambientEventPath, true);
		bassEventInstance = AudioManager.Instance.CreateLoop(bassEventPath);
		if (bassEventInstance.isValid())
		{
			bassEventInstance.setParameterByName(SoundParams.Intensity, 0f);
			bassEventInstance.start();
		}
	}

	public void PlaySpawnCrown()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null))
		{
			AudioManager.Instance.PlayOneShot(crownEventPath, Player.transform.position);
		}
	}

	public void StopAll()
	{
		AudioManager.Instance.StopLoop(ambientEventInstance);
		AudioManager.Instance.StopLoop(bassEventInstance);
	}

	private void Update()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null))
		{
			float num = Vector3.Distance(Player.transform.position, BassFadeObject.transform.position);
			if (bassEventInstance.isValid() && Player.transform.position.y <= BassFadeObject.transform.position.y)
			{
				bassEventInstance.setParameterByName(SoundParams.Intensity, 1f - Mathf.Clamp01(num / MaxDistance));
			}
			if (ambientEventInstance.isValid() && Player.transform.position.y <= BassFadeObject.transform.position.y)
			{
				ambientEventInstance.setParameterByName(SoundParams.Intensity, Mathf.Clamp01(num / MaxDistance));
			}
		}
	}

	private void OnDisable()
	{
		StopAll();
	}

	private void OnDrawGizmos()
	{
		if (BassFadeObject != null)
		{
			Utils.DrawCircleXY(BassFadeObject.transform.position, MaxDistance, Color.yellow);
		}
	}
}
