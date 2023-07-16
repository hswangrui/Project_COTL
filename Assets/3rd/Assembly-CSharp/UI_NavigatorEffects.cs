using System;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

public class UI_NavigatorEffects : BaseMonoBehaviour
{
	public Image UIScreenGoop;

	public ParticleSystemForceField particleForce;

	public float forceMult = 0.1f;

	public float particleSpeed = 10f;

	private Vector3 targetOffset = Vector3.zero;

	private Vector3 newOffset = Vector3.zero;

	private Vector3 parOffset = Vector3.zero;

	private Vector3 curOffset = Vector3.zero;

	private float smoothTime = 0.3f;

	private Vector3 velocityOffset = Vector3.zero;

	private float curveDeltaTime;

	private void OnEnable()
	{
		UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
		instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnChangeSelection));
	}

	private void OnDisable()
	{
		if (MonoSingleton<UINavigatorNew>.Instance != null)
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnChangeSelection));
		}
	}

	private void OnChangeSelection(Selectable newSelectable, Selectable previousSelectable)
	{
		newOffset = newSelectable.transform.position - previousSelectable.transform.position;
		targetOffset += newOffset / 10000f;
		parOffset = -(newSelectable.transform.position - previousSelectable.transform.position) / 100f;
	}

	private void Update()
	{
		curOffset = Vector3.SmoothDamp(curOffset, targetOffset, ref velocityOffset, smoothTime, 1f, Time.unscaledDeltaTime);
		UIScreenGoop.material.SetVector("_UINavOffset", curOffset);
		if (particleForce != null)
		{
			parOffset = Vector3.MoveTowards(parOffset, Vector3.zero, Time.unscaledDeltaTime * particleSpeed);
			particleForce.directionX = parOffset.x * forceMult;
			particleForce.directionY = parOffset.y * forceMult;
		}
	}
}
