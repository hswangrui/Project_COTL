using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RandomObjectPicker : BaseMonoBehaviour
{
	public enum SelectionMode
	{
		RandomChance,
		RandomSingle
	}

	public List<string> Objects = new List<string>();

	public UnityAction ObjectCreated;

	public Transform CreatedObject;

	public SelectionMode selectionMode;

	[Range(0f, 100f)]
	public int chanceToEnable;

	private void Start()
	{
		switch (selectionMode)
		{
		case SelectionMode.RandomChance:
		{
			if (Objects == null || Objects.Count <= 0)
			{
				break;
			}
			for (int i = 0; i < Objects.Count; i++)
			{
				if (Random.Range(0, 100) > chanceToEnable)
				{
					continue;
				}
				ObjectPool.Spawn(Objects[i], base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
				{
					CreatedObject = obj.transform;
					CreatedObject.localPosition = Vector3.zero;
					CreatedObject.gameObject.SetActive(true);
					UnityAction objectCreated = ObjectCreated;
					if (objectCreated != null)
					{
						objectCreated();
					}
				});
			}
			break;
		}
		case SelectionMode.RandomSingle:
		{
			if (Objects == null || Objects.Count <= 0)
			{
				break;
			}
			int index = Random.Range(0, Objects.Count);
			ObjectPool.Spawn(Objects[index], base.transform.position, Quaternion.identity, base.transform, delegate(GameObject obj)
			{
				CreatedObject = obj.transform;
				CreatedObject.localPosition = Vector3.zero;
				CreatedObject.gameObject.SetActive(true);
				UnityAction objectCreated2 = ObjectCreated;
				if (objectCreated2 != null)
				{
					objectCreated2();
				}
			});
			break;
		}
		}
	}
}
