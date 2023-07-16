using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Task
{
	public Task_Type Type;

	public GameObject TargetObject;

	public StateMachine state;

	public TaskDoer t;

	public float Timer;

	public Vector3 TargetV3;

	public bool ClearOnComplete = true;

	public Task CurrentTask;

	public Task ParentTask;

	public string SpineSkin = "normal";

	public string SpineHatSlot = "";

	public virtual void StartTask(TaskDoer t, GameObject TargetObject)
	{
		this.t = t;
		state = t.gameObject.GetComponent<StateMachine>();
		state.CURRENT_STATE = StateMachine.State.Idle;
		this.TargetObject = TargetObject;
	}

	public virtual void ClearTask()
	{
		TargetObject = null;
		if (ClearOnComplete)
		{
			t.ClearTask();
		}
	}

	public static Task GetTaskByType(Task_Type TaskType)
	{
		switch (TaskType)
		{
		case Task_Type.FOLLOW:
			return new Task_Follow();
		case Task_Type.CHOP_WOOD:
			return new Task_ChopWood();
		case Task_Type.COMBAT:
			return new Task_Combat();
		case Task_Type.FARMER:
			return new Task_Farmer();
		case Task_Type.COOK:
			return new Task_Cook();
		case Task_Type.DANCER:
			return new Task_Dancer();
		case Task_Type.DEFENCE_TOWER:
			return new Task_DefenceTower();
		case Task_Type.COLLECT_DUNGEON_RESOURCES:
			return new Task_CollectDungeonResources();
		case Task_Type.ARCHER:
			return new Task_Archer();
		case Task_Type.SHIELD:
			return new Task_Combat_Shield();
		case Task_Type.ASTROLOGIST:
			return new Task_Astrologist();
		case Task_Type.BARRACKS:
			return new Task_Barracks();
		case Task_Type.IMPRISONED:
			return new Task_Imprisoned();
		default:
			return null;
		}
	}

	public virtual void TaskUpdate()
	{
	}

	public void PathToTargetObject(TaskDoer t, float FollowDistance)
	{
		float angle = Utils.GetAngle(t.transform.position, TargetObject.transform.position);
		Vector3 position = TargetObject.transform.position + new Vector3((0f - FollowDistance) * Mathf.Cos(angle * ((float)Math.PI / 180f)), (0f - FollowDistance) * Mathf.Sin(angle * ((float)Math.PI / 180f)), 0f);
		position = (Vector3)AstarPath.active.GetNearest(position, UnitObject.constraint).node.position;
		t.givePath(position);
	}

	public void PathToPosition(Vector3 Position)
	{
		Position = (Vector3)AstarPath.active.GetNearest(Position, UnitObject.constraint).node.position;
		t.givePath(Position);
	}

	public void PathToTargetObjectFormation(TaskDoer t, float FollowDistance)
	{
		float num = 90 * t.Position;
		TargetV3 = TargetObject.transform.position + new Vector3(1f * Mathf.Cos(num * ((float)Math.PI / 180f)), 1f * Mathf.Sin(num * ((float)Math.PI / 180f)), 0f);
		TargetV3 = (Vector3)AstarPath.active.GetNearest(TargetV3, UnitObject.constraint).node.position;
		t.givePath(TargetV3);
	}

	public virtual void DoAttack(float AttackRange, StateMachine.State NextState = StateMachine.State.RecoverFromAttack)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid/attack", t.transform.position);
		Timer = 0f;
		state.CURRENT_STATE = NextState;
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Enemies/Weapons/Swipe.prefab");
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			Swipe component = obj.Result.GetComponent<Swipe>();
			float facingAngle = state.facingAngle;
			Vector3 position = t.transform.position + new Vector3(AttackRange * Mathf.Cos(facingAngle * ((float)Math.PI / 180f)), AttackRange * Mathf.Sin(facingAngle * ((float)Math.PI / 180f)), -0.5f);
			component.Init(position, facingAngle, t.health.team, t.health, null, AttackRange);
		};
	}

	public void ClearCurrentTask()
	{
		if (CurrentTask != null)
		{
			CurrentTask.ClearTask();
		}
		CurrentTask = null;
	}
}
