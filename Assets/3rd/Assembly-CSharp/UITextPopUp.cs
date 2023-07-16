using System.Collections;
using TMPro;
using UnityEngine;

public class UITextPopUp : BaseMonoBehaviour
{
	public TextMeshProUGUI Text;

	public RectTransform rectTransform;

	public CanvasGroup canvasGroup;

	private GameObject LockObject;

	private Vector3 Offset;

	private Canvas canvas;

	private bool Moving;

	private float Scale = 3f;

	private float ScaleSpeed;

	public float Speed = 50f;

	public float LifeDuration = 3f;

	private Vector3 DistanceTravelled;

	public static void Create(string String, Color Color, Vector3 Position, Vector3 Offset, GameObject LockObject)
	{
		Object.Instantiate(Resources.Load<UITextPopUp>("Prefabs/UI/UI Text PopUp"), GlobalCanvasReference.Instance).Play(String, Color, Position, Offset, LockObject);
	}

	public static void Create(string String, Color Color, GameObject LockObject, Vector3 Offset)
	{
		Object.Instantiate(Resources.Load<UITextPopUp>("Prefabs/UI/UI Text PopUp"), GlobalCanvasReference.Instance).Play(String, Color, Vector3.zero, Offset, LockObject);
	}

	public void Play(string String, Color Color, Vector3 Position, Vector3 Offset, GameObject LockObject)
	{
		Text.text = String;
		Text.color = Color;
		this.Offset = Offset;
		this.LockObject = LockObject;
		canvas = GetComponentInParent<Canvas>();
		rectTransform.position = Camera.main.WorldToScreenPoint(Position);
		StartCoroutine(Loop());
		StartCoroutine(Move());
	}

	private IEnumerator Loop()
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < LifeDuration))
			{
				break;
			}
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, Progress / 0.25f);
			ScaleSpeed += (1f - Scale) * 0.4f;
			Scale += (ScaleSpeed *= 0.6f);
			rectTransform.localScale = Vector3.one * Scale;
			yield return null;
		}
		Moving = true;
		yield return new WaitForSeconds(0.5f);
		Progress = 0f;
		float Duration = 0.3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, Progress / Duration);
			yield return null;
		}
		StopAllCoroutines();
		Object.Destroy(base.gameObject);
	}

	private IEnumerator Move()
	{
		while (true)
		{
			if (LockObject != null)
			{
				rectTransform.position = Camera.main.WorldToScreenPoint(LockObject.transform.position + Offset);
				if (Moving)
				{
					DistanceTravelled += new Vector3(0f, Speed * Time.deltaTime * canvas.scaleFactor);
					rectTransform.position += DistanceTravelled;
				}
			}
			else if (Moving)
			{
				rectTransform.position += new Vector3(0f, Speed * Time.deltaTime * canvas.scaleFactor);
			}
			yield return null;
		}
	}
}
