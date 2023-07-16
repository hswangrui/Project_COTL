using System.Collections;
using Spine.Unity;
using UnityEngine;

public class EnemyShootGrenadeBullet : BaseMonoBehaviour
{
	public enum ShootDirectionMode
	{
		Player,
		Looking,
		Facing
	}

	public GameObject Prefab;

	public SimpleSpineFlash SimpleSpineFlash;

	public bool SpineAnimations;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string ShootAnimation;

	[SpineAnimation("", "", true, false, dataField = "Spine")]
	public string AfterShootAnimation;

	public bool ShootFromGameObject;

	public Transform ShootFromGameObjectTransform;

	public float WaitBetweenShooting = 5f;

	public Vector2 DelayBetweenShots = new Vector2(0.1f, 0.3f);

	public float NumberOfShotsToFire = 5f;

	public Vector2 DistanceRange = new Vector2(2f, 3f);

	public float DistanceFromPlayerToFire = 5f;

	public float GravSpeed = -15f;

	public float AnticipationTime;

	public ShootDirectionMode ShootDirection;

	public float Arc = 360f;

	public Vector2 RandomArcOffset = new Vector2(0f, 0f);

	private StateMachine state;

	private GameObject g;

	private GrenadeBullet GrenadeBullet;

	private bool Shooting;

	private float CacheShootDirectionCache;

	private float Direction
	{
		get
		{
			switch (ShootDirection)
			{
			case ShootDirectionMode.Player:
				if (!(PlayerFarming.Instance == null))
				{
					return Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position);
				}
				return 0f;
			case ShootDirectionMode.Facing:
				return state.facingAngle;
			case ShootDirectionMode.Looking:
				return state.LookAngle;
			default:
				return 0f;
			}
		}
	}

	private void OnEnable()
	{
		state = GetComponent<StateMachine>();
		Shooting = false;
	}

	private void Update()
	{
		if (Time.frameCount % 10 == 0 && !Shooting)
		{
			StateMachine.State cURRENT_STATE = state.CURRENT_STATE;
			if ((uint)cURRENT_STATE <= 1u && PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < DistanceFromPlayerToFire)
			{
				StartCoroutine(Shoot());
			}
		}
	}

	private IEnumerator Shoot()
	{
		Shooting = true;
		if (AnticipationTime > 0f)
		{
			float Progress = 0f;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.deltaTime);
				if (!(num < AnticipationTime))
				{
					break;
				}
				SimpleSpineFlash.FlashWhite(Progress / AnticipationTime);
				yield return null;
			}
			SimpleSpineFlash.FlashWhite(false);
		}
		if (SpineAnimations)
		{
			Spine.AnimationState.SetAnimation(0, ShootAnimation, false);
			Spine.AnimationState.AddAnimation(0, AfterShootAnimation, true, 0f);
		}
		CacheShootDirectionCache = Direction;
		int i = -1;
		while (true)
		{
			int num2 = i + 1;
			i = num2;
			if (!((float)num2 < NumberOfShotsToFire))
			{
				break;
			}
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.transform.position);
			Vector3 position = (ShootFromGameObject ? ShootFromGameObjectTransform.position : base.transform.position);
			position.z = 0f;
			GrenadeBullet = ObjectPool.Spawn(Prefab, position, Quaternion.identity).GetComponent<GrenadeBullet>();
			GrenadeBullet.Play(-1f, CacheShootDirectionCache - Arc / 2f + Arc / NumberOfShotsToFire * (float)i + Random.Range(RandomArcOffset.x, RandomArcOffset.y), Random.Range(DistanceRange.x, DistanceRange.y), GravSpeed);
			yield return new WaitForSeconds(Random.Range(DelayBetweenShots.x, DelayBetweenShots.y));
		}
		yield return new WaitForSeconds(WaitBetweenShooting);
		Shooting = false;
	}
}
