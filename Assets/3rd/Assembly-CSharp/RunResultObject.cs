using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RunResultObject : BaseMonoBehaviour
{
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private PauseInventoryItem runItem;

	[SerializeField]
	private PauseInventoryItem baseItem;

	[SerializeField]
	private PauseInventoryItem totalItem;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private const float moveDuration = 0.5f;

	private RunResults runResults;

	public void Init(InventoryItem item, float delay)
	{
		runResults = GetComponentInParent<RunResults>();
		int quantity = item.quantity;
		int num = ((Inventory.GetItemByType(item.type) != null) ? (Inventory.GetItemByType(item.type).quantity - item.quantity) : 0);
		runItem.Init((InventoryItem.ITEM_TYPE)item.type, quantity);
		baseItem.Init((InventoryItem.ITEM_TYPE)item.type, num);
		scrollRect.StartCoroutine(Merge(item, quantity + num, delay));
	}

	private IEnumerator Merge(InventoryItem item, int total, float delay)
	{
		canvasGroup.alpha = 0f;
		float t4 = 0f;
		while (t4 < delay)
		{
			t4 += Time.unscaledDeltaTime * runResults.TimeScale;
			yield return null;
		}
		base.gameObject.SetActive(true);
		yield return new WaitForEndOfFrame();
		if (scrollRect.verticalNormalizedPosition != 0f)
		{
			scrollRect.DOVerticalNormalizedPos(0f, 0.25f).SetUpdate(UpdateType.Manual);
			t4 = 0f;
			while (t4 < 0.25f)
			{
				t4 += Time.unscaledDeltaTime * runResults.TimeScale;
				yield return null;
			}
		}
		canvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
		t4 = 0f;
		while (t4 < 0.25f)
		{
			t4 += Time.unscaledDeltaTime * runResults.TimeScale;
			yield return null;
		}
		PauseInventoryItem movingRunItem = Object.Instantiate(runItem, base.transform);
		movingRunItem.Init((InventoryItem.ITEM_TYPE)item.type, 1, false);
		((RectTransform)movingRunItem.transform).DOAnchorPosX(0f, 0.5f).SetEase(Ease.InBack).SetUpdate(UpdateType.Manual);
		runItem.SetGrey();
		PauseInventoryItem movingBaseItem = Object.Instantiate(baseItem, base.transform);
		movingBaseItem.Init((InventoryItem.ITEM_TYPE)item.type, 1, false);
		((RectTransform)movingBaseItem.transform).DOAnchorPosX(0f, 0.5f).SetEase(Ease.InBack).SetUpdate(UpdateType.Manual);
		baseItem.SetGrey();
		t4 = 0f;
		while (t4 < 0.5f)
		{
			t4 += Time.unscaledDeltaTime * runResults.TimeScale;
			yield return null;
		}
		movingRunItem.gameObject.SetActive(false);
		movingBaseItem.gameObject.SetActive(false);
		totalItem.gameObject.SetActive(true);
		totalItem.Init((InventoryItem.ITEM_TYPE)item.type, total);
		totalItem.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f).SetUpdate(UpdateType.Manual);
	}
}
