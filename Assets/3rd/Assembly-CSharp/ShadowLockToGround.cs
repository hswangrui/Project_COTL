using UnityEngine;

public class ShadowLockToGround : BaseMonoBehaviour
{
	private Vector3 Position;

	public Transform LockXToObject;

	private void LateUpdate()
	{
		Position = base.transform.position;
		Position.z = 0f;
		if (LockXToObject != null)
		{
			Position.x = LockXToObject.position.x;
		}
		base.transform.position = Position;
	}
}
