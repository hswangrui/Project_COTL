using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Base : BaseMonoBehaviour
{
	public RectTransform Container;

	public GameObject HungerBar;

	public Image HungerProgressRing;

	public TextMeshProUGUI HungerIcon;

	public Image HungerRedGlow;

	public float LerpSpeed = 1f;

	public GameObject HappinessBar;

	public Image ProgressRingHappiness;

	public Image HappinessRedGlow;

	public TextMeshProUGUI HappinessIcon;

	private string FaceVeryHappy = "\uf599";

	private string FaceHappy = "\uf118";

	private string FaceMeh = "\uf11a";

	private string FaceSad = "\uf119";

	private string FaceVerySad = "\uf5b4";

	public GameObject FaithBar;

	public Image ProgressFaithBar;

	public Image FaithRedGlow;

	public TextMeshProUGUI FaithIcon;

	public Image FaithFlashRed;

	public Image FaithFlashGreen;

	public float FaithProgress;

	public Transform FaithBarBottomPosition;

	public Transform FaithBarTopPosition;

	public GameObject FollowerCount;

	public TextMeshProUGUI FollowerAmount;

	public TextMeshProUGUI FollowerIcon;

	public TextMeshProUGUI DisciplesAmount;

	public GameObject DiscipleParent;

	public TextMeshProUGUI CoinsAmount;

	public GameObject CoinsParent;

	private bool _offscreen;

	private Vector3 StartPos;

	private Vector3 MovePos;

	public UI_Transitions UITransition;

	private bool HidHappiness;

	private bool HidHunger;

	private bool HidFollowerCount;

	private int DiscipleCount;

	private Coroutine cLerpBarRoutine;

	private void Start()
	{
		int count = DataManager.Instance.Followers.Count;
		_offscreen = count <= 0;
		StartCoroutine(ShakeIcon());
	}

	private void OnEnable()
	{
		DiscipleParent.SetActive(false);
		FollowerCount.SetActive(DataManager.Instance.Followers.Count > 0);
		FollowerAmount.text = DataManager.Instance.Followers.Count.ToString();
		CoinsParent.SetActive(Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) > 0);
		CoinsAmount.text = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD).ToString();
		if (!DataManager.Instance.HappinessEnabled)
		{
			HideHappiness();
		}
		else
		{
			bool hidHappiness = HidHappiness;
		}
	}

	private void UpdateDiscipleCount(bool Shake)
	{
		DiscipleCount = FollowerBrain.DiscipleCount();
		if (DiscipleCount > 0)
		{
			DiscipleParent.SetActive(true);
			if (Shake)
			{
				DisciplesAmount.transform.DOShakeScale(1f);
			}
			DisciplesAmount.text = DiscipleCount.ToString();
		}
		else
		{
			DiscipleParent.SetActive(false);
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void OnHappinessStateChanged(int followerID, float newValue, float oldValue, float change)
	{
		Debug.Log("CALLED HAPPINESS");
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			num += allBrain.Stats.Happiness;
			num2 += allBrain.Stats.Satiation + (75f - allBrain.Stats.Starvation);
			num3 += 1f;
		}
		FaithProgress = num / (100f * num3);
		if (cLerpBarRoutine != null)
		{
			StopCoroutine(cLerpBarRoutine);
		}
		cLerpBarRoutine = StartCoroutine(LerpBarRoutine());
		FaithRedGlow.color = new Color(1f, 0f, 0f, FaithProgress * -1f + 0.5f);
	}

	private void HideHappiness()
	{
		HidHappiness = true;
		HappinessBar.SetActive(false);
	}

	private void ShowHappiness()
	{
		StartCoroutine(ShowHappinessRoutine());
	}

	private IEnumerator ShowHappinessRoutine()
	{
		float Progress = 0f;
		float Duration = 1f;
		HappinessBar.SetActive(true);
		HappinessBar.transform.localScale = Vector3.zero;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				HappinessBar.transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
	}

	private void HideHunger()
	{
		HidHappiness = true;
		HappinessBar.SetActive(false);
	}

	private void ShowHunger()
	{
		StartCoroutine(ShowHungerRoutine());
	}

	private IEnumerator ShowHungerRoutine()
	{
		float Progress = 0f;
		float Duration = 1f;
		HungerBar.SetActive(true);
		HungerBar.transform.localScale = Vector3.zero;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				HungerBar.transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, Progress / Duration);
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator ShakeIcon()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			UpdateFace();
			if (HungerProgressRing.fillAmount < 0.25f)
			{
				HungerIcon.transform.DOKill();
				HungerIcon.transform.DOShakeScale(0.75f, 0.5f);
				yield return new WaitForSeconds(1f);
				HungerIcon.transform.localScale = Vector3.one;
			}
			if (ProgressRingHappiness.fillAmount < 0.25f)
			{
				HappinessIcon.transform.DOKill();
				HappinessIcon.transform.DOShakeScale(0.75f, 0.5f);
				yield return new WaitForSeconds(1f);
				HappinessIcon.transform.localScale = Vector3.one;
			}
			yield return null;
		}
	}

	private void UpdateFace()
	{
		string text = HappinessIcon.text;
		if (ProgressRingHappiness.fillAmount < 0.15f)
		{
			HappinessIcon.text = FaceVerySad;
		}
		else if (ProgressRingHappiness.fillAmount < 0.33f)
		{
			HappinessIcon.text = FaceSad;
		}
		else if (ProgressRingHappiness.fillAmount < 0.45f)
		{
			HappinessIcon.text = FaceMeh;
		}
		else if (ProgressRingHappiness.fillAmount < 0.85f)
		{
			HappinessIcon.text = FaceHappy;
		}
		else if (ProgressRingHappiness.fillAmount < 1f)
		{
			HappinessIcon.text = FaceVeryHappy;
		}
		if (text != HappinessIcon.text)
		{
			HappinessIcon.transform.DOKill();
			HappinessIcon.transform.DOShakeScale(1f);
		}
	}

	private void Update()
	{
		int itemQuantity = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD);
		CoinsParent.SetActive(itemQuantity > 0);
		CoinsAmount.text = itemQuantity.ToString();
		FollowerAmount.text = DataManager.Instance.Followers.Count.ToString();
		FollowerCount.SetActive(DataManager.Instance.Followers.Count > 0);
	}

	private IEnumerator LerpBarRoutine()
	{
		Debug.Log("Played Coroutine");
		yield return new WaitForSeconds(0.2f);
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				ProgressFaithBar.rectTransform.localPosition = Vector3.Lerp(FaithBarBottomPosition.localPosition, FaithBarTopPosition.localPosition, Mathf.SmoothStep(0f, 1f, FaithProgress));
				yield return null;
				continue;
			}
			break;
		}
	}
}
