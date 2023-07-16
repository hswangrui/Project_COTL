using System.Collections;
using Spine.Unity;
using UnityEngine;

public class Interaction_Chalice : Interaction
{
	public enum DrinkType
	{
		Poison,
		Vitality
	}

	public SkeletonAnimation Spine;

	private string sString;

	[HideInInspector]
	public bool Activating;

	public DrinkType Drink;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
	}

	public override void GetLabel()
	{
		base.Label = (Activating ? "" : sString);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StartCoroutine(DrinkFromChalice());
	}

	private IEnumerator DrinkFromChalice()
	{
		Activating = true;
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		Spine.gameObject.SetActive(false);
		switch (Drink)
		{
		case DrinkType.Poison:
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
			state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			yield return new WaitForEndOfFrame();
			PlayerFarming.Instance.simpleSpineAnimator.Animate("chalice-drink-bad", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			yield return new WaitForSeconds(3.5f);
			CameraManager.shakeCamera(0.5f);
			yield return new WaitForSeconds(2.3333333f);
			GameManager.GetInstance().OnConversationEnd();
			yield return new WaitForSeconds(0.5f);
			if (PlayerFarming.Instance.GetComponent<HealthPlayer>().HP - 4f <= 0f)
			{
				PlayerFarming.Instance.health.DealDamage(4f, base.gameObject, base.transform.position);
			}
			else
			{
				PlayerFarming.Instance.health.HP -= 4f;
			}
			break;
		case DrinkType.Vitality:
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 4f);
			state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			yield return new WaitForEndOfFrame();
			PlayerFarming.Instance.simpleSpineAnimator.Animate("chalice-drink-good", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
			yield return new WaitForSeconds(3.5f);
			CameraManager.shakeCamera(0.5f);
			yield return new WaitForSeconds(2.3333333f);
			GameManager.GetInstance().OnConversationEnd();
			yield return new WaitForSeconds(0.5f);
			PlayerFarming.Instance.GetComponent<HealthPlayer>().BlueHearts += 4f;
			break;
		}
		Drink++;
	}
}
