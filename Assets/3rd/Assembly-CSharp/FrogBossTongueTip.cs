using UnityEngine;

public class FrogBossTongueTip : UnitObject
{
	private FrogBossTongue tongue;

	public override void Awake()
	{
		base.Awake();
		tongue = GetComponentInParent<FrogBossTongue>();
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		tongue.DealDamageToBoss(Attacker, AttackLocation, AttackType, FromBehind);
	}
}
