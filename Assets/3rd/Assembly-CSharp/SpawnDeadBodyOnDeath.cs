using UnityEngine;

public class SpawnDeadBodyOnDeath : BaseMonoBehaviour
{
	private Health health;

	public DeadBodySliding deadBodySliding;

	public float Speed = 1700f;

	private void OnEnable()
	{
		health = GetComponent<Health>();
		health.OnDie += OnDie;
	}

	private void OnDisable()
	{
		health.OnDie -= OnDie;
	}

	public void ReEnable(float Speed)
	{
		base.enabled = true;
		this.Speed = Speed;
	}

	public void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (!(deadBodySliding == null))
		{
			GameObject obj = deadBodySliding.gameObject.Spawn();
			DeadBodySliding component = obj.GetComponent<DeadBodySliding>();
			obj.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
			obj.transform.parent = base.transform.parent;
			bool explode = TrinketManager.HasTrinket(TarotCards.Card.Skull);
			if ((bool)component && (bool)Attacker)
			{
				component.Init(base.gameObject, Utils.GetAngle(Attacker.transform.position, base.transform.position), Speed, explode);
			}
		}
	}
}
