using UnityEngine;

public class ChainLink : BaseMonoBehaviour
{
	public float xSpeed;

	private float xTension = 0.1f;

	private float xDampening = 0.3f;

	public float ySpeed;

	private float yTension = 0.1f;

	private float yDampening = 0.3f;

	public float zSpeed;

	private float zTension = 0.1f;

	private float zDampening = 0.3f;

	private float grav = 0.01f;

	public float x
	{
		get
		{
			return base.transform.position.x;
		}
		set
		{
			Vector3 position = new Vector3(value, base.transform.position.y, base.transform.position.z);
			base.transform.position = position;
		}
	}

	public float y
	{
		get
		{
			return base.transform.position.y;
		}
		set
		{
			Vector3 position = new Vector3(base.transform.position.x, value, base.transform.position.z);
			base.transform.position = position;
		}
	}

	public float z
	{
		get
		{
			return base.transform.position.z;
		}
		set
		{
			Vector3 position = new Vector3(base.transform.position.x, base.transform.position.y, value);
			base.transform.position = position;
		}
	}

	public void Init(float x, float y, float z)
	{
		base.transform.position = new Vector3(x, y, z);
	}

	public void UpdatePositions(Vector3 TargetPosition)
	{
		float num = base.transform.position.y - TargetPosition.y;
		ySpeed += yTension * num - ySpeed * yDampening;
		y -= ySpeed;
		float num2 = x - TargetPosition.x;
		xSpeed += xTension * num2 - xSpeed * xDampening;
		x -= xSpeed;
		float num3 = z - TargetPosition.z;
		zSpeed += zTension * num3 - zSpeed * xDampening;
		z -= zSpeed - grav;
		base.transform.position = new Vector3(x, y, z);
	}
}
