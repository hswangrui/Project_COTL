using MMRoomGeneration;
using UnityEngine;

public class LootEgg : MonoBehaviour
{
	[SerializeField]
	private GameObject followerToSpawn;

	private Health health;

	private void Start()
	{
		health = GetComponent<Health>();
		health.OnDie += Health_OnDie;
	}

	private void Health_OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		health.OnDie -= Health_OnDie;
		Object.Instantiate(followerToSpawn, base.transform.position, Quaternion.identity, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent).GetComponent<Interaction_FollowerSpawn>().Play("");
	}
}
