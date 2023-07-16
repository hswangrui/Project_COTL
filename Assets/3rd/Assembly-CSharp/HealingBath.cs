using UnityEngine;

public class HealingBath : BaseMonoBehaviour
{
	private float Timer;

	public ParticleSystem particleSystem;

	private HealthPlayer playerHealth;

	private void Start()
	{
		particleSystem.Stop();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			Timer = 0f;
			particleSystem.Play();
			playerHealth = collision.gameObject.GetComponent<HealthPlayer>();
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!(collision.gameObject.tag == "Player"))
		{
			return;
		}
		if (DataManager.Instance.PLAYER_HEALTH < DataManager.Instance.PLAYER_TOTAL_HEALTH)
		{
			if ((Timer += Time.deltaTime) > 2f)
			{
				Timer = 0f;
				playerHealth.Heal(1f);
			}
		}
		else
		{
			particleSystem.Stop();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			particleSystem.Stop();
		}
	}
}
