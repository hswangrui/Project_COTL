using System;
using UnityEngine;

[Serializable]
public class TypeAndPlacementObject
{
	public StructureBrain.TYPES Type;

	[HideInInspector]
	public StructureBrain.Categories Category;

	public GameObject PlacementObject;

	public Sprite IconImage;

	[HideInInspector]
	public TypeAndPlacementObjects.Tier Tier;
}
