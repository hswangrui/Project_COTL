using DG.Tweening;
using TMPro;
using UnityEngine;

public class LetterBox : BaseMonoBehaviour
{
	public static LetterBox Instance;

	public RectTransform Top;

	public RectTransform Bottom;

	public static bool IsPlaying;

	private Canvas canvas;

	public float DisplayOffset;

	[SerializeField]
	private CanvasGroup skipCutsceneGroup;

	[SerializeField]
	private TMP_Text subtitle;

	[SerializeField]
	private CanvasGroup subtitleCanvasGroup;

	public Animator animator;

	private static bool CacheInvincible;

	private void OnEnable()
	{
		Instance = this;
		canvas = GetComponentInParent<Canvas>();
		Hide();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Start()
	{
	}

	public static void Show(bool SnapLetterBox)
	{
		if (PlayerFarming.Instance != null && PlayerFarming.Instance.health != null)
		{
			PlayerFarming.Instance.health.invincible = CacheInvincible;
		}
		if (!IsPlaying)
		{
			Instance.skipCutsceneGroup.alpha = 0f;
		}
		Instance.StopAllCoroutines();
		Instance.subtitleCanvasGroup.DOFade(0f, 0f);
		IsPlaying = true;
		if (!SnapLetterBox)
		{
			Instance.ShowLetterBox();
		}
		else
		{
			Instance.SnapShowLetterBox();
		}
	}

	private void SnapShowLetterBox()
	{
		if (HUD_Manager.Instance != null)
		{
			HUD_Manager.Instance.Hide(true, 0);
		}
		Instance.Top.gameObject.SetActive(true);
		Instance.Bottom.gameObject.SetActive(true);
		Instance.animator.SetInteger("State", 0);
	}

	private void ShowLetterBox()
	{
		if (HUD_Manager.Instance != null)
		{
			HUD_Manager.Instance.Hide(false, 0);
		}
		Instance.Top.gameObject.SetActive(true);
		Instance.Bottom.gameObject.SetActive(true);
		Instance.animator.SetInteger("State", 1);
	}

	public static void Hide(bool ShowHUD = true)
	{
		if (PlayerFarming.Instance != null && PlayerFarming.Instance.health != null)
		{
			CacheInvincible = PlayerFarming.Instance.health.invincible;
			PlayerFarming.Instance.health.invincible = true;
		}
		if (ShowHUD && HUD_Manager.Instance != null)
		{
			HUD_Manager.Instance.Show(0);
		}
		Instance.animator.SetInteger("State", -1);
		Instance.StopAllCoroutines();
		IsPlaying = false;
	}

	public void ShowSkipPrompt()
	{
		skipCutsceneGroup.DOKill();
		skipCutsceneGroup.DOFade(1f, 0.5f);
	}

	public void HideSkipPrompt()
	{
		skipCutsceneGroup.DOKill();
		skipCutsceneGroup.DOFade(0f, 0.1f);
	}

	public void ShowSubtitle(string text)
	{
		subtitle.text = text;
		subtitleCanvasGroup.DOKill();
		subtitleCanvasGroup.DOFade(1f, 1f);
	}
}
