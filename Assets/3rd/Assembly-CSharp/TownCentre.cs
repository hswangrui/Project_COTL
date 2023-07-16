using System.Collections.Generic;
using UnityEngine;

public class TownCentre : BaseMonoBehaviour
{
	public Transform Centre;

	public static TownCentre Instance;

	public float Height;

	public float Width;

	private static Vector3 CachedCentrePos;

	private static float CachedHeight;

	private static float CachedWidth;

	private static List<float> townCentreRandomAngles = new List<float>(numTownCentreRandomAngles);

	private static int numTownCentreRandomAngles = 50;

	private static int townCentreRandomIndex = 0;

	private void Awake()
	{
		Instance = this;
		CachedCentrePos = Centre.position;
		CachedHeight = Height;
		CachedWidth = Width;
		for (int i = 0; i < numTownCentreRandomAngles; i++)
		{
			townCentreRandomAngles.Add(360f / (float)numTownCentreRandomAngles * (float)i);
		}
		int count = townCentreRandomAngles.Count;
		int num = count - 1;
		for (int j = 0; j < num; j++)
		{
			int index = Random.Range(j, count);
			float value = townCentreRandomAngles[j];
			townCentreRandomAngles[j] = townCentreRandomAngles[index];
			townCentreRandomAngles[index] = value;
		}
		townCentreRandomIndex = 0;
	}

	public Vector3 RandomPositionInTownCentre()
	{
		Vector3 vector;
		do
		{
			vector = Centre.position + new Vector3(Random.Range((0f - Width) / 2f, Width / 2f), Random.Range((0f - Height) / 2f, Height / 2f));
		}
		while (Vector3.Distance(vector, Centre.position) < 2f && Width > 0f && Height > 0f);
		return vector;
	}

	public static Vector3 RandomCircleFromTownCentre(float Radius)
	{
		return CachedCentrePos + (Vector3)(Random.insideUnitCircle * Radius);
	}

	public static Vector3 RandomPositionInCachedTownCentre()
	{
		townCentreRandomIndex = (townCentreRandomIndex + 1) % numTownCentreRandomAngles;
		if (townCentreRandomIndex >= townCentreRandomAngles.Count)
		{
			return Vector3.zero;
		}
		return CachedCentrePos + (Vector3)Utils.DegreeToVector2(townCentreRandomAngles[townCentreRandomIndex]) * Random.Range(2f, Mathf.Max(CachedWidth, CachedHeight));
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
		Gizmos.DrawCube(Centre.position, new Vector3(Width, Height, 0f));
	}
}
