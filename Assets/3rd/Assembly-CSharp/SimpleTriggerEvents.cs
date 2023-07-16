using System;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTriggerEvents : MonoBehaviour
{
	[Serializable]
	public class Collider2DEvent : UnityEvent<Collider2D>
	{
	}

	public Collider2DEvent onEnter;

	public Collider2DEvent onStay;

	public Collider2DEvent onExit;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		onEnter.Invoke(collider);
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		onExit.Invoke(collider);
	}

	private void OnTriggerStay2D(Collider2D collider)
	{
		onStay.Invoke(collider);
	}
}
