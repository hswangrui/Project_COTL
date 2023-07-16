using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using Lamb.UI.FollowerSelect;
using MMTools;
using Spine.Unity;
using src.Extensions;
using UnityEngine;

public class Interaction_SacrificeFollowerToGold : Interaction
{
	public List<Interaction_SimpleConversation> StatueConversations = new List<Interaction_SimpleConversation>();

	public SkeletonAnimation Spine;

	public List<GameObject> Statues = new List<GameObject>();

	private string sLabel;

	public Interaction_KeyPiece KeyPiecePrefab;

	private void Start()
	{
		UpdateLocalisation();
		int num = -1;
		while (++num < Statues.Count)
		{
			Statues[num].SetActive(num < DataManager.Instance.MidasFollowerStatueCount);
		}
		Spine.gameObject.SetActive(false);
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabel = ScriptLocalization.Interactions.SacrificeFollower;
	}

	public override void GetLabel()
	{
		if (DataManager.Instance.MidasFollowerStatueCount < 4)
		{
			base.Label = sLabel;
		}
		else
		{
			base.Label = "";
		}
	}

	public override void OnInteract(StateMachine state)
	{
		if (FollowerBrain.AllBrains.Count > 0)
		{
			base.OnInteract(state);
			PlayerFarming.Instance.GoToAndStop(base.transform.position + new Vector3(-1f, -2.5f), base.gameObject, false, false, delegate
			{
				StartCoroutine(SacrificeFollowerRoutine());
			});
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator SacrificeFollowerRoutine()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject);
		yield return new WaitForSeconds(0.5f);
		UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate();
		//followerSelectInstance.VotingType = TwitchVoting.VotingType.SACRIFICE_TO_MIDAS;
		followerSelectInstance.Show(FollowerBrain.AllAvailableFollowerBrains());
		UIFollowerSelectMenuController uIFollowerSelectMenuController = followerSelectInstance;
		uIFollowerSelectMenuController.OnFollowerSelected = (Action<FollowerInfo>)Delegate.Combine(uIFollowerSelectMenuController.OnFollowerSelected, (Action<FollowerInfo>)delegate(FollowerInfo followerInfo)
		{
			AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
			StartCoroutine(SpawnFollower(followerInfo.ID));
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController2 = followerSelectInstance;
		uIFollowerSelectMenuController2.OnHidden = (Action)Delegate.Combine(uIFollowerSelectMenuController2.OnHidden, (Action)delegate
		{
			followerSelectInstance = null;
		});
		UIFollowerSelectMenuController uIFollowerSelectMenuController3 = followerSelectInstance;
		uIFollowerSelectMenuController3.OnCancel = (Action)Delegate.Combine(uIFollowerSelectMenuController3.OnCancel, (Action)delegate
		{
			GameManager.GetInstance().OnConversationEnd();
		});
	}

	private IEnumerator SpawnFollower(int ID)
	{
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		AudioManager.Instance.SetMusicRoomID(1, "drum_layer");
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject);
		Spine.gameObject.SetActive(true);
		while (Spine.AnimationState == null)
		{
			yield return null;
		}
		Spine.AnimationState.SetAnimation(0, "enter", false);
		Spine.AnimationState.AddAnimation(0, "animation", true, 0f);
		yield return new WaitForSeconds(1f);
		FollowerManager.SpawnedFollower spawnedFollower = FollowerManager.SpawnCopyFollower(FollowerManager.FindFollowerInfo(ID), base.transform.position, base.transform.parent, PlayerFarming.Location);
		spawnedFollower.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
		spawnedFollower.Follower.Spine.AnimationState.SetAnimation(1, "spawn-in", false);
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "Reactions/react-scared-long", false, 0f);
		spawnedFollower.Follower.Spine.AnimationState.AddAnimation(1, "idle-sad", true, 0f);
		FollowerManager.RemoveFollowerBrain(ID);
		GameManager.GetInstance().OnConversationNext(spawnedFollower.Follower.gameObject);
		yield return new WaitForSeconds(2f);
		Spine.AnimationState.SetAnimation(0, "warning", true);
		float Progress = 0f;
		float Duration = 3.75f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			GameManager.GetInstance().CameraSetTargetZoom(Mathf.Lerp(9f, 4f, Mathf.SmoothStep(0f, 1f, Progress / Duration)));
			CameraManager.shakeCamera(0.3f + 0.5f * (Progress / Duration));
			yield return null;
		}
		CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.3f);
		FollowerManager.CleanUpCopyFollower(spawnedFollower);
		AudioManager.Instance.PlayOneShot("event:/followers/turn_to_gold_sequence", base.transform.position);
		GameObject s = Statues[DataManager.Instance.MidasFollowerStatueCount];
		s.SetActive(true);
		s.transform.localScale = Vector3.one * 1.2f;
		s.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		Vector3 TargetPosition = s.transform.position;
		s.transform.position = base.transform.position;
		BiomeConstants.Instance.EmitSmokeInteractionVFX(s.transform.position, new Vector3(2f, 2f, 1f));
		Spine.AnimationState.SetAnimation(0, "idle", true);
		GameManager.GetInstance().OnConversationNext(s, 5f);
		GameManager.GetInstance().HitStop();
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().OnConversationNext(s, 8f);
		Spine.AnimationState.SetAnimation(0, "warning", true);
		s.transform.DOMove(base.transform.position + Vector3.back, 1f).SetEase(Ease.OutBack);
		yield return new WaitForSeconds(0.5f);
		s.transform.DOMove(TargetPosition + Vector3.back, 2f).SetEase(Ease.OutBack);
		yield return new WaitForSeconds(1.5f);
		s.transform.DOMove(TargetPosition, 0.5f).SetEase(Ease.InBack).OnComplete(delegate
		{
			BiomeConstants.Instance.EmitSmokeInteractionVFX(s.transform.position, new Vector3(2f, 2f, 1f));
			CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.3f);
			Spine.AnimationState.SetAnimation(0, "idle", true);
		});
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		StatueConversations[DataManager.Instance.MidasFollowerStatueCount].gameObject.SetActive(true);
		StatueConversations[DataManager.Instance.MidasFollowerStatueCount].Callback.AddListener(delegate
		{
			base.HasChanged = true;
			DataManager.Instance.MidasFollowerStatueCount++;
			base.enabled = true;
			StartCoroutine(GiveKeyPieceRoutine());
		});
		base.enabled = false;
		yield return new WaitForEndOfFrame();
		HUD_Manager.Instance.Hide(true);
	}

	private IEnumerator GiveKeyPieceRoutine()
	{
		while (MMConversation.isPlaying)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew(true, true);
		Interaction_KeyPiece KeyPiece = UnityEngine.Object.Instantiate(KeyPiecePrefab, Spine.transform.position + Vector3.back * 0.75f, Quaternion.identity, base.transform.parent);
		KeyPiece.transform.localScale = Vector3.zero;
		KeyPiece.transform.DOScale(Vector3.one, 2f).SetEase(Ease.OutBack);
		GameManager.GetInstance().OnConversationNext(KeyPiece.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		yield return new WaitForSeconds(1f);
		KeyPiece.transform.DOMove(PlayerFarming.Instance.transform.position + Vector3.back * 0.5f, 1f).SetEase(Ease.InBack);
		yield return new WaitForSeconds(1f);
		yield return null;
		Spine.AnimationState.SetAnimation(0, "exit", false);
		GameManager.GetInstance().OnConversationEnd(false);
		KeyPiece.OnInteract(PlayerFarming.Instance.state);
	}
}
