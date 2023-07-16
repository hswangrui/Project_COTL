using System.Collections;
using UnityEngine;

public class EyesInWoods : BaseMonoBehaviour
{
	public Animator animator;

	public bool toggle;

	public float waitTime;

	private void Start()
	{
		waitTime = Random.Range(4f, 14f);
		StartCoroutine(WaitToChangeState());
	}

	private void TurnOn()
	{
		Debug.Log("on");
		waitTime = Random.Range(4f, 14f);
		toggle = true;
		animator.Play("Eyes-In");
		StopAllCoroutines();
		StartCoroutine(WaitToChangeState());
	}

	private void TurnOff()
	{
		Debug.Log("off");
		waitTime = Random.Range(4f, 14f);
		toggle = false;
		animator.Play("Eyes-Out");
		StopAllCoroutines();
		StartCoroutine(WaitToChangeState());
	}

	private IEnumerator WaitToChangeState()
	{
		Debug.Log("Started Coroutine");
		yield return new WaitForSecondsRealtime(5f);
		Debug.Log("WaitTimeOver");
		if (toggle)
		{
			TurnOff();
		}
		else
		{
			TurnOn();
		}
	}
}
