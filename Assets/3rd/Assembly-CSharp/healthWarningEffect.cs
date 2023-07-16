using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class healthWarningEffect : MonoBehaviour
{
	public Image healthOverlay;

	public Image poisonOverlay;

	public float playerHealth;

	public float playerPoison;

	private float hpCache;

	private bool turnedonPoison;

	private bool turnedonHealth;

	private void Start()
	{
		healthOverlay.gameObject.SetActive(false);
		poisonOverlay.gameObject.SetActive(false);
		healthOverlay.color = new Color(healthOverlay.color.r, healthOverlay.color.g, healthOverlay.color.b, 0f);
	}

	private IEnumerator DisableHealth()
	{
		yield return new WaitForSeconds(1f);
		healthOverlay.gameObject.SetActive(false);
		turnedonHealth = false;
	}

	private void Update()
	{
		if (PlayerFarming.Instance == null || PlayerFarming.Instance.health == null)
		{
			return;
		}
		playerHealth = PlayerFarming.Instance.health.HP + PlayerFarming.Instance.health.BlueHearts + PlayerFarming.Instance.health.BlackHearts + PlayerFarming.Instance.health.SpiritHearts;
		playerPoison = PlayerFarming.Instance.health.poisonTimer / 2f;
		if (playerPoison > 0f)
		{
			if (!turnedonPoison)
			{
				turnedonPoison = true;
				poisonOverlay.gameObject.SetActive(true);
			}
			poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, playerPoison);
		}
		else
		{
			poisonOverlay.color = new Color(poisonOverlay.color.r, poisonOverlay.color.g, poisonOverlay.color.b, 0f);
			if (turnedonPoison)
			{
				poisonOverlay.gameObject.SetActive(false);
				turnedonPoison = false;
			}
		}
		if (playerHealth == hpCache)
		{
			return;
		}
		healthOverlay.DOKill();
		float obj = playerHealth;
		if (!4f.Equals(obj))
		{
			if (!3f.Equals(obj))
			{
				if (!2f.Equals(obj))
				{
					if (!1f.Equals(obj))
					{
						if (0f.Equals(obj))
						{
							healthOverlay.DOKill();
							healthOverlay.DOFade(0f, 0.5f);
						}
					}
					else
					{
						healthOverlay.DOKill();
						healthOverlay.DOFade(1f, 0.5f);
					}
				}
				else
				{
					healthOverlay.DOKill();
					healthOverlay.DOFade(0.66f, 0.5f);
				}
			}
			else
			{
				healthOverlay.DOKill();
				healthOverlay.DOFade(0.33f, 0.5f);
			}
		}
		else
		{
			healthOverlay.DOKill();
			healthOverlay.DOFade(0.2f, 0.5f);
		}
		bool flag = false;
		if (playerHealth <= 4f && DataManager.Instance.PLAYER_TOTAL_HEALTH > 4f)
		{
			flag = true;
		}
		else if (DataManager.Instance.PLAYER_TOTAL_HEALTH <= 4f)
		{
			flag = playerHealth < DataManager.Instance.PLAYER_TOTAL_HEALTH / 2f;
		}
		if (flag)
		{
			turnedonHealth = true;
			healthOverlay.gameObject.SetActive(true);
		}
		else if (turnedonHealth)
		{
			StartCoroutine(DisableHealth());
			healthOverlay.DOFade(0f, 0.5f);
		}
		hpCache = playerHealth;
	}
}
