using System;
using I2.Loc;

[Serializable]
public class Objectives_KillEnemies : ObjectivesData
{
	[Serializable]
	public class FinalizedData_KillEnemies : ObjectivesDataFinalized
	{
		public Enemy EnemyType;

		public int EnemiesKilledRequired;

		public override string GetText()
		{
			return string.Format(ScriptLocalization.Objectives.KillEnemies, UnitObject.GetLocalisedEnemyName(EnemyType), EnemiesKilledRequired, EnemiesKilledRequired);
		}
	}

	public Enemy enemyType;

	public int enemiesKilledBeforeObjectiveBegan = -1;

	public int enemiesKilledRequired;

	private int enemiesKilled
	{
		get
		{
			return DataManager.Instance.GetEnemiesKilled(enemyType) - enemiesKilledBeforeObjectiveBegan;
		}
	}

	public override string Text
	{
		get
		{
			return string.Format(ScriptLocalization.Objectives.KillEnemies, UnitObject.GetLocalisedEnemyName(enemyType), enemiesKilled, enemiesKilledRequired);
		}
	}

	public Objectives_KillEnemies()
	{
	}

	public Objectives_KillEnemies(string groupId, Enemy enemyType, int killsRequired, float questDuration)
		: base(groupId, questDuration)
	{
		Type = Objectives.TYPES.KILL_ENEMIES;
		this.enemyType = enemyType;
		enemiesKilledRequired = killsRequired;
	}

	public override void Init(bool initialAssigning)
	{
		base.Init(initialAssigning);
		if (enemiesKilledBeforeObjectiveBegan == -1)
		{
			enemiesKilledBeforeObjectiveBegan = DataManager.Instance.GetEnemiesKilled(enemyType);
		}
	}

	public override ObjectivesDataFinalized GetFinalizedData()
	{
		return new FinalizedData_KillEnemies
		{
			GroupId = GroupId,
			Index = Index,
			EnemyType = enemyType,
			EnemiesKilledRequired = enemiesKilledRequired,
			UniqueGroupID = UniqueGroupID
		};
	}

	protected override bool CheckComplete()
	{
		if (base.CheckComplete())
		{
			return enemiesKilled >= enemiesKilledRequired;
		}
		return false;
	}
}
