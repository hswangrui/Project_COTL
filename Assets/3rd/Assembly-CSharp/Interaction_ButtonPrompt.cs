using UnityEngine;
using UnityEngine.Events;

public class Interaction_ButtonPrompt : Interaction
{
	private Indicator IndicatorInstance;

	private GameObject Player;

	public string Label_;

	public UnityEvent callbackRing;

	private void Start()
	{
	}

	public override void GetLabel()
	{
	}

	public override void OnInteract(StateMachine state)
	{
		callbackRing.Invoke();
	}
}
