using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

public class BellTower : Interaction
{
	public UnityEvent OnHitCallback;

	private Health health;

	public GameObject Bell;

	private float BellSpeed;

	private float zRotation;

	[Range(0.1f, 0.9f)]
	public float Elastic = 0.1f;

	[Range(0.1f, 0.9f)]
	public float Friction = 0.9f;

	public float ImpactForce = 20f;

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.DinnerBell;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Vector3 position = base.transform.position;
		AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", position);
		Vector3 position2 = PlayerFarming.Instance.gameObject.transform.position;
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(position, position2));
		BellSpeed = ImpactForce * (float)((position2.x < position.x) ? 1 : (-1));
		if (OnHitCallback != null)
		{
			OnHitCallback.Invoke();
		}
		foreach (Follower item in FollowerManager.FollowersAtLocation(FollowerLocation.Base))
		{
			item.Brain.SetPersonalOverrideTask(FollowerTaskType.EatMeal);
		}
	}

	private new void Update()
	{
		BellSpeed += (0f - zRotation) * Elastic;
		zRotation += (BellSpeed *= Friction);
		Bell.transform.eulerAngles = new Vector3(-60f, 0f, zRotation);
	}
}
