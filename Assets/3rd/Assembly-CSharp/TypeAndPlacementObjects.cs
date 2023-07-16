using System.Collections.Generic;

public class TypeAndPlacementObjects : BaseMonoBehaviour
{
	public enum Tier
	{
		Zero,
		One,
		Two,
		Three
	}

	private static TypeAndPlacementObjects _Instance;

	public List<TypeAndPlacementObject> TypeAndPlacementObject = new List<TypeAndPlacementObject>();

	private void Awake()
	{
		_Instance = this;
	}

	private void OnDestroy()
	{
		_Instance = null;
		TypeAndPlacementObject.Clear();
	}

	public static TypeAndPlacementObject GetByType(StructureBrain.TYPES Type)
	{
		if (_Instance != null)
		{
			foreach (TypeAndPlacementObject item in _Instance.TypeAndPlacementObject)
			{
				if (item.Type == Type)
				{
					return item;
				}
			}
		}
		return null;
	}
}
