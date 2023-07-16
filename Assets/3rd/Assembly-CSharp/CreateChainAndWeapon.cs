using UnityEngine;

public class CreateChainAndWeapon : BaseMonoBehaviour
{
	public bool SpawnChain = true;

	public GameObject ChainPrefab;

	public GameObject WeaponPrefab;

	public GameObject ChainPoint;

	private Chain Chain;

	[HideInInspector]
	public WeaponPet Weapon;

	private void OnEnable()
	{
		Weapon = Object.Instantiate(WeaponPrefab, base.transform.position, Quaternion.identity).GetComponent<WeaponPet>();
		if (SpawnChain)
		{
			Chain = Object.Instantiate(ChainPrefab, base.transform.position, Quaternion.identity).GetComponent<Chain>();
			Chain.FixedPoint1 = ChainPoint.transform;
			Chain.FixedPoint2 = Weapon.ChainPoint;
		}
	}
}
