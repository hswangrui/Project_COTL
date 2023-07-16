using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CropController : BaseMonoBehaviour
{
	public InventoryItem.ITEM_TYPE SeedType;

	public List<GameObject> CropStates = new List<GameObject>();

	public GameObject BumperCropObject;

	public GameObject GrowRateIcons;

	public int growth;

	public void SetCropImage(float growthStage, bool BeneffitedFromFertilizer, FollowerLocation Location)
	{
		growthStage = Mathf.Min(growthStage, CropGrowthTimes(SeedType));
		if (BeneffitedFromFertilizer && growthStage >= CropGrowthTimes(SeedType))
		{
			int num = -1;
			while (++num < CropStates.Count)
			{
				CropStates[num].SetActive(false);
			}
			BumperCropObject.GetComponentInChildren<Structure>().CreateStructure(Location, base.transform.position, !BumperCropObject.activeSelf, false);
			BumperCropObject.gameObject.SetActive(true);
			if (!BumperCropObject.activeSelf)
			{
				BumperCropObject.transform.DOKill();
				BumperCropObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
			}
			return;
		}
		int num2 = -1;
		while (++num2 < CropStates.Count)
		{
			int num3 = Mathf.FloorToInt(growthStage / CropGrowthTimes(SeedType) * (float)(CropStates.Count - 1));
			if (num2 == num3 && growthStage >= CropGrowthTimes(SeedType))
			{
				CropStates[num2].GetComponentInChildren<Structure>().CreateStructure(Location, base.transform.position, !CropStates[num2].activeSelf, false);
			}
			if (num2 == num3 && !CropStates[num2].activeSelf)
			{
				BumperCropObject.transform.DOKill();
				CropStates[num2].transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
			}
			CropStates[num2].SetActive(num2 == num3);
		}
	}

	public void SetGrowRateIcons(bool show)
	{
		GrowRateIcons.SetActive(show);
	}

	public void HideAll()
	{
		foreach (GameObject cropState in CropStates)
		{
			if (cropState != null)
			{
				cropState.SetActive(false);
			}
		}
		BumperCropObject.gameObject.SetActive(false);
	}

	public void Harvest()
	{
		FarmPlot componentInParent = GetComponentInParent<FarmPlot>();
		if ((bool)componentInParent)
		{
			componentInParent.Harvested();
		}
	}

	public static int CropStatesForSeedType(InventoryItem.ITEM_TYPE seedType)
	{
		if (seedType == InventoryItem.ITEM_TYPE.SEED || seedType == InventoryItem.ITEM_TYPE.SEED_PUMPKIN)
		{
			return 4;
		}
		return 4;
	}

	public static float CropGrowthTimes(InventoryItem.ITEM_TYPE seedType)
	{
		switch (seedType)
		{
		case InventoryItem.ITEM_TYPE.SEED:
		case InventoryItem.ITEM_TYPE.SEED_PUMPKIN:
			return 9f;
		case InventoryItem.ITEM_TYPE.SEED_BEETROOT:
			return 12f;
		case InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER:
			return 18f;
		case InventoryItem.ITEM_TYPE.SEED_MUSHROOM:
			return 15f;
		default:
			return 9f;
		}
	}

	public static Vector2Int GetHarvestsPerSeedRange(InventoryItem.ITEM_TYPE seedType)
	{
		return new Vector2Int(2, 5);
	}
}
