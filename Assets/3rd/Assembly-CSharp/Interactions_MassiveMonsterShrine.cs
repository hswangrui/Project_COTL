using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;

public class Interactions_MassiveMonsterShrine : Interaction
{
	public SpriteRenderer Shrine;

	public GameObject ShrineNormal;

	public GameObject ShrineNight;

	public bool usedShrine;

	private bool CheckAvailibity()
	{
		if (DataManager.GetFollowerSkinUnlocked("MassiveMonster"))
		{
			return true;
		}
		return false;
	}

	public override void GetLabel()
	{
		base.GetLabel();
		if (CheckAvailibity())
		{
			usedShrine = true;
			Interactable = false;
			base.Label = "";
		}
		else if (TimeManager.IsNight)
		{
			Interactable = true;
			base.Label = ScriptLocalization.Interactions.Pray;
		}
		else
		{
			Interactable = false;
			base.Label = "";
		}
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		if (CheckAvailibity())
		{
			usedShrine = true;
			Interactable = false;
		}
		else
		{
			PhaseChanged();
			TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(PhaseChanged));
		}
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(PhaseChanged));
	}

	private new void OnDestroy()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(PhaseChanged));
	}

	private void PhaseChanged()
	{
		ShrineNormal.SetActive(false);
		ShrineNight.SetActive(false);
		if (TimeManager.IsNight)
		{
			ShrineNight.SetActive(true);
		}
		else
		{
			ShrineNormal.SetActive(true);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Interactable = false;
		StartCoroutine(PrayRoutine());
	}

	private IEnumerator PrayRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/rituals/blood_sacrifice", PlayerFarming.Instance.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 12f);
		PlayerFarming.Instance.CustomAnimation("pray", false);
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < 10; i++)
		{
			SoulCustomTarget.Create(base.gameObject, PlayerFarming.Instance.transform.position, Color.red, null);
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(0.5f);
		base.gameObject.transform.DOShakePosition(1f, new Vector3(0.25f, 0f, 0f));
		Shrine.DOColor(Color.red, 1f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 1f);
		AudioManager.Instance.PlayOneShot("event:/Stings/gamble_win", base.transform.position);
		yield return new WaitForSeconds(1f);
		Shrine.DOColor(Color.white, 0.2f);
		FollowerSkinCustomTarget.Create(base.transform.position, PlayerFarming.Instance.transform.position, 1f, "MassiveMonster", PickedUp);
		ShrineNormal.SetActive(true);
		ShrineNight.SetActive(false);
	}

	private void PickedUp()
	{
		DataManager.Instance.PrayedAtMassiveMonsterShrine = true;
		GameManager.GetInstance().OnConversationEnd();
		Interactable = false;
	}
}
