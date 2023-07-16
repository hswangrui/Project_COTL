using System;
using UnityEngine;

public class Bullet : BaseMonoBehaviour
{
	public Health target;

	public float Damage = 1f;

	public float Speed = 0.05f;

	public float angle;

	private void Start()
	{
	}

	private void Update()
	{
		if (target == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Move();
		if (Vector2.Distance(base.transform.position, target.transform.position) <= Speed)
		{
			target.DealDamage(Damage, base.gameObject, base.transform.position);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public virtual void Move()
	{
		angle = Utils.GetAngle(base.transform.position, target.transform.position);
		base.transform.eulerAngles = new Vector3(0f, 0f, angle);
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(angle * ((float)Math.PI / 180f)), 0f);
	}
}
