using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeLine : BaseMonoBehaviour
{
	public enum State
	{
		Open,
		Closed
	}

	public List<GameObject> Barricades = new List<GameObject>();

	private BoxCollider2D Collider;

	public State StartingPosition = State.Closed;

	public float OpeningTime = 0.5f;

	public float ClosingTime = 0.5f;

	private void Start()
	{
		Collider = GetComponentInChildren<BoxCollider2D>();
		foreach (GameObject barricade in Barricades)
		{
			barricade.SetActive(StartingPosition == State.Closed);
		}
		Collider.enabled = StartingPosition == State.Closed;
	}

	public void Open()
	{
		StartCoroutine(OpenRoutine());
	}

	private IEnumerator OpenRoutine()
	{
		Collider.enabled = false;
		foreach (GameObject barricade in Barricades)
		{
			barricade.SetActive(false);
			yield return new WaitForSeconds(0.02f);
		}
	}

	public void Close()
	{
		StartCoroutine(CloseRoutine());
	}

	private IEnumerator CloseRoutine()
	{
		Collider.enabled = true;
		foreach (GameObject barricade in Barricades)
		{
			barricade.SetActive(true);
			yield return new WaitForSeconds(0.02f);
		}
	}
}
