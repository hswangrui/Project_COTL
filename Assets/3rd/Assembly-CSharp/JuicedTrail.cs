using Spine.Unity;
using UnityEngine;

public class JuicedTrail : MonoBehaviour
{
	public SimpleSpineDeactivateAfterPlay SimpleSpineDeactivateAfterPlay;

	public EnemyBurrowingTrail EnemyBurrowingTrail;

	public SkeletonAnimation SkeletonAnimation;

	public ColliderEvents ColliderEvents;

	public void SetContinious(bool continous)
	{
		SimpleSpineDeactivateAfterPlay.enabled = !continous;
		EnemyBurrowingTrail.enabled = !continous;
		SkeletonAnimation.AnimationState.GetCurrent(0).Loop = continous;
		EnemyBurrowingTrail.DamageCollider.enabled = continous;
	}

	private void Update()
	{
		if (!EnemyBurrowingTrail.enabled)
		{
			EnemyBurrowingTrail.DamageCollider.enabled = true;
		}
	}
}
