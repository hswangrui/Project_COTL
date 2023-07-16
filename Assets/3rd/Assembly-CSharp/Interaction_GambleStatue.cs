using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

public class Interaction_GambleStatue : Interaction
{
	private bool Activating;

	public GameObject PayResourceReceivePosition;

	public int Cost = 5;

	public List<GameObject> ResourceListLeft;

	public List<GameObject> ResourceListRight;

	public List<InventoryItem.ITEM_TYPE> RandomResourceTypes = new List<InventoryItem.ITEM_TYPE>();

	private InventoryItem.ITEM_TYPE ResourceType = InventoryItem.ITEM_TYPE.LOG;

	public CameraInclude CameraInclude;

	public SkeletonAnimation Spine;

	[SpineAnimation("", "Spine", true, false)]
	public string IdleAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string FailedAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string SingleRewardAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string SingleRewardAnimationLoop;

	[SpineAnimation("", "Spine", true, false)]
	public string DoubleRewardAnimation;

	[SpineAnimation("", "Spine", true, false)]
	public string DoubleRewardAnimationLoop;

	[SpineAnimation("", "Spine", true, false)]
	public string CalculatingLoop;

	private string sString;

	private void Start()
	{
		SetResources();
	}

	private void SetResources()
	{
		ResourceType = RandomResourceTypes[Random.Range(0, RandomResourceTypes.Count)];
		foreach (GameObject item in ResourceListLeft)
		{
			item.GetComponent<InventoryItemDisplay>().SetImage(ResourceType);
		}
		foreach (GameObject item2 in ResourceListRight)
		{
			item2.GetComponent<InventoryItemDisplay>().SetImage(ResourceType);
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sString = ScriptLocalization.Interactions.MakeOffering;
	}

	public override void GetLabel()
	{
		base.Label = (Activating ? "" : (sString + " " + CostFormatter.FormatCost(InventoryItem.ITEM_TYPE.SOUL, Cost)));
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (Inventory.Souls > Cost)
		{
			StartCoroutine(PayDevotion());
		}
		else
		{
			MonoSingleton<Indicator>.Instance.PlayShake();
		}
	}

	private IEnumerator PayDevotion()
	{
		Activating = true;
		state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		PlayerFarming.Instance.simpleSpineAnimator.Animate("devotion/devotion-start", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("devotion/devotion-loop", 0, true, 0f);
		yield return new WaitForSeconds(1f);
		float SoulsToGive = Cost;
		float NumSouls = 0f;
		while (true)
		{
			float num = NumSouls + 1f;
			NumSouls = num;
			if (!(num <= SoulsToGive))
			{
				break;
			}
			if (NumSouls == SoulsToGive)
			{
				SoulCustomTarget.Create(PayResourceReceivePosition, PlayerFarming.Instance.CameraBone.transform.position, Color.white, delegate
				{
					Gamble();
				});
				yield return new WaitForSeconds(0.5f);
			}
			else
			{
				SoulCustomTarget.Create(PayResourceReceivePosition, PlayerFarming.Instance.CameraBone.transform.position, Color.white, null);
				yield return new WaitForSeconds(0.1f - 0.1f * (NumSouls / SoulsToGive));
			}
		}
		PlayerFarming.Instance.simpleSpineAnimator.Animate("devotion/devotion-stop", 0, false);
		PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
	}

	private void Gamble()
	{
		StartCoroutine(GambleRoutine());
	}

	private IEnumerator GambleRoutine()
	{
		CameraInclude.enabled = false;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PayResourceReceivePosition, 7f);
		Spine.AnimationState.SetAnimation(0, CalculatingLoop, true);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.3f, 1.5f);
		yield return new WaitForSeconds(2f);
		CameraManager.shakeCamera(0.5f);
		Spine.AnimationState.SetAnimation(0, IdleAnimation, true);
		yield return new WaitForSeconds(1f);
		switch (Random.Range(0, 4))
		{
		case 0:
		case 1:
			StartCoroutine(FailRoutine());
			break;
		case 2:
			StartCoroutine(SingleRewardRoutine());
			break;
		case 3:
			StartCoroutine(DoubleRewardRoutine());
			break;
		}
	}

	private IEnumerator SingleRewardRoutine()
	{
		Spine.AnimationState.SetAnimation(0, SingleRewardAnimation, false);
		Spine.AnimationState.AddAnimation(0, SingleRewardAnimationLoop, true, 0f);
		yield return new WaitForSeconds(0.4f);
		GameManager.GetInstance().OnConversationEnd();
		foreach (GameObject item in ResourceListLeft)
		{
			item.SetActive(false);
			InventoryItem.Spawn(ResourceType, 1, item.transform.position + new Vector3(0f, -0.1f, 0f), 0f);
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator DoubleRewardRoutine()
	{
		Spine.AnimationState.SetAnimation(0, DoubleRewardAnimation, false);
		Spine.AnimationState.AddAnimation(0, DoubleRewardAnimationLoop, true, 0f);
		yield return new WaitForSeconds(0.4f);
		GameManager.GetInstance().OnConversationEnd();
		int i = -1;
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num >= ResourceListLeft.Count)
			{
				break;
			}
			ResourceListLeft[i].SetActive(false);
			InventoryItem.Spawn(ResourceType, 1, ResourceListLeft[i].transform.position + new Vector3(0f, -0.1f, 0f), 0f);
			ResourceListRight[i].SetActive(false);
			InventoryItem.Spawn(ResourceType, 1, ResourceListRight[i].transform.position + new Vector3(0f, -0.1f, 0f), 0f);
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator FailRoutine()
	{
		Cost++;
		Spine.AnimationState.SetAnimation(0, FailedAnimation, false);
		Spine.AnimationState.AddAnimation(0, IdleAnimation, true, 0f);
		yield return new WaitForSeconds(1.5f);
		Activating = false;
		GameManager.GetInstance().OnConversationEnd();
		CameraInclude.enabled = true;
	}
}
