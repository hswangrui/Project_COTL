using System;
using UnityEngine;

public class ThrowWorshipper : BaseMonoBehaviour
{
	private float vz;

	private float childZ;

	private Rigidbody2D rb;

	private float vx;

	private float vy;

	private float Speed;

	public float FacingAngle;

	public GameObject child;

	private float Timer;

	private float BloodTimer;

	public GameObject blood;

	public float StartVZ = -2f;

	public float StartSpeed = 5f;

	private SimpleSpineAnimator SpineAnimator;

	private Vector3 Scale = Vector3.one;

	private Vector2 ScaleSpeed;

	private float Rotation;

	public float BounceSpeed = 0.8f;

	public float BounceZSpeed = -0.8f;

	public Action OnComplete { get; set; }

	private void OnEnable()
	{
		rb = GetComponent<Rigidbody2D>();
		vz = StartVZ;
		childZ = -0.5f;
		Speed = StartSpeed;
		SpineAnimator = GetComponentInChildren<SimpleSpineAnimator>();
	}

	private void OnDisable()
	{
	}

	private void Update()
	{
		ScaleSpeed.x += (1f - Scale.x) * 0.3f;
		Scale.x += (ScaleSpeed.x *= 0.8f);
		ScaleSpeed.y += (1f - Scale.y) * 0.3f;
		Scale.y += (ScaleSpeed.y *= 0.8f);
		BounceChild();
		vx = Speed * Mathf.Cos(FacingAngle * ((float)Math.PI / 180f));
		vy = Speed * Mathf.Sin(FacingAngle * ((float)Math.PI / 180f));
		if ((BloodTimer += Time.deltaTime) > 0.05f)
		{
			BloodTimer = 0f;
			UnityEngine.Object.Instantiate(blood, child.transform.position, Quaternion.identity, base.transform.parent);
		}
	}

	private void FixedUpdate()
	{
		if (!(rb == null))
		{
			rb.MovePosition(rb.position + new Vector2(vx, vy) * Time.deltaTime);
		}
	}

	private void BounceChild()
	{
		if (childZ >= 0f)
		{
			if (vz > 0.1f)
			{
				Speed *= BounceSpeed;
				vz *= BounceZSpeed;
				Scale.x = 0.5f;
				Scale.y = 1.5f;
				SpineAnimator.Animate("thrown-bounce", 0, false);
				SpineAnimator.AddAnimate("thrown", 0, true, 0f);
			}
			else
			{
				vz = 0f;
				Action onComplete = OnComplete;
				if (onComplete != null)
				{
					onComplete();
				}
				OnComplete = null;
				base.enabled = false;
			}
			childZ = 0f;
		}
		else
		{
			vz += 0.02f;
		}
		childZ += vz;
		child.transform.localPosition = new Vector3(0f, 0f, childZ);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		FacingAngle += 180 + UnityEngine.Random.Range(-20, 20);
		FacingAngle %= 360f;
		vx = Speed * Mathf.Cos(FacingAngle * ((float)Math.PI / 180f));
		vy = Speed * Mathf.Sin(FacingAngle * ((float)Math.PI / 180f));
	}
}
