using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Bomb : BaseMonoBehaviour
{
	private static List<Bomb> bombs = new List<Bomb>();

	[SerializeField]
	private float explodeDelay = 3f;

	[SerializeField]
	private Transform container;

	private float moveTimer;

	private float explodeTimer;

	private Health health;

	private Rigidbody2D rigidbody;

	private float childZ;

	private float vx;

	private float vy;

	private float vz;

	private float Scale;

	private float facingAngle;

	private float speed;

	private Vector2 SquishScale = Vector2.one;

	private Vector2 SquishScaleSpeed = Vector2.zero;

	public static void CreateBomb(Vector3 position, Health health, Transform parent)
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync((health != null && health.team == Health.Team.PlayerTeam) ? "Assets/Prefabs/Enemies/Weapons/Bomb Friendly.prefab" : "Assets/Prefabs/Enemies/Weapons/Bomb.prefab", position, Quaternion.identity, parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			Bomb component = obj.Result.GetComponent<Bomb>();
			obj.Result.transform.position = position;
			component.health = health;
			AudioManager.Instance.PlayOneShot("event:/explosion/bomb_fuse", obj.Result);
		};
	}

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody2D>();
		vz = UnityEngine.Random.Range(-0.3f, -0.15f);
		speed = UnityEngine.Random.Range(5f, 10f);
		facingAngle = UnityEngine.Random.Range(0, 360);
		bombs.Add(this);
	}

	private void OnDestroy()
	{
		bombs.Remove(this);
	}

	private void Update()
	{
		explodeTimer += Time.deltaTime;
		moveTimer += Time.deltaTime;
		if (explodeTimer / explodeDelay > 1f && base.gameObject.activeInHierarchy)
		{
			Explosion.CreateExplosion(base.transform.position, Health.Team.KillAll, health, 5f);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (moveTimer < 1f)
		{
			Scale += (1f - Scale) / 7f;
			SquishScaleSpeed.x += (1f - SquishScale.x) * 0.3f;
			SquishScale.x += (SquishScaleSpeed.x *= 0.7f);
			SquishScaleSpeed.y += (1f - SquishScale.y) * 0.3f;
			SquishScale.y += (SquishScaleSpeed.y *= 0.7f);
			if (Time.timeScale > 0f)
			{
				base.transform.localScale = new Vector3(Scale * SquishScale.x, Scale * SquishScale.y, Scale);
			}
			container.transform.localPosition = new Vector3(0f, 0f, childZ);
			Bounce();
		}
		base.transform.localScale = Vector3.one * (1f + Mathf.PingPong(Time.time * 2f, 0.3f) - 0.15f);
	}

	private void OnDisable()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void FixedUpdate()
	{
		if (!(rigidbody == null))
		{
			speed += (0f - speed) / 12f * GameManager.FixedDeltaTime;
			vx = speed * Mathf.Cos(facingAngle * ((float)Math.PI / 180f));
			vy = speed * Mathf.Sin(facingAngle * ((float)Math.PI / 180f));
			rigidbody.MovePosition(rigidbody.position + new Vector2(vx, vy) * Time.fixedDeltaTime);
		}
	}

	private void Bounce()
	{
		if (childZ > 0f)
		{
			if (vz > 0.08f)
			{
				vz *= -0.4f;
				SquishScale = new Vector2(0.8f, 1.2f);
			}
			else
			{
				vz = 0f;
			}
			childZ = 0f;
		}
		else
		{
			vz += 0.02f;
		}
		childZ += vz;
		container.transform.localPosition = new Vector3(0f, 0f, childZ);
	}
}
