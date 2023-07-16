using System.Xml.Serialization;
using UnityEngine;

public class Structures_Research1 : Structures_Research
{
	private const int MAX_SLOT_COUNT = 3;

	[XmlIgnore]
	private bool[] _slotReserved = new bool[3];

	public override bool[] SlotReserved
	{
		get
		{
			return _slotReserved;
		}
	}

	public override Vector3 GetResearchPosition(int slotIndex)
	{
		Vector3 position = Data.Position;
		switch (slotIndex)
		{
		case 0:
			position += new Vector3(-0.5f, -0.5f);
			break;
		case 1:
			position += new Vector3(2.5f, 2.5f);
			break;
		case 2:
			position += new Vector3(-0.5f, 2.5f);
			break;
		}
		return position;
	}
}
