using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIJellyFishBank : BaseMonoBehaviour
{
	public CanvasGroup Menu;

	public CanvasGroup MenuMoney;

	public CanvasGroup MenuMoneyInvestmentAmount;

	public CanvasGroup MenuConfirm;

	public PauseInventoryItem resourceIndicator;

	public TextMeshProUGUI NewInvestmentTxt;

	public TextMeshProUGUI InvestmentProfitTxt;

	public TextMeshProUGUI ConfirmationText;

	public TextMeshProUGUI InitialInvestmentTxt;

	public Animator animator;

	[SerializeField]
	private AnimationCurve inputCurve;

	public List<GameObject> GraphPoints = new List<GameObject>();

	public GameObject LowPoint;

	public GameObject HighPoint;

	public int InvestmentAdd;

	public int InitialInvestment;

	public JellyFishInvestment localInvestment;

	private Action _Callback;

	private float InputDelay;

	private bool DoingSomething;

	private bool inMenu;

	private bool withdrawl;

	private bool confirmWithdrawl;

	private void UpdateLocalInvestment()
	{
		localInvestment = DataManager.Instance.Investment;
	}

	public void Play(Action Callback)
	{
		_Callback = Callback;
		Menu.alpha = 1f;
		MenuMoney.alpha = 0f;
		MenuMoney.interactable = false;
		MenuConfirm.interactable = false;
		resourceIndicator.GetComponent<CanvasGroup>().alpha = 0f;
		CheckInvestment();
		InputDelay = 0.3f;
	}

	private void Update()
	{
		resourceIndicator.Init(InventoryItem.ITEM_TYPE.BLACK_GOLD, Inventory.GetItemQuantity(20));
		if ((InputManager.UI.GetCancelButtonDown() || InputManager.Gameplay.GetPauseButtonDown()) && !inMenu)
		{
			Action callback = _Callback;
			if (callback != null)
			{
				callback();
			}
			animator.Play("Out");
		}
	}

	private void UpdateInvestmentAmount(int investmentAmount)
	{
		InitialInvestmentTxt.text = "<sprite name=icon_blackgold>";
		int length = investmentAmount.ToString().Length;
		int num = 4 - length;
		string text = "<color=#6e6d69>0</color>";
		for (int i = 0; i < num; i++)
		{
			InitialInvestmentTxt.text = (InitialInvestmentTxt.text += text);
		}
		InitialInvestmentTxt.text += investmentAmount;
	}

	private void SetGraph()
	{
		if (DataManager.Instance.CheckInvestmentExist())
		{
			int num = 0;
			{
				foreach (GameObject graphPoint in GraphPoints)
				{
					graphPoint.SetActive(false);
					if (DataManager.Instance.Investment.InvestmentDays.Count > num)
					{
						Debug.Log("Checking index of investment day: " + num + "Investment Count = " + DataManager.Instance.Investment.InvestmentDays.Count);
						if (DataManager.Instance.Investment.InvestmentDays != null && DataManager.Instance.Investment.InvestmentDays[num] != null)
						{
							graphPoint.SetActive(true);
							graphPoint.GetComponent<TextMeshProUGUI>().text = DataManager.Instance.Investment.InvestmentDays[num].InterestRate.ToString();
							graphPoint.transform.position = Vector3.Lerp(new Vector3(graphPoint.transform.position.x, LowPoint.gameObject.transform.position.y, graphPoint.transform.position.z), new Vector3(graphPoint.transform.position.x, HighPoint.gameObject.transform.position.y, graphPoint.transform.position.z), (Mathf.Abs(DataManager.Instance.Investment.InvestmentDays[num].InterestRate) + 0.3f) / 0.6f);
							if (Mathf.Sign(DataManager.Instance.Investment.InvestmentDays[num].InterestRate) == 1f)
							{
								graphPoint.GetComponent<TextMeshProUGUI>().color = StaticColors.GreenColor;
							}
							else
							{
								graphPoint.GetComponent<TextMeshProUGUI>().color = StaticColors.RedColor;
							}
						}
					}
					num++;
				}
				return;
			}
		}
		foreach (GameObject graphPoint2 in GraphPoints)
		{
			graphPoint2.SetActive(false);
		}
	}

	public void Withdrawl()
	{
		Menu.alpha = 0f;
		Menu.interactable = true;
		MenuMoney.alpha = 1f;
		MenuMoney.interactable = true;
		InvestmentAdd = 0;
		NewInvestmentTxt.text = InvestmentAdd.ToString();
		resourceIndicator.GetComponent<CanvasGroup>().alpha = 1f;
		resourceIndicator.Init(InventoryItem.ITEM_TYPE.BLACK_GOLD, Inventory.GetItemQuantity(20));
		StartCoroutine(WithdrawlRoutine(true));
	}

	private IEnumerator WithdrawlRoutine(bool Withdrawl)
	{
		withdrawl = Withdrawl;
		while (InputManager.UI.GetAcceptButtonDown())
		{
			yield return null;
		}
		Debug.Log("Withdrawl/Deposit Routine");
		inMenu = true;
		DoingSomething = false;
		float maxDelay = 0.25f;
		float minDelay = 1E-05f;
		float holdTimeToReachMin = 2f;
		float progress = 0f;
		int movingDirection = 0;
		while (true)
		{
			if (InputManager.UI.GetCancelButtonDown() || (InputManager.Gameplay.GetPauseButtonDown() && !DoingSomething))
			{
				StartCoroutine(ExitMenu());
				yield break;
			}
			if ((InputManager.UI.GetHorizontalAxis() > -0.3f && movingDirection == -1) || (InputManager.UI.GetHorizontalAxis() < 0.3f && movingDirection == 1))
			{
				InputDelay = 0f;
				progress = 0f;
				movingDirection = 0;
			}
			int amountPerRound3 = 1 + Mathf.CeilToInt(progress / 2f);
			if ((InputDelay -= Time.unscaledDeltaTime) < 0f && !DoingSomething)
			{
				for (int q3 = 0; q3 < amountPerRound3; q3++)
				{
					if (InputManager.UI.GetHorizontalAxis() < -0.3f)
					{
						if (InvestmentAdd > 0)
						{
							InvestmentAdd--;
							NewInvestmentTxt.text = InvestmentAdd.ToString();
							AudioManager.Instance.PlayOneShot("event:/ui/arrow_change_selection", base.gameObject);
						}
						else
						{
							AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
							NewInvestmentTxt.gameObject.transform.DOPunchPosition(new Vector3(1f, 0f, 0f), 0.5f);
							yield return new WaitForSeconds(0.5f);
						}
						InputDelay = Mathf.Lerp(maxDelay, minDelay, inputCurve.Evaluate(Mathf.Clamp01(progress / holdTimeToReachMin)));
						movingDirection = -1;
					}
					else
					{
						if (!(InputManager.UI.GetHorizontalAxis() > 0.3f))
						{
							continue;
						}
						if (!Withdrawl)
						{
							if (InvestmentAdd <= Inventory.GetItemQuantity(20) - 1)
							{
								AudioManager.Instance.PlayOneShot("event:/ui/arrow_change_selection", base.gameObject);
								InvestmentAdd++;
								NewInvestmentTxt.text = InvestmentAdd.ToString();
							}
							else
							{
								AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
								NewInvestmentTxt.gameObject.transform.DOPunchPosition(new Vector3(1f, 0f, 0f), 0.5f);
								InputDelay = maxDelay;
								yield return new WaitForSeconds(0.5f);
							}
						}
						else if (DataManager.Instance.CheckInvestmentExist())
						{
							if (InvestmentAdd < DataManager.Instance.Investment.InitialInvestment)
							{
								AudioManager.Instance.PlayOneShot("event:/ui/arrow_change_selection", base.gameObject);
								InvestmentAdd++;
								NewInvestmentTxt.text = InvestmentAdd.ToString();
							}
							else
							{
								AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
								NewInvestmentTxt.gameObject.transform.DOPunchPosition(new Vector3(1f, 0f, 0f), 0.5f);
								InputDelay = maxDelay;
								yield return new WaitForSeconds(0.5f);
							}
						}
						else
						{
							AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
							NewInvestmentTxt.gameObject.transform.DOPunchPosition(new Vector3(1f, 0f, 0f), 0.5f);
							InputDelay = maxDelay;
							yield return new WaitForSeconds(0.5f);
						}
						InputDelay = Mathf.Lerp(maxDelay, minDelay, inputCurve.Evaluate(Mathf.Clamp01(progress / holdTimeToReachMin)));
						movingDirection = 1;
					}
				}
			}
			if (InputManager.UI.GetHorizontalAxis() > 0.3f || InputManager.UI.GetHorizontalAxis() < -0.3f)
			{
				progress += Time.deltaTime;
			}
			else
			{
				InputDelay = 0f;
				progress = 0f;
				movingDirection = 0;
			}
			if (InputDelay == 0f && InputManager.UI.GetAcceptButtonDown())
			{
				if (InvestmentAdd > 0)
				{
					break;
				}
				AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback", base.gameObject);
				NewInvestmentTxt.gameObject.transform.DOPunchPosition(new Vector3(1f, 0f, 0f), 0.5f);
				yield return null;
			}
			yield return null;
		}
		Debug.Log("Do Withdrawl / Deposit");
		DoingSomething = true;
		AudioManager.Instance.PlayOneShot("event:/ui/confirm_selection", base.gameObject);
		MenuConfirm.alpha = 1f;
		MenuMoney.alpha = 0f;
		MenuConfirm.interactable = true;
		MenuMoney.interactable = false;
		SetCofirmationText(Withdrawl);
		while (!confirmWithdrawl)
		{
			yield return null;
		}
		MenuMoneyInvestmentAmount.alpha = 0f;
		if (!Withdrawl)
		{
			SetInvestment(InvestmentAdd);
			float increment2 = 2f / (float)InvestmentAdd;
			int amountPerRound3 = Mathf.Clamp(InvestmentAdd / 250, 1, int.MaxValue);
			int q3 = 0;
			while (q3 < InvestmentAdd && q3 < InvestmentAdd)
			{
				for (int i = 0; i < amountPerRound3; i++)
				{
					if (q3 >= InvestmentAdd)
					{
						break;
					}
					Inventory.ChangeItemQuantity(20, -1);
					NewInvestmentTxt.text = InvestmentAdd.ToString();
					InitialInvestment++;
					UpdateInvestmentAmount(InitialInvestment);
					q3++;
				}
				yield return new WaitForSeconds(increment2);
			}
		}
		else
		{
			SetInvestment(InvestmentAdd * -1);
			float increment2 = 2f / (float)InvestmentAdd;
			int amountPerRound3 = Mathf.Clamp(InvestmentAdd / 250, 1, int.MaxValue);
			int q3 = 0;
			while (q3 < InvestmentAdd && q3 < InvestmentAdd)
			{
				for (int j = 0; j < amountPerRound3; j++)
				{
					if (q3 >= InvestmentAdd)
					{
						break;
					}
					Inventory.AddItem(20, 1);
					NewInvestmentTxt.text = InvestmentAdd.ToString();
					InitialInvestment--;
					UpdateInvestmentAmount(InitialInvestment);
					q3++;
				}
				yield return new WaitForSeconds(increment2);
			}
		}
		yield return new WaitForSeconds(0.3f);
		confirmWithdrawl = false;
		MenuMoneyInvestmentAmount.alpha = 1f;
		StartCoroutine(ExitMenu());
	}

	public void Deposit()
	{
		Menu.alpha = 0f;
		MenuMoney.alpha = 1f;
		Menu.interactable = false;
		MenuMoney.interactable = true;
		InvestmentAdd = 0;
		NewInvestmentTxt.text = InvestmentAdd.ToString();
		resourceIndicator.GetComponent<CanvasGroup>().alpha = 1f;
		resourceIndicator.Init(InventoryItem.ITEM_TYPE.BLACK_GOLD, Inventory.GetItemQuantity(20));
		StartCoroutine(WithdrawlRoutine(false));
	}

	private IEnumerator ExitMenu()
	{
		resourceIndicator.GetComponent<CanvasGroup>().alpha = 0f;
		while (InputManager.UI.GetCancelButtonDown())
		{
			yield return null;
		}
		inMenu = false;
		MenuMoney.alpha = 0f;
		MenuMoney.interactable = false;
		MenuConfirm.interactable = false;
		Menu.interactable = true;
		Menu.alpha = 1f;
		SetGraph();
	}

	private void OnEnable()
	{
	}

	public void GetInvestmentTotal()
	{
	}

	public void ConfirmDepositWithdrawl()
	{
		MenuConfirm.alpha = 0f;
		MenuMoney.alpha = 1f;
		MenuMoney.interactable = true;
		MenuConfirm.interactable = false;
		confirmWithdrawl = true;
	}

	public void DeclineDepositWithdrawl()
	{
		MenuConfirm.alpha = 0f;
		MenuMoney.alpha = 1f;
		MenuMoney.interactable = true;
		MenuConfirm.interactable = false;
		StopCoroutine(WithdrawlRoutine(false));
		StartCoroutine(WithdrawlRoutine(withdrawl));
	}

	public void SetCofirmationText(bool _Withdrawl)
	{
		if (_Withdrawl)
		{
			ConfirmationText.text = "Withdrawl: " + InvestmentAdd;
		}
		else
		{
			ConfirmationText.text = "Deposit: " + InvestmentAdd;
		}
	}

	public void CheckInvestment()
	{
		if (DataManager.Instance.CheckInvestmentExist())
		{
			int num = TimeManager.CurrentDay - DataManager.Instance.Investment.LastDayCheckedInvestment;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					JellyFishInvestmentDay jellyFishInvestmentDay = new JellyFishInvestmentDay();
					Vector2 vector = RunInvestment(DataManager.Instance.Investment.InvestmentDays[DataManager.Instance.Investment.InvestmentDays.Count - 1].InvestmentAmount);
					jellyFishInvestmentDay.Day = (DataManager.Instance.Investment.LastDayCheckedInvestment += i);
					jellyFishInvestmentDay.InvestmentAmount = (int)vector.x;
					jellyFishInvestmentDay.InterestRate = vector.y;
					DataManager.Instance.Investment.InitialInvestment += jellyFishInvestmentDay.InvestmentAmount;
					DataManager.Instance.Investment.InvestmentDays.Add(jellyFishInvestmentDay);
					Debug.Log("Run Investments, day : " + jellyFishInvestmentDay.Day + " Interest Earned: " + jellyFishInvestmentDay.InvestmentAmount);
				}
				DataManager.Instance.Investment.LastDayCheckedInvestment = TimeManager.CurrentDay;
				SetGraph();
			}
			else
			{
				SetGraph();
				Debug.Log("Don't run investments, last invest: " + DataManager.Instance.Investment.LastDayCheckedInvestment + "Current Day: " + TimeManager.CurrentDay);
			}
			InitialInvestment = DataManager.Instance.Investment.InitialInvestment;
			UpdateInvestmentAmount(InitialInvestment);
			float f = DataManager.Instance.Investment.InitialInvestment - DataManager.Instance.Investment.ActualInvestedAmount;
			InvestmentProfitTxt.text = "Profit: " + f;
			if (Mathf.Sign(f) == 1f)
			{
				InvestmentProfitTxt.color = StaticColors.GreenColor;
			}
			else
			{
				InvestmentProfitTxt.color = StaticColors.RedColor;
			}
		}
		else
		{
			SetGraph();
			UpdateInvestmentAmount(0);
			InvestmentProfitTxt.text = "";
			Debug.Log("No Investments");
		}
	}

	public Vector2 RunInvestment(int InvestmentAmount)
	{
		Debug.Log("Calculating Investment of " + InvestmentAmount);
		int num = UnityEngine.Random.Range(0, 100);
		float num2 = ((num <= 50) ? 0.1f : ((num <= 80) ? (-0.1f) : ((num <= 88) ? (-0.2f) : ((num <= 90) ? (-0.3f) : ((num > 98) ? 0.3f : 0.2f)))));
		float num3 = (float)InvestmentAmount * num2;
		if ((int)num3 == 0)
		{
			if (num2 >= 0.1f)
			{
				num3 = 1f;
			}
			else if (num2 <= 0.1f)
			{
				num3 = -1f;
			}
		}
		Debug.Log("Investment Ran: " + num3 + "Return of: " + (float)InvestmentAmount * num2);
		return new Vector2((int)num3, num2);
	}

	public bool checkInvestmentDay()
	{
		foreach (JellyFishInvestmentDay investmentDay in DataManager.Instance.Investment.InvestmentDays)
		{
			if (investmentDay.Day == TimeManager.CurrentDay)
			{
				return true;
			}
		}
		return false;
	}

	public int returnInvestmentDay()
	{
		int num = 0;
		foreach (JellyFishInvestmentDay investmentDay in DataManager.Instance.Investment.InvestmentDays)
		{
			if (investmentDay.Day == TimeManager.CurrentDay)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void SetInvestment(int InvestmentAdd)
	{
		JellyFishInvestmentDay jellyFishInvestmentDay = new JellyFishInvestmentDay();
		localInvestment = new JellyFishInvestment();
		Debug.Log("Set Investment");
		if (DataManager.Instance.CheckInvestmentExist())
		{
			Debug.Log("Investment Exists");
			DataManager.Instance.Investment.ActualInvestedAmount += InvestmentAdd;
			DataManager.Instance.Investment.InitialInvestment += InvestmentAdd;
			localInvestment = DataManager.Instance.Investment;
			if (!checkInvestmentDay())
			{
				Debug.Log("Create Investment Day, Day: " + TimeManager.CurrentDay + " Total Investment On Day: " + DataManager.Instance.Investment.InitialInvestment);
				jellyFishInvestmentDay.Day = TimeManager.CurrentDay;
				jellyFishInvestmentDay.InvestmentAmount = (InvestmentAdd += DataManager.Instance.Investment.InitialInvestment);
				localInvestment.InvestmentDays.Add(jellyFishInvestmentDay);
			}
			else
			{
				Debug.Log("Add to existing investment");
				DataManager.Instance.Investment.InvestmentDays[returnInvestmentDay()].InvestmentAmount = (InvestmentAdd += DataManager.Instance.Investment.InitialInvestment);
			}
		}
		else
		{
			Debug.Log("Set Investment, create new one");
			localInvestment.InitialInvestment += InvestmentAdd;
			localInvestment.InvestmentDay = TimeManager.CurrentDay;
			jellyFishInvestmentDay.Day = TimeManager.CurrentDay;
			jellyFishInvestmentDay.InvestmentAmount = InvestmentAdd;
			localInvestment.LastDayCheckedInvestment = TimeManager.CurrentDay;
			localInvestment.ActualInvestedAmount = InvestmentAdd;
			localInvestment.InvestmentDays.Add(jellyFishInvestmentDay);
			DataManager.Instance.CreateInvestment(localInvestment);
		}
		UpdateInvestments();
	}

	public void UpdateInvestments()
	{
		if (DataManager.Instance.CheckInvestmentExist())
		{
			Debug.Log("Investment does exist");
		}
		else
		{
			Debug.Log("Investment doesn't exist");
		}
	}
}
