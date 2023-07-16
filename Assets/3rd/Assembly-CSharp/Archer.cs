using UnityEngine;

public class Archer : FormationFighter
{
	public GameObject Arrow;

	public float MinimumRange = 4f;

	private Task_Archer ArcherTask;

	public override void SetTask()
	{
		AddNewTask(Task_Type.ARCHER, false);
		ArcherTask = CurrentTask as Task_Archer;
		ArcherTask.Init(DetectEnemyRange, AttackRange, LoseEnemyRange, PreAttackDuration, PostAttackDuration, MinimumRange, Arrow);
		BreakAttacksOnHit = true;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (ArcherTask != null)
		{
			ArcherTask.ClearTarget();
		}
	}
}
