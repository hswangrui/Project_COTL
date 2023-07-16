using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ThrownDagger : MonoBehaviour
{
	public GameObject AxeContainer;

	public SpriteRenderer SpriteRenderer;

	public SpriteRenderer SpriteRendererHeavy;

	public SpriteRenderer ShadowSpriteRenderer;

	public Vector3 StartingPosition;

	public float ScaleTime = 0.5f;

	public float WaitScaleTime = 0.5f;

	public float MoveTime = 0.5f;

	public float LifeTimeDuration = 0.5f;

	public float ShrinkTime = 0.5f;

	public Vector2 ShakeIntensity = new Vector2(1f, 1.2f);

	public Ease ThrowEase = Ease.InBack;

	private float Damage = 1f;

	public Collider2D DamageCollider;

	private List<Collider2D> collider2DList;

	private Health CollisionHealth;

	public static void SpawnThrownDagger(Vector3 position, float Damage, float Delay, Sprite DaggerImage)
	{
		GameManager.GetInstance().StartCoroutine(DelayCallback(Delay, delegate
		{
			SpawnThrownDagger(position, Damage, DaggerImage);
		}));
	}

	private static IEnumerator DelayCallback(float delay, Action callback)
	{
		yield return new WaitForSeconds(delay);
		if (callback != null)
		{
			callback();
		}
	}

	public static void SpawnThrownDagger(Vector3 position, float Damage, Sprite DaggerImage)
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Thrown Dagger.prefab", position, Quaternion.identity, BiomeGenerator.Instance.CurrentRoom.generateRoom.transform);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			ThrownDagger component = obj.Result.GetComponent<ThrownDagger>();
			component.Damage = Damage;
			component.Throw(DaggerImage);
		};
	}

	private void OnEnable()
	{
		SpriteRenderer.enabled = false;
		SpriteRendererHeavy.enabled = false;
		ShadowSpriteRenderer.enabled = false;
	}

	private void Throw(Sprite DaggerImage)
	{
		StartCoroutine(ThrowRoutine(DaggerImage));
	}

	private IEnumerator ThrowRoutine(Sprite DaggerImage)
	{
		AxeContainer.transform.localPosition = StartingPosition;
		AxeContainer.transform.localScale = Vector3.zero;
		AxeContainer.transform.DOScale(Vector3.one, ScaleTime).SetEase(Ease.OutBack);
		AudioManager.Instance.PlayOneShot("event:/enemy/chaser_boss/chaser_boss_egg_spawn", base.gameObject);
		if (DaggerImage != null)
		{
			SpriteRenderer.enabled = true;
			SpriteRendererHeavy.enabled = false;
			SpriteRenderer.sprite = DaggerImage;
		}
		else
		{
			SpriteRenderer.enabled = false;
			SpriteRendererHeavy.enabled = true;
		}
		float x = ShadowSpriteRenderer.transform.localScale.x;
		ShadowSpriteRenderer.transform.localScale = Vector3.zero;
		ShadowSpriteRenderer.enabled = true;
		ShadowSpriteRenderer.transform.DOScale(x, ScaleTime).SetEase(Ease.OutBack);
		yield return new WaitForSeconds(WaitScaleTime);
		AudioManager.Instance.PlayOneShot("event:/weapon/melee_swing_fast", base.gameObject);
		AxeContainer.transform.DOLocalMove(Vector3.zero, MoveTime).SetEase(ThrowEase);
		yield return new WaitForSeconds(MoveTime);
		CameraManager.instance.ShakeCameraForDuration(ShakeIntensity.x, ShakeIntensity.y, 0.3f);
		SpriteRenderer.transform.localScale = new Vector3(1.5f, 0.5f);
		SpriteRenderer.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
		AudioManager.Instance.PlayOneShot("event:/enemy/impact_normal", base.gameObject);
		collider2DList = new List<Collider2D>();
		DamageCollider.GetContacts(collider2DList);
		foreach (Collider2D collider2D in collider2DList)
		{
			CollisionHealth = collider2D.gameObject.GetComponent<Health>();
			if (CollisionHealth != null && !CollisionHealth.invincible && !CollisionHealth.untouchable && (CollisionHealth.team != Health.Team.PlayerTeam || CollisionHealth.IsCharmedEnemy))
			{
				Health.AttackTypes attackType = Health.AttackTypes.Projectile;
				if (CollisionHealth.HasShield)
				{
					attackType = Health.AttackTypes.Heavy;
				}
				CollisionHealth.DealDamage(Damage, base.gameObject, base.transform.position, false, attackType, false, Health.AttackFlags.Penetration);
			}
		}
		yield return new WaitForSeconds(LifeTimeDuration);
		AxeContainer.transform.DOScale(Vector3.zero, ShrinkTime).SetEase(Ease.InBack);
		ShadowSpriteRenderer.transform.DOScale(Vector3.zero, ShrinkTime).SetEase(Ease.InBack);
		yield return new WaitForSeconds(ShrinkTime);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
