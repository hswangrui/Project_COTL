using UnityEngine;

public class Pet : UnitObject
{
	public GameObject Owner;

	private Vector3 FollowPosition;

	private float Delay = 2f;

	public float FollowDistance = 2f;

	private float LookAround;

	private float FacingAngle;

	private void Start()
	{
		Delay = 2f + Random.Range(0f, 1f);
	}

	public override void Update()
	{
		base.Update();
		switch (state.CURRENT_STATE)
		{
		case StateMachine.State.Idle:
			state.facingAngle = FacingAngle + 45f * Mathf.Cos(LookAround += 0.03f) * GameManager.DeltaTime;
			if (Owner != null && (Vector3.Distance(Owner.transform.position, base.transform.position) > 3f || (Delay -= Time.deltaTime) < 0f))
			{
				NewPosition();
				givePath(FollowPosition);
			}
			break;
		case StateMachine.State.Moving:
			Delay = Random.Range(3, 10);
			FacingAngle = state.facingAngle;
			LookAround = 90f;
			break;
		}
	}

	private void NewPosition()
	{
		if (!(Owner == null))
		{
			Vector2 vector = Random.insideUnitCircle * 2f;
			FollowPosition = Owner.transform.position + new Vector3(vector.x, vector.y, 0f);
			FollowPosition = (Vector3)AstarPath.active.GetNearest(FollowPosition, UnitObject.constraint).node.position;
		}
	}
}
