using System.Collections.Generic;
using UnityEngine;

public class BoidFlocking : BaseMonoBehaviour
{
	private static List<BoidFlocking> Boids = new List<BoidFlocking>();

	public float LoopingLimit = 6f;

	public bool EnableMovement;

	public float MoveSpeed = 1f;

	public Vector3 CurrentDirection;

	public Vector3 Velocity;

	public bool EnableAlignment;

	public float AlignmentDistance = 1f;

	public Vector3 AlignmentDirection;

	public bool EnableAvoidance;

	public float AvoidanceDistance = 1f;

	public Vector3 AvoidanceDirection;

	public bool EnableCohesion;

	public float CohesionDistance = 1f;

	public Vector3 CohesionDirection;

	public GameObject AttractionObject;

	public Vector3 AttractionDirection;

	private float Distance;

	private void OnEnable()
	{
		Boids.Add(this);
	}

	private void OnDisable()
	{
		Boids.Remove(this);
	}

	private void Update()
	{
		AlignmentDirection = Vector3.zero;
		int num = 0;
		if (EnableAlignment)
		{
			foreach (BoidFlocking boid in Boids)
			{
				Distance = Vector3.Distance(boid.transform.position, base.transform.position);
				if (boid != this && Distance < AlignmentDistance)
				{
					AlignmentDirection += boid.Velocity;
					num++;
				}
			}
		}
		if (num > 0)
		{
			AlignmentDirection /= (float)num;
			AlignmentDirection.Normalize();
		}
		CohesionDirection = Vector3.zero;
		num = 0;
		if (EnableCohesion)
		{
			foreach (BoidFlocking boid2 in Boids)
			{
				Distance = Vector3.Distance(boid2.transform.position, base.transform.position);
				if (boid2 != this && Distance < CohesionDistance)
				{
					CohesionDirection += boid2.transform.position;
					num++;
				}
			}
		}
		if (num > 0)
		{
			CohesionDirection /= (float)num;
			CohesionDirection -= base.transform.position;
			CohesionDirection.Normalize();
		}
		AvoidanceDirection = Vector3.zero;
		num = 0;
		if (EnableAvoidance)
		{
			foreach (BoidFlocking boid3 in Boids)
			{
				Distance = Vector3.Distance(boid3.transform.position, base.transform.position);
				if (boid3 != this && Distance < AvoidanceDistance)
				{
					AvoidanceDirection += boid3.transform.position;
					num++;
				}
			}
		}
		if (num > 0)
		{
			AvoidanceDirection /= (float)num;
			AvoidanceDirection -= base.transform.position;
			AvoidanceDirection *= -1f;
			AvoidanceDirection.Normalize();
		}
		CurrentDirection += CohesionDirection + AvoidanceDirection + AlignmentDirection;
		Velocity = CurrentDirection;
		CurrentDirection.Normalize();
		if (EnableMovement)
		{
			base.transform.position += CurrentDirection * MoveSpeed * Time.deltaTime;
		}
		if (Vector3.Distance(Vector3.zero, base.transform.position) > LoopingLimit)
		{
			base.transform.position -= base.transform.position * 2f;
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, AvoidanceDistance, Color.white);
		if (EnableAvoidance)
		{
			Utils.DrawCircleXY(base.transform.position + AvoidanceDirection, 0.1f, Color.red);
		}
		if (EnableCohesion)
		{
			Utils.DrawCircleXY(base.transform.position + CohesionDirection, 0.1f, Color.blue);
		}
		if (EnableAlignment)
		{
			Utils.DrawCircleXY(base.transform.position + AlignmentDirection, 0.1f, Color.yellow);
		}
		Utils.DrawCircleXY(base.transform.position + CurrentDirection, 0.1f, Color.green);
	}
}
