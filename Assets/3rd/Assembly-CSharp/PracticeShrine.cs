using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;

public class PracticeShrine : MonoBehaviour
{
	public Health Health;

	public SkeletonAnimation Spine;

	public FollowerLocation Boss;

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string DefeatedSkin;

	private void OnEnable()
	{
		Health.OnHit += OnHit;
		Health.OnDie += OnDie;
		Health.OnPoisonedHit += OnHit;
		if (DataManager.Instance.BossesCompleted.Contains(Boss))
		{
			Debug.Log("USE SKIN!!");
			Spine.skeleton.SetSkin(DefeatedSkin);
			Spine.skeleton.SetSlotsToSetupPose();
			Spine.AnimationState.Apply(Spine.skeleton);
		}
		Spine.AnimationState.Event += HandleEvent;
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

	private void OnDisable()
	{
		Health.OnHit -= OnHit;
		Health.OnPoisonedHit -= OnHit;
		Health.OnDie -= OnDie;
		Spine.AnimationState.Event -= HandleEvent;
	}

	private void OnHit(GameObject attacker, Vector3 attacklocation, Health.AttackTypes attacktype, bool frombehind)
	{
		int num = Mathf.FloorToInt((1f - Health.HP / Health.totalHP) * 4f);
		Spine.AnimationState.SetAnimation(0, num.ToString(), false);
		Debug.Log(num);
	}

	private void OnDie(GameObject attacker, Vector3 attacklocation, Health victim, Health.AttackTypes attacktype, Health.AttackFlags attackflags)
	{
		GameManager.GetInstance().StartCoroutine(ReformRoutine());
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
