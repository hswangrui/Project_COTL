using System.Collections;
using UnityEngine;

public class EnemyBurrowingTrail : BaseMonoBehaviour
{
	[SerializeField]
	private float damageColliderDelay = 0.3f;

	[SerializeField]
	private Collider2D damageCollider;

	public Collider2D DamageCollider
	{
		get
		{
			return damageCollider;
		}
	}

	private void OnEnable()
	{
		damageCollider.enabled = false;
		StartCoroutine(EnableColliderIE());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		damageCollider.enabled = true;
	}

	private IEnumerator EnableColliderIE()
	{
		yield return new WaitForSeconds(damageColliderDelay);
		damageCollider.enabled = true;
		float timer = 0f;
		while (true)
		{
			float num;
			timer = (num = timer + Time.deltaTime);
			if (!(num >= 0.5f))
			{
				break;
			}
			while (PlayerRelic.TimeFrozen)
			{
				yield return null;
			}
			yield return null;
		}
		damageCollider.enabled = false;
	}
}
