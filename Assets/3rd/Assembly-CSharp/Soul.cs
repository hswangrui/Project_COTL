using System;
using UnityEngine;

public class Soul : BaseMonoBehaviour
{
	private GameObject Target;

	private float Delay = 0.5f;

	private float Speed = -15f;

	private float Angle;

	private float TargetAngle;

	public GameObject Image;

	private float ImageZ;

	private float ImageZSpeed;

	private float TurnSpeed = 7f;

	private void Start()
	{
		if (Target == null)
		{
			Target = GameObject.FindGameObjectWithTag("Player");
		}
		Angle = Utils.GetAngle(base.transform.position, Target.transform.position);
	}

	private void Update()
	{
		if (Target == null)
		{
			Target = GameObject.FindGameObjectWithTag("Player");
		}
		if (Target == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		ImageZSpeed += (-1f - ImageZ) * 0.3f;
		ImageZ += (ImageZSpeed *= 0.7f);
		Image.transform.localPosition = Vector3.zero + Vector3.forward * ImageZ;
		if (!((Delay -= Time.deltaTime) > 0f))
		{
			if (Delay < -1f)
			{
				TurnSpeed = Mathf.Lerp(TurnSpeed, 2f, 10f * Time.deltaTime);
			}
			TargetAngle = Utils.GetAngle(base.transform.position, Target.transform.position);
			Angle += Mathf.Atan2(Mathf.Sin((TargetAngle - Angle) * ((float)Math.PI / 180f)), Mathf.Cos((TargetAngle - Angle) * ((float)Math.PI / 180f))) * 57.29578f / (TurnSpeed * GameManager.DeltaTime);
			if (Speed < 20f)
			{
				Speed += 1f;
			}
			base.transform.position = base.transform.position + new Vector3(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)) * Time.deltaTime, Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f)) * Time.deltaTime);
			if (Vector2.Distance(base.transform.position, Target.transform.position) < Speed * Time.deltaTime)
			{
				CollectMe();
			}
		}
	}

	private void CollectMe()
	{
		PlayerFarming.Instance.GetSoul();
		BiomeConstants.Instance.EmitHitVFXSoul(Image.gameObject.transform.position, Quaternion.identity);
		CameraManager.shakeCamera(0.2f, Angle);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
