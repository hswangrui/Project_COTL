using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIDungeonLayer : BaseMonoBehaviour
{
	public static UIDungeonLayer Instance;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Image leaderIcon;

	[SerializeField]
	private Sprite[] leaderIcons;

	[SerializeField]
	private UIDungeonLayerNode[] nodes;

	public static void Play(int currentLayer, float duration, FollowerLocation location)
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load("Prefabs/UI/UI Dungeon Layer") as GameObject, GameObject.FindGameObjectWithTag("Canvas").transform).GetComponent<UIDungeonLayer>();
		}
		if (location == FollowerLocation.Dungeon1_1)
		{
			Instance.leaderIcon.sprite = Instance.leaderIcons[0];
		}
		if (location == FollowerLocation.Dungeon1_2)
		{
			Instance.leaderIcon.sprite = Instance.leaderIcons[1];
		}
		if (location == FollowerLocation.Dungeon1_3)
		{
			Instance.leaderIcon.sprite = Instance.leaderIcons[2];
		}
		if (location == FollowerLocation.Dungeon1_4)
		{
			Instance.leaderIcon.sprite = Instance.leaderIcons[3];
		}
		for (int i = 0; i < Instance.nodes.Length; i++)
		{
			if (i < currentLayer - 1)
			{
				Instance.nodes[i].SetState(UIDungeonLayerNode.State.Visted);
			}
			else if (i == currentLayer - 1)
			{
				Instance.nodes[i].SetState(UIDungeonLayerNode.State.Selected);
			}
		}
		Instance.StartCoroutine(Instance.ShowIE(duration));
	}

	private IEnumerator ShowIE(float duration)
	{
		canvasGroup.alpha = 0f;
		canvasGroup.DOFade(1f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(duration);
		canvasGroup.DOFade(0f, 0.5f);
		yield return new WaitForSeconds(0.5f);
		Hide();
	}

	public static void Hide()
	{
		if ((bool)Instance)
		{
			Object.Destroy(Instance.gameObject);
		}
	}
}
