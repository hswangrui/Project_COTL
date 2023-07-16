using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIAddFuel : BaseMonoBehaviour
{
	[SerializeField]
	public Vector3 offset;

	[Space]
	[SerializeField]
	private Image fuelBar;

	[SerializeField]
	private Image fuelFlashBar;

	[Space]
	[SerializeField]
	private Color emptyBarColor;

	[SerializeField]
	private Color emptyFlashBarColor;

	[SerializeField]
	private Color halfBarColor;

	[SerializeField]
	private Color halfFlashBarColor;

	[SerializeField]
	private Color fullBarColor;

	[SerializeField]
	private Color fullFlashBarColor;

	private bool hiding = true;

	private Camera camera;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private RectTransform rectTransform;

	private Interaction_AddFuel fuelInteraction;

	private Coroutine fuelIncreasedRoutine;

	private float targetFuelBarAmount;

	public bool IsShowing
	{
		get
		{
			return !hiding;
		}
	}

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		canvas = GetComponentInParent<Canvas>();
		rectTransform = GetComponent<RectTransform>();
		canvasGroup.alpha = 0f;
		base.gameObject.SetActive(false);
	}

	public void Show(Interaction_AddFuel fuelInteraction)
	{
		base.gameObject.SetActive(true);
		camera = Camera.main;
		this.fuelInteraction = fuelInteraction;
		this.fuelInteraction.OnFuelModified += FuelUpdated;
		FuelUpdated((float)fuelInteraction.Structure.Structure_Info.Fuel / (float)fuelInteraction.Structure.Structure_Info.MaxFuel);
		hiding = false;
	}

	public void Hide()
	{
		hiding = true;
		if ((bool)fuelInteraction)
		{
			fuelInteraction.OnFuelModified -= FuelUpdated;
		}
	}

	private void LateUpdate()
	{
		if (fuelInteraction != null)
		{
			Vector3 position = camera.WorldToScreenPoint(fuelInteraction.LockPosition.position) + offset * canvas.scaleFactor;
			rectTransform.position = position;
		}
		if (!hiding)
		{
			if (canvasGroup.alpha < 1f)
			{
				canvasGroup.alpha += Time.deltaTime * 5f;
			}
		}
		else if (canvasGroup != null && canvasGroup.alpha > 0f)
		{
			canvasGroup.alpha -= 5f * Time.deltaTime;
		}
		else if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void FuelUpdated(float normFuelAmount)
	{
		if (base.gameObject.activeInHierarchy && normFuelAmount != targetFuelBarAmount)
		{
			if (fuelIncreasedRoutine != null)
			{
				StopCoroutine(fuelIncreasedRoutine);
				ForceFuelAmount(targetFuelBarAmount);
			}
			targetFuelBarAmount = normFuelAmount;
			fuelIncreasedRoutine = StartCoroutine(FuelBarUpdated(normFuelAmount));
		}
	}

	public void ForceFuelAmount(float normFuelAmount)
	{
		fuelBar.fillAmount = normFuelAmount;
		fuelFlashBar.fillAmount = normFuelAmount;
		SetBarColor(normFuelAmount, fuelBar, emptyBarColor, halfBarColor, fullBarColor, 0f);
		SetBarColor(normFuelAmount, fuelFlashBar, emptyFlashBarColor, halfFlashBarColor, fullFlashBarColor, 0f);
		targetFuelBarAmount = normFuelAmount;
	}

	private IEnumerator FuelBarUpdated(float normAmount)
	{
		yield return StartCoroutine(BarUpdated(normAmount, fuelBar, fuelFlashBar));
		fuelIncreasedRoutine = null;
	}

	private IEnumerator BarUpdated(float normAmount, Image bar, Image flashBar)
	{
		Image image;
		Image secondMovingBar;
		if (normAmount < bar.fillAmount)
		{
			image = bar;
			secondMovingBar = flashBar;
			SetBarColor(normAmount, bar, emptyBarColor, halfBarColor, fullBarColor);
		}
		else
		{
			image = flashBar;
			secondMovingBar = bar;
			SetBarColor(normAmount, flashBar, emptyFlashBarColor, halfFlashBarColor, fullFlashBarColor);
		}
		image.fillAmount = normAmount;
		yield return new WaitForSeconds(0.3f);
		if (secondMovingBar == flashBar)
		{
			SetBarColor(normAmount, flashBar, emptyFlashBarColor, halfFlashBarColor, fullFlashBarColor);
		}
		else
		{
			SetBarColor(normAmount, bar, emptyBarColor, halfBarColor, fullBarColor);
		}
		float fromAmount = secondMovingBar.fillAmount;
		float t = 0f;
		while (t < 0.25f)
		{
			float t2 = t / 0.25f;
			secondMovingBar.fillAmount = Mathf.Lerp(fromAmount, normAmount, t2);
			t += Time.deltaTime;
			yield return null;
		}
		secondMovingBar.fillAmount = normAmount;
	}

	private void SetBarColor(float normAmount, Image bar, Color color1, Color color2, Color color3, float duration = 0.3f)
	{
		Color endValue = ((!(normAmount < 0.5f)) ? Color.Lerp(color2, color3, normAmount) : Color.Lerp(color1, color2, normAmount));
		bar.DOColor(endValue, duration);
	}
}
