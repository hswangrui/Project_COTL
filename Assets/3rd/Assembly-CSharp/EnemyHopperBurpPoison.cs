using System.Collections;
using UnityEngine;

public class EnemyHopperBurpPoison : EnemyHopperBurp
{
	[SerializeField]
	private PoisonBomb poisonBomb;

	[SerializeField]
	private float bombDuration;

	protected override IEnumerator ShootProjectileRoutine()
	{
		CameraManager.shakeCamera(0.2f, LookAngle);
		Object.Instantiate(poisonBomb, targetObject.transform.position, Quaternion.identity, base.transform.parent).Play(base.transform.position, bombDuration);
		yield return new WaitForEndOfFrame();
	}
}
