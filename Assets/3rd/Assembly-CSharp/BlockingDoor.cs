using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingDoor : BaseMonoBehaviour
{
	public enum State
	{
		Open,
		Closed
	}

	public static List<BlockingDoor> BlockingDoors = new List<BlockingDoor>();

	private BoxCollider2D Collider;

	public State StartingPosition = State.Closed;

	public float OpeningTime = 4f;

	public float ClosingTime = 4f;

	private void OnEnable()
	{
		BlockingDoors.Add(this);
	}

	private void OnDisable()
	{
		BlockingDoors.Remove(this);
	}

	private void Start()
	{
		Collider = GetComponentInChildren<BoxCollider2D>();
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, (StartingPosition != State.Closed) ? 2 : 0);
		Collider.enabled = StartingPosition == State.Closed;
		Bounds bounds = Collider.bounds;
		AstarPath.active.UpdateGraphs(bounds);
	}

	public static void OpenAll()
	{
		foreach (BlockingDoor blockingDoor in BlockingDoors)
		{
			blockingDoor.Open();
		}
	}

	public void Open()
	{
		ActivateMiniMap.DisableTeleporting = false;
		StopAllCoroutines();
		StartCoroutine(OpenRoutine());
	}

	private IEnumerator OpenRoutine()
	{
		Bounds bounds = Collider.bounds;
		Collider.enabled = false;
		AstarPath.active.UpdateGraphs(bounds);
		float Progress = 0f;
		Vector3 StartPosition = base.transform.localPosition;
		Vector3 TargetPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 2f);
		Vector3 Position = base.transform.localPosition;
		float x = base.transform.localPosition.x;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < OpeningTime))
			{
				break;
			}
			Position.z = Mathf.SmoothStep(StartPosition.z, TargetPosition.z, Progress / OpeningTime);
			Position.x = x + Random.Range(-0.02f, 0.02f);
			base.transform.localPosition = Position;
			yield return null;
		}
		base.transform.localPosition = new Vector3(x, base.transform.localPosition.y, Position.z);
	}

	public static void CloseAll()
	{
		ActivateMiniMap.DisableTeleporting = true;
		foreach (BlockingDoor blockingDoor in BlockingDoors)
		{
			blockingDoor.Close();
		}
	}

	public void Close()
	{
		StopAllCoroutines();
		StartCoroutine(CloseRoutine());
	}

	private IEnumerator CloseRoutine()
	{
		Collider.enabled = true;
		Bounds bounds = Collider.bounds;
		AstarPath.active.UpdateGraphs(bounds);
		float Progress = 0f;
		Vector3 StartPosition = base.transform.localPosition;
		Vector3 TargetPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 0f);
		Vector3 Position = base.transform.localPosition;
		float x = base.transform.localPosition.x;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < ClosingTime))
			{
				break;
			}
			Position.z = Mathf.SmoothStep(StartPosition.z, TargetPosition.z, Progress / ClosingTime);
			Position.x = x + Random.Range(-0.02f, 0.02f);
			base.transform.localPosition = Position;
			yield return null;
		}
		base.transform.localPosition = new Vector3(x, base.transform.localPosition.y, Position.z);
	}
}
