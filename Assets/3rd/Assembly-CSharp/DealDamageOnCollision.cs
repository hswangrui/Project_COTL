using UnityEngine;

public class DealDamageOnCollision : BaseMonoBehaviour
{
	private Health EnemyHealth;

	public bool NoDamageWhileDodging;

	public bool IncludeStayingInCollider;

	private bool DamageDealt;

	private void CheckCollision(GameObject collisionGameObject)
	{
		if (!IncludeStayingInCollider || DamageDealt)
		{
			return;
		}
		EnemyHealth = collisionGameObject.GetComponent<Health>();
		if (EnemyHealth != null && (!NoDamageWhileDodging || EnemyHealth.state.CURRENT_STATE != StateMachine.State.Dodging))
		{
			DamageDealt = true;
			EnemyHealth.DealDamage(2f, base.gameObject, base.transform.position);
			CameraManager.shakeCamera(0.5f, Utils.GetAngle(base.transform.position, EnemyHealth.transform.position));
			if (EnemyHealth.team == Health.Team.PlayerTeam)
			{
				GameManager.GetInstance().HitStop();
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Debug.Log("On Collision Enter");
		if (!IncludeStayingInCollider)
		{
			CheckCollision(collision.gameObject);
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		Debug.Log("On Collision Stay");
		CheckCollision(collision.gameObject);
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		Debug.Log("On Collision Exit");
		DamageDealt = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Debug.Log("On Trigger Enter");
		if (!IncludeStayingInCollider)
		{
			CheckCollision(collision.gameObject);
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		Debug.Log("On Trigger Stay");
		CheckCollision(collision.gameObject);
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		Debug.Log("On Trigger Exit");
		DamageDealt = false;
	}
}
