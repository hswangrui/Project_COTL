using DG.Tweening;
using UnityEngine;

public class ItemGauge : MonoBehaviour
{
	[SerializeField]
	private GameObject needle;

	[SerializeField]
	private Vector3 startingPoint;

	[SerializeField]
	private Vector3 endPoint;

	private void Awake()
	{
		SetPosition(0f);
	}

	public void SetPosition(float norm)
	{
		Vector3 endValue = Vector3.Lerp(startingPoint, endPoint, norm);
		needle.transform.DOKill();
		needle.transform.DOLocalMove(endValue, 0.25f).SetEase(Ease.OutBack);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(base.transform.TransformPoint(startingPoint), 0.05f);
		Gizmos.DrawSphere(base.transform.TransformPoint(endPoint), 0.05f);
	}
}
