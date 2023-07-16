using System.Collections;
using UnityEngine;

public class ProfileSlideAnimation : MonoBehaviour
{
	public float Duration;

	public float Delay;

	public Vector3 StartPosition;

	public Vector3 EndPosition;

	private void OnEnable()
	{
		base.transform.localPosition = StartPosition;
		StartCoroutine(Animate());
	}

	private void OnDisable()
	{
		base.transform.localPosition = StartPosition;
	}

	private IEnumerator Animate()
	{
		float timeElapsed = 0f;
		while (timeElapsed < Delay)
		{
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		while (timeElapsed < Duration + Delay)
		{
			base.transform.localPosition = Vector3.Lerp(StartPosition, EndPosition, (timeElapsed - Delay) / Duration);
			timeElapsed += Time.deltaTime;
			yield return null;
		}
		base.transform.localPosition = EndPosition;
	}
}
