using UnityEngine;

public class ChainConnection : BaseMonoBehaviour
{
	public void UpdatePosition(Vector3 Position1, Vector3 Position2)
	{
		base.transform.position = Position1;
		base.transform.eulerAngles = new Vector3(-60f, 0f, Vector2.Angle(Position1, Position2) * 57.29578f);
	}
}
