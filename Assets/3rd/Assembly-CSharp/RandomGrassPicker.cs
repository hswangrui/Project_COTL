using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGrassPicker : BaseMonoBehaviour
{
	public List<FlowerTypes> Flowers = new List<FlowerTypes>();

	public string DefaultGrass;

	private bool GrassPicked;

	private GameObject createdObject;

	public bool GrassCut;

	public FollowerLocation currentLocation = FollowerLocation.None;

	private bool isCut;

	private bool checkedQuests;

	private bool FoundOne;

	private int r;

	private void Awake()
	{
		base.transform.GetChild(0).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		if (!FoundOne)
		{
			LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(UpdateGrass));
		}
		else
		{
			SpawnObject();
		}
	}

	private FlowerTypes GetGrassFromLocation()
	{
		foreach (FlowerTypes flower in Flowers)
		{
			foreach (FollowerLocation item in flower.Location)
			{
				if (item == currentLocation)
				{
					return flower;
				}
			}
		}
		return null;
	}

	private void CheckQuests()
	{
		FlowerTypes grassFromLocation = GetGrassFromLocation();
		if (currentLocation == FollowerLocation.Dungeon1_1 || currentLocation == FollowerLocation.Dungeon1_2 || currentLocation == FollowerLocation.Dungeon1_3 || currentLocation == FollowerLocation.Dungeon1_4)
		{
			for (int num = DataManager.Instance.Objectives.Count - 1; num >= 0; num--)
			{
				ObjectivesData objectivesData = DataManager.Instance.Objectives[num];
				if (currentLocation == FollowerLocation.Dungeon1_1)
				{
					if (objectivesData.Type == Objectives.TYPES.COLLECT_ITEM && ((Objectives_CollectItem)objectivesData).ItemType == InventoryItem.ITEM_TYPE.FLOWER_RED)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 2f;
					}
					else if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.FLOWER_RED) <= 10)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 1.5f;
					}
					checkedQuests = true;
					break;
				}
				if (currentLocation == FollowerLocation.Dungeon1_2)
				{
					if (objectivesData.Type == Objectives.TYPES.COLLECT_ITEM && ((Objectives_CollectItem)objectivesData).ItemType == InventoryItem.ITEM_TYPE.MUSHROOM_SMALL)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 2f;
					}
					else if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.MUSHROOM_SMALL) <= 10)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 1.25f;
					}
					checkedQuests = true;
					break;
				}
				if (currentLocation == FollowerLocation.Dungeon1_3)
				{
					if (objectivesData.Type == Objectives.TYPES.COLLECT_ITEM && ((Objectives_CollectItem)objectivesData).ItemType == InventoryItem.ITEM_TYPE.CRYSTAL)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 2f;
					}
					else if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.CRYSTAL) <= 10)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 1.5f;
					}
					checkedQuests = true;
					break;
				}
				if (currentLocation == FollowerLocation.Dungeon1_4)
				{
					if (objectivesData.Type == Objectives.TYPES.COLLECT_ITEM && ((Objectives_CollectItem)objectivesData).ItemType == InventoryItem.ITEM_TYPE.SPIDER_WEB)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 2f;
					}
					else if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.SPIDER_WEB) <= 10)
					{
						grassFromLocation.PercentageChanceToSpawn.y = grassFromLocation.PercentageChanceToSpawn.y * 1.5f;
					}
					checkedQuests = true;
					break;
				}
			}
		}
		checkedQuests = true;
	}

	private void OnDisable()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateGrass));
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

	private void OnDestroy()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateGrass));
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
		try
		{
			if (!FoundOne)
			{
				return;
			}
			foreach (FlowerTypes flower in Flowers)
			{
				foreach (FollowerLocation item in flower.Location)
				{
					if (item != currentLocation)
					{
						continue;
					}
					if (GrassPicked)
					{
						ObjectPool.Spawn(flower.Grass, base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
						{
							createdObject = obj;
							createdObject.transform.position = base.transform.position;
							createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
							if (isCut)
							{
								createdObject.GetComponent<LongGrass>().SetCut();
							}
							else
							{
								createdObject.GetComponent<LongGrass>().ResetCut();
							}
						});
						break;
					}
					ObjectPool.Spawn(flower.Flower, base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
					{
						createdObject = obj;
						createdObject.transform.position = base.transform.position;
						createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
						if (isCut)
						{
							createdObject.GetComponent<LongGrass>().SetCut();
						}
						else
						{
							createdObject.GetComponent<LongGrass>().ResetCut();
						}
					});
					break;
				}
			}
		}
		catch (Exception message)
		{
			Debug.Log(message);
		}
	}

	private void RandomGrassPicker_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (Victim.gameObject == createdObject)
		{
			isCut = true;
		}
	}

	private void SpawnDefault()
	{
		ObjectPool.Spawn(DefaultGrass, base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
		{
			createdObject = obj;
			createdObject.transform.position = base.transform.position;
			createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
			if (isCut)
			{
				createdObject.GetComponent<LongGrass>().SetCut();
			}
			else
			{
				createdObject.GetComponent<LongGrass>().ResetCut();
			}
		});
	}

	private void UpdateGrass()
	{
		FoundOne = false;
		currentLocation = PlayerFarming.Location;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateGrass));
		if (!checkedQuests)
		{
			CheckQuests();
		}
		if (DataManager.Instance.dungeonRun <= 3 && currentLocation == FollowerLocation.Dungeon1_1)
		{
			SpawnDefault();
			return;
		}
		foreach (FlowerTypes flower in Flowers)
		{
			foreach (FollowerLocation item in flower.Location)
			{
				if (item != currentLocation)
				{
					continue;
				}
				if (currentLocation == FollowerLocation.Dungeon1_2 && DataManager.Instance.SozoStoryProgress == -1)
				{
					flower.PercentageChanceToSpawn.y = 0f;
				}
				FoundOne = true;
				r = UnityEngine.Random.Range(0, 100);
				if ((float)r <= flower.PercentageChanceToSpawn.y)
				{
					GrassPicked = false;
					ObjectPool.Spawn(flower.Flower, base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
					{
						createdObject = obj;
						createdObject.transform.position = base.transform.position;
						createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
						if (isCut)
						{
							createdObject.GetComponent<LongGrass>().SetCut();
						}
						else
						{
							createdObject.GetComponent<LongGrass>().ResetCut();
						}
					});
					break;
				}
				GrassPicked = true;
				ObjectPool.Spawn(flower.Grass, base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
				{
					createdObject = obj;
					createdObject.transform.position = base.transform.position;
					createdObject.GetComponent<Health>().OnDie += RandomGrassPicker_OnDie;
					if (isCut)
					{
						createdObject.GetComponent<LongGrass>().SetCut();
					}
					else
					{
						createdObject.GetComponent<LongGrass>().ResetCut();
					}
				});
				break;
			}
		}
		if (!FoundOne)
		{
			SpawnDefault();
		}
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(UpdateGrass));
	}
}
