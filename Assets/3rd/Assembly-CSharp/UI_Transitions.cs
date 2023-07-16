using System.Collections;
using UnityEngine;

public class UI_Transitions : BaseMonoBehaviour
{
	public Transform Container;

	public Vector3 StartPos;

	public Vector3 MovePos;

	public bool Hidden;

	public bool Revealed;

	private void Start()
	{
	}

	private void OnEnable()
	{
	}

	private void GetStartingPos()
	{
		StartPos = Container.localPosition;
	}

	public void MoveBackOutFunction()
	{
		StopAllCoroutines();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(MoveBarOut());
		}
	}

	public void MoveBackInFunction()
	{
		StopAllCoroutines();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(MoveBarIn());
		}
	}

	public IEnumerator MoveBarOut()
	{
		Hidden = true;
		Vector3 currentPos = Container.localPosition;
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.localPosition = Vector3.Lerp(currentPos, MovePos, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.localPosition = MovePos;
		Revealed = false;
	}

	public void hideBar()
	{
		Hidden = true;
		Revealed = false;
		if (Container != null)
		{
			Container.localPosition = MovePos;
		}
	}

	public IEnumerator MoveBarIn()
	{
		Vector3 currentPos = Container.localPosition;
		Hidden = false;
		float Progress = 0f;
		float Duration = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.localPosition = Vector3.Lerp(currentPos, StartPos, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		Container.localPosition = StartPos;
		Revealed = true;
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.parent.transform.localToWorldMatrix;
		Gizmos.DrawLine(StartPos, MovePos);
		Gizmos.DrawSphere(StartPos, 5f);
		Gizmos.DrawSphere(MovePos, 5f);
	}

	private void Update()
	{
	}
}
