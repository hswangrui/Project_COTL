using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_Navigator_MoveButton : BaseMonoBehaviour
{
	public UI_NavigatorSimple UINavigator;

	public GameObject ButtonToMove;

	public CanvasGroup canvasGroup;

	private Vector3 velocity = Vector3.zero;

	public Selectable oldSelectable;

	public bool canvasOff;

	private void Start()
	{
		oldSelectable = UINavigator.selectable;
		DOTween.Init();
	}

	private void Update()
	{
		if (!canvasGroup.interactable || canvasGroup.alpha == 0f)
		{
			canvasOff = true;
			return;
		}
		if (canvasOff)
		{
			MoveButton();
		}
		if (UINavigator.selectable != oldSelectable)
		{
			MoveButton();
		}
	}

	public void MoveButton()
	{
		canvasOff = false;
		oldSelectable = UINavigator.selectable;
		ButtonToMove.transform.localScale = Vector3.one;
		StopAllCoroutines();
		StartCoroutine(MoveButtonRoutine());
	}

	private IEnumerator MoveButtonRoutine()
	{
		yield return new WaitForEndOfFrame();
		ButtonToMove.transform.localScale = Vector3.one;
		ButtonToMove.transform.DOShakeScale(0.1f, new Vector3(-0.1f, 0.1f, 1f), 5, 90f, false);
		Vector3 position = UINavigator.selectable.transform.position;
		Vector3 targetLocalPosition = ButtonToMove.transform.parent.InverseTransformPoint(position);
		Vector3 currentLocalPosition = ButtonToMove.transform.localPosition;
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * 5f);
			if (!(num <= 1f))
			{
				break;
			}
			ButtonToMove.transform.localPosition = Vector3.SmoothDamp(targetLocalPosition, currentLocalPosition, ref velocity, Progress);
			yield return null;
		}
		ButtonToMove.transform.localScale = Vector3.one;
		yield return null;
	}
}
