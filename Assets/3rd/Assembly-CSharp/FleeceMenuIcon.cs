using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FleeceMenuIcon : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public enum States
	{
		Locked,
		Unlocked,
		Available,
		Equipped
	}

	public int FleeceNumber;

	public Image LockedIcon;

	public Image FleeceIcon;

	public RectTransform Container;

	public Image FleeceOutline;

	public Image FleeceEquipped;

	public Material normalMaterial;

	public Material bwMaterial;

	public States State;

	public void Init()
	{
		if (DataManager.Instance.UnlockedFleeces.Contains(FleeceNumber))
		{
			State = ((DataManager.Instance.PlayerFleece != FleeceNumber) ? States.Unlocked : States.Equipped);
		}
		else
		{
			State = ((Inventory.TempleKeys > 0) ? States.Available : States.Locked);
		}
		FleeceEquipped.enabled = false;
		FleeceOutline.enabled = false;
		FleeceIcon.material = bwMaterial;
		switch (State)
		{
		case States.Locked:
			LockedIcon.enabled = true;
			FleeceIcon.color = Color.black;
			break;
		case States.Available:
			LockedIcon.enabled = true;
			FleeceIcon.color = Color.black;
			break;
		case States.Unlocked:
			LockedIcon.enabled = false;
			FleeceIcon.color = Color.white;
			break;
		case States.Equipped:
			LockedIcon.enabled = false;
			FleeceIcon.color = Color.white;
			FleeceEquipped.enabled = true;
			break;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		Container.DOKill();
		Container.DOScale(Vector3.one * 1.1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
		FleeceOutline.enabled = true;
		FleeceIcon.material = normalMaterial;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Container.DOKill();
		Container.DOScale(Vector3.one * 1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
		FleeceOutline.enabled = false;
		FleeceIcon.material = bwMaterial;
	}
}
