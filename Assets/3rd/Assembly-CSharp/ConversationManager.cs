using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationManager : BaseMonoBehaviour
{
	public static ConversationManager instance;

	private bool IN_CONVERSATION;

	private GameObject Speaker;

	public GameObject OptionWheel;

	public Image SpeechBubble;

	private RectTransform SpeechBubbleRT;

	public TextMeshProUGUI SpeechText;

	public List<Response> Responses;

	public List<TextMeshProUGUI> ResponseText;

	private float Angle;

	private float PointerAngle;

	private float AngleApprox;

	private float ArrowAngle;

	public GameObject Arrow;

	public GameObject Pointer;

	public GameObject PointerDistance;

	private GameObject CurrentGameObject;

	public static int CURRENT_ANSWER;

	private void OnEnable()
	{
		instance = this;
		HideAll();
	}

	public static ConversationManager GetInstance()
	{
		return instance;
	}

	private void Update()
	{
		if (IN_CONVERSATION)
		{
			Vector3 position = Speaker.transform.position;
			position = Camera.main.WorldToScreenPoint(position);
			SpeechBubbleRT.position = position;
			MoveOptionWheel();
		}
	}

	private void OnDisable()
	{
		instance = null;
		HideAll();
	}

	public void NewConversation(string text, string ID, List<Response> Responses)
	{
		ShowAll();
		SpeechText.text = text;
		float num = SpeechText.preferredWidth + 40f;
		if (num > SpeechText.rectTransform.sizeDelta.x)
		{
			num = SpeechText.rectTransform.sizeDelta.x + 40f;
		}
		float y = SpeechText.preferredHeight + 40f;
		SpeechBubble.rectTransform.sizeDelta = new Vector2(num, y);
		SpeechBubbleRT = SpeechBubble.GetComponent<RectTransform>();
		GameManager.GetInstance().CameraSetConversationMode(true);
		GameManager.GetInstance().RemoveAllFromCamera();
		StateMachine component = GameObject.FindWithTag("Player").GetComponent<StateMachine>();
		component.CURRENT_STATE = StateMachine.State.InActive;
		if (component != null)
		{
			component.CURRENT_STATE = StateMachine.State.InActive;
		}
		this.Responses = Responses;
		for (int i = 0; i < Responses.Count; i++)
		{
			ResponseText[i].text = Responses[i].text;
		}
		IN_CONVERSATION = true;
	}

	private void EndConversation()
	{
		IN_CONVERSATION = false;
		StateMachine component = GameObject.FindWithTag("Player").GetComponent<StateMachine>();
		component.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().CameraSetConversationMode(false);
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().AddPlayerToCamera();
		component = Speaker.GetComponent<StateMachine>();
		if (component != null)
		{
			component.CURRENT_STATE = StateMachine.State.Idle;
		}
		HideAll();
		if (Responses[CURRENT_ANSWER] != null && Responses[CURRENT_ANSWER].Callback != null)
		{
			Responses[CURRENT_ANSWER].Callback();
		}
	}

	private void ShowAll()
	{
		CURRENT_ANSWER = 0;
		Angle = (PointerAngle = (AngleApprox = 0f));
		Arrow.transform.eulerAngles = Vector3.zero;
		Pointer.transform.eulerAngles = Vector3.zero;
		SpeechBubble.gameObject.SetActive(true);
		OptionWheel.SetActive(true);
		for (int i = 0; i < ResponseText.Count; i++)
		{
			ResponseText[i].text = "";
		}
	}

	private void HideAll()
	{
		IN_CONVERSATION = false;
		SpeechBubble.gameObject.SetActive(false);
		OptionWheel.SetActive(false);
	}

	private void MoveOptionWheel()
	{
		if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > 0.2f || Mathf.Abs(InputManager.UI.GetVerticalAxis()) > 0.2f)
		{
			Angle = Utils.GetAngle(new Vector3(InputManager.UI.GetHorizontalAxis(), InputManager.UI.GetVerticalAxis()), Vector3.zero) + 90f;
			CheckDistance();
		}
		PointerAngle += Mathf.Atan2(Mathf.Sin((Angle - PointerAngle) * ((float)Math.PI / 180f)), Mathf.Cos((Angle - PointerAngle) * ((float)Math.PI / 180f))) * 57.29578f / 3f;
		Pointer.transform.eulerAngles = new Vector3(0f, 0f, PointerAngle);
		if (CurrentGameObject != null)
		{
			AngleApprox = Utils.GetAngle(CurrentGameObject.transform.localPosition, Arrow.transform.localPosition) + 90f;
			ArrowAngle += Mathf.Atan2(Mathf.Sin((AngleApprox - ArrowAngle) * ((float)Math.PI / 180f)), Mathf.Cos((AngleApprox - ArrowAngle) * ((float)Math.PI / 180f))) * 57.29578f / 3f;
			Arrow.transform.eulerAngles = new Vector3(0f, 0f, ArrowAngle);
			CurrentGameObject.transform.localScale = new Vector3(1.1f, 1.1f);
		}
		if (InputManager.UI.GetAcceptButtonUp())
		{
			EndConversation();
		}
	}

	private void CheckDistance()
	{
		float num = float.MaxValue;
		for (int i = 0; i < ResponseText.Count; i++)
		{
			TextMeshProUGUI textMeshProUGUI = ResponseText[i];
			textMeshProUGUI.transform.localScale = new Vector3(1f, 1f);
			float num2 = Vector2.Distance(PointerDistance.transform.position, textMeshProUGUI.transform.position);
			if (num2 < num && textMeshProUGUI.text != "")
			{
				CURRENT_ANSWER = i;
				CurrentGameObject = textMeshProUGUI.gameObject;
				num = num2;
			}
		}
	}
}
