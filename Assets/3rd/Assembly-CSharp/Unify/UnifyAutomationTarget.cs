using Pathfinding;
using UnityEngine;

namespace Unify
{
	public class UnifyAutomationTarget : MonoBehaviour
	{
		public GameObject Player;

		public string PlayerObjectName;

		private Path Path;

		private bool ActiveTarget;

		private bool ReachedTarget;

		private Seeker seeker;

		private Vector3 CurrentTarget;

		private int CurrentTargetIndex;

		private float RadiusToTarget = 0.5f;

		private void Start()
		{
			seeker = GetComponent<Seeker>();
			Path = null;
			ActiveTarget = false;
			ReachedTarget = false;
			if (string.IsNullOrEmpty(PlayerObjectName))
			{
				PlayerObjectName = "Player - Prisoner(Clone)";
			}
		}

		private void Update()
		{
			if (Player == null)
			{
				Player = GameObject.FindWithTag("Player");
			}
			else if (ActiveTarget)
			{
				if (Vector3.Distance(base.transform.position, Player.transform.position) < 1f)
				{
					Debug.Log("Reached Unify Navigation Target");
					ReachedTarget = true;
					UnifyManager.Instance.AutomationClient.SendGameEvent("LeftStick:0.0;0.0");
					UnifyManager.Instance.AutomationClient.SendGameEvent("GAME_TARGET");
				}
				else if (Path == null)
				{
					seeker.StartPath(Player.transform.position, base.transform.position, OnPathComplete);
				}
				else if (Path.IsDone())
				{
					SendInputToAutomation(Path);
				}
			}
		}

		internal bool GetActive()
		{
			return ActiveTarget;
		}

		internal void SetActive(bool active)
		{
			ActiveTarget = active;
			Debug.Log("Unify Automation Target active: " + active);
		}

		public bool PlayerReachedTarget()
		{
			return ReachedTarget;
		}

		public void OnPathComplete(Path p)
		{
			if (p.error)
			{
				Debug.LogError("Unify Autamtion Target no path to player.");
				return;
			}
			Path = p;
			CurrentTargetIndex = 0;
			CurrentTarget = Path.vectorPath[CurrentTargetIndex];
		}

		private void SendInputToAutomation(Path Path)
		{
			if (Vector3.Distance(CurrentTarget, Player.transform.position) < RadiusToTarget && Path.vectorPath.Count > CurrentTargetIndex)
			{
				CurrentTarget = Path.vectorPath[++CurrentTargetIndex];
				Vector2 vector = CurrentTarget - Player.transform.position;
				vector.Normalize();
				UnifyManager.Instance.AutomationClient.SendGameEvent(string.Format("LeftStick:{0};{1}", vector.x, vector.y * -1f));
			}
		}
	}
}
