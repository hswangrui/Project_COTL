using UnityEngine;

public class Interaction_TimeToken : Interaction
{
	public string LabelName = "Time Token";

	public int timeToAdd = 20;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void GetLabel()
	{
		base.Label = LabelName;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		PickUp();
	}

	public void PickUp()
	{
		CameraManager.shakeCamera(0.3f, Random.Range(0, 360));
		HUD_Timer.Timer += timeToAdd;
		Object.Destroy(base.gameObject);
	}
}
