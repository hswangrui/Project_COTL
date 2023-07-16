using System;

public class Structures_CompostBin : StructureBrain
{
	public Action UpdateCompostState;

	public int CompostCount
	{
		get
		{
			return GetGrassCount();
		}
	}

	public int PoopCount
	{
		get
		{
			return GetPoopCount();
		}
	}

	public virtual int CompostCost
	{
		get
		{
			return 50;
		}
	}

	public virtual int PoopToCreate
	{
		get
		{
			return 15;
		}
	}

	public virtual float COMPOST_DURATION
	{
		get
		{
			return 400f;
		}
	}

	public void AddGrass()
	{
		AddGrass(1);
		Data.Progress = ((Data.Progress == 0f) ? TimeManager.TotalElapsedGameTime : Data.Progress);
		Action updateCompostState = UpdateCompostState;
		if (updateCompostState != null)
		{
			updateCompostState();
		}
	}

	public void AddPoop()
	{
		SetGrass(0);
		AddPoop(PoopToCreate);
		Action updateCompostState = UpdateCompostState;
		if (updateCompostState != null)
		{
			updateCompostState();
		}
	}

	public void CollectPoop()
	{
		SetPoop(0);
		Action updateCompostState = UpdateCompostState;
		if (updateCompostState != null)
		{
			updateCompostState();
		}
	}

	private int GetGrassCount()
	{
		for (int i = 0; i < Data.Inventory.Count; i++)
		{
			if (Data.Inventory[i].type == 35)
			{
				return Data.Inventory[i].quantity;
			}
		}
		return 0;
	}

	private int GetPoopCount()
	{
		for (int i = 0; i < Data.Inventory.Count; i++)
		{
			if (Data.Inventory[i].type == 39)
			{
				return Data.Inventory[i].quantity;
			}
		}
		return 0;
	}

	public void AddGrass(int amount)
	{
		bool flag = false;
		for (int i = 0; i < Data.Inventory.Count; i++)
		{
			if (Data.Inventory[i].type == 35)
			{
				Data.Inventory[i].quantity += amount;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.GRASS, amount));
		}
	}

	public void SetGrass(int amount)
	{
		for (int i = 0; i < Data.Inventory.Count; i++)
		{
			if (Data.Inventory[i].type == 35)
			{
				Data.Inventory[i].quantity = amount;
			}
		}
	}

	public void AddPoop(int amount)
	{
		bool flag = false;
		for (int i = 0; i < Data.Inventory.Count; i++)
		{
			if (Data.Inventory[i].type == 39)
			{
				Data.Inventory[i].quantity += amount;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.POOP, amount));
		}
	}

	public void SetPoop(int amount)
	{
		for (int i = 0; i < Data.Inventory.Count; i++)
		{
			if (Data.Inventory[i].type == 39)
			{
				Data.Inventory[i].quantity = amount;
			}
		}
	}
}
