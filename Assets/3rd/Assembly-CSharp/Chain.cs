using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Chain : BaseMonoBehaviour
{
	public Transform FixedPoint1;

	public Transform FixedPoint2;

	private List<float> DeltasLX = new List<float>();

	private List<float> DeltasRX = new List<float>();

	private List<float> DeltasLY = new List<float>();

	private List<float> DeltasRY = new List<float>();

	private List<float> DeltasLZ = new List<float>();

	private List<float> DeltasRZ = new List<float>();

	private ChainLink[] Links;

	private ChainConnection[] Connections;

	public float xSpread = 0.025f;

	public float ySpread = 0.025f;

	public float zSpread = 0.025f;

	private void Start()
	{
		Links = GetComponentsInChildren<ChainLink>();
		Connections = GetComponentsInChildren<ChainConnection>();
		int num = 0;
		ChainLink[] links = Links;
		foreach (ChainLink chainLink in links)
		{
			DeltasLX.Add(0f);
			DeltasRX.Add(0f);
			DeltasLY.Add(0f);
			DeltasRY.Add(0f);
			DeltasLZ.Add(0f);
			DeltasRZ.Add(0f);
			if (++num % 2 == 0)
			{
				chainLink.transform.localScale = chainLink.transform.localScale * 0.5f;
			}
		}
		for (num = 0; num < Links.Length; num++)
		{
			Links[num].transform.position = Vector3.Lerp(FixedPoint1.position, FixedPoint2.position, num / Links.Length);
		}
	}

	public void SetConnection(Transform ConnectionTransform)
	{
		FixedPoint2 = ConnectionTransform;
		base.gameObject.SetActive(true);
	}

	public void Disconnect()
	{
		FixedPoint2 = null;
	}

	private void Update()
	{
		if (FixedPoint1 == null || FixedPoint2 == null)
		{
			base.gameObject.SetActive(false);
			return;
		}
		for (int i = 0; i < Links.Length; i++)
		{
			if (i == 0)
			{
				Links[i].transform.position = FixedPoint1.position;
			}
			else if (i == Links.Length - 1)
			{
				Links[i].transform.position = FixedPoint2.position;
			}
			else
			{
				Links[i].UpdatePositions(Links[i].transform.position);
			}
		}
		for (int j = 0; j < Connections.Length; j++)
		{
			Connections[j].UpdatePosition(Links[j].transform.position, Links[j + 1].transform.position);
		}
		for (int k = 0; k < 8; k++)
		{
			for (int l = 0; l < Links.Length; l++)
			{
				if (l > 0)
				{
					DeltasLX[l] = xSpread * (Links[l].x - Links[l - 1].x);
					Links[l - 1].xSpeed -= DeltasLX[l];
					DeltasLY[l] = ySpread * (Links[l].y - Links[l - 1].y);
					Links[l - 1].ySpeed -= DeltasLY[l];
					DeltasLZ[l] = zSpread * (Links[l].z - Links[l - 1].z);
					Links[l - 1].zSpeed -= DeltasLZ[l];
				}
				if (l < Links.Length - 1)
				{
					DeltasRX[l] = xSpread * (Links[l].x - Links[l + 1].x);
					Links[l + 1].xSpeed -= DeltasRX[l];
					DeltasRY[l] = ySpread * (Links[l].y - Links[l + 1].y);
					Links[l + 1].ySpeed -= DeltasRY[l];
					DeltasRZ[l] = zSpread * (Links[l].z - Links[l + 1].z);
					Links[l + 1].zSpeed -= DeltasRZ[l];
				}
			}
			for (int m = 0; m < Links.Length; m++)
			{
				if (m > 0)
				{
					Links[m - 1].x += DeltasLX[m];
					Links[m - 1].y += DeltasLY[m];
					Links[m - 1].z += DeltasLZ[m];
				}
				if (m < Links.Length - 1)
				{
					Links[m + 1].x += DeltasRX[m];
					Links[m + 1].y += DeltasRY[m];
					Links[m + 1].z += DeltasRZ[m];
				}
			}
		}
	}
}
