using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDoctrineIcon : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public TextMeshProUGUI Name;

	public Image Selected;

	public Image Icon;

	public Image LockIcon;

	public DoctrineUpgradeSystem.DoctrineType Type;

	public bool Locked;

	private void OnEnable()
	{
		Selected.enabled = false;
	}

	public void Play(DoctrineUpgradeSystem.DoctrineType Type)
	{
		Locked = false;
		this.Type = Type;
		Name.text = DoctrineUpgradeSystem.GetLocalizedName(Type);
		Icon.sprite = DoctrineUpgradeSystem.GetIcon(Type);
		LockIcon.enabled = false;
	}

	public void PlayLocked()
	{
		Locked = true;
		Name.text = "";
		Icon.enabled = false;
		LockIcon.enabled = true;
	}

	public void OnSelect(BaseEventData eventData)
	{
		Selected.enabled = true;
		base.transform.DOScale(Vector3.one * 1.05f, 0.2f);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		Selected.enabled = false;
		base.transform.DOScale(Vector3.one, 0.3f);
	}
}
