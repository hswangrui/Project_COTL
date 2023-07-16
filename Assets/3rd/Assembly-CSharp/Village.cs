using System.Collections.Generic;
using UnityEngine;

public class Village : BaseMonoBehaviour
{
	public List<GameObject> VillageStructures;

	public GameObject Villager;

	public GameObject King;

	private List<Villager> Villagers = new List<Villager>();

	private void Start()
	{
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if ((i != 0 || j != 0) && Random.Range(0, 100) < 75)
				{
					int index = Random.Range(0, VillageStructures.Count);
					Object.Instantiate(VillageStructures[index], base.transform.position + new Vector3(i * 2, j * 2), Quaternion.identity);
				}
			}
		}
		Vector2 vector = Random.insideUnitCircle * 3f;
		Villager component = Object.Instantiate(King, base.transform.position + new Vector3(vector.x, vector.y), Quaternion.identity).GetComponent<Villager>();
		component.gameObject.GetComponent<Pet>().Owner = base.gameObject;
		Villagers.Add(component);
		for (int k = 0; k < 3; k++)
		{
			vector = Random.insideUnitCircle * 3f;
			component = Object.Instantiate(Villager, base.transform.position + new Vector3(vector.x, vector.y), Quaternion.identity).GetComponent<Villager>();
			component.gameObject.GetComponent<Pet>().Owner = base.gameObject;
			Villagers.Add(component);
		}
	}

	public void VillagersAttack(GameObject Attacker)
	{
		foreach (Villager villager in Villagers)
		{
			villager.formationFighter.enabled = true;
			villager.health.team = Health.Team.Team2;
		}
	}
}
