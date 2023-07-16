using System;
using UnityEngine;

public class ChildPlacementHelper : BaseMonoBehaviour
{
	public float circleRadius = 2.5f;

	public float circlePlacementOffset;

	public GameObject[] GameObjects;

	private float angle;

	private Vector3 dir;

	public bool FaceTowards = true;

	public bool flipToFaceCenter;

	public float zOffset = 0.1f;

	public float yOffset = 0.1f;

	public float xOffset = 0.1f;

	public Color Color_0;

	public Color Color_1;

	public float zPosMin;

	public float zPosMax = -1f;

	private void GetChildren()
	{
		GameObjects = new GameObject[base.transform.childCount];
		int num = -1;
		while (++num < base.transform.childCount)
		{
			GameObjects[num] = base.transform.GetChild(num).gameObject;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void PutObjectsInCircle()
	{
		int num = -1;
		while (++num < GameObjects.Length)
		{
			float f = (float)num * (360f / (float)GameObjects.Length) * ((float)Math.PI / 180f) + circlePlacementOffset;
			Vector3 position = base.transform.position + new Vector3(circleRadius * Mathf.Cos(f), circleRadius * Mathf.Sin(f));
			GameObjects[num].transform.position = position;
		}
		GameObject[] gameObjects = GameObjects;
		foreach (GameObject gameObject in gameObjects)
		{
			StateMachine component = gameObject.GetComponent<StateMachine>();
			if (!(component != null))
			{
				continue;
			}
			if (FaceTowards)
			{
				Vector3 direction = base.transform.position - gameObject.transform.position;
				direction = base.transform.InverseTransformDirection(direction);
				angle = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
			}
			else
			{
				Vector3 direction2 = gameObject.transform.position - base.transform.position;
				direction2 = gameObject.transform.InverseTransformDirection(direction2);
				angle = Mathf.Atan2(direction2.y, direction2.x) * 57.29578f;
			}
			component.LookAngle = angle;
			component.facingAngle = angle;
			if (!flipToFaceCenter)
			{
				continue;
			}
			if ((angle > 90f && angle < -90f) || (angle > -90f && angle < 90f))
			{
				if (Mathf.Sign(gameObject.transform.localScale.x) == 1f)
				{
					gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
				}
			}
			else if (Mathf.Sign(gameObject.transform.localScale.x) == -1f)
			{
				gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1f, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
			}
		}
	}

	public void SpawnObjectsThroughNoise()
	{
		int num = -1;
		while (++num < GameObjects.Length)
		{
			float f = (float)num * (360f / (float)GameObjects.Length) * ((float)Math.PI / 180f);
			Vector3 vector2 = base.transform.position + new Vector3(circleRadius * Mathf.Cos(f), circleRadius * Mathf.Sin(f));
			Vector2 vector = UnityEngine.Random.insideUnitCircle * circleRadius;
			vector.x += base.transform.position.x;
			vector.y += base.transform.position.y;
			Vector3 position = new Vector3(vector.x, vector.y, base.gameObject.transform.position.z);
			GameObjects[num].transform.position = position;
		}
	}

	public void IncemementZ()
	{
		int num = -1;
		float num2 = GameObjects[0].transform.position.z;
		while (++num < GameObjects.Length)
		{
			GameObjects[num].transform.position = new Vector3(GameObjects[num].transform.position.x, GameObjects[num].transform.position.y, num2);
			num2 += zOffset;
		}
	}

	public void IncemementY()
	{
		int num = -1;
		float num2 = GameObjects[0].transform.position.y;
		while (++num < GameObjects.Length)
		{
			GameObjects[num].transform.position = new Vector3(GameObjects[num].transform.position.x, num2, GameObjects[num].transform.position.z);
			num2 += yOffset;
		}
	}

	public void IncemementX()
	{
		int num = -1;
		float num2 = GameObjects[0].transform.position.x;
		while (++num < GameObjects.Length)
		{
			GameObjects[num].transform.position = new Vector3(num2, GameObjects[num].transform.position.y, GameObjects[num].transform.position.z);
			num2 += xOffset;
		}
	}

	public void Incemement_Color()
	{
		int num = -1;
		float num2 = 0f;
		while (++num < GameObjects.Length)
		{
			SpriteRenderer component = GameObjects[num].gameObject.GetComponent<SpriteRenderer>();
			num2 += 0.1f;
			component.color = Color.Lerp(Color_0, Color_1, num2);
		}
	}

	public void Incemement_Color(Color Color_0, Color Color_1)
	{
		int num = -1;
		float num2 = 0f;
		while (++num < GameObjects.Length)
		{
			SpriteRenderer component = GameObjects[num].gameObject.GetComponent<SpriteRenderer>();
			num2 += 0.1f;
			component.color = Color.Lerp(Color_0, Color_1, num2);
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, circleRadius, Color.green);
	}

	public void RandomZPosition()
	{
		int num = -1;
		while (++num < GameObjects.Length)
		{
			float z = UnityEngine.Random.Range(zPosMin, zPosMax);
			GameObjects[num].transform.position = new Vector3(GameObjects[num].transform.position.x, GameObjects[num].transform.position.y, z);
		}
	}
}
