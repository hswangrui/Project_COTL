using System.Collections.Generic;
using UnityEngine;

public class SimpleSetCamera : BaseMonoBehaviour
{
	public bool AutomaticallyActivate = true;

	public Camera camera;

	public AnimationCurve animationCurve;

	public float Duration = 2f;

	public float ActivateDistance = 2f;

	public Vector3 ActivateOffset = Vector3.zero;

	public float DectivateDistance = 2f;

	private static List<SimpleSetCamera> Cameras = new List<SimpleSetCamera>();

	private bool OnCam;

	private GameObject Player;

	public bool Active = true;

	private void OnEnable()
	{
		Cameras.Add(this);
		camera.enabled = false;
	}

	private void Update()
	{
		if (!AutomaticallyActivate || !Active || (Player = GameObject.FindWithTag("Player")) == null)
		{
			return;
		}
		if (!OnCam && Vector3.Distance(Player.transform.position, base.transform.position + ActivateOffset) <= ActivateDistance)
		{
			OnCam = true;
			Play();
		}
		if (OnCam && Vector3.Distance(Player.transform.position, base.transform.position + ActivateOffset) > ActivateDistance)
		{
			OnCam = false;
			if (GameManager.GetInstance().CamFollowTarget.TargetCamera == camera)
			{
				Reset();
			}
		}
	}

	public static void DisableAll()
	{
		Debug.Log("DisableAll");
		foreach (SimpleSetCamera camera in Cameras)
		{
			if (camera.OnCam)
			{
				camera.OnCam = false;
				camera.Reset();
			}
			camera.Active = false;
		}
	}

	public static void EnableAll()
	{
		Debug.Log("EnableAll");
		foreach (SimpleSetCamera camera in Cameras)
		{
			camera.Active = true;
		}
	}

	private void OnDisable()
	{
		Cameras.Remove(this);
		if (OnCam)
		{
			OnCam = false;
			if (GameManager.GetInstance() != null && GameManager.GetInstance().CamFollowTarget.TargetCamera == camera)
			{
				Reset();
			}
		}
	}

	public void Play()
	{
		OnCam = true;
		GameManager.GetInstance().CamFollowTarget.SetTargetCamera(camera, Duration, animationCurve);
	}

	public void Reset()
	{
		OnCam = false;
		GameManager.GetInstance().CamFollowTarget.ResetTargetCamera(DectivateDistance);
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, ActivateDistance, Color.green);
	}
}
