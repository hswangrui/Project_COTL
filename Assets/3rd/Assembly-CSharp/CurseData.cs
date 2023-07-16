using UnityEngine;

[CreateAssetMenu(menuName = "Massive Monster/Curse Data")]
public class CurseData : EquipmentData
{
	public GameObject Prefab;

	public GameObject SecondaryPrefab;

	public string PerformActionAnimation;

	public string PerformActionAnimationLoop;

	public bool CanAim = true;

	public float Damage = 2f;

	public float Delay = 0.5f;

	public float CastingDuration = 0.1f;

	[Range(0f, 1f)]
	public float Chance = 1f;
}
