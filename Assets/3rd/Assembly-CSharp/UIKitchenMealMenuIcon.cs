using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIKitchenMealMenuIcon : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	private CanvasGroup canvasGroup;

	public InventoryItem.ITEM_TYPE Type;

	public TextMeshProUGUI CostText;

	public Image Icon;

	public Image IconSelected;

	public Transform ShakeObject;

	public Material BlackAndWhiteMaterial;

	public GameObject NewIcon;

	public Image FlashIcon;

	public Image CookingRing;

	public Image HungerBar;

	public Image FaithBar;

	public GameObject requiresFuelText;

	private Coroutine cShake;

	private Vector2 ShakeVector;

	private Coroutine cSelectionRoutine;

	public bool CanAfford { get; private set; }

	public int IngredientVariant { get; private set; }

	public void Play(InventoryItem.ITEM_TYPE Type, int ingredientVariant, float Delay, bool queued = false, int queuedIndex = 0, bool fadeIn = true)
	{
		IconSelected.enabled = false;
		base.gameObject.SetActive(true);
		this.Type = Type;
		IngredientVariant = ingredientVariant;
		Icon.sprite = CookingData.GetIcon(Type);
		CostText.text = GetCostText(Type, ingredientVariant);
		NewIcon.SetActive(false);
		FlashIcon.gameObject.SetActive(false);
		canvasGroup = GetComponent<CanvasGroup>();
		if (fadeIn)
		{
			StartCoroutine(FadeIn(Delay));
		}
		else
		{
			canvasGroup.alpha = 1f;
		}
		bool flag = queuedIndex == 0 && queued;
		bool flag2 = true;
		CanAfford = CheckCanAfford(Type, ingredientVariant) || queued;
		if ((!CanAfford && !queued) || (queued && (!flag2 || !flag)))
		{
			Icon.material = BlackAndWhiteMaterial;
			Icon.material.SetFloat("_GrayscaleLerpFade", 1f);
			if ((bool)requiresFuelText)
			{
				requiresFuelText.SetActive(!flag2);
			}
		}
		if ((bool)HungerBar)
		{
			HungerBar.fillAmount = (float)CookingData.GetSatationAmount(Type) / 100f;
		}
		if ((bool)FaithBar)
		{
			FaithBar.fillAmount = (float)CookingData.GetFaithAmount(Type) / 12f;
		}
	}

	public void UpdateCookingProgress(float normTime)
	{
		CookingRing.fillAmount = normTime;
	}

	private IEnumerator FadeIn(float Delay)
	{
		canvasGroup.alpha = 0f;
		yield return new WaitForSecondsRealtime(Delay);
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			canvasGroup.alpha = Progress / Duration;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	private IEnumerator Reveal(float Delay)
	{
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		yield return new WaitForSecondsRealtime(Delay * 2f);
		StartCoroutine(ShakeNewIcon());
		float Progress = 0f;
		float Duration2 = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			base.transform.localScale = Vector3.one * Mathf.Lerp(2.5f, 1f, Mathf.SmoothStep(0f, 1f, Progress / Duration2));
			canvasGroup.alpha = Progress / Duration2;
			yield return null;
		}
		FlashIcon.gameObject.SetActive(true);
		Progress = 0f;
		Duration2 = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			FlashIcon.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), Progress / Duration2);
			yield return null;
		}
		FlashIcon.gameObject.SetActive(false);
	}

	private IEnumerator ShakeNewIcon()
	{
		NewIcon.SetActive(true);
		float Progress = 0f;
		float Duration = 0.3f;
		float TargetScale = NewIcon.transform.localScale.x;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			NewIcon.transform.localScale = Vector3.one * Mathf.Lerp(1f, TargetScale, Progress / Duration);
			yield return null;
		}
		NewIcon.transform.localScale = Vector3.one * TargetScale;
		float Wobble = Random.Range(0, 360);
		float WobbleSpeed = 4f;
		while (true)
		{
			Transform obj = NewIcon.transform;
			float num;
			Wobble = (num = Wobble + WobbleSpeed * Time.unscaledDeltaTime);
			obj.eulerAngles = new Vector3(0f, 0f, 15f * Mathf.Cos(num));
			yield return null;
		}
	}

	private bool CheckCanAfford(InventoryItem.ITEM_TYPE type, int ingredientVariant)
	{
		bool buildingsFree = CheatConsole.BuildingsFree;
		return true;
	}

	private string GetCostText(InventoryItem.ITEM_TYPE type, int ingredientVariant)
	{
		return "";
	}

	public void Shake()
	{
		if (cShake != null)
		{
			StopCoroutine(cShake);
		}
		cShake = StartCoroutine(ShakeRoutine());
	}

	private IEnumerator ShakeRoutine()
	{
		float Progress = 0f;
		float Duration = 5f;
		ShakeVector.y = 1000f;
		ShakeObject.localPosition = Vector3.zero;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			ShakeVector.y += (0f - ShakeVector.x) * 0.2f / Time.unscaledDeltaTime;
			ShakeVector.x += (ShakeVector.y *= 0.7f) * Time.unscaledDeltaTime;
			ShakeObject.localPosition = Vector3.left * ShakeVector.x;
			yield return null;
		}
		ShakeObject.localPosition = Vector3.zero;
	}

	public void OnSelect(BaseEventData eventData)
	{
		IconSelected.enabled = true;
		NewIcon.SetActive(false);
		if (cSelectionRoutine != null)
		{
			StopCoroutine(cSelectionRoutine);
		}
		cSelectionRoutine = StartCoroutine(Selected(base.transform.localScale.x, 1.2f));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		IconSelected.enabled = false;
		if (cSelectionRoutine != null)
		{
			StopCoroutine(cSelectionRoutine);
		}
		cSelectionRoutine = StartCoroutine(DeSelected());
	}

	private IEnumerator Selected(float Starting, float Target)
	{
		float Progress = 0f;
		float Duration = 0.1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(Starting, Target, Progress / Duration);
			base.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		base.transform.localScale = Vector3.one * Target;
	}

	private IEnumerator DeSelected()
	{
		float Progress = 0f;
		float Duration = 0.3f;
		float StartingScale = base.transform.localScale.x;
		float TargetScale = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(StartingScale, TargetScale, Progress / Duration);
			base.transform.localScale = Vector3.one * num2;
			yield return null;
		}
		base.transform.localScale = Vector3.one * TargetScale;
	}
}
