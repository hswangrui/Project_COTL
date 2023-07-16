using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIDungeonLayerNode : BaseMonoBehaviour
{
	public enum State
	{
		None,
		Visted,
		Selected
	}

	[SerializeField]
	private Image vistedIcon;

	[SerializeField]
	private Image selectedIcon;

	[SerializeField]
	private Image connectionBar;

	public void SetState(State state)
	{
		vistedIcon.gameObject.SetActive(state == State.Visted);
		selectedIcon.gameObject.SetActive(state == State.Selected || state == State.Visted);
		switch (state)
		{
		case State.Selected:
			StartCoroutine(SelectedIE());
			break;
		case State.Visted:
			vistedIcon.fillAmount = 1f;
			selectedIcon.fillAmount = 1f;
			if ((bool)connectionBar)
			{
				connectionBar.fillAmount = 1f;
			}
			break;
		}
	}

	private IEnumerator SelectedIE()
	{
		selectedIcon.fillAmount = 0f;
		yield return new WaitForSeconds(0.25f);
		if ((bool)connectionBar)
		{
			connectionBar.DOFillAmount(1f, 0.5f).SetEase(Ease.Linear);
			yield return new WaitForSeconds(0.5f);
		}
		selectedIcon.DOFillAmount(1f, 0.5f).SetEase(Ease.Linear);
		yield return new WaitForSeconds(0.75f);
		vistedIcon.gameObject.SetActive(true);
		vistedIcon.color = Color.white;
		base.transform.localScale = Vector3.one * 1.25f;
		base.transform.DOPunchScale(Vector3.one * 0.25f, 0.5f, 1);
	}
}
