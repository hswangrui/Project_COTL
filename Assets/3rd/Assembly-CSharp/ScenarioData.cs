using Map;
using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/Scenario Data")]
public class ScenarioData : ScriptableObject
{
	public DungeonSandboxManager.ScenarioType ScenarioType;

	public int ScenarioIndex;

	public DungeonSandboxManager.ScenarioModifier ScenarioModifiers;

	public MapConfig MapConfig;

	public int FleeceType { get; set; }
}
