using System.Collections;
using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationCentreScreen : BaseMonoBehaviour
{
	public RectTransform rectTransform;

	public CanvasGroup canvasGroup;

	public TextMeshProUGUI Text;

	public CanvasGroup textCanvasGroup;

	public GameObject container;

	public Image background;

	[SerializeField]
	private CanvasGroup _backgroundCanvasGroup;

	public static NotificationCentreScreen Instance;

	private Vector3 _containerStartingPos;

	private void OnEnable()
	{
		Instance = this;
		Text.text = "";
	}

	private void Start()
	{
		Text.text = "";
		container.SetActive(false);
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static void Play(NotificationCentre.NotificationType Notification)
	{
		if (Instance == null)
		{
			NotificationCentre.Instance.PlayGenericNotification(Notification);
			return;
		}
		Instance.container.gameObject.SetActive(true);
		Instance.Text.text = LocalizationManager.GetTranslation("Notifications/" + Notification);
		Instance.StartCoroutine(Instance.PlayRoutine());
		Instance.transform.SetAsLastSibling();
	}

	public static void Play(string Notification)
	{
		if (Instance == null)
		{
			NotificationCentre.Instance.PlayGenericNotification(Notification);
			return;
		}
		Instance.container.gameObject.SetActive(true);
		Instance.Text.text = Notification;
		Instance.StartCoroutine(Instance.PlayRoutine());
		Instance.transform.SetAsLastSibling();
	}

	private IEnumerator PlayRoutine()
	{
		textCanvasGroup.alpha = 0f;
		_backgroundCanvasGroup.DOKill();
		_backgroundCanvasGroup.alpha = 0f;
		_backgroundCanvasGroup.DOFade(1f, 1f).SetUpdate(true).SetEase(Ease.OutQuad);
		yield return new WaitForSecondsRealtime(1f);
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir_Short", (PlayerFarming.Instance != null) ? PlayerFarming.Instance.gameObject : base.gameObject);
		textCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true).SetEase(Ease.OutQuad);
		yield return new WaitForSeconds(2.5f);
		textCanvasGroup.DOFade(0f, 1f).SetUpdate(true).SetEase(Ease.InQuad);
		yield return new WaitForSeconds(0.25f);
		_backgroundCanvasGroup.DOFade(0f, 1f).SetUpdate(true).SetEase(Ease.InQuad);
		yield return new WaitForSeconds(1f);
		Stop();
	}

	public void FadeAndStop()
	{
		StopAllCoroutines();
		textCanvasGroup.DOKill();
		textCanvasGroup.DOFade(0f, 0.5f).SetUpdate(true).SetEase(Ease.InQuad);
		_backgroundCanvasGroup.DOKill();
		_backgroundCanvasGroup.DOFade(0f, 0.5f).SetUpdate(true).SetEase(Ease.InQuad)
			.OnComplete(Stop);
	}

	public void Stop()
	{
		StopAllCoroutines();
		Text.text = "";
		container.SetActive(false);
	}
}
