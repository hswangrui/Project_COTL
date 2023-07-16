using UnityEngine;

public class Interaction_Elevator : Interaction
{
	public enum Floor
	{
		FirstFloor,
		SecondFloor,
		ThirdFloor
	}

	public Floor MyFloor;

	public Interaction_Elevator TargetElevator;

	public LineRenderer lineRenderer;

	private string sGrapple;

	public override void GetLabel()
	{
		base.Label = sGrapple;
	}

	private void Start()
	{
		UpdateLocalisation();
		lineRenderer.SetPosition(0, base.transform.position);
		lineRenderer.SetPosition(1, TargetElevator.transform.position);
		if (TargetElevator.transform.position.x < base.transform.position.x)
		{
			lineRenderer.gameObject.SetActive(false);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		state.GetComponent<PlayerController>();
	}

	private new void OnDrawGizmos()
	{
		if (TargetElevator != null)
		{
			Utils.DrawLine(base.transform.position, TargetElevator.transform.position, Color.yellow);
		}
	}
}
