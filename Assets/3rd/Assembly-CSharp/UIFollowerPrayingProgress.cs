using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UIFollowerPrayingProgress : BaseMonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _radialProgress;

	[SerializeField]
	private Follower _follower;

	private bool _shown;

	private bool _flashing;

	private void Awake()
	{
		base.transform.localScale = Vector3.zero;
		base.gameObject.SetActive(false);
	}

	public void Show()
	{
		if (!_shown)
		{
			base.gameObject.SetActive(true);
			_shown = true;
			base.transform.DOKill();
			base.transform.localScale = Vector3.zero;
			base.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
		}
	}

	public void Hide()
	{
		if (_shown)
		{
			_shown = false;
			base.transform.DOKill();
			base.transform.localScale = Vector3.one;
			base.transform.DOScale(Vector3.zero, 0.3f).SetUpdate(true).SetEase(Ease.InBack)
				.OnComplete(delegate
				{
					base.gameObject.SetActive(false);
				});
		}
	}

	private void Update()
	{
		if (_shown && !_flashing && !(_follower == null) && _follower.Brain != null && _follower.Brain.Info != null && _follower.Brain.CurrentTask != null)
		{
			float num = 0f;
			FollowerTask_Pray followerTask_Pray;
			FollowerTask_PrayPassive followerTask_PrayPassive;
			if ((followerTask_Pray = _follower.Brain.CurrentTask as FollowerTask_Pray) != null)
			{
				num = followerTask_Pray.GetDurationPerDevotion(_follower);
			}
			else if ((followerTask_PrayPassive = _follower.Brain.CurrentTask as FollowerTask_PrayPassive) != null)
			{
				num = followerTask_PrayPassive.GetDurationPerDevotion(_follower);
			}
			_radialProgress.material.SetFloat("_Arc2", 360f * (1f - _follower.Brain.Info.PrayProgress / num));
		}
	}

	public void Flash()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.gameObject.SetActive(true);
		}
		StopAllCoroutines();
		StartCoroutine(DoFlash());
	}

	private IEnumerator DoFlash()
	{
		_flashing = true;
		Color transparent = new Color(0f, 0f, 0f, 0f);
		_radialProgress.color = Color.white;
		yield return new WaitForSeconds(0.2f);
		_radialProgress.DOColor(transparent, 0.5f);
		yield return new WaitForSeconds(0.5f);
		_radialProgress.material.SetFloat("_Arc2", 360f);
		_radialProgress.color = StaticColors.RedColor;
		_flashing = false;
	}
}
