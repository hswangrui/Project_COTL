using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FollowerAdorationUI : MonoBehaviour
{
	private bool IsPlaying;

	public Follower follower;

	public GameObject BarContainer;

	public GameObject CompleteContainer;

	public BarControllerNonUI bc;

	public BarControllerNonUI BarController;

	public Sequence Sequence;

	public Follower Follower;

	private void Awake()
	{
		BarContainer.gameObject.SetActive(false);
	}

	private void Start()
	{
		if (follower != null && follower.Brain != null && follower.Brain.Stats != null && BarController != null)
		{
			BarController.SetBarSize(Follower.Brain.Stats.Adoration / Follower.Brain.Stats.MAX_ADORATION, false);
		}
		base.transform.localScale = Vector3.zero;
		if (follower.Brain == null)
		{
			Follower obj = follower;
			obj.OnFollowerBrainAssigned = (Action)Delegate.Combine(obj.OnFollowerBrainAssigned, new Action(AddListener));
		}
		else
		{
			AddListener();
		}
	}

	private void AddListener()
	{
		interaction_FollowerInteraction obj = follower.Interaction_FollowerInteraction;
		obj.OnGivenRewards = (Action)Delegate.Combine(obj.OnGivenRewards, new Action(SetObjects));
	}

	private void OnDestroy()
	{
		if (Sequence != null)
		{
			Sequence.Kill();
			Sequence = null;
		}
		base.transform.DOKill();
		if (follower != null)
		{
			if (follower.Interaction_FollowerInteraction != null)
			{
				interaction_FollowerInteraction obj = follower.Interaction_FollowerInteraction;
				obj.OnGivenRewards = (Action)Delegate.Remove(obj.OnGivenRewards, new Action(SetObjects));
			}
			Follower obj2 = follower;
			obj2.OnFollowerBrainAssigned = (Action)Delegate.Remove(obj2.OnFollowerBrainAssigned, new Action(AddListener));
		}
	}

	public void Show()
	{
		if (!IsPlaying && DataManager.Instance.ShowLoyaltyBars)
		{
			EnableBarContainerGameobject();
			base.transform.DOKill();
			BarController.SetBarSize(Follower.Brain.Stats.Adoration / Follower.Brain.Stats.MAX_ADORATION, false);
			if (follower.Brain.Stats.HasLevelledUp)
			{
				base.transform.localScale = Vector3.one * 0.7f;
			}
			else
			{
				base.transform.localScale = Vector3.zero;
				base.transform.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
			}
			SetObjects();
		}
	}

	private IEnumerator FlashLevelUpIcon()
	{
		while (follower.Brain.Stats.HasLevelledUp)
		{
			CompleteContainer.transform.DOKill();
			CompleteContainer.transform.DOPunchScale(new Vector3(0.15f, 0.15f), 0.5f);
			yield return new WaitForSeconds(2f);
		}
		CompleteContainer.transform.DOKill();
	}

	public void SetObjects()
	{
		if (follower.Brain.Location != 0)
		{
			EnableBarContainerGameobject();
			BarContainer.SetActive(!follower.Brain.Stats.HasLevelledUp);
			CompleteContainer.transform.DOKill();
			CompleteContainer.transform.DOPunchScale(new Vector3(0.15f, 0.15f), 0.5f);
			CompleteContainer.SetActive(follower.Brain.Stats.HasLevelledUp);
			if (follower.Brain.Stats.HasLevelledUp && base.transform.localScale != Vector3.one * 0.7f)
			{
				base.transform.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
			}
			if (follower.Brain.Stats.HasLevelledUp)
			{
				bc.enabled = false;
			}
		}
	}

	public void Hide()
	{
		if (IsPlaying || base.transform.localScale == Vector3.zero)
		{
			return;
		}
		if (follower.Brain.Stats.HasLevelledUp)
		{
			SetObjects();
			return;
		}
		base.transform.DOKill();
		base.transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true).SetEase(Ease.InBack)
			.OnComplete(delegate
			{
				BarContainer.gameObject.SetActive(false);
			});
	}

	private void Test(FollowerBrain.AdorationActions Action)
	{
		follower.Brain.AddAdoration(Action, null);
	}

	public IEnumerator IncreaseAdorationIE()
	{
		if (DataManager.Instance.ShowLoyaltyBars)
		{
			EnableBarContainerGameobject();
			IsPlaying = true;
			yield return new WaitForSeconds(0.01f);
			Debug.Log("INCREASE ADORATION!  " + Follower.Brain.Stats.Adoration + "  " + Follower.Brain.Stats.MAX_ADORATION);
			if (Sequence != null)
			{
				Sequence.Kill();
			}
			float num = 0f;
			Sequence = DOTween.Sequence();
			Sequence.SetUpdate(true);
			if (base.transform.localScale != Vector3.one * 0.7f)
			{
				base.transform.localScale = Vector3.zero;
				Sequence.Append(base.transform.DOScale(Vector3.one * 0.7f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true));
				num += 0.5f;
			}
			Sequence.AppendCallback(delegate
			{
				BarController.SetBarSize(Follower.Brain.Stats.Adoration / Follower.Brain.Stats.MAX_ADORATION, true, true);
			});
			Sequence.AppendInterval(1f).SetUpdate(true);
			num += 1f;
			Sequence.Append(base.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetUpdate(true));
			num += 0.5f;
			Sequence.Play();
			yield return new WaitForSecondsRealtime(num);
			BarContainer.gameObject.SetActive(false);
			IsPlaying = false;
		}
	}

	private void EnableBarContainerGameobject()
	{
		if (BarContainer != null)
		{
			BarContainer.gameObject.SetActive(true);
		}
		FaithCanvasOptimization componentInParent = GetComponentInParent<FaithCanvasOptimization>();
		if (componentInParent != null)
		{
			componentInParent.ActivateCanvas();
		}
	}
}
