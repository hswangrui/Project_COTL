using System.Collections;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BreakableSpiderNest : BaseMonoBehaviour
{
	[SerializeField]
	private SkeletonAnimation spine;

	[SerializeField]
	private float duration;

	[SerializeField]
	public AssetReferenceGameObject[] enemiesList;

	[SerializeField]
	private Vector2 amount;

	[SerializeField]
	private AnimationCurve heightCurve;

	[SerializeField]
	private AnimationCurve moveCurve;

	[SerializeField]
	private float height;

	[SerializeField]
	private float radius;

	[SerializeField]
	private float popOutDuration;

	[SerializeField]
	private float growDuration;

	[SerializeField]
	private string spawnAnimation = "";

	[SerializeField]
	private GameObject spawnParticle;

	[SerializeField]
	private Renderer renderer;

	[SerializeField]
	private LayerMask layersToCheck;

	private float spawnTimestamp = -1f;

	private ShowHPBar hpBar;

	private Health health;

	private Color originalColor;

	private bool spawned;

	private void OnEnable()
	{
		health = GetComponent<Health>();
		hpBar = GetComponent<ShowHPBar>();
		health.OnHit += OnHit;
		health.OnDie += OnDie;
		originalColor = renderer.material.color;
		StartCoroutine(Sub());
	}

	private IEnumerator Sub()
	{
		while (spine.AnimationState == null)
		{
			yield return null;
		}
		spine.AnimationState.Event += AnimationState_Event;
	}

	private void OnDisable()
	{
		if (spine.AnimationState != null)
		{
			spine.AnimationState.Event -= AnimationState_Event;
		}
	}

	private void AnimationState_Event(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "break")
		{
			AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile");
		}
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		StartCoroutine(HitIE());
	}

	private IEnumerator HitIE()
	{
		renderer.material.color = Color.red;
		yield return new WaitForSeconds(0.1f);
		renderer.material.color = originalColor;
	}

	private void Update()
	{
		if (PlayerRelic.TimeFrozen)
		{
			if (spine != null)
			{
				spine.timeScale = 0.0001f;
			}
			return;
		}
		if (spine != null)
		{
			spine.timeScale = 1f;
		}
		if (spawnTimestamp == -1f)
		{
			if (GameManager.RoomActive)
			{
				spawnTimestamp = (GameManager.GetInstance() ? (GameManager.GetInstance().CurrentTime + duration) : (Time.time + duration));
			}
			return;
		}
		GameManager instance = GameManager.GetInstance();
		if ((((object)instance != null) ? new float?(instance.CurrentTime) : null) > spawnTimestamp)
		{
			SpawnEnemies();
		}
	}

	private void SpawnEnemies()
	{
		if (spawned)
		{
			return;
		}
		spawned = true;
		AudioManager.Instance.PlayOneShot("event:/boss/frog/tongue_impact", base.gameObject);
		int num = (int)Random.Range(amount.x, amount.y + 1f);
		for (int i = 0; i < num; i++)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(enemiesList[Random.Range(0, enemiesList.Length)], base.transform.position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				EnemySpider component = obj.Result.GetComponent<EnemySpider>();
				Interaction_Chest instance = Interaction_Chest.Instance;
				if ((object)instance != null)
				{
					instance.AddEnemy(component.health);
				}
				EnemyRoundsBase instance2 = EnemyRoundsBase.Instance;
				if ((object)instance2 != null)
				{
					instance2.AddEnemyToRound(component.health);
				}
				component.health.CanIncreaseDamageMultiplier = health.CanIncreaseDamageMultiplier;
				SkeletonAnimation[] componentsInChildren = component.GetComponentsInChildren<SkeletonAnimation>();
				foreach (SkeletonAnimation skeletonAnimation in componentsInChildren)
				{
					if (growDuration != 0f)
					{
						Vector3 localScale = skeletonAnimation.transform.localScale;
						skeletonAnimation.transform.localScale = Vector3.zero;
						skeletonAnimation.transform.DOScale(localScale, growDuration).SetEase(Ease.Linear);
					}
					if (!string.IsNullOrEmpty(spawnAnimation))
					{
						skeletonAnimation.AnimationState.SetAnimation(0, spawnAnimation, false);
						skeletonAnimation.AnimationState.AddAnimation(0, "idle", true, 0f);
					}
				}
				component.StartCoroutine(SpawnAnimIE(component));
				component.StartCoroutine(DelayedEnemyHealthEnable(component));
				Object.Instantiate(spawnParticle, base.transform.position, Quaternion.identity);
				hpBar.DestroyHPBar();
			};
		}
		health.DealDamage(health.totalHP, base.gameObject, base.transform.position);
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		base.gameObject.SetActive(false);
		Bounds bounds = GetComponent<Collider2D>().bounds;
		AstarPath.active.UpdateGraphs(bounds);
	}

	private IEnumerator SpawnAnimIE(EnemySpider enemy)
	{
		Vector3 fromPosition = enemy.transform.position;
		Vector3 targetPosition = fromPosition + (Vector3)Random.insideUnitCircle * radius;
		float startTime = GameManager.GetInstance().CurrentTime;
		Vector3 normalized = (targetPosition - fromPosition).normalized;
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(fromPosition, radius, normalized, 0f, layersToCheck);
		if ((bool)raycastHit2D.collider && Vector3.Dot((Vector3)raycastHit2D.point - fromPosition, normalized) > 0f)
		{
			targetPosition = fromPosition + (fromPosition - (Vector3)raycastHit2D.point).normalized * radius;
		}
		float t = 0f;
		while (t < popOutDuration)
		{
			float time = GameManager.GetInstance().TimeSince(startTime) / popOutDuration;
			if (enemy.Spine.transform.parent == enemy.transform)
			{
				enemy.Spine.transform.localPosition = -Vector3.forward * heightCurve.Evaluate(time) * height;
			}
			else
			{
				enemy.Spine.transform.position = enemy.transform.TransformPoint(-Vector3.forward * heightCurve.Evaluate(time) * height);
			}
			enemy.transform.position = Vector3.Lerp(fromPosition, targetPosition, moveCurve.Evaluate(time));
			t += Time.deltaTime;
			yield return null;
			if (enemy.Spine.transform.parent == enemy.transform)
			{
				enemy.Spine.transform.localPosition = Vector3.zero;
			}
			else
			{
				enemy.Spine.transform.position = enemy.transform.TransformPoint(Vector3.zero);
			}
		}
		enemy.transform.position = enemy.Spine.transform.position;
		enemy.Spine.transform.localPosition = Vector3.zero;
	}

	private IEnumerator DelayedEnemyHealthEnable(UnitObject enemy)
	{
		enemy.health.enabled = false;
		yield return new WaitForSeconds(0.5f);
		enemy.health.enabled = true;
		Collider2D[] array = Physics2D.OverlapCircleAll(enemy.transform.position, 0.5f);
		foreach (Collider2D collider2D in array)
		{
			Health component = collider2D.GetComponent<Health>();
			if (component != null && component.team == Health.Team.Neutral)
			{
				collider2D.GetComponent<Health>().DealDamage(2.1474836E+09f, enemy.gameObject, Vector3.Lerp(component.transform.position, enemy.transform.position, 0.7f));
			}
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, radius, Color.red);
	}
}
