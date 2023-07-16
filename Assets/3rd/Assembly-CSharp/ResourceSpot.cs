using UnityEngine;

[ExecuteInEditMode]
public class ResourceSpot : BaseMonoBehaviour
{
	private int _current;

	private int Current
	{
		get
		{
			return _current;
		}
		set
		{
			_current = value;
			if (_current > base.transform.childCount - 1)
			{
				_current = 0;
			}
			if (_current < 0)
			{
				_current = base.transform.childCount - 1;
			}
		}
	}

	private void OnEnable()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			Current = 0;
			return;
		}
		Object.Destroy(base.gameObject);
		int num = Random.Range(0, 3);
		StructuresData structuresData = null;
		switch (num)
		{
		case 0:
		{
			int variantIndex = (((double)Random.value < 0.5) ? 1 : 2);
			structuresData = StructuresData.GetInfoByType(StructureBrain.TYPES.TREE, variantIndex);
			break;
		}
		case 1:
			structuresData = StructuresData.GetInfoByType(StructureBrain.TYPES.ROCK, 0);
			break;
		case 2:
			structuresData = StructuresData.GetInfoByType(StructureBrain.TYPES.COTTON_PLANT, 0);
			break;
		}
		Object.Instantiate(Resources.Load(structuresData.PrefabPath) as GameObject, base.transform.parent, true).transform.position = base.transform.position;
		GameManager.RecalculatePaths();
	}

	public void Next()
	{
		int current = Current + 1;
		Current = current;
		int num = -1;
		while (++num < base.transform.childCount)
		{
			base.transform.GetChild(num).gameObject.SetActive(num == Current);
		}
	}

	public void Previous()
	{
		int current = Current - 1;
		Current = current;
		int num = -1;
		while (++num < base.transform.childCount)
		{
			base.transform.GetChild(num).gameObject.SetActive(num == Current);
		}
	}
}
