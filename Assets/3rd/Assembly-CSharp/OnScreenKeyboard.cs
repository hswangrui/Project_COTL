using Lamb.UI;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnScreenKeyboard : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public UnityEvent OnSkip;

	public GameObject debugKeyboard;

	public string title;

	public string description;

	private bool keyboardShown;

	private static bool isSubmit;

	public bool keepInputSelect;

	private MMInputField inputField;

	private GameObject prevSelectable;

	public bool SubmitOnClose = true;

	protected Callback<GamepadTextInputDismissed_t> m_GamepadTextInputDismissed;

	private string prevText = "";

	private bool Selectable;

	public static bool IsSubmit
	{
		get
		{
			if (isSubmit)
			{
				isSubmit = false;
				return true;
			}
			return false;
		}
	}

	private void Start()
	{
		Debug.Log("OSK:Start");
		Debug.Log("Registered native keyboard callbacks.");
		inputField = GetComponent<MMInputField>();
	}

	private void OnDestroy()
	{
	}

	public void ShowKeyboard()
	{
		Debug.Log("Show keyboard");
		isSubmit = false;
		keepInputSelect = true;
		if (!keyboardShown)
		{
			string text = GetComponent<MMInputField>().text;
			if (text == null)
			{
				text = "";
			}
			else
			{
				prevText = text;
			}
			if (debugKeyboard != null)
			{
				debugKeyboard.SetActive(true);
				debugKeyboard.transform.Find("CurrentText").gameObject.GetComponent<Text>().text = text;
				keyboardShown = true;
			}
			prevSelectable = EventSystem.current.currentSelectedGameObject;
			Debug.Log("Current Selectable: " + prevSelectable);
			Debug.Log("Shown keyboard: " + keyboardShown);
		}
	}

	public void KeyboardFinished(string message)
	{
		Debug.Log("Keyboard Text Entry Finished: " + message);
		keyboardShown = false;
		keepInputSelect = false;
		if (message == "")
		{
			inputField.text = prevText;
		}
		else
		{
			inputField.text = message;
		}
		if (SubmitOnClose)
		{
			isSubmit = true;
			inputField.onEndEdit.Invoke(message);
		}
		if (prevSelectable != null)
		{
			prevSelectable.GetComponent<Selectable>().Select();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!Selectable)
		{
			Selectable = true;
			return;
		}
		Debug.Log("Input Element selected: " + base.gameObject.name);
		ShowKeyboard();
	}

	public void Update()
	{
		if (keyboardShown && keepInputSelect && EventSystem.current.currentSelectedGameObject != inputField.gameObject)
		{
			inputField.Select();
		}
	}

	private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t pCallback)
	{
	}
}
