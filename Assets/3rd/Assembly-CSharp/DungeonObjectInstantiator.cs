using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonObjectInstantiator : BaseMonoBehaviour
{
	[Serializable]
	public class DecorationAndLocation
	{
		public GameObject[] Decorations;

		public FollowerLocation Location;
	}

	public List<DecorationAndLocation> DecorationsAndLocations = new List<DecorationAndLocation>();

	[SerializeField]
	private GameObject placeholderObj;

	private bool spawned;

	private void OnEnable()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(Start));
	}

	private void OnDisable()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(Start));
	}

	private void Start()
	{
		if (spawned)
		{
			return;
		}
		GameObject obj = placeholderObj;
		if ((object)obj != null)
		{
			obj.gameObject.SetActive(false);
		}
		foreach (DecorationAndLocation decorationsAndLocation in DecorationsAndLocations)
		{
			if (decorationsAndLocation.Location == PlayerFarming.Location)
			{
				ObjectPool.Spawn(decorationsAndLocation.Decorations[UnityEngine.Random.Range(0, decorationsAndLocation.Decorations.Length)], base.transform);
				break;
			}
		}
		spawned = true;
	}
}
