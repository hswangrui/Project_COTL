using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollBarMoveTo : BaseMonoBehaviour
{
	public RectTransform maskTransform;

	private ScrollRect mScrollRect;

	private RectTransform mScrollTransform;

	private RectTransform mContent;

	public IEnumerator CenterOnItem(RectTransform target)
	{
		Vector3 worldPointInWidget = GetWorldPointInWidget(mScrollTransform, GetWidgetWorldPoint(target));
		Vector3 vector = GetWorldPointInWidget(mScrollTransform, GetWidgetWorldPoint(maskTransform)) - worldPointInWidget;
		vector.z = 0f;
		if (!mScrollRect.horizontal)
		{
			vector.x = 0f;
		}
		if (!mScrollRect.vertical)
		{
			vector.y = 0f;
		}
		Vector2 vector2 = new Vector2(vector.x / (mContent.rect.size.x - mScrollTransform.rect.size.x), vector.y / (mContent.rect.size.y - mScrollTransform.rect.size.y));
		Vector2 newNormalizedPosition = mScrollRect.normalizedPosition - vector2;
		if (mScrollRect.movementType != 0)
		{
			newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
			newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
		}
		Vector3 StartPos = mScrollRect.normalizedPosition;
		float timer = 0f;
		float Duration = 0.25f;
		while (timer < Duration)
		{
			mScrollRect.normalizedPosition = Vector3.Lerp(StartPos, newNormalizedPosition, timer / Duration);
			timer += Time.deltaTime;
			yield return null;
		}
		mScrollRect.normalizedPosition = newNormalizedPosition;
	}

	private void Awake()
	{
		mScrollRect = GetComponent<ScrollRect>();
		mScrollTransform = mScrollRect.transform as RectTransform;
		mContent = mScrollRect.content;
		Reset();
	}

	private void Reset()
	{
		if (!(maskTransform == null))
		{
			return;
		}
		Mask componentInChildren = GetComponentInChildren<Mask>(true);
		if ((bool)componentInChildren)
		{
			maskTransform = componentInChildren.rectTransform;
		}
		if (maskTransform == null)
		{
			RectMask2D componentInChildren2 = GetComponentInChildren<RectMask2D>(true);
			if ((bool)componentInChildren2)
			{
				maskTransform = componentInChildren2.rectTransform;
			}
		}
	}

	private Vector3 GetWidgetWorldPoint(RectTransform target)
	{
		Vector3 vector = new Vector3((0.5f - target.pivot.x) * target.rect.size.x, (0.5f - target.pivot.y) * target.rect.size.y, 0f);
		Vector3 position = target.localPosition + vector;
		return target.parent.TransformPoint(position);
	}

	private Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
	{
		return target.InverseTransformPoint(worldPoint);
	}
}
