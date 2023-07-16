using UnityEngine;

public class MonsterHive : BaseMonoBehaviour
{
	public GameObject MonsterPrefab;

	public GameObject Den;

	public GameObject HoomanTrap;

	public Worshipper worshipper;

	private Health health;

	private void Start()
	{
		GameObject obj = Object.Instantiate(Resources.Load("Prefabs/Units/Wild Life/Big Spider") as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true);
		obj.transform.position = Den.transform.position;
		obj.GetComponent<BigSpider>().MonsterDen = this;
		health = GetComponent<Health>();
		health.OnDie += OnDie;
		health.OnHit += OnHit;
	}

	private void OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (worshipper != null)
		{
			worshipper.FreeFromHive();
		}
		worshipper = null;
	}

	private void OnDie(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		if (worshipper != null)
		{
			worshipper.FreeFromHive();
		}
		worshipper = null;
		int num = -1;
		while (++num < 1)
		{
			Object.Instantiate(Resources.Load("Prefabs/Units/Wild Life/Big Spider") as GameObject, GameObject.FindGameObjectWithTag("Unit Layer").transform, true).transform.position = Den.transform.position;
		}
	}

	private void Update()
	{
		if (worshipper != null)
		{
			worshipper.transform.position = HoomanTrap.transform.position + new Vector3(0f, 0f, -0.2f);
		}
	}
}
