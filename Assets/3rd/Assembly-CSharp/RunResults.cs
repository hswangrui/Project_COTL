using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class RunResults : BaseMonoBehaviour
{
	public TextMeshProUGUI DayText;

	public GameObject TextObjectPrefab;

	public CanvasGroup canvasGroup;

	public RectTransform Container;

	public ScrollBarController scrollBarController;

	public GameObject scrollBar;

	public RunResultObject BlackSoulsResult;

	public RunResultObject KillsResult;

	public Action Callback;

	public static RunResults Instance;

	public List<InventoryItem.ITEM_TYPE> Blacklist = new List<InventoryItem.ITEM_TYPE>
	{
		InventoryItem.ITEM_TYPE.SEEDS,
		InventoryItem.ITEM_TYPE.INGREDIENTS,
		InventoryItem.ITEM_TYPE.MEALS
	};

	private bool triggered;

	public float TimeScale { get; private set; } = 2f;


	private bool CheckOnBlacklist(InventoryItem.ITEM_TYPE type)
	{
		bool flag = false;
		foreach (InventoryItem.ITEM_TYPE item in Blacklist)
		{
			if (type == item)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	public static void Play(Action Callback)
	{
		if (Instance == null)
		{
			Instance = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/UI/UI Run Results"), GameObject.FindWithTag("Canvas").transform) as GameObject).GetComponent<RunResults>();
		}
		if (!Instance.triggered)
		{
			Instance.triggered = true;
			Instance.Callback = Callback;
			Instance.StartCoroutine(Instance.DisplayRunResultsRoutine());
		}
	}

	private IEnumerator DisplayRunResultsRoutine()
	{
		Time.timeScale = 0f;
		float num = TimeManager.TotalElapsedGameTime - DataManager.Instance.dungeonRunDuration;
		Debug.Log(num);
		int num2 = Mathf.FloorToInt(num / 60f);
		int num3 = Mathf.FloorToInt(num % 60f);
		Debug.Log("MINUTES: " + num2);
		Debug.Log("SECONDS: " + num3);
		string text = num2 + ":" + ((num3 < 10) ? "0" : "") + num3;
		DayText.text = "<sprite name=\"icon_blackSoul\"> x" + DataManager.Instance.dungeonRunXPOrbs + " | Time: " + text;
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num4;
			Progress = (num4 = Progress + Time.unscaledDeltaTime);
			if (!(num4 < Duration))
			{
				break;
			}
			canvasGroup.alpha = Progress / Duration;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		float delay = 0.25f;
		foreach (InventoryItem item in Inventory.itemsDungeon)
		{
			if (!CheckOnBlacklist((InventoryItem.ITEM_TYPE)item.type))
			{
				UnityEngine.Object.Instantiate(TextObjectPrefab, Container).GetComponent<RunResultObject>().Init(item, delay);
				delay += 1.25f;
			}
		}
		float t = 0f;
		while (!InputManager.UI.GetAcceptButtonDown() && t < delay)
		{
			DOTween.ManualUpdate(Time.unscaledDeltaTime * TimeScale, Time.unscaledDeltaTime * TimeScale);
			t += Time.unscaledDeltaTime * TimeScale;
			yield return null;
		}
		TimeScale = 7.5f;
		yield return new WaitForSecondsRealtime(0.1f);
		while (!InputManager.UI.GetAcceptButtonDown())
		{
			DOTween.ManualUpdate(Time.unscaledDeltaTime * TimeScale, Time.unscaledDeltaTime * TimeScale);
			yield return null;
		}
		scrollBarController.enabled = true;
		scrollBar.SetActive(true);
		Inventory.ClearDungeonItems();
		Time.timeScale = 1f;
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
		Callback = null;
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
