using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusStatueDevotionDamage : BaseMonoBehaviour
{
	public CameraInclude cameraInclude;

	public Transform DevotionPosition;

	public bool DealDamage = true;

	public int DevotionToGivePerSoul = 2;

	public float SoulsToGive = 30f;

	private bool Activated;

	private Health health;

	private int HP = 3;

	public List<Transform> ShakeTransforms;

	private Vector2[] Shake = new Vector2[0];

	private void OnEnable()
	{
		health = GetComponent<Health>();
		health.OnHit += Health_OnHit;
		Shake = new Vector2[ShakeTransforms.Count];
	}

	private void OnDisable()
	{
		health.OnHit -= Health_OnHit;
	}

	private void Health_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		Health component = Attacker.GetComponent<Health>();
		if (!(component == null) && !Activated)
		{
			HP--;
			ShakeImages();
			HideImages();
			if (HP <= 0)
			{
				Activated = true;
				StartCoroutine(GiveDevotionRoutine(component, Attacker, true));
			}
			else
			{
				StartCoroutine(GiveDevotionRoutine(component, Attacker, false));
			}
		}
	}

	private void Update()
	{
		int num = -1;
		while (++num < Shake.Length)
		{
			if (ShakeTransforms[num] != base.transform)
			{
				Shake[num].y += (0f - Shake[num].x) * 0.2f;
				Shake[num].x += (Shake[num].y *= 0.9f) * Time.deltaTime * 60f;
				ShakeTransforms[num].localPosition = new Vector3(Shake[num].x, ShakeTransforms[num].localPosition.y, ShakeTransforms[num].localPosition.z);
			}
		}
	}

	private void TestImages(int HP)
	{
		this.HP = HP;
		HideImages();
	}

	private void HideImages()
	{
		int num = -1;
		while (++num < ShakeTransforms.Count)
		{
			ShakeTransforms[num].gameObject.SetActive((float)num >= (1f - (float)HP / 3f) * (float)ShakeTransforms.Count);
		}
	}

	public void ShakeImages()
	{
		int num = -1;
		while (++num < Shake.Length)
		{
			Shake[num] = new Vector2(Random.Range(0.1f, 0.2f) * (float)((!((double)Random.value < 0.5)) ? 1 : (-1)), 0f);
		}
	}

	private IEnumerator GiveDevotionRoutine(Health AttackerHealth, GameObject Target, bool GiveDevotion)
	{
		if (DealDamage)
		{
			AttackerHealth.DealDamage(1f, base.gameObject, Vector3.Lerp(base.transform.position, AttackerHealth.transform.position, 0.8f));
		}
		if (!GiveDevotion)
		{
			yield break;
		}
		health.enabled = false;
		yield return new WaitForSeconds(0.5f);
		cameraInclude.enabled = false;
		float NumSouls = 0f;
		while (true)
		{
			float num = NumSouls + 1f;
			NumSouls = num;
			if (!(num <= SoulsToGive) || Target == null)
			{
				break;
			}
			if (GameManager.HasUnlockAvailable() && !DataManager.Instance.DeathCatBeaten)
			{
				SoulCustomTarget.Create(Target, DevotionPosition.position, Color.white, delegate
				{
					Inventory.AddItem(10, DevotionToGivePerSoul);
				});
			}
			else
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position + Vector3.back, 0f).SetInitialSpeedAndDiraction(8f + Random.Range(-0.5f, 1f), 270 + Random.Range(-90, 90));
			}
			yield return new WaitForSeconds(0.2f - 0.2f * (NumSouls / SoulsToGive));
		}
	}
}
