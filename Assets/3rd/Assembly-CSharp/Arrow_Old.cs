using System;
using UnityEngine;

public class Arrow_Old : Bullet
{
	private float vz;

	private float grav = 0.003f;

	private float displayAngle;

	private void Start()
	{
		float num = Vector2.Distance(target.transform.position, base.transform.position) * 2f / Speed;
		float num2 = grav * num;
		vz = num2 / 2f;
	}

	public override void Move()
	{
		vz -= grav;
		displayAngle = Utils.GetAngle(base.transform.position, base.transform.position + new Vector3(Speed * Mathf.Cos(angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(angle * ((float)Math.PI / 180f)) + vz, 0f));
		angle = Utils.GetAngle(base.transform.position, target.transform.position);
		base.transform.eulerAngles = new Vector3(0f, 0f, displayAngle);
		base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(angle * ((float)Math.PI / 180f)) + vz, 0f);
	}
}
