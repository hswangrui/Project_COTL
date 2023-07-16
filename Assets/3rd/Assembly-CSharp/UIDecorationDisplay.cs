using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public class UIDecorationDisplay : BaseMonoBehaviour
{
	[SerializeField]
	private TMP_Text title;

	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private TMP_Text descriptionText;

	public CanvasGroup canvasGroup;

	[Space]
	[SerializeField]
	private Vector3 offset;

	private RectTransform rectTransform;

	private GameObject lockPosition;

	[SerializeField]
	private Camera camera;

	private StructureBrain.TYPES structureType;

	public void Play(StructureBrain.TYPES structureType, GameObject lockPos)
	{
		this.structureType = structureType;
		LocalizeText();
		camera = Camera.main;
		lockPosition = lockPos;
		rectTransform = base.transform as RectTransform;
		Vector3 endValue = offset;
		offset += Vector3.up * 165f;
		DOTween.To(() => offset, delegate(Vector3 x)
		{
			offset = x;
		}, endValue, 0.3f).SetEase(Ease.OutBack);
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		DOTween.To(() => canvasGroup.alpha, delegate(float x)
		{
			canvasGroup.alpha = x;
		}, 1f, 0.5f);
	}

	public void Shake(float progress, float normAmount)
	{
		rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, normAmount);
		container.localPosition = Random.insideUnitCircle * progress * 2f;
	}

	private void LateUpdate()
	{
		if (!(lockPosition == null))
		{
			Vector2 vector = camera.WorldToScreenPoint(lockPosition.transform.position) + offset;
			rectTransform.position = vector;
		}
	}

	private void OnEnable()
	{
		LocalizationManager.OnLocalizeEvent += LocalizeText;
	}

	private void OnDisable()
	{
		LocalizationManager.OnLocalizeEvent -= LocalizeText;
	}

	private void LocalizeText()
	{
		title.text = StructuresData.LocalizedName(structureType);
		descriptionText.text = StructuresData.LocalizedDescription(structureType);
	}
}
