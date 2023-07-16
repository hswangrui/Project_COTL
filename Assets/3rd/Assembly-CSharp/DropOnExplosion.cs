using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DropOnExplosion : MonoBehaviour
{
	public EnemyExploder EnemyExploder;

	[SerializeField]
	private GameObject poisonPrefab;

	[SerializeField]
	private int amount = 3;

	[SerializeField]
	private float radius = 1f;

	public AssetReferenceGameObject[] EnemyList;

	public int NumToSpawn = 5;

	[SerializeField]
	private float growSpeed = 0.3f;

	[SerializeField]
	private Ease growEase = Ease.OutCubic;

	[SerializeField]
	private float spawnSpitOutForce = 0.7f;

	private void OnEnable()
	{
		EnemyExploder enemyExploder = EnemyExploder;
		enemyExploder.OnExplode = (Action)Delegate.Combine(enemyExploder.OnExplode, new Action(OnExplode));
	}

	private void OnDisable()
	{
		EnemyExploder enemyExploder = EnemyExploder;
		enemyExploder.OnExplode = (Action)Delegate.Remove(enemyExploder.OnExplode, new Action(OnExplode));
	}

	private void OnExplode()
	{
		for (int i = 0; i < amount; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * radius;
			UnityEngine.Object.Instantiate(poisonPrefab, base.transform.position + (Vector3)vector, Quaternion.identity, base.transform.parent);
		}
		for (int j = 0; j < NumToSpawn; j++)
		{
			Health.team2.Add(null);
			Interaction_Chest instance = Interaction_Chest.Instance;
			if ((object)instance != null)
			{
				instance.Enemies.Add(null);
			}
			Vector3 position = base.transform.position;
			float f = 360f / (float)NumToSpawn * (float)j * ((float)Math.PI / 180f);
			Vector3 direction = new Vector3(Mathf.Cos(f), Mathf.Sin(f));
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(EnemyList[UnityEngine.Random.Range(0, EnemyList.Length)], position, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				if (Health.team2.Contains(null))
				{
					Health.team2.Remove(null);
					Interaction_Chest instance2 = Interaction_Chest.Instance;
					if ((object)instance2 != null)
					{
						instance2.Enemies.Remove(null);
					}
				}
				EnemyExploder component = obj.Result.GetComponent<EnemyExploder>();
				component.givePath(component.transform.position + direction * 2f);
				EnemyRoundsBase instance3 = EnemyRoundsBase.Instance;
				if ((object)instance3 != null)
				{
					instance3.AddEnemyToRound(component.GetComponent<Health>());
				}
				Interaction_Chest instance4 = Interaction_Chest.Instance;
				if ((object)instance4 != null)
				{
					instance4.AddEnemy(obj.Result.GetComponent<Health>());
				}
				if (growSpeed != 0f)
				{
					component.Spine.transform.localScale = Vector3.zero;
					component.Spine.transform.DOScale(1f, growSpeed).SetEase(growEase);
				}
				float angle = Utils.GetAngle(base.transform.position, base.transform.position + direction) * ((float)Math.PI / 180f);
				component.DoKnockBack(angle, spawnSpitOutForce, 0.75f);
				component.chase = true;
				DropLootOnDeath component2 = component.GetComponent<DropLootOnDeath>();
				if ((bool)component2)
				{
					component2.GiveXP = false;
				}
			};
		}
	}
}
