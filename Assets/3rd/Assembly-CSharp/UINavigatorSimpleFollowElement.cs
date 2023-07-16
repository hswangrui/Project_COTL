using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UINavigatorSimpleFollowElement : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private UI_NavigatorSimple _navigatorSimple;

	private Vector3 _velocity = Vector3.zero;

	private Selectable _previousSelectable;

	private bool _forceInstantNext;

	private void Start()
	{
		DOTween.Init();
	}

	private void OnEnable()
	{
		UI_NavigatorSimple navigatorSimple = _navigatorSimple;
		navigatorSimple.OnDefaultSetComplete = (Action)Delegate.Combine(navigatorSimple.OnDefaultSetComplete, new Action(OnDefaultSelectableSet));
		UI_NavigatorSimple navigatorSimple2 = _navigatorSimple;
		navigatorSimple2.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Combine(navigatorSimple2.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnSelectableChanged));
	}

	private void OnDisable()
	{
		UI_NavigatorSimple navigatorSimple = _navigatorSimple;
		navigatorSimple.OnDefaultSetComplete = (Action)Delegate.Remove(navigatorSimple.OnDefaultSetComplete, new Action(OnDefaultSelectableSet));
		UI_NavigatorSimple navigatorSimple2 = _navigatorSimple;
		navigatorSimple2.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Remove(navigatorSimple2.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnSelectableChanged));
	}

	private void OnDefaultSelectableSet()
	{
		DoMoveButton(_navigatorSimple.selectable, true);
	}

	private void OnSelectableChanged(Selectable newSelectable, Selectable previous)
	{
		DoMoveButton(newSelectable);
	}

	private void DoMoveButton(Selectable to, bool instant = false)
	{
		DoShakeScale();
		StopAllCoroutines();
		if ((bool)to.GetComponent<UIIgnoreFollowElement>())
		{
			_rectTransform.localPosition = Vector3.one * float.MaxValue;
			_forceInstantNext = true;
		}
		else
		{
			StartCoroutine(MoveButtonRoutine(to, instant));
		}
	}

	private IEnumerator MoveButtonRoutine(Selectable moveTo, bool instant = false)
	{
		yield return null;
		Vector3 position = moveTo.transform.position;
		Vector3 targetLocalPosition = _rectTransform.parent.InverseTransformPoint(position);
		Vector3 currentLocalPosition = _rectTransform.localPosition;
		if (!instant && !_forceInstantNext)
		{
			float progress = 0f;
			while (true)
			{
				float num;
				progress = (num = progress + Time.unscaledDeltaTime * 5f);
				if (!(num <= 1f))
				{
					break;
				}
				_rectTransform.localPosition = Vector3.SmoothDamp(targetLocalPosition, currentLocalPosition, ref _velocity, progress);
				yield return null;
			}
		}
		_forceInstantNext = false;
		_rectTransform.localPosition = targetLocalPosition;
		_rectTransform.localScale = Vector3.one;
	}

	private void DoShakeScale()
	{
		DOTween.Kill(_rectTransform);
		_rectTransform.localScale = Vector3.one;
		_rectTransform.DOShakeScale(0.1f, new Vector3(-0.1f, 0.1f, 1f), 5, 90f, false).SetUpdate(true);
	}
}
