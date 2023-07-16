using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class NonUILayout : MonoBehaviour
{
	public enum Alignment
	{
		Beginning,
		Center,
		End
	}

	[SerializeField]
	private float elementSize = 2f;

	[SerializeField]
	private bool clearCache;

	[SerializeField]
	private bool isVertical;

	[SerializeField]
	private Alignment alignment = Alignment.Center;

	[SerializeField]
	private bool _animated;

	public int ActiveChildrenCount;

	private HashSet<NonUILayoutElement> elements = new HashSet<NonUILayoutElement>();

	private NonUILayoutElement thisElement;

	private void Start()
	{
		thisElement = GetComponent<NonUILayoutElement>();
		RefreshElements();
	}

	private void AdjustPositions()
	{
		ActiveChildrenCount = elements.Where((NonUILayoutElement e) => e.gameObject.activeSelf && e.enabled && !e.IgnoreLayout).Count();
		if (thisElement != null && ActiveChildrenCount == 0)
		{
			thisElement.IgnoreLayout = true;
			thisElement.ParentLayout.RefreshElements();
		}
		else if (thisElement != null && thisElement.IgnoreLayout && ActiveChildrenCount > 0)
		{
			thisElement.IgnoreLayout = false;
			thisElement.ParentLayout.RefreshElements();
		}
		int num = 0;
		bool flag = ActiveChildrenCount % 2 != 0;
		foreach (NonUILayoutElement element in elements)
		{
			if (!element.gameObject.activeSelf || !element.enabled || element.IgnoreLayout)
			{
				continue;
			}
			switch (alignment)
			{
			case Alignment.Beginning:
			{
				int num2 = num;
				Vector3 vector = ((!isVertical) ? new Vector3(elementSize * (float)num2, 0f, 0f) : new Vector3(0f, (0f - elementSize) * (float)num2, 0f));
				if (_animated)
				{
					element.transform.DOKill();
					element.transform.DOLocalMove(vector, 0.25f).SetEase(Ease.OutSine);
				}
				else
				{
					element.transform.localPosition = vector;
				}
				num++;
				break;
			}
			case Alignment.Center:
			{
				int num2 = num - ActiveChildrenCount / 2;
				if (num2 < 0 && !flag)
				{
					num2++;
				}
				float num3 = (flag ? 0f : ((num - ActiveChildrenCount / 2 >= 0) ? 0.5f : (-0.5f)));
				Vector3 vector = ((!isVertical) ? new Vector3(elementSize * ((float)num2 + num3), 0f, 0f) : new Vector3(0f, (0f - elementSize) * ((float)num2 + num3), 0f));
				if (_animated)
				{
					element.transform.DOKill();
					element.transform.DOLocalMove(vector, 0.25f).SetEase(Ease.InOutSine);
				}
				else
				{
					element.transform.localPosition = vector;
				}
				num++;
				break;
			}
			case Alignment.End:
			{
				int num2 = num;
				Vector3 vector = ((!isVertical) ? new Vector3((0f - elementSize) * (float)(ActiveChildrenCount - 1 - num2), 0f, 0f) : new Vector3(0f, elementSize * (float)(ActiveChildrenCount - 1 - num2), 0f));
				if (_animated)
				{
					element.transform.DOKill();
					element.transform.DOLocalMove(vector, 0.25f).SetEase(Ease.InOutSine);
				}
				else
				{
					element.transform.localPosition = vector;
				}
				num++;
				break;
			}
			}
		}
	}

	public void RefreshElements()
	{
		elements.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			NonUILayoutElement component;
			base.transform.GetChild(i).TryGetComponent<NonUILayoutElement>(out component);
			if (component != null)
			{
				component.ParentLayout = this;
				elements.Add(component);
			}
		}
		AdjustPositions();
	}

	public void OnElementEnabled(NonUILayoutElement element)
	{
		if (elements.Contains(element))
		{
			AdjustPositions();
		}
	}

	public void OnElementDisabled(NonUILayoutElement element)
	{
		if (elements.Contains(element))
		{
			AdjustPositions();
		}
	}

	private void OnValidate()
	{
		RefreshElements();
	}
}
