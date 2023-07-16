using System;
using UnityEngine;

public class TrapBarrel : BaseMonoBehaviour
{
	public float ExplosionSize = 5f;

	public SpriteRenderer sprite;

	private Health health;

	private CircleCollider2D circleCollider2D;

	private Rigidbody2D rb2D;

	private float Speed;

	private Vector2 DirectionalSpeed;

	private bool RightFacing;

	private float Angle;

	private float Z = -0.5f;

	private float ZSpeed = -4f;

	private Vector2 ScaleX = new Vector2(0.5f, 0f);

	private Vector2 ScaleY = new Vector2(2f, 0f);

	private float DestroyTimer;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnHit += OnHit;
		circleCollider2D = GetComponent<CircleCollider2D>();
		rb2D = GetComponent<Rigidbody2D>();
	}

	public void ChangeSprite(Sprite NewSprite)
	{
		sprite.sprite = NewSprite;
	}

	public void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		CameraManager.shakeCamera(1f, Utils.GetAngle(Attacker.transform.position, base.transform.position));
		GameManager.GetInstance().HitStop();
		Speed = 15f;
		Angle = Utils.GetAngle(Attacker.transform.position, base.transform.position);
		base.gameObject.layer = LayerMask.NameToLayer("Bullets");
		Health health = null;
		float num = float.MaxValue;
		float angle = 0f;
		float num2 = 90f;
		foreach (Health allUnit in Health.allUnits)
		{
			float angle2 = Utils.GetAngle(base.transform.position, allUnit.transform.position);
			float num3 = Vector2.Distance(base.transform.position, allUnit.transform.position);
			if (allUnit != this.health && !allUnit.InanimateObject && allUnit.team == Health.Team.Team2 && num3 < 8f && num3 < num && Mathf.Abs(Angle - angle2) < 180f && angle2 > Angle - num2 && angle2 < Angle + num2)
			{
				health = allUnit;
				num = num3;
				angle = angle2;
			}
		}
		if (health != null)
		{
			Angle = angle;
		}
		DirectionalSpeed = new Vector2(Speed * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Speed * Mathf.Sin(Angle * ((float)Math.PI / 180f)));
		if (DirectionalSpeed.x > 0f)
		{
			RightFacing = true;
		}
		else
		{
			RightFacing = false;
		}
	}

	private void BounceMe()
	{
		ZSpeed += 0.5f * GameManager.DeltaTime;
		Z += ZSpeed * Time.deltaTime;
		if (Z >= 0f)
		{
			if (OnGround(base.transform.position))
			{
				ScaleX.x = 1.5f;
				ScaleY.x = 0.5f;
				ZSpeed *= -1f;
				Z = 0f;
			}
			else
			{
				ScaleX.x -= 0.2f * GameManager.DeltaTime;
				ScaleX.x = Mathf.Max(0f, ScaleX.x);
				ScaleY.x -= 0.2f * GameManager.DeltaTime;
				ScaleY.x = Mathf.Max(0f, ScaleY.x);
				if ((DestroyTimer += Time.deltaTime) > 0.5f || ScaleX.x <= 0f)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
		else
		{
			ScaleX.y += (1f - ScaleX.x) * 0.3f;
			ScaleX.x += (ScaleX.y *= 0.8f);
			ScaleY.y += (1f - ScaleY.x) * 0.3f;
			ScaleY.x += (ScaleY.y *= 0.8f);
		}
		sprite.transform.parent.transform.parent.localPosition = Vector3.forward * Z;
		sprite.transform.parent.transform.parent.localScale = new Vector3(ScaleX.x, ScaleY.y, 1f);
	}

	private void Update()
	{
		sprite.transform.parent.Rotate(new Vector3(0f, Speed * (float)((!RightFacing) ? 1 : (-1)), 0f));
		if (Speed <= 0f)
		{
			return;
		}
		BounceMe();
		foreach (Health item in Health.team2)
		{
			if (item != null && item != health && MagnitudeFindDistanceBetween(base.transform.position, item.transform.position) < 0.25f)
			{
				health.DealDamage(float.MaxValue, base.gameObject, Vector3.zero);
				Explosion.CreateExplosion(base.transform.position + Vector3.forward * Z, Health.Team.PlayerTeam, health, ExplosionSize);
				UnityEngine.Object.Destroy(base.gameObject);
				break;
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (Speed > 0f && collision.gameObject.layer != LayerMask.NameToLayer("Obstacles") && collision.gameObject.layer != LayerMask.NameToLayer("Player"))
		{
			health.DealDamage(float.MaxValue, base.gameObject, Vector3.zero);
			Explosion.CreateExplosion(base.transform.position + Vector3.forward * Z, Health.Team.PlayerTeam, health, ExplosionSize);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void FixedUpdate()
	{
		rb2D.MovePosition(rb2D.position + DirectionalSpeed * Time.fixedDeltaTime);
	}

	private bool OnGround(Vector3 Position)
	{
		LayerMask layerMask = LayerMask.GetMask("Island");
		RaycastHit hitInfo;
		if (Physics.Raycast(Position, Vector3.forward, out hitInfo, 10f, layerMask))
		{
			return true;
		}
		return false;
	}

	private float MagnitudeFindDistanceBetween(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		return num * num + num2 * num2 + num3 * num3;
	}
}
