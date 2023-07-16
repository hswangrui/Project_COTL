using DG.Tweening;
using UnityEngine;

public class ShakeOnHit : BaseMonoBehaviour
{
	public Health health;

	private Quaternion StartRotation;

	private Bouncer bouncer;

	private void Start()
	{
		bouncer = base.transform.GetComponent<Bouncer>();
		StartRotation = base.transform.rotation;
		if ((bool)health)
		{
			health.OnHit += OnHit;
		}
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes type, bool hit)
	{
		Shake();
		if ((bool)bouncer)
		{
			UnitObject component = Attacker.GetComponent<UnitObject>();
			if ((bool)component)
			{
				bouncer.bounceUnit(component, component.transform.position - base.transform.position);
			}
		}
	}

	public void Shake()
	{
		base.transform.DOKill();
		base.transform.rotation = StartRotation;
		base.transform.DOPunchRotation(new Vector3(0f, 0f, 10f), 1f);
	}
}
