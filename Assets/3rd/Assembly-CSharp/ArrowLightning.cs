using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Ara;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ArrowLightning : MonoBehaviour
{
	[SerializeField]
	private AraTrail trail;

	[SerializeField]
	private TrailRenderer lowQualityTrail;

	[SerializeField]
	private Material altMaterial;

	public Health Target { get; private set; }

	public static void CreateProjectiles(float damage, float duration, Health owner, Health target, Vector3 localPosition, Action callback = null, bool useAltMaterial = false)
	{
		CreateProjectiles(damage, duration, owner, target.transform.position, target.transform, localPosition, delegate
		{
			if (target != null)
			{
				target.DealDamage(damage, owner.gameObject, target.transform.position);
			}
			Action action = callback;
			if (action != null)
			{
				action();
			}
		}, useAltMaterial);
	}

	public static void CreateProjectiles(float damage, float duration, Health owner, Vector3 target, Transform parent, Vector3 localPosition, Action callback = null, bool useAltMaterial = false)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/moving_spike_trap/moving_spike_trap_start", target);
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Enemies/Weapons/ArrowLightning.prefab", target, Quaternion.identity, owner.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			if (!(obj.Result == null))
			{
				ArrowLightning arrow = obj.Result.GetComponent<ArrowLightning>();
				arrow.transform.parent = parent;
				arrow.transform.localPosition = localPosition;
				arrow.trail.enabled = true;
				arrow.lowQualityTrail.enabled = false;
				arrow.trail.Clear();
				if (useAltMaterial)
				{
					arrow.trail.materials[0] = arrow.altMaterial;
				}
				Material[] materials = arrow.trail.materials;
				for (int i = 0; i < materials.Length; i++)
				{
					materials[i].DOFade(1f, 0f);
				}
				CameraManager.instance.ShakeCameraForDuration(0.1f, 0.2f, duration);
				arrow.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.Linear).OnComplete(delegate
				{
					if (arrow != null)
					{
						CameraManager.instance.ShakeCameraForDuration(1f, 1.25f, 0.15f);
						BiomeConstants.Instance.EmitSmokeExplosionVFX(arrow.transform.position);
						BiomeConstants.Instance.EmitGroundSmashVFXParticles(arrow.transform.position, 3f);
						Explosion.CreateExplosion(arrow.transform.position, Health.Team.PlayerTeam, owner, 2f, -1f, damage);
						if (arrow.trail != null)
						{
							arrow.StartCoroutine(_003CCreateProjectiles_003Eg__Delay_007C8_2(1f, delegate
							{
								if (arrow != null)
								{
									arrow.trail.emit = false;
									Material[] materials2 = arrow.trail.materials;
									for (int j = 0; j < materials2.Length; j++)
									{
										materials2[j].DOFade(0f, 0.5f);
									}
								}
							}));
						}
						arrow.transform.parent = null;
						UnityEngine.Object.Destroy(arrow.gameObject, 2f);
					}
					Action action = callback;
					if (action != null)
					{
						action();
					}
				});
			}
		};
	}

	[CompilerGenerated]
	internal static IEnumerator _003CCreateProjectiles_003Eg__Delay_007C8_2(float delay, Action c)
	{
		yield return new WaitForSeconds(delay);
		if (c != null)
		{
			c();
		}
	}
}
