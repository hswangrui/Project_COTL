using System;
using UnityEngine;

namespace CotL.Projectiles
{
	public abstract class ProjectileCircleBase : BaseMonoBehaviour
	{
		public virtual void Init(float radius)
		{
		}

		public virtual void InitDelayed(GameObject target, float radius, float shootDelay, Action onShoot = null)
		{
		}
	}
}
