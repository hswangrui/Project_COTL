using System.Collections.Generic;
using UnityEngine;

public class PlacementStructureHighlighter : BaseMonoBehaviour
{
	public StructureBrain.TYPES StructureType;

	public float Distance;

	public Vector3 PositionOffset;

	private List<Structure> _inRange = new List<Structure>();

	private List<Structure> _outOfRange = new List<Structure>();

	private List<Structure> _newInRange = new List<Structure>();

	private List<Structure> _newOutOfRange = new List<Structure>();

	private BoxCollider2D bounds;

	private void Start()
	{
		foreach (Structure structure in Structure.Structures)
		{
			if (structure.Type == StructureType)
			{
				_outOfRange.Add(structure);
			}
		}
		bounds = GameManager.GetInstance().GetComponent<BoxCollider2D>();
		if (bounds == null)
		{
			bounds = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
			bounds.isTrigger = true;
		}
		bounds.size = Vector2.one * Distance;
		bounds.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, -45f));
	}

	private void OnDestroy()
	{
		for (int i = 0; i < _inRange.Count; i++)
		{
			SetHighlight(_inRange[i], false);
		}
		_inRange.Clear();
		_outOfRange.Clear();
		_newInRange.Clear();
		_newOutOfRange.Clear();
	}

	private void Update()
	{
		bounds.transform.position = base.transform.position + PositionOffset;
		for (int i = 0; i < _outOfRange.Count; i++)
		{
			Structure structure = _outOfRange[i];
			if (bounds.OverlapPoint(structure.transform.position))
			{
				_newInRange.Add(structure);
				_outOfRange.RemoveAt(i--);
				SetHighlight(structure, true);
			}
		}
		for (int j = 0; j < _inRange.Count; j++)
		{
			Structure structure2 = _inRange[j];
			if (!bounds.OverlapPoint(structure2.transform.position))
			{
				_newOutOfRange.Add(structure2);
				_inRange.RemoveAt(j--);
				SetHighlight(structure2, false);
			}
		}
		_inRange.AddRange(_newInRange);
		_newInRange.Clear();
		_outOfRange.AddRange(_newOutOfRange);
		_newOutOfRange.Clear();
	}

	private void SetHighlight(Structure structure, bool active)
	{
		SpriteRenderer[] componentsInChildren = structure.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = (active ? Color.green : Color.white);
		}
	}
}
