using System.Collections;
using UnityEngine;

public class WeaponMoundCard : BaseMonoBehaviour
{
	public Chain Chain;

	public Transform Card;

	private Transform CrownBone;

	public Vector3 CardOffset;

	private float xWobble;

	private float yWobble;

	public void Play(Transform CrownBone)
	{
		this.CrownBone = CrownBone;
		Chain.FixedPoint1 = CrownBone;
		Card.position = CrownBone.position;
		StartCoroutine(DoRoutine());
		xWobble = Random.Range(0, 180);
		yWobble = Random.Range(0, 180);
	}

	private IEnumerator DoRoutine()
	{
		float Progress = 0f;
		bool Loop = true;
		while (Loop)
		{
			if (Progress < 1f)
			{
				Progress += Time.deltaTime * 5f;
			}
			Card.position = CrownBone.position + (Vector3.up * 2f + CardOffset) * Mathf.SmoothStep(0f, 1f, Progress) + new Vector3(0.1f * Mathf.Cos(xWobble += Time.deltaTime * 2f), 0.1f * Mathf.Cos(yWobble += Time.deltaTime * 2f));
			yield return null;
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}
}
