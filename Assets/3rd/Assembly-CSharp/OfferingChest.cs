using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class OfferingChest : MonoBehaviour
{
	public GameObject Container;

	public Interaction_Trader TraderInteraction;

	public GameObject chestOpen;

	public GameObject chestClosed;

	public AnimationCurve ChestFallCurve;

	private void OnEnable()
	{
		CheckAvailability();
		Interaction_BaseTeleporter.OnPlayerTeleportedIn = (Action)Delegate.Combine(Interaction_BaseTeleporter.OnPlayerTeleportedIn, new Action(CheckReveal));
	}

	private void OnDisable()
	{
		Interaction_BaseTeleporter.OnPlayerTeleportedIn = (Action)Delegate.Remove(Interaction_BaseTeleporter.OnPlayerTeleportedIn, new Action(CheckReveal));
	}

	private void CheckReveal()
	{
		if (DataManager.Instance.OnboardedOfferingChest && !DataManager.Instance.RevealOfferingChest)
		{
			StartCoroutine(RevealRoutine());
		}
	}

	private void Reveal()
	{
		StartCoroutine(RevealRoutine());
	}

	public void ChangeChest()
	{
		if (chestOpen.activeSelf)
		{
			AudioManager.Instance.PlayOneShot("event:/ui/close_menu", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/material/wood_barrel_impact", base.transform.position);
			chestOpen.SetActive(false);
			chestClosed.SetActive(true);
			chestClosed.transform.DOKill();
			chestClosed.transform.DOPunchScale(new Vector3(0.25f, -0.25f), 0.5f);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/ui/open_menu", PlayerFarming.Instance.transform.position);
			AudioManager.Instance.PlayOneShot("event:/material/wood_barrel_impact", base.transform.position);
			chestOpen.SetActive(true);
			chestClosed.SetActive(false);
			chestOpen.transform.DOKill();
			chestClosed.transform.DOPunchScale(new Vector3(0.25f, -0.25f), 0.5f);
		}
	}

	private IEnumerator RevealRoutine()
	{
		while (PlayerFarming.Instance.state.CURRENT_STATE != 0)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_mid", base.transform.position);
		DataManager.Instance.OnboardedOfferingChest = true;
		DataManager.Instance.RevealOfferingChest = true;
		CheckAvailability();
		Container.transform.localPosition = new Vector3(0f, 0f, -1f);
		Container.transform.localScale = Vector3.zero;
		TweenerCore<Vector3, Vector3, VectorOptions> t = Container.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
		yield return new WaitForSeconds(0.5f);
		Container.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(ChestFallCurve);
		yield return new WaitForSeconds(0.3f);
		AudioManager.Instance.PlayOneShot("event:/building/finished_wood", base.transform.position);
		t.Kill();
		Container.transform.localScale = new Vector3(1.5f, 0.5f);
		Container.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(base.transform.position, new Vector3(2f, 2f, 1f));
		CameraManager.instance.ShakeCameraForDuration(0.8f, 1f, 0.3f);
		yield return new WaitForSeconds(2f);
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
	}

	private void CheckAvailability()
	{
		if (!DataManager.Instance.RevealOfferingChest)
		{
			TraderInteraction.enabled = false;
			Container.gameObject.SetActive(false);
		}
		else
		{
			TraderInteraction.enabled = true;
			Container.gameObject.SetActive(true);
		}
	}
}
