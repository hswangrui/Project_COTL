using System;
using UnityEngine;

public class FireFly : BaseMonoBehaviour
{
	private Vector3 InitialPosition;

	public float MovementRange = 5f;

	public float VerticalRange = 2f;

	private Vector3 Destination;

	private Vector3 Velocity;

	public float MaxSpeed = 5f;

	public float AvoidSpeed = 20f;

	public float AvoidDistance = 10f;

	public float Decay = 0.9f;

	public Gradient gradient;

	public SpriteRenderer Sprite;

	public float ColorDistance = 2f;

	private void Start()
	{
		InitialPosition = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
		GetNewDestination();
	}

	private void Update()
	{
		Velocity += (Destination - base.transform.position) / MaxSpeed;
		if (Vector3.Distance(base.transform.position, Destination) < 0.5f)
		{
			GetNewDestination();
		}
		foreach (Health allUnit in Health.allUnits)
		{
			if (allUnit.team == Health.Team.PlayerTeam && Vector2.Distance(base.transform.position, allUnit.transform.position) < 2f)
			{
				float f = Utils.GetAngle(allUnit.transform.position, base.transform.position) * ((float)Math.PI / 180f);
				Velocity += new Vector3(AvoidSpeed * Mathf.Cos(f), AvoidSpeed * Mathf.Sin(f), 0f);
				Destination = base.transform.position + new Vector3(AvoidSpeed * Mathf.Cos(f), AvoidSpeed * Mathf.Sin(f), 0f);
			}
		}
		Velocity *= Decay;
		base.transform.position += Velocity * Time.deltaTime;
		base.transform.forward = Camera.main.transform.forward;
		Sprite.color = gradient.Evaluate(Vector2.Distance(base.transform.position, Vector3.zero) / ColorDistance);
	}

	private void GetNewDestination()
	{
		Destination = InitialPosition + new Vector3(UnityEngine.Random.Range(0f - MovementRange, MovementRange), UnityEngine.Random.Range(0f - MovementRange, MovementRange), 0f - UnityEngine.Random.Range(0.5f, VerticalRange));
	}
}
