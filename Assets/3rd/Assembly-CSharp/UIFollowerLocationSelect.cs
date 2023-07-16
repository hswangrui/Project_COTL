using System;
using UnityEngine;
using UnityEngine.UI;

public class UIFollowerLocationSelect : BaseMonoBehaviour
{
	public UI_NavigatorSimple UINav;

	public Transform BaseContainer;

	public Transform LocationContainer;

	public GameObject IconPrefab;

	private FollowerInformationBox followerInfoBox;

	private GameObject g;

	private FollowerInformationBox icon;

	private void OnEnable()
	{
		UI_NavigatorSimple uINav = UINav;
		uINav.OnSelectDown = (Action)Delegate.Combine(uINav.OnSelectDown, new Action(OnSelect));
		UI_NavigatorSimple uINav2 = UINav;
		uINav2.OnDefaultSetComplete = (Action)Delegate.Combine(uINav2.OnDefaultSetComplete, new Action(OnDefaultSetComplete));
		UI_NavigatorSimple uINav3 = UINav;
		uINav3.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Combine(uINav3.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelection));
		UI_NavigatorSimple uINav4 = UINav;
		uINav4.OnCancelDown = (Action)Delegate.Combine(uINav4.OnCancelDown, new Action(OnCancelClose));
	}

	private void OnDisable()
	{
		UI_NavigatorSimple uINav = UINav;
		uINav.OnSelectDown = (Action)Delegate.Remove(uINav.OnSelectDown, new Action(OnSelect));
		UI_NavigatorSimple uINav2 = UINav;
		uINav2.OnDefaultSetComplete = (Action)Delegate.Remove(uINav2.OnDefaultSetComplete, new Action(OnDefaultSetComplete));
		UI_NavigatorSimple uINav3 = UINav;
		uINav3.OnChangeSelection = (UI_NavigatorSimple.ChangeSelection)Delegate.Remove(uINav3.OnChangeSelection, new UI_NavigatorSimple.ChangeSelection(OnChangeSelection));
		UI_NavigatorSimple uINav4 = UINav;
		uINav4.OnCancelDown = (Action)Delegate.Remove(uINav4.OnCancelDown, new Action(OnCancelClose));
	}

	private void Start()
	{
		Time.timeScale = 0f;
		Populate(FollowerLocation.Base, BaseContainer);
		Populate(PlayerFarming.Location, LocationContainer);
	}

	private void Populate(FollowerLocation Location, Transform Container)
	{
		foreach (FollowerBrain item in FollowerManager.FollowerBrainsByHomeLocation(Location))
		{
			g = UnityEngine.Object.Instantiate(IconPrefab, Container);
			g.SetActive(true);
			icon = g.GetComponent<FollowerInformationBox>();
			icon.Configure(item._directInfoAccess);
			icon.followBrain = item;
			if (UINav.selectable == null)
			{
				UINav.startingItem = g.GetComponent<Selectable>();
				UINav.setDefault();
			}
		}
	}

	private void OnDefaultSetComplete()
	{
		OnChangeSelection(UINav.selectable, null);
	}

	private void OnChangeSelection(Selectable NewSelectable, Selectable PrevSelectable)
	{
		AudioManager.Instance.PlayOneShot("event:/upgrade_statue/upgrade_statue_scroll", base.gameObject);
	}

	public void OnSelect()
	{
		if (UINav.selectable == null)
		{
			return;
		}
		icon = UINav.selectable.GetComponent<FollowerInformationBox>();
		if (icon.transform.parent == BaseContainer)
		{
			icon.transform.parent = LocationContainer;
			icon.followBrain.SetNewHomeLocation(PlayerFarming.Location);
			icon.followBrain.Stats.WorkerBeenGivenOrders = false;
			if (icon.followBrain.Info.FollowerRole == FollowerRole.Worshipper)
			{
				icon.followBrain.Info.FollowerRole = FollowerRole.Worker;
			}
			icon.followBrain.CompleteCurrentTask();
		}
		else if (icon.transform.parent == LocationContainer)
		{
			icon.transform.parent = BaseContainer;
			icon.followBrain.SetNewHomeLocation(FollowerLocation.Base);
			icon.followBrain.CompleteCurrentTask();
		}
	}

	private void OnCancelClose()
	{
		Time.timeScale = 1f;
		HUD_Manager.Instance.Show();
		Close();
	}

	private void Close()
	{
		AudioManager.Instance.PlayOneShot("event:/upgrade_statue/upgrade_statue_close", base.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
