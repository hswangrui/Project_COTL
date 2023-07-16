using System.Collections;
using Spine.Unity;
using UnityEngine;

public class ProjectilePatternBase : BaseMonoBehaviour
{
	public delegate void ProjectileEvent();

	[SerializeField]
	private string key;

	protected SkeletonAnimation spine;

	protected float timeScale
	{
		get
		{
			if (spine != null)
			{
				return spine.timeScale;
			}
			return 1f;
		}
	}

	public static event ProjectileEvent OnProjectileSpawned;

	private void Awake()
	{
		spine = GetComponentInChildren<SkeletonAnimation>();
	}

	public virtual IEnumerator ShootIE(float delay = 0f, GameObject target = null, Transform parent = null)
	{
		yield break;
	}

	protected void SpawnedProjectile()
	{
		ProjectileEvent onProjectileSpawned = ProjectilePatternBase.OnProjectileSpawned;
		if (onProjectileSpawned != null)
		{
			onProjectileSpawned();
		}
	}
}
