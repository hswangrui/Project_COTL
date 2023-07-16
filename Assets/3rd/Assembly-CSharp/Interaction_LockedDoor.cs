using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using UnityEngine;

public class Interaction_LockedDoor : Interaction
{
	private bool Activated;

	private string sOpenDoor;

	public float OpeningTime = 1f;

	private BoxCollider2D Collider;

	public static List<Interaction_LockedDoor> LockedDoors = new List<Interaction_LockedDoor>();

	public float ShakeAmount = 0.2f;

	public float v1 = 0.4f;

	public float v2 = 0.7f;

	public Transform ShakeObject;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		LockedDoors.Add(this);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		LockedDoors.Remove(this);
	}

	private void Start()
	{
		UpdateLocalisation();
		Collider = GetComponentInChildren<BoxCollider2D>();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sOpenDoor = ScriptLocalization.Interactions.OpenDoor;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sOpenDoor);
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			base.OnInteract(state);
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, state.transform.position));
			StopAllCoroutines();
			if (!BiomeGenerator.Instance.HasKey)
			{
				StartCoroutine(DoShake());
			}
			else
			{
				OpenAll();
			}
		}
	}

	public static void OpenAll()
	{
		foreach (Interaction_LockedDoor lockedDoor in LockedDoors)
		{
			lockedDoor.Open();
		}
	}

	public void Open()
	{
		Activated = true;
		StopAllCoroutines();
		StartCoroutine(OpenRoutine());
	}

	private IEnumerator OpenRoutine()
	{
		BiomeGenerator.Instance.HasKey = false;
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
			if (Progress / OpeningTime >= 0.5f && Collider.enabled)
			{
				Collider.enabled = false;
			}
			Position.z = Mathf.SmoothStep(StartPosition.z, TargetPosition.z, Progress / OpeningTime);
			base.transform.localPosition = Position;
			yield return null;
		}
		base.transform.localPosition = new Vector3(x, base.transform.localPosition.y, Position.z);
	}

	private IEnumerator DoShake()
	{
		float Timer = 0f;
		float ShakeSpeed2 = ShakeAmount;
		float Shake = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 3f)
			{
				ShakeSpeed2 += (0f - Shake) * v1;
				float num2 = Shake;
				ShakeSpeed2 = (num = ShakeSpeed2 * v2);
				Shake = num2 + num;
				ShakeObject.localPosition = Vector3.left * Shake;
				yield return null;
				continue;
			}
			break;
		}
	}
}
