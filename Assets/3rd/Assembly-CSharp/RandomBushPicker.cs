using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBushPicker : BaseMonoBehaviour
{
	public List<ObjectTypes> Objects = new List<ObjectTypes>();

	private GameObject createdObject;

	public bool GrassCut;

	public FollowerLocation currentLocation = FollowerLocation.None;

	private bool isDestroyed;

	private int spawnedIndex;

	private bool FoundOne;

	private int r;

	private void Awake()
	{
		base.transform.GetChild(0).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		Objects.Clear();
	}

	private void OnEnable()
	{
		if (!FoundOne)
		{
			if (PlayerFarming.Location == FollowerLocation.None)
			{
				LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(UpdateObject));
			}
			else
			{
				UpdateObject();
			}
		}
		else
		{
			SpawnObject();
		}
	}

	private void OnDisable()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateObject));
		if ((bool)createdObject)
		{
			createdObject.GetComponent<Health>().OnDie -= RandomGrassPicker_OnDie;
			GameManager instance = GameManager.GetInstance();
			if ((object)instance != null)
			{
				instance.StartCoroutine(FrameDelayRecycle());
			}
		}
	}

	private IEnumerator FrameDelayRecycle()
	{
		yield return new WaitForEndOfFrame();
		if ((bool)createdObject)
		{
			ObjectPool.Recycle(createdObject);
		}
	}

	private void SpawnObject()
	{
		if (FoundOne && spawnedIndex != -1 && GetComponentInChildren<LongGrass>() == null)
		{
			foreach (ObjectTypes @object in Objects)
			{
				foreach (FollowerLocation item in @object.Location)
				{
					if (item == currentLocation)
					{
						if (@object.Objects[spawnedIndex] != null)
						{
							LongGrass component = ObjectPool.Spawn(@object.Objects[spawnedIndex], base.transform, base.transform.position).GetComponent<LongGrass>();
							createdObject = component.gameObject;
						}
						break;
					}
				}
			}
		}
		if ((bool)createdObject)
		{
			createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
			createdObject.transform.position = base.transform.position;
			if (isDestroyed)
			{
				createdObject.GetComponent<LongGrass>().SetCut();
			}
			else
			{
				createdObject.GetComponent<LongGrass>().ResetCut();
			}
		}
	}

	private void RandomGrassPicker_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (Victim.gameObject == createdObject)
		{
			isDestroyed = true;
		}
	}

	private void UpdateObject()
	{
		if (GetComponentInChildren<LongGrass>() != null)
		{
			return;
		}
		FoundOne = false;
		currentLocation = PlayerFarming.Location;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateObject));
		foreach (ObjectTypes @object in Objects)
		{
			foreach (FollowerLocation item in @object.Location)
			{
				if (item != currentLocation)
				{
					continue;
				}
				FoundOne = true;
				r = UnityEngine.Random.Range(0, 100);
				if ((float)r <= @object.PercentageChanceToSpawn.y)
				{
					spawnedIndex = UnityEngine.Random.Range(0, @object.Objects.Length);
					if (@object.Objects[spawnedIndex] != null)
					{
						createdObject = ObjectPool.Spawn(@object.Objects[spawnedIndex], base.transform, base.transform.position);
						createdObject.transform.position = base.transform.position;
						createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
					}
					break;
				}
				spawnedIndex = -1;
			}
		}
		if (createdObject != null)
		{
			createdObject.transform.position = base.transform.position;
			if (isDestroyed)
			{
				createdObject.GetComponent<LongGrass>().SetCut();
			}
			else
			{
				createdObject.GetComponent<LongGrass>().ResetCut();
			}
		}
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateObject));
	}
}
