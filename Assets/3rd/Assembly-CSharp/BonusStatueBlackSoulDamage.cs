using System.Collections;
using UnityEngine;

public class BonusStatueBlackSoulDamage : BaseMonoBehaviour
{
	private Health health;

	private void OnEnable()
	{
		health = GetComponent<Health>();
		health.OnHit += Health_OnHit;
	}

	private void OnDisable()
	{
		health.OnHit -= Health_OnHit;
	}

	private void Health_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		Health component = Attacker.GetComponent<Health>();
		if (!(component == null))
		{
			StartCoroutine(GiveBlackSoulsRoutine(component, Attacker));
		}
	}

	private IEnumerator GiveBlackSoulsRoutine(Health AttackerHealth, GameObject Target)
	{
		yield return new WaitForSeconds(0.25f);
		AttackerHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, AttackerHealth.transform.position, 0.8f));
		yield return new WaitForSeconds(1f);
		float SoulsToGive = 0f;
		while (true)
		{
			float num = SoulsToGive + 1f;
			SoulsToGive = num;
			if (!(num <= 20f) || Target == null)
			{
				break;
			}
			if (GameManager.HasUnlockAvailable() && !DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(Target, base.transform.position, Color.black, GiveDevotion, 0.2f);
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
			yield return new WaitForSeconds(0.2f - 0.2f * (SoulsToGive / 20f));
		}
	}

	private void GiveDevotion()
	{
		Inventory.AddItem(30, 1);
	}
}
