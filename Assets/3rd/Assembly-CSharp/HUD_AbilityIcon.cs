using System.Collections;
using DG.Tweening;
using UnityEngine;

public class HUD_AbilityIcon : MonoBehaviour
{
	public Transform Container;

	public void Play(float Delay)
	{
		StartCoroutine(TweenInRoutine(Delay));
	}

	private IEnumerator TweenInRoutine(float Delay)
	{
		Container.transform.localPosition = new Vector3(0f, -200f);
		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(Delay);
		Sequence s = DOTween.Sequence();
		s.Append(Container.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack));
		s.Append(Container.transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack));
		s.Append(Container.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack));
	}
}
