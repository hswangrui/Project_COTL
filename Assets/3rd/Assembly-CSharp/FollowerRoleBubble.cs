using DG.Tweening;
using TMPro;
using UnityEngine;

public class FollowerRoleBubble : MonoBehaviour
{
	[SerializeField]
	private Follower follower;

	[SerializeField]
	private TMP_Text icon;

	private bool IsPlaying;

	private void Start()
	{
		base.transform.localScale = Vector3.zero;
	}

	public void Show()
	{
		if (!IsPlaying)
		{
			icon.text = FontImageNames.IconForRole(follower.Brain.Info.FollowerRole);
			if (icon.text == "" || follower.Brain.Info.CursedState != 0)
			{
				base.transform.localScale = Vector3.zero;
				return;
			}
			base.transform.DOKill();
			base.transform.localScale = Vector3.zero;
			base.transform.DOScale(Vector3.one * 0.7f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
		}
	}

	public void Hide()
	{
		if (!IsPlaying)
		{
			base.transform.DOKill();
			base.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
		}
	}
}
