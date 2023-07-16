using System;
using System.Collections;
using System.Collections.Generic;
using Lamb.UI;
using Spine;
using Spine.Unity;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class EnemyHasShield : BaseMonoBehaviour
{
	public float layer1ShieldChanceOverride = -1f;

	public float layer2ShieldChanceOverride = -1f;

	public bool allowBeforeGotHeavyAttack;

	public bool dropBlackSoulsWhileShielded = true;

	private Health health;

	public SkeletonAnimation Spine;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string SkinWithShield;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string SkinNoShield;

	public float CameraShake = 2f;

	public int maxParticles = 10;

	public List<Sprite> ParticleChunks;

	private DropLootOnHit lootDropper;

	public static Action OnTutorialShown;

	private void OnEnable()
	{
		health = GetComponent<Health>();
	}

	public void AddShield()
	{
		if ((bool)health && health.HasShield)
		{
			return;
		}
		if (SetShieldSkin())
		{
			health.HasShield = true;
			health.BlackSoulOnHit = true;
			health.OnPenetrationHit += Health_OnPenetrationHit;
			health.OnHit += Health_OnHit;
			Debug.Log("Enemy given shield");
			if (dropBlackSoulsWhileShielded)
			{
				lootDropper = base.gameObject.GetComponent<DropLootOnHit>();
				if (lootDropper == null)
				{
					lootDropper = base.gameObject.AddComponent<DropLootOnHit>();
					lootDropper.LootToDrop = InventoryItem.ITEM_TYPE.BLACK_SOUL;
					lootDropper.DontDropOnPlayerFullAmmo = true;
					Debug.Log("Enemy given black soul loot dropper");
				}
			}
		}
		else
		{
			Debug.Log("Enemy not given shield");
		}
	}

	public bool SetShieldSkin()
	{
		Skin skin = new Skin("New Skin");
		if (skin != null && SkinWithShield != null && Spine.Skeleton != null && Spine.Skeleton.Data != null && Spine.Skeleton.Data.FindSkin(SkinWithShield) != null)
		{
			skin.AddSkin(Spine.Skeleton.Data.FindSkin(SkinWithShield));
			Spine.Skeleton.SetSkin(skin);
			Spine.skeleton.SetSlotsToSetupPose();
			return true;
		}
		return false;
	}

	public void SetNoShieldSkin()
	{
		new Skin("New Skin").AddSkin(Spine.Skeleton.Data.FindSkin(SkinWithShield));
		Spine.Skeleton.SetSkin(SkinNoShield);
		Spine.skeleton.SetSlotsToSetupPose();
	}

	private void OnDisable()
	{
		health.OnPenetrationHit -= Health_OnPenetrationHit;
		health.OnHit -= Health_OnHit;
	}

	private void Health_OnPenetrationHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (AttackType == Health.AttackTypes.Heavy && health.HasShield)
		{
			health.OnPenetrationHit -= Health_OnPenetrationHit;
			health.OnHit -= Health_OnHit;
			health.HasShield = false;
			UnityEngine.Object.Destroy(lootDropper);
			SetNoShieldSkin();
			DestroyShieldFX(Attacker);
			GameManager.GetInstance().HitStop();
		}
	}

	private void Health_OnHit(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind)
	{
		if (health.HasShield && DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.ShieldedEnemies))
		{
			StartCoroutine(WaitForEndOfFrame());
		}
	}

	private IEnumerator WaitForEndOfFrame()
	{
		health.untouchable = true;
		yield return new WaitForSecondsRealtime(0.1f);
		UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.ShieldedEnemies);
		uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
		{
			health.untouchable = false;
			Action onTutorialShown = OnTutorialShown;
			if (onTutorialShown != null)
			{
				onTutorialShown();
			}
		});
	}

	private void DestroyShieldFX(GameObject Attacker)
	{
		AudioManager.Instance.PlayOneShot("event:/enemy/shield_break", base.gameObject);
		CameraManager.shakeCamera(CameraShake);
		int num = -1;
		if (ParticleChunks != null && ParticleChunks.Count > 0)
		{
			while (++num < maxParticles)
			{
				Particle_Chunk.AddNew(base.transform.position, Utils.GetAngle(Attacker.transform.position, base.transform.position) + (float)UnityEngine.Random.Range(-20, 20), ParticleChunks);
			}
		}
	}
}
