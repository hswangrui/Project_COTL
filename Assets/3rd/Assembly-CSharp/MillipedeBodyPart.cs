using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MillipedeBodyPart : BaseMonoBehaviour
{
	private static List<MillipedeBodyPart> bodyParts = new List<MillipedeBodyPart>();

	[SerializeField]
	private SkeletonAnimation Spine;

	private EnemyMillipede millipede;

	private Health health;

	private bool dropped;

	private void Awake()
	{
		millipede = GetComponentInParent<EnemyMillipede>();
		health = GetComponent<Health>();
		health.OnDamaged += OnDamaged;
	}

	private void OnDestroy()
	{
		if ((bool)health)
		{
			health.OnDamaged -= OnDamaged;
		}
		bodyParts.Remove(this);
	}

	private void OnDamaged(GameObject attacker, Vector3 attackLocation, float damage, Health.AttackTypes attackType, Health.AttackFlags attackFlag)
	{
		if (attacker != millipede.gameObject && !dropped)
		{
			millipede.DamageFromBody(attacker, attackLocation, damage, attackType, attackFlag);
		}
	}

	public void DroppedPart()
	{
		base.transform.localScale = Vector3.one;
		bodyParts.Add(this);
		dropped = true;
	}

	public void SpawnEnemy(AssetReferenceGameObject[] enemies, Transform parent)
	{
		StartCoroutine(SpawnIE(enemies, parent));
	}

	private IEnumerator SpawnIE(AssetReferenceGameObject[] enemies, Transform parent)
	{
		yield return Spine.YieldForAnimation("transform");
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		for (int i = 0; i < Random.Range(1, 3); i++)
		{
			Addressables.InstantiateAsync(enemies[Random.Range(0, enemies.Length)], base.transform.position, Quaternion.identity, parent);
		}
		base.gameObject.SetActive(false);
	}
}
