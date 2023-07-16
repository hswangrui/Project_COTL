using System;
using System.Collections;
using DG.Tweening;
using MMBiomeGeneration;
using UnityEngine;

public class DungeonLayerStatue : BaseMonoBehaviour
{
	[Serializable]
	private struct Statue
	{
		public GameObject[] LayerObjects;
	}

	[SerializeField]
	private Statue[] statues;

	[SerializeField]
	private GameObject[] layerNodes;

	public static bool ShownDungeonLayer;

	private void Start()
	{
		base.gameObject.SetActive(false);
	}

	private void OnBiomeActive()
	{
		BiomeGenerator.OnRoomActive -= OnBiomeActive;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(ShowIE());
		}
	}

	private IEnumerator ShowIE()
	{
		int num = GameManager.CurrentDungeonLayer - 1;
		layerNodes[num].SetActive(true);
		layerNodes[num].transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 1);
		ShownDungeonLayer = true;
		yield return new WaitForEndOfFrame();
	}
}
