using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DwellingAssignMenu : BaseMonoBehaviour
{
	public GameObject ImagePrefab;

	private StateMachine PlayerState;

	public RectTransform RightArrow;

	public RectTransform LeftArrow;

	public TextMeshProUGUI Name;

	public TextMeshProUGUI Description;

	private int current_selection;

	private float selectionDelay;

	private List<Image> icons;

	private Dwelling dwelling;

	private GameObject CurrentWorkerIcon;

	private int CURRENT_SELECTION
	{
		get
		{
			return current_selection;
		}
		set
		{
			current_selection = value;
			if (current_selection < 0)
			{
				current_selection = DataManager.Instance.Followers.Count - 1;
			}
			if (current_selection > DataManager.Instance.Followers.Count - 1)
			{
				current_selection = 0;
			}
			selectionDelay = 0.2f;
		}
	}

	public void Show(string Name, string Description, Dwelling dwelling)
	{
		this.Name.text = Name;
		this.Description.text = Description;
		this.dwelling = dwelling;
		base.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		if (GameObject.FindGameObjectWithTag("Player") != null)
		{
			PlayerState = GameObject.FindGameObjectWithTag("Player").GetComponent<StateMachine>();
			PlayerState.CURRENT_STATE = StateMachine.State.InActive;
		}
		AddCurrentWorshipperIcon();
		icons = new List<Image>();
		int num = -1;
		GameObject gameObject = null;
		int num2 = -1;
		int num3 = -1;
		while (++num < DataManager.Instance.Followers.Count)
		{
			if (num % 6 == 0)
			{
				num2++;
				num3 = 0;
			}
			else
			{
				num3++;
			}
			gameObject = Object.Instantiate(ImagePrefab, Vector3.zero, Quaternion.identity);
			gameObject.transform.parent = base.gameObject.transform;
			gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-525 + 220 * num3, LeftArrow.transform.localPosition.y + (float)(num2 * -250));
			gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f);
			FollowerInfo followerInfo = DataManager.Instance.Followers[num];
			gameObject.GetComponent<WorshipperIcon>().Name.text = followerInfo.Name;
			icons.Add(gameObject.GetComponent<Image>());
		}
		CURRENT_SELECTION = 0;
	}

	private void AddCurrentWorshipperIcon()
	{
		Worshipper worshipper = null;
		if (worshipper != null)
		{
			CurrentWorkerIcon = Object.Instantiate(ImagePrefab, Vector3.zero, Quaternion.identity);
			CurrentWorkerIcon.transform.parent = base.gameObject.transform;
			CurrentWorkerIcon.GetComponent<RectTransform>().localPosition = new Vector3(600f, 300f);
			WorshipperIcon component = CurrentWorkerIcon.GetComponent<WorshipperIcon>();
			component.Name.text = worshipper.wim.v_i.Name;
			component.Icon.color = new Color(worshipper.wim.v_i.color_r, worshipper.wim.v_i.color_g, worshipper.wim.v_i.color_b);
		}
	}

	private void OnDisable()
	{
		foreach (Image icon in icons)
		{
			Object.Destroy(icon.gameObject);
		}
		icons.Clear();
		Object.Destroy(CurrentWorkerIcon);
		CurrentWorkerIcon = null;
		if (PlayerState != null)
		{
			PlayerState.CURRENT_STATE = StateMachine.State.Idle;
		}
	}

	private void Update()
	{
		selectionDelay -= Time.deltaTime;
		if (InputManager.UI.GetHorizontalAxis() > 0.3f && selectionDelay < 0f)
		{
			int cURRENT_SELECTION = CURRENT_SELECTION + 1;
			CURRENT_SELECTION = cURRENT_SELECTION;
		}
		if (InputManager.UI.GetHorizontalAxis() < -0.3f && selectionDelay < 0f)
		{
			int cURRENT_SELECTION = CURRENT_SELECTION - 1;
			CURRENT_SELECTION = cURRENT_SELECTION;
		}
		if (InputManager.UI.GetHorizontalAxis() > -0.3f && InputManager.UI.GetHorizontalAxis() < 0.3f && InputManager.UI.GetVerticalAxis() > -0.3f && InputManager.UI.GetVerticalAxis() < 0.3f)
		{
			selectionDelay = 0f;
		}
		for (int i = 0; i < icons.Count; i++)
		{
			if (i == CURRENT_SELECTION)
			{
				icons[i].transform.localScale = new Vector3(1.5f, 1.5f);
			}
		}
		if (InputManager.UI.GetCancelButtonUp())
		{
			base.gameObject.SetActive(false);
		}
		if (InputManager.UI.GetAcceptButtonUp())
		{
			base.gameObject.SetActive(false);
			if (PlayerState != null)
			{
				AssignWorshipper();
			}
		}
	}

	public void AssignWorshipper()
	{
		Worshipper worshipper = null;
		if (worshipper != null)
		{
			Worshipper.ClearDwelling(worshipper);
		}
	}
}
