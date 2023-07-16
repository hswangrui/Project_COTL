using System.Collections;
using UnityEngine;

public class FollowerXPProgressBar : BaseMonoBehaviour
{
	public SpriteRenderer ProgressBar;

	public SpriteRenderer ProgressBG;

	public SpriteRenderer ProgressInstant;

	public Follower follower;

	private bool _shown;

	private Coroutine cUpdateBar;

	private void OnEnable()
	{
		Hide();
		Transform obj = ProgressBar.transform;
		Vector3 localScale = (ProgressInstant.transform.localScale = new Vector3(0f, ProgressInstant.transform.localScale.y));
		obj.localScale = localScale;
		StartCoroutine(WaitForBrain());
	}

	private IEnumerator WaitForBrain()
	{
		while (follower.Brain == null)
		{
			yield return null;
		}
	}

	private void OnDisable()
	{
	}

	public void Hide()
	{
		_shown = false;
		SetVisibility();
	}

	public void Show()
	{
		_shown = true;
		SetVisibility();
		if (cUpdateBar != null)
		{
			StopCoroutine(cUpdateBar);
		}
		cUpdateBar = StartCoroutine(UpdateBar());
	}

	private void SetVisibility()
	{
		ProgressBar.gameObject.SetActive(_shown);
		ProgressBG.gameObject.SetActive(_shown);
		ProgressInstant.gameObject.SetActive(_shown);
	}

	private IEnumerator UpdateBar()
	{
		Vector3 localScale = ProgressBar.transform.localScale;
		yield return new WaitForSeconds(1f);
		Hide();
	}
}
