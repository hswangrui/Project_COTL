using DG.Tweening;
using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
	public static PlayerCanvas Instance;

	private CanvasGroup canvasGroup;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		Instance = this;
	}

	private void OnEnable()
	{
		canvasGroup.alpha = 0f;
	}

	private void Update()
	{
		if (LetterBox.IsPlaying && GameManager.IsDungeon(PlayerFarming.Location))
		{
			if (canvasGroup.alpha == 1f)
			{
				canvasGroup.DOKill();
				canvasGroup.DOFade(0f, 0.25f);
			}
		}
		else if (canvasGroup.alpha == 0f)
		{
			canvasGroup.DOKill();
			canvasGroup.DOFade(1f, 0.25f);
		}
	}
}
