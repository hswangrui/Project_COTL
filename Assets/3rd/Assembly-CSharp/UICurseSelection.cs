using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UICurseSelection : BaseMonoBehaviour
{
	[SerializeField]
	private TarotCards.Card curseType;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image lockIcon;

	[SerializeField]
	private Image selectedIcon;

	public Material NormalMaterial;

	public Material BWMaterial;

	public TarotCards.Card CurseType
	{
		get
		{
			return curseType;
		}
	}

	private void OnEnable()
	{
		lockIcon.gameObject.SetActive(true);
	}

	public void Selected()
	{
		base.transform.DOScale(1.2f, 0.25f).SetEase(Ease.Linear).SetUpdate(true);
		icon.material = NormalMaterial;
	}

	public void Unselected()
	{
		base.transform.DOScale(1f, 0.25f).SetEase(Ease.Linear).SetUpdate(true);
		icon.material = BWMaterial;
	}
}
