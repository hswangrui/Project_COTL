using UnityEngine;

public class WaterInteractionRiver : BaseMonoBehaviour
{
	public GameObject waterVFX;

	public float splashFrequency = 0.075f;

	public float lastPlayerSplash;

	private void Start()
	{
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			AudioManager.Instance.playerFootstepOverride = "event:/player/footstep_water";
			if (PlayerFarming.Instance != null)
			{
				PlayerFarming.Instance.SetActiveDustEffect(false);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			AudioManager.Instance.playerFootstepOverride = string.Empty;
			if (PlayerFarming.Instance != null)
			{
				PlayerFarming.Instance.SetActiveDustEffect(true);
			}
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		UnitObject component = other.GetComponent<UnitObject>();
		if ((bool)component)
		{
			if (component.gameObject.tag == "Player" && lastPlayerSplash < Time.realtimeSinceStartup - 0.3f)
			{
				Object.Instantiate(waterVFX, other.transform.position, Quaternion.identity);
				lastPlayerSplash = Time.realtimeSinceStartup;
			}
			if (Random.value < splashFrequency)
			{
				Object.Instantiate(waterVFX, other.transform.position, Quaternion.identity);
			}
		}
	}

	private void OnDisable()
	{
		AudioManager.Instance.playerFootstepOverride = string.Empty;
	}

	private void OnDestroy()
	{
		AudioManager.Instance.playerFootstepOverride = string.Empty;
	}
}
