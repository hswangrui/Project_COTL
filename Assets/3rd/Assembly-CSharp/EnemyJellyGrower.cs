using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EnemyJellyGrower : EnemyExploder
{
	[SerializeField]
	private float increaseAmount;

	[SerializeField]
	private Vector2 growInterval;

	[SerializeField]
	private float decreasePerHit;

	[SerializeField]
	private float maxGrowSize;

	[SerializeField]
	private float anticipationDuration = 0.5f;

	public GameObject Prefab;

	public Vector2 DelayBetweenShots = new Vector2(0.1f, 0.3f);

	public float NumberOfShotsToFire = 5f;

	public float GravSpeed = -15f;

	public Vector2 RandomArcOffset = new Vector2(0f, 0f);

	public Vector2 ShootDistanceRange = new Vector2(2f, 3f);

	public Vector3 ShootOffset;

	private GameObject g;

	private GrenadeBullet GrenadeBullet;

	private bool killed;

	private bool anticipating;

	private float anticipateTimer;

	private float growTimestamp = -1f;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!inRange)
		{
			return;
		}
		if (growTimestamp == -1f)
		{
			growTimestamp = gm.CurrentTime + Random.Range(growInterval.x, growInterval.y);
		}
		if (!anticipating && gm.CurrentTime > growTimestamp)
		{
			Grow();
		}
		if (anticipating)
		{
			anticipateTimer += Time.deltaTime;
			float num = anticipateTimer / anticipationDuration;
			simpleSpineFlash.FlashWhite(num);
			if (num >= 1f)
			{
				Pop();
			}
		}
	}

	public override void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		base.OnHit(Attacker, AttackLocation, AttackType, FromBehind);
		Spine.transform.localScale -= Vector3.one * decreasePerHit;
		Spine.transform.localPosition = new Vector3(Spine.transform.localPosition.x, Spine.transform.localPosition.y, Spine.transform.localScale.x / maxGrowSize);
		growTimestamp = gm.CurrentTime + Random.Range(growInterval.x, growInterval.y);
		if (Spine.transform.localScale.x < 1f && !killed)
		{
			killed = true;
			health.DealDamage(health.totalHP, Attacker, AttackLocation, false, Health.AttackTypes.Heavy);
		}
	}

	private void Grow()
	{
		if (Spine.transform.localScale.x >= maxGrowSize)
		{
			anticipating = true;
			anticipateTimer = 0f;
			return;
		}
		Spine.transform.localScale += Vector3.one * increaseAmount;
		Spine.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f).SetEase(Ease.OutBounce);
		Spine.transform.DOLocalMove(new Vector3(Spine.transform.localPosition.x, Spine.transform.localPosition.y, Spine.transform.localScale.x / maxGrowSize), 0.25f);
		anticipating = false;
		simpleSpineFlash.FlashWhite(false);
		growTimestamp = gm.CurrentTime + Random.Range(growInterval.x, growInterval.y);
	}

	private void Pop()
	{
		anticipating = false;
		StartCoroutine(ShootRoutine());
		health.DealDamage(health.totalHP, base.gameObject, base.transform.position, false, Health.AttackTypes.Heavy);
	}

	private IEnumerator ShootRoutine()
	{
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.4f, 0.2f);
		float randomStartAngle = Random.Range(0, 360);
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if ((float)num < NumberOfShotsToFire)
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", base.transform.position);
				float angle = randomStartAngle + Random.Range(RandomArcOffset.x, RandomArcOffset.y);
				GrenadeBullet = ObjectPool.Spawn(Prefab, base.transform.parent, base.transform.position + ShootOffset, Quaternion.identity).GetComponent<GrenadeBullet>();
				GrenadeBullet.Play(-1f, angle, Random.Range(ShootDistanceRange.x, ShootDistanceRange.y), Random.Range(GravSpeed - 0.5f, GravSpeed + 0.5f), health.team);
				randomStartAngle = Mathf.Repeat(randomStartAngle + 360f / NumberOfShotsToFire, 360f);
				if (DelayBetweenShots != Vector2.zero)
				{
					yield return new WaitForSeconds(Random.Range(DelayBetweenShots.x, DelayBetweenShots.y));
				}
				continue;
			}
			break;
		}
	}
}
