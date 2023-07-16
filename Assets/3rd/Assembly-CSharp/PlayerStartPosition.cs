using System.Collections;
using MMBiomeGeneration;
using UnityEngine;

public class PlayerStartPosition : BaseMonoBehaviour
{
	public bool HideHud;

	public bool AnimateCameraIn;

	private float Progress;

	private float Duration = 4f;

	private void OnEnable()
	{
		BiomeGenerator.OnBiomeChangeRoom += OnBiomeChangeRoom;
	}

	private void Start()
	{
		if (HideHud)
		{
			HUD_Manager.Instance.Hide(true, 0);
		}
	}

	private void OnDisable()
	{
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
	}

	private void OnBiomeChangeRoom()
	{
		GameObject gameObject = GameObject.FindWithTag("Player");
		if (gameObject != null)
		{
			if (AnimateCameraIn)
			{
				StartCoroutine(AnimateCameraInRoutine());
			}
			gameObject.transform.position = base.transform.transform.position;
			gameObject.GetComponent<StateMachine>().facingAngle = -85f;
			gameObject.GetComponentInChildren<SimpleSpineAnimator>().Animate("intro/idle", 0, true);
		}
	}

	private IEnumerator AnimateCameraInRoutine()
	{
		Progress = 0f;
		while (Progress < Duration)
		{
			Progress += Time.deltaTime;
			CameraFollowTarget.Instance.targetDistance = Mathf.SmoothStep(4f, 10f, Progress / Duration);
			yield return null;
		}
	}
}
