using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Explosion : BaseMonoBehaviour, ICurseProduct
{
	public ColliderEvents DamageCollider;

	private Health.Team _team = Health.Team.PlayerTeam;

	private Health Origin;

	private float Damage = -1f;

	private float Team2Damage = -1f;

	public Transform[] FXChildren;

	private bool Init;

	public bool DoKnockback;

	public static GameObject explosionPrefab;

	private float shakeMultiplier;

	private Health.AttackFlags attackFlags;

	private Health EnemyHealth;

	private Health.Team team
	{
		get
		{
			return _team;
		}
		set
		{
			_team = value;
		}
	}

	public static void CreateExplosion(Vector3 position, Health.Team team, Health Origin, float Size, float Damage = -1f, float Team2Damage = -1f, bool useKnockback = false, float shakeMultiplier = 1f, Health.AttackFlags attackFlags = (Health.AttackFlags)0)
	{
		if (explosionPrefab == null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Enemies/Weapons/Explosion.prefab");
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
			{
				explosionPrefab = obj.Result;
				SpawnExplosion(explosionPrefab, position, team, Origin, Size, Damage, Team2Damage, useKnockback, 1f, attackFlags);
			};
		}
		else
		{
			SpawnExplosion(explosionPrefab, position, team, Origin, Size, Damage, Team2Damage, useKnockback, shakeMultiplier, attackFlags);
		}
	}

	private static void SpawnExplosion(GameObject explosionPrefab, Vector3 position, Health.Team team, Health Origin, float Size, float Damage = -1f, float Team2Damage = -1f, bool useKnockback = false, float shakeMultiplier = 1f, Health.AttackFlags attackFlags = (Health.AttackFlags)0)
	{
		GameObject gameObject = ObjectPool.Spawn(explosionPrefab, null, position, Quaternion.identity);
		AudioManager.Instance.PlayOneShot("event:/explosion/explosion", gameObject.transform.position);
		if (team == Health.Team.PlayerTeam)
		{
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		}
		if (team == Health.Team.PlayerTeam)
		{
			MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		}
		gameObject.transform.position = position;
		Explosion component = gameObject.GetComponent<Explosion>();
		component.team = team;
		Transform[] fXChildren = component.FXChildren;
		for (int i = 0; i < fXChildren.Length; i++)
		{
			fXChildren[i].localScale = Vector3.one * (Size / 4f);
		}
		component.Origin = Origin;
		component.Damage = Damage;
		component.Team2Damage = Team2Damage;
		component.DamageCollider.GetComponent<CircleCollider2D>().radius = Size * 0.5f;
		component.DoKnockback = useKnockback;
		component.DamageCollider.OnTriggerEnterEvent += component.DamageCollider_OnTriggerEnterEvent;
		component.DamageCollider.SetActive(false);
		component.shakeMultiplier = shakeMultiplier;
		component.Init = true;
		component.attackFlags = attackFlags;
	}

	public static void CreateExplosionCustomFX(Vector3 position, Health.Team team, Health Origin, float Size, GameObject ExplosionFXPrefab, float Damage = -1f, float Team2Damage = -1f)
	{
		GameObject gameObject = ObjectPool.Spawn(ExplosionFXPrefab);
		AudioManager.Instance.PlayOneShot("event:/explosion/explosion", gameObject.transform.position);
		if (team == Health.Team.PlayerTeam)
		{
			MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		}
		gameObject.transform.position = position;
		Explosion component = gameObject.GetComponent<Explosion>();
		component.team = team;
		Transform[] fXChildren = component.FXChildren;
		for (int i = 0; i < fXChildren.Length; i++)
		{
			fXChildren[i].localScale = Vector3.one * (Size / 4f);
		}
		component.Origin = Origin;
		component.Damage = Damage;
		component.Team2Damage = Team2Damage;
		component.DamageCollider.GetComponent<CircleCollider2D>().radius = Size * 0.5f;
		component.DamageCollider.OnTriggerEnterEvent += component.DamageCollider_OnTriggerEnterEvent;
		component.DamageCollider.SetActive(false);
		component.Init = true;
	}

	private void OnEnable()
	{
		CameraManager.instance.ShakeCameraForDuration(0.5f * shakeMultiplier, 0.6f * shakeMultiplier, 0.3f * shakeMultiplier, false);
		StartCoroutine(DelayDestroy());
	}

	private IEnumerator DelayDestroy()
	{
		while (!Init)
		{
			yield return null;
		}
		if (DamageCollider != null)
		{
			DamageCollider.SetActive(true);
			yield return new WaitForSeconds(0.2f);
			DamageCollider.OnTriggerEnterEvent -= DamageCollider_OnTriggerEnterEvent;
			DamageCollider.SetActive(false);
			yield return new WaitForSeconds(2f);
		}
		Init = false;
		ObjectPool.Recycle(base.gameObject);
	}

	private void DamageCollider_OnTriggerEnterEvent(Collider2D collider)
	{
		if (Damage == -1f)
		{
			Damage = 1f;
		}
		if (Team2Damage == -1f)
		{
			Team2Damage = 3 + DataManager.Instance.GetDungeonNumber(PlayerFarming.Location);
		}
		EnemyHealth = collider.GetComponentInParent<Health>();
		if (EnemyHealth != null && EnemyHealth.team != team && EnemyHealth != Origin)
		{
			EnemyHealth.DealDamage((EnemyHealth.team == Health.Team.Team2) ? Team2Damage : Damage, base.gameObject, Vector3.Lerp(base.transform.position, EnemyHealth.transform.position, 0.7f), false, Health.AttackTypes.Heavy, false, attackFlags);
		}
		if (DoKnockback)
		{
			EnemyHealth.gameObject.GetComponent<UnitObject>().DoKnockBack(collider.gameObject, 1f, 2f);
		}
	}
}
