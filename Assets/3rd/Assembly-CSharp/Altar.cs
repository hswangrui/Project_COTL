using System.Collections.Generic;
using UnityEngine;

public class Altar : BaseMonoBehaviour
{
	public List<Transform> NewRecruitPositions = new List<Transform>();

	public List<Transform> SacrificePositions = new List<Transform>();

	public GameObject CentrePoint;

	public static Altar Instance;

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		Instance = null;
	}
}
