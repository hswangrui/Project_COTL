using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFollowerInteractionWheel : BaseMonoBehaviour
{
	[Serializable]
	public class ActivityChoice
	{
		public FollowerCommands Activity;

		public WheelPosition WheelPosition;

		public RectTransform rectTransform;

		public TextMeshProUGUI Text;

		public GameObject NotificationIcon;

		public Image Image;

		public float Angle;

		public Vector3 StartingPosition;

		public string Description;

		public void Init(Follower Follower, FollowerCommands Activity, bool showNotification)
		{
			rectTransform.gameObject.SetActive(true);
			this.Activity = Activity;
			rectTransform.gameObject.name = WheelPosition.ToString();
			Text = rectTransform.gameObject.GetComponentInChildren<TextMeshProUGUI>();
			Text.color = TextColor;
			Text.text = LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}", Activity));
			Description = LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}/Description", Activity));
			if (Activity == FollowerCommands.GiveWorkerCommand_2 && !Follower.Brain.Stats.WorkerBeenGivenOrders)
			{
				Text.text = "<color=yellow>" + LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}", Activity)) + "</color>";
			}
			if (NotificationIcon != null)
			{
				NotificationIcon.SetActive(showNotification);
			}
			StartingPosition = rectTransform.position;
		}
	}

	public Transform Container;

	private static Color TextColor = new Color(0.9960784f, 0.9411765f, 0.827451f);

	public CanvasGroup canvasGroup;

	public Action<FollowerCommands> CallbackClose;

	public Action CallbackCancel;

	private CanvasScaler canvas;

	public static UIFollowerInteractionWheel Instance = null;

	public RectTransform Pointer;

	private float PointerSpeed = 25f;

	public TextMeshProUGUI SermonName;

	public TextMeshProUGUI SermonDescription;

	public List<ActivityChoice> ActivityChoices = new List<ActivityChoice>();

	private float ActivityDistance = 200f;

	private Follower Follower;

	private string TitleText;

	private ActivityChoice Closest;

	private static float Angle = 4.712389f;

	private float PointerDistance = 50f;

	private void GetAngles()
	{
		foreach (ActivityChoice activityChoice in ActivityChoices)
		{
			activityChoice.Angle = Utils.GetAngle(Vector3.zero, activityChoice.rectTransform.localPosition);
			activityChoice.Init(null, FollowerCommands.Talk, false);
		}
	}

	private void SetPositions()
	{
		foreach (ActivityChoice activityChoice in ActivityChoices)
		{
			activityChoice.rectTransform.localPosition = Vector3.Lerp(activityChoice.rectTransform.localPosition, new Vector3(ActivityDistance * Mathf.Cos(activityChoice.Angle * ((float)Math.PI / 180f)), ActivityDistance * Mathf.Sin(activityChoice.Angle * ((float)Math.PI / 180f))), 25f * Time.unscaledDeltaTime);
		}
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void Play(Follower Follower, List<CommandPosition> CommandsAndPositions, Action<FollowerCommands> CallbackClose, Action CallbackCancel, bool ResetAngle = false, FollowerCommands Title = FollowerCommands.None)
	{
		this.Follower = Follower;
		TitleText = ((Title == FollowerCommands.None) ? "" : LocalizationManager.GetTranslation(string.Format("FollowerInteractions/{0}", Title)));
		canvas = GetComponentInParent<CanvasScaler>();
		foreach (ActivityChoice activityChoice in ActivityChoices)
		{
			activityChoice.rectTransform.gameObject.SetActive(false);
		}
		foreach (CommandPosition CommandsAndPosition in CommandsAndPositions)
		{
			if (CommandsAndPosition == null)
			{
				continue;
			}
			foreach (ActivityChoice activityChoice2 in ActivityChoices)
			{
				if (activityChoice2.WheelPosition == CommandsAndPosition.WheelPosition)
				{
					activityChoice2.Init(Follower, CommandsAndPosition.FollowerCommand, CommandsAndPosition.ShowNotification);
				}
			}
		}
		this.CallbackClose = CallbackClose;
		this.CallbackCancel = CallbackCancel;
		StartCoroutine(DoLoop());
		TextMeshProUGUI sermonName = SermonName;
		string text2 = (SermonDescription.text = "");
		sermonName.text = text2;
		if (ResetAngle)
		{
			Angle = 4.712389f;
		}
	}

	private IEnumerator DoLoop()
	{
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
			canvasGroup.alpha = Progress / Duration2;
			yield return null;
		}
		canvasGroup.alpha = 1f;
		while (true)
		{
			if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > 0.3f || Mathf.Abs(InputManager.UI.GetVerticalAxis()) > 0.3f)
			{
				Angle = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.UI.GetHorizontalAxis(), InputManager.UI.GetVerticalAxis())) * ((float)Math.PI / 180f);
			}
			Pointer.localPosition = Vector3.Lerp(Pointer.localPosition, new Vector3(PointerDistance * Mathf.Cos(Angle), PointerDistance * Mathf.Sin(Angle)), Time.unscaledDeltaTime * PointerSpeed);
			Pointer.eulerAngles = new Vector3(0f, 0f, Utils.GetAngle(Vector3.zero, Pointer.localPosition) - 90f);
			Closest = null;
			float num2 = float.MaxValue;
			foreach (ActivityChoice activityChoice in ActivityChoices)
			{
				float num3 = Vector3.Distance(Pointer.position, activityChoice.StartingPosition);
				if (activityChoice.rectTransform.gameObject.activeSelf && num3 < num2)
				{
					num2 = num3;
					Closest = activityChoice;
				}
			}
			float num4 = -1f;
			foreach (ActivityChoice activityChoice2 in ActivityChoices)
			{
				Vector3 b = Vector3.one * ((Closest == activityChoice2) ? 1.1f : 1f);
				activityChoice2.rectTransform.localScale = Vector3.Lerp(activityChoice2.rectTransform.localScale, b, ((num4 += 1f) + 25f) * Time.unscaledDeltaTime);
				num2 = ((Closest == activityChoice2) ? 220f : 200f);
				activityChoice2.Text.color = ((Closest == activityChoice2) ? Color.yellow : TextColor);
				activityChoice2.rectTransform.localPosition = Vector3.Lerp(activityChoice2.rectTransform.localPosition, new Vector3(num2 * Mathf.Cos(activityChoice2.Angle * ((float)Math.PI / 180f)), num2 * Mathf.Sin(activityChoice2.Angle * ((float)Math.PI / 180f))), 25f * Time.unscaledDeltaTime);
			}
			if (Closest != null)
			{
				SermonName.text = ((TitleText == "") ? Closest.Text.text : TitleText);
				SermonDescription.text = Closest.Description;
			}
			if (Closest == null || !InputManager.UI.GetAcceptButtonDown())
			{
				if (InputManager.UI.GetCancelButtonDown() && CallbackCancel != null)
				{
					Action callbackCancel = CallbackCancel;
					if (callbackCancel != null)
					{
						callbackCancel();
					}
					break;
				}
				yield return null;
				continue;
			}
			Action<FollowerCommands> callbackClose = CallbackClose;
			if (callbackClose != null)
			{
				callbackClose(Closest.Activity);
			}
			break;
		}
		Progress = 0f;
		Duration2 = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration2))
			{
				break;
			}
			canvasGroup.alpha = 1f - Progress / Duration2;
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
