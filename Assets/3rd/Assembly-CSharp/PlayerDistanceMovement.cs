using DG.Tweening;
using UnityEngine;

public class PlayerDistanceMovement : BaseMonoBehaviour
{
	public GameObject MovePos;

	public GameObject objectToMove;

	public float distanceToMove;

	public float dist;

	public float distPercent;

	public Vector3 StartPos;

	private bool resetting;

	private void Start()
	{
		StartPos = base.transform.position;
	}

	private void Update()
	{
		if (PlayerFarming.Instance != null && !resetting)
		{
			dist = Vector3.Distance(PlayerFarming.Instance.gameObject.transform.position, base.transform.position);
			if (dist <= distanceToMove)
			{
				distPercent = Mathf.Abs(dist / distanceToMove - 1f);
				objectToMove.transform.position = Vector3.Lerp(StartPos, MovePos.transform.position, Mathf.SmoothStep(0f, 1f, distPercent));
			}
		}
	}

	public void ForceReset()
	{
		resetting = true;
		objectToMove.transform.DOMove(StartPos, 0.25f).OnComplete(delegate
		{
			resetting = false;
		});
	}
}
