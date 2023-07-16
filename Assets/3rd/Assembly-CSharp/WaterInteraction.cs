using UnityEngine;

public class WaterInteraction : BaseMonoBehaviour
{
	public bool inTrigger;

	public Collider2D playerCollider;

	public float timer;

	public Vector3 PlayerOldPosition;

	public float TimeToSpawn = 1f;

	public GameObject waterVFX;

	public Vector3 prefabOffset;

	private void Start()
	{
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player")
		{
			playerCollider = other;
			inTrigger = true;
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
			playerCollider = other;
			inTrigger = false;
			AudioManager.Instance.playerFootstepOverride = string.Empty;
			if (PlayerFarming.Instance != null)
			{
				PlayerFarming.Instance.SetActiveDustEffect(true);
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

	private void Update()
	{
		if (inTrigger && playerCollider != null && (timer += Time.deltaTime) >= TimeToSpawn && PlayerOldPosition != playerCollider.gameObject.transform.position)
		{
			Object.Instantiate(position: (PlayerOldPosition = playerCollider.gameObject.transform.position) + prefabOffset, original: waterVFX, rotation: Quaternion.identity);
			timer = 0f;
		}
	}
}
