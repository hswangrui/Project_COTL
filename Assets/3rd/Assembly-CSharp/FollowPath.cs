using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
public class FollowPath : BaseMonoBehaviour
{
	public delegate void Action();

	private Seeker seeker;

	private StateMachine state;

	private Vector2 targetLocation;

	public float maxSpeed = 0.05f;

	private float speed;

	public float vx;

	public float vy;

	public LayerMask layerToCheck;

	private List<Vector3> pathToFollow;

	private int currentWaypoint;

	public event Action NewPath;

	private void Start()
	{
	}

	private void Awake()
	{
		seeker = GetComponent<Seeker>();
		state = GetComponent<StateMachine>();
		if (state == null)
		{
			state = base.gameObject.AddComponent<StateMachine>();
		}
	}

	public void givePath(Vector3 targetLocation)
	{
		if (CheckLineOfSight(targetLocation, Vector2.Distance(base.transform.position, targetLocation)))
		{
			state.CURRENT_STATE = StateMachine.State.Moving;
			pathToFollow = new List<Vector3>();
			Vector3 item = (Vector3)AstarPath.active.GetNearest(targetLocation).node.position;
			pathToFollow.Add(item);
			currentWaypoint = 0;
			MouseManager.placeTarget(targetLocation);
			state.facingAngle = Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]);
		}
		else
		{
			seeker.StartPath(base.transform.position, targetLocation);
		}
		if (this.NewPath != null)
		{
			this.NewPath();
		}
	}

	private bool CheckLineOfSight(Vector3 pointToCheck, float distance)
	{
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, pointToCheck - base.transform.position, distance, layerToCheck);
		if (raycastHit2D.collider != null)
		{
			Debug.DrawRay(base.transform.position, (raycastHit2D.collider.transform.position - base.transform.position) * Vector2.Distance(base.transform.position, raycastHit2D.collider.transform.position), Color.yellow);
			return false;
		}
		Debug.DrawRay(base.transform.position, pointToCheck - base.transform.position, Color.green);
		return true;
	}

	public void startPath(Path p)
	{
		if (!p.error)
		{
			state.CURRENT_STATE = StateMachine.State.Moving;
			pathToFollow = new List<Vector3>();
			for (int i = 0; i < p.vectorPath.Count; i++)
			{
				pathToFollow.Add(p.vectorPath[i]);
			}
			currentWaypoint = 0;
			MouseManager.placeTarget(pathToFollow[pathToFollow.Count - 1]);
			state.facingAngle = Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]);
		}
	}

	protected virtual void OnEnable()
	{
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Combine(obj.pathCallback, new OnPathDelegate(startPath));
	}

	public void OnDisable()
	{
		seeker.CancelCurrentPathRequest();
		Seeker obj = seeker;
		obj.pathCallback = (OnPathDelegate)Delegate.Remove(obj.pathCallback, new OnPathDelegate(startPath));
	}

	private void Update()
	{
		if (pathToFollow == null)
		{
			return;
		}
		if (currentWaypoint > pathToFollow.Count)
		{
			speed += (0f - speed) / 4f;
			move();
			return;
		}
		if (currentWaypoint == pathToFollow.Count)
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			currentWaypoint++;
			pathToFollow = null;
			return;
		}
		if (speed < maxSpeed)
		{
			speed += (maxSpeed - speed) / 7f;
		}
		state.facingAngle = Utils.GetAngle(base.transform.position, pathToFollow[currentWaypoint]);
		move();
		if (Vector2.Distance(base.transform.position, pathToFollow[currentWaypoint]) <= ((currentWaypoint == pathToFollow.Count - 1) ? 0.1f : 0.5f))
		{
			currentWaypoint++;
		}
	}

	private void move()
	{
		Vector3 position = base.transform.position + new Vector3(speed * Mathf.Cos(state.facingAngle * ((float)Math.PI / 180f)), speed * Mathf.Sin(state.facingAngle * ((float)Math.PI / 180f)), 0f);
		base.transform.position = position;
	}

	private void FixedUpdate()
	{
		Rigidbody2D component = base.gameObject.GetComponent<Rigidbody2D>();
		if (!(component == null))
		{
			component.MovePosition(component.position + new Vector2(vx, vy) * Time.deltaTime);
		}
	}

	public static Quaternion FaceObject(Vector2 startingPosition, Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - startingPosition;
		return Quaternion.AngleAxis(Mathf.Atan2(vector.y, vector.x) * 57.29578f, Vector3.forward);
	}
}
