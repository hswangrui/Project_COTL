using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class SpiderBomb : EnemyBomb
{
	[Space]
	[SerializeField]
	private EnemySpider[] spiders;

	[SerializeField]
	private Vector2 spawnAmount;

	[SerializeField]
	private float spawnRadius;

	[SerializeField]
	private float popOutDuration;

	[SerializeField]
	private float height;

	[SerializeField]
	private float growDuration;

	[SerializeField]
	private AnimationCurve heightCurve;

	[SerializeField]
	private AnimationCurve moveCurve;

	[SerializeField]
	private string spawnAnimation = "spawn";

	[SerializeField]
	private LayerMask layersToCheck;

	[Space]
	[SerializeField]
	private GameObject additionalObjectToSpawn;

	[SerializeField]
	private int additionalAmount;

	[SerializeField]
	private float additionalRadius;

	protected override void BombLanded()
	{
		AudioManager.Instance.PlayOneShot("event:/fishing/splash", base.gameObject);
		int num = Random.Range((int)spawnAmount.x, (int)spawnAmount.y + 1);
		for (int i = 0; i < num; i++)
		{
			EnemySpider enemySpider = Object.Instantiate(spiders[Random.Range(0, spiders.Length)], base.transform.position, Quaternion.identity, base.transform.parent);
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.AddEnemy(enemySpider.health);
			}
			SkeletonAnimation[] componentsInChildren = enemySpider.GetComponentsInChildren<SkeletonAnimation>();
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
			enemySpider.StartCoroutine(SpawnAnimIE(enemySpider));
			enemySpider.StartCoroutine(DelayedEnemyHealthEnable(enemySpider));
		}
		for (int k = 0; k < additionalAmount; k++)
		{
			Object.Instantiate(additionalObjectToSpawn, base.transform.position + (Vector3)Random.insideUnitCircle * additionalRadius, Quaternion.identity, base.transform.parent);
		}
	}

	private IEnumerator SpawnAnimIE(EnemySpider enemy)
	{
		Vector3 fromPosition = enemy.transform.position;
		Vector3 targetPosition = fromPosition + (Vector3)Random.insideUnitCircle * spawnRadius;
		float startTime = GameManager.GetInstance().CurrentTime;
		Vector3 normalized = (targetPosition - fromPosition).normalized;
		RaycastHit2D raycastHit2D = Physics2D.CircleCast(fromPosition, spawnRadius, normalized, 0f, layersToCheck);
		if ((bool)raycastHit2D.collider && Vector3.Dot((Vector3)raycastHit2D.point - fromPosition, normalized) > 0f)
		{
			targetPosition = fromPosition + (fromPosition - (Vector3)raycastHit2D.point).normalized * spawnRadius;
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
		}
		if (enemy.Spine.transform.parent == enemy.transform)
		{
			enemy.Spine.transform.localPosition = Vector3.zero;
		}
		else
		{
			enemy.Spine.transform.position = enemy.transform.TransformPoint(Vector3.zero);
		}
	}

	private IEnumerator DelayedEnemyHealthEnable(UnitObject enemy)
	{
		enemy.health.enabled = false;
		yield return new WaitForSeconds(0.5f);
		enemy.health.enabled = true;
	}
}
