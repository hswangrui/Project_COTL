using System;
using System.Collections;
using DG.Tweening;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

public class UINavigatorFollowElement : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rectTransform;

	private Vector3 _velocity = Vector3.zero;

	private Selectable _previousSelectable;

	private bool _forceInstantNext;

	private void Start()
	{
		DOTween.Init();
	}

	private void OnEnable()
	{
		UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
		instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance.OnDefaultSetComplete, new Action<Selectable>(OnDefaultSelectableSet));
		UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
		instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectableChanged));
	}

	private void OnDisable()
	{
		if (MonoSingleton<UINavigatorNew>.Instance != null)
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance.OnDefaultSetComplete, new Action<Selectable>(OnDefaultSelectableSet));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance2.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectableChanged));
		}
	}

	private void OnDefaultSelectableSet(Selectable selectable)
	{
		DoMoveButton(selectable, true);
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
