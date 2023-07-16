using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class StealDevotionShrine : MonoBehaviour
{
	public Health Health;

	public SkeletonAnimation Spine;

	private bool BossBeatn;

	public bool droppedStone;

	public GameObject VortexObject;

	public GameObject ContainerToHide;

	private void OnEnable()
	{
		Health.OnHit += OnHit;
		Health.OnDie += OnDie;
		Health.OnPoisonedHit += OnHit;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Combine(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
		Spine.AnimationState.Event += HandleEvent;
	}

	private void OnDisable()
	{
		Health.OnHit -= OnHit;
		Health.OnPoisonedHit -= OnHit;
		Health.OnDie -= OnDie;
		Spine.AnimationState.Event -= HandleEvent;
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
	}

	private void OnPlayerLocationSet()
	{
		LocationManager.OnPlayerLocationSet = (Action)Delegate.Remove(LocationManager.OnPlayerLocationSet, new Action(OnPlayerLocationSet));
		BossBeatn = DataManager.Instance.DungeonCompleted(PlayerFarming.Location, GameManager.Layer2);
		string text = "";
		switch (PlayerFarming.Location)
		{
		case FollowerLocation.Dungeon1_1:
			text = "1";
			break;
		case FollowerLocation.Dungeon1_2:
			text = "2";
			break;
		case FollowerLocation.Dungeon1_3:
			text = "3";
			break;
		case FollowerLocation.Dungeon1_4:
			text = "4";
			break;
		}
		Spine.skeleton.SetSkin(text + ((BossBeatn && !DungeonSandboxManager.Active) ? "_defeated" : "") + ((DataManager.Instance.DeathCatBeaten && !DungeonSandboxManager.Active) ? "_2" : ""));
		Spine.skeleton.SetSlotsToSetupPose();
		Spine.AnimationState.Apply(Spine.skeleton);
	}

	private void HandleEvent(TrackEntry trackEntry, global::Spine.Event e)
	{
		if (e.Data.Name == "piece")
		{
			AudioManager.Instance.PlayOneShot("event:/material/stone_impact", base.gameObject);
		}
		else if (e.Data.Name == "final_piece")
		{
			AudioManager.Instance.PlayOneShot("event:/building/finished_stone", base.gameObject);
		}
	}

	private void OnHit(GameObject attacker, Vector3 attacklocation, Health.AttackTypes attacktype, bool frombehind)
	{
		int num = Mathf.FloorToInt((1f - Health.HP / Health.totalHP) * 4f);
		Spine.AnimationState.SetAnimation(0, num.ToString(), false);
		Debug.Log(num);
	}

	private void OnDie(GameObject attacker, Vector3 attacklocation, Health victim, Health.AttackTypes attacktype, Health.AttackFlags attackflags)
	{
		if (DungeonSandboxManager.Active)
		{
			FaithAmmo.Reload();
		}
		if (!BossBeatn)
		{
			if (!droppedStone)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.STONE, UnityEngine.Random.Range(1, 3), base.transform.position);
				droppedStone = true;
			}
			GameManager.GetInstance().StartCoroutine(ReformRoutine());
		}
		else if (!DungeonSandboxManager.Active)
		{
			GetComponent<CircleCollider2D>().enabled = false;
			ContainerToHide.SetActive(false);
			VortexObject.SetActive(true);
		}
		Spine.AnimationState.SetAnimation(0, "4", false);
		AudioManager.Instance.PlayOneShot("event:/player/Curses/explosive_shot", base.transform.position);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(base.transform.position);
		CameraManager.shakeCamera(5f);
	}

	private IEnumerator ReformRoutine()
	{
		yield return null;
		Spine.AnimationState.SetAnimation(0, "4", false);
		yield return new WaitForSeconds(2f);
		Spine.AnimationState.SetAnimation(0, "reform", false);
		Spine.AnimationState.AddAnimation(0, "0", false, 0f);
		yield return new WaitForSeconds(6.133333f);
		Health.HP = Health.totalHP;
		Health.enabled = true;
	}
}
