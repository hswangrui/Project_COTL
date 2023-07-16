using System.Collections;
using Spine.Unity;
using UnityEngine;

public class Interaction_Pyre : Interaction
{
	public GameObject ChoiceIndicator;

	public GameObject CameraObject;

	private bool Activated;

	private string sSacrificialPyre;

	private string sRescue;

	private string sRescueSubtitle;

	private string sLightPyre;

	private string sLightPyreSubtitle;

	private ChoiceIndicator c;

	private FollowerInfo _followerInfo;

	private FollowerOutfit _outfit;

	public SkeletonAnimation followerSpine;

	public GameObject Fire;

	private void Start()
	{
		HasSecondaryInteraction = false;
		UpdateLocalisation();
		InitFollower();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void OnInteract(StateMachine state)
	{
		Activated = true;
		base.OnInteract(state);
		GameObject gameObject = Object.Instantiate(ChoiceIndicator, GameObject.FindWithTag("Canvas").transform);
		c = gameObject.GetComponent<ChoiceIndicator>();
		c.Show(sRescue, sRescueSubtitle, sLightPyre, sLightPyreSubtitle, CutDown, LightPyre, base.transform.position);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(CameraObject, 8f);
	}

	private new void Update()
	{
		if (c != null)
		{
			c.UpdatePosition(base.transform.position);
		}
	}

	private void InitFollower()
	{
		_followerInfo = FollowerInfo.NewCharacter(PlayerFarming.Location);
		_outfit = new FollowerOutfit(_followerInfo);
		_outfit.SetOutfit(followerSpine, false);
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sSacrificialPyre);
	}

	public void CutDown()
	{
		StartCoroutine(CutDownRoutine());
	}

	private IEnumerator CutDownRoutine()
	{
		float duration = followerSpine.AnimationState.SetAnimation(0, "spider-out", false).Animation.Duration;
		yield return new WaitForSeconds(duration);
		duration = followerSpine.AnimationState.SetAnimation(0, "spider-pop2", false).Animation.Duration;
		CameraManager.shakeCamera(0.5f);
		yield return new WaitForSeconds(duration);
		FollowerManager.CreateNewFollower(_followerInfo, base.transform.position + Vector3.back * 3f, true);
		Object.Destroy(followerSpine.gameObject);
		GameManager.GetInstance().OnConversationEnd();
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	public void LightPyre()
	{
		StartCoroutine(LightPyreRoutine());
	}

	private IEnumerator LightPyreRoutine()
	{
		GameManager.GetInstance().OnConversationNext(CameraObject, 5f);
		yield return new WaitForSeconds(1.5f);
		Fire.SetActive(true);
		CameraManager.shakeCamera(0.8f);
		GameManager.GetInstance().OnConversationNext(CameraObject, 7f);
		yield return new WaitForSeconds(2f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		if (DataManager.Instance.FirstTimeResurrecting)
		{
			UIAbilityUnlock.Play(UIAbilityUnlock.Ability.PyreResurrect);
			DataManager.Instance.FirstTimeResurrecting = false;
			yield return new WaitForSeconds(0.5f);
		}
		ResurrectOnHud.ResurrectionType = ResurrectionType.Pyre;
	}
}
