using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using TMPro;
using Unify.Input;
using UnityEngine;
using UnityEngine.UI;

public class UICommandFollowerWheel : BaseMonoBehaviour
{
	[Serializable]
	public class ActivityChoice
	{
		public enum AvailableCommands
		{
			ChopTrees,
			ClearWeeds,
			ClearRubble,
			Cancel
		}

		public AvailableCommands Activity;

		public RectTransform rectTransform;

		public TextMeshProUGUI Text;

		public Image Image;

		public float Angle;

		public Vector3 StartingPosition;

		public void Init()
		{
			rectTransform.gameObject.name = Activity.ToString();
			Text = rectTransform.gameObject.GetComponentInChildren<TextMeshProUGUI>();
			Text.color = Color.white;
			Text.text = Activity.ToString();
			StartingPosition = rectTransform.position;
		}
	}

	public CanvasGroup canvasGroup;

	public Action<ActivityChoice.AvailableCommands> CallbackClose;

	private CanvasScaler canvas;

	public static UICommandFollowerWheel Instance;

	public GameObject FollowerPortraitPrefab;

	public RectTransform Pointer;

	public List<ActivityChoice> ActivityChoices = new List<ActivityChoice>();

	public float ActivityDistance = 300f;

	private ActivityChoice Closest;

	private float Angle = 4.712389f;

	private float PointerDistance = 310f;

	private float PointerSpeed = 25f;

	private bool Released;

	private Player Input
	{
		get
		{
			return RewiredInputManager.MainPlayer;
		}
	}

	private void GetAngles()
	{
		foreach (ActivityChoice activityChoice in ActivityChoices)
		{
			activityChoice.Angle = Utils.GetAngle(Vector3.zero, activityChoice.rectTransform.localPosition);
			activityChoice.Init();
		}
	}

	private void SetPositions()
	{
		foreach (ActivityChoice activityChoice in ActivityChoices)
		{
			activityChoice.rectTransform.localPosition = Vector3.Lerp(activityChoice.rectTransform.localPosition, new Vector3(ActivityDistance * Mathf.Cos(activityChoice.Angle * ((float)Math.PI / 180f)), ActivityDistance * Mathf.Sin(activityChoice.Angle * ((float)Math.PI / 180f))), 25f * Time.unscaledDeltaTime);
		}
	}

	public void AddPortrait(FollowerBrainInfo f)
	{
		FollowerCommandWheelPortrait component = UnityEngine.Object.Instantiate(FollowerPortraitPrefab, FollowerPortraitPrefab.transform.parent).GetComponent<FollowerCommandWheelPortrait>();
		component.gameObject.SetActive(true);
		component.Play(f);
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

	private void Start()
	{
		canvas = GetComponentInParent<CanvasScaler>();
		foreach (ActivityChoice activityChoice in ActivityChoices)
		{
			activityChoice.Init();
		}
		StartCoroutine(DoLoop());
	}

	private IEnumerator DoLoop()
	{
		Pointer.gameObject.SetActive(false);
		while (InputManager.UI.GetPageNavigateLeftHeld())
		{
			foreach (ActivityChoice activityChoice in ActivityChoices)
			{
				activityChoice.rectTransform.localScale = Vector3.zero;
			}
			yield return null;
		}
		Pointer.gameObject.SetActive(true);
		Time.timeScale = 0.1f;
		PlayerFarming.Instance.Spine.UseDeltaTime = false;
		while (true)
		{
			if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > 0.3f || Mathf.Abs(InputManager.UI.GetVerticalAxis()) > 0.3f)
			{
				Angle = Utils.GetAngle(Vector3.zero, new Vector3(InputManager.UI.GetHorizontalAxis(), InputManager.UI.GetVerticalAxis())) * ((float)Math.PI / 180f);
			}
			Pointer.localPosition = Vector3.Lerp(Pointer.localPosition, new Vector3(PointerDistance * Mathf.Cos(Angle), PointerDistance * Mathf.Sin(Angle)), Time.unscaledDeltaTime * PointerSpeed);
			Pointer.eulerAngles = new Vector3(0f, 0f, Utils.GetAngle(Vector3.zero, Pointer.localPosition) - 90f);
			Closest = null;
			float num = float.MaxValue;
			foreach (ActivityChoice activityChoice2 in ActivityChoices)
			{
				float num2 = Vector3.Distance(Pointer.position, activityChoice2.StartingPosition);
				if (activityChoice2.rectTransform.gameObject.activeSelf && num2 < num)
				{
					num = num2;
					Closest = activityChoice2;
				}
			}
			float num3 = -1f;
			foreach (ActivityChoice activityChoice3 in ActivityChoices)
			{
				Vector3 b = Vector3.one * ((Closest == activityChoice3) ? 1.1f : 1f);
				activityChoice3.rectTransform.localScale = Vector3.Lerp(activityChoice3.rectTransform.localScale, b, ((num3 += 1f) + 25f) * Time.unscaledDeltaTime);
				num = ((Closest == activityChoice3) ? 220f : 200f);
				activityChoice3.Text.color = ((Closest == activityChoice3) ? Color.yellow : Color.white);
				activityChoice3.rectTransform.localPosition = Vector3.Lerp(activityChoice3.rectTransform.localPosition, new Vector3(num * Mathf.Cos(activityChoice3.Angle * ((float)Math.PI / 180f)), num * Mathf.Sin(activityChoice3.Angle * ((float)Math.PI / 180f))), 25f * Time.unscaledDeltaTime);
			}
			if (Closest != null && Released && InputManager.UI.GetAcceptButtonDown())
			{
				break;
			}
			if (!InputManager.UI.GetAcceptButtonDown())
			{
				Released = true;
			}
			yield return null;
		}
		ActivityChoice.AvailableCommands activity = Closest.Activity;
		Debug.Log(Closest.Activity);
		foreach (ActivityChoice activityChoice4 in ActivityChoices)
		{
			activityChoice4.Init();
			if (activityChoice4.Image != null)
			{
				activityChoice4.Image.color = ((Closest.Activity == activityChoice4.Activity) ? Color.white : Color.black);
			}
		}
		Time.timeScale = 1f;
		PlayerFarming.Instance.Spine.UseDeltaTime = true;
		StartCoroutine(CallBackRoutine());
		float Progress = 0f;
		float Duration = 0.3f;
		while (true)
		{
			float num4;
			Progress = (num4 = Progress + Time.unscaledDeltaTime);
			if (!(num4 < Duration))
			{
				break;
			}
			if (Closest.Image != null && Time.frameCount % 15 == 0)
			{
				Closest.Image.color = ((Closest.Image.color == Color.white) ? Color.black : Color.white);
			}
			canvasGroup.alpha = 1f - Progress / Duration;
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private IEnumerator CallBackRoutine()
	{
		yield return new WaitForSeconds(0.1f);
		Action<ActivityChoice.AvailableCommands> callbackClose = CallbackClose;
		if (callbackClose != null)
		{
			callbackClose(Closest.Activity);
		}
	}
}
