using System.Collections;
using UnityEngine;

public class ForestScuttlerSlamBarricade : BaseMonoBehaviour
{
	public ColliderEvents ColliderEvents;

	public SpriteRenderer SpriteRenderer;

	public float RaiseDuration = 0.1f;

	private Health EnemyHealth;

	private void OnEnable()
	{
		SpriteRenderer.gameObject.SetActive(false);
		ColliderEvents.OnTriggerEnterEvent += OnDamageTriggerEnter;
		ColliderEvents.SetActive(false);
	}

	private void OnDisable()
	{
		ColliderEvents.OnTriggerEnterEvent -= OnDamageTriggerEnter;
		ColliderEvents.SetActive(false);
	}

	public void Play(float Delay)
	{
		base.gameObject.SetActive(true);
		StartCoroutine(PlayRoutine(Delay));
	}

	private IEnumerator PlayRoutine(float Delay)
	{
		base.transform.localScale = Vector3.zero;
		yield return new WaitForSeconds(Delay);
		SpriteRenderer.gameObject.SetActive(true);
		ColliderEvents.SetActive(true);
		float Progress = 0f;
		float Duration2 = RaiseDuration;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			base.transform.localScale = Vector3.one * (Progress / Duration2);
			yield return null;
		}
		base.transform.localScale = Vector3.one;
		yield return new WaitForSeconds(0.2f);
		Progress = 0f;
		Duration2 = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			base.transform.localScale = Vector3.one * (1f - Progress / Duration2);
			yield return null;
		}
		ColliderEvents.SetActive(false);
		base.gameObject.SetActive(false);
	}

	private void OnDamageTriggerEnter(Collider2D collider)
	{
		EnemyHealth = collider.GetComponent<Health>();
		if (EnemyHealth != null && EnemyHealth.team != Health.Team.Team2)
		{
			EnemyHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f));
		}
	}
}
