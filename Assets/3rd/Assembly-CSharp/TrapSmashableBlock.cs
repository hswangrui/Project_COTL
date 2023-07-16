using UnityEngine;

public class TrapSmashableBlock : MonoBehaviour
{
	private Health health;

	private void Awake()
	{
		health = GetComponent<Health>();
	}

	private void OnCollisionEnter2D(Collision2D col)
	{
		if (col.collider.gameObject.tag == "Player" && (TrinketManager.HasTrinket(TarotCards.Card.WalkThroughBlocks) || DataManager.Instance.PlayerScaleModifier > 1))
		{
			health.DealDamage(health.HP, col.collider.gameObject, base.transform.position);
		}
	}
}
