using Spine.Unity;
using UnityEngine;

public class PlayerVFXManager : BaseMonoBehaviour
{
	public SimpleSFX RedThunderStrike;

	public GameObject chargingParticles;

	public SkeletonRenderer playerSpine;

	public StateMachine state;

	public void EmitGhostTwirlAttack()
	{
		stopEmitChargingParticles();
	}

	public void emitChargingParticles()
	{
		chargingParticles.SetActive(true);
	}

	public void stopEmitChargingParticles()
	{
		chargingParticles.SetActive(false);
	}
}
