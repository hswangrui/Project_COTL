using System.Text;

public class Structures_Outhouse : StructureBrain
{
	public override bool IsFull
	{
		get
		{
			return GetPoopCount() >= Capacity(Data.Type);
		}
	}

	public static int Capacity(TYPES Type)
	{
		switch (Type)
		{
		case TYPES.OUTHOUSE:
			return 5;
		case TYPES.OUTHOUSE_2:
			return 15;
		default:
			return 0;
		}
	}

	public int GetPoopCount()
	{
		int num = 0;
		foreach (InventoryItem item in Data.Inventory)
		{
			if (item.type == 39)
			{
				num += item.quantity;
			}
		}
		return num;
	}

	public override void ToDebugString(StringBuilder sb)
	{
		base.ToDebugString(sb);
		sb.AppendLine(string.Format("Poop: {0}/{1}", GetPoopCount(), Capacity(Data.Type)));
	}
}
