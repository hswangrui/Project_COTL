using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResurrectOnHud : BaseMonoBehaviour
{
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Image image;

	private static ResurrectOnHud Instance;

	public static ResurrectionType OverridenResurrectionType;

	public static bool HasRessurection
	{
		get
		{
			return ResurrectionType != ResurrectionType.None;
		}
	}

	public static ResurrectionType ResurrectionType
	{
		get
		{
			return DataManager.Instance.ResurrectionType;
		}
		set
		{
			bool hasRessurection = HasRessurection;
			DataManager.Instance.ResurrectionType = value;
			if (Instance != null)
			{
				if (hasRessurection && !HasRessurection)
				{
					Instance.Hide();
				}
				else if (!hasRessurection && HasRessurection)
				{
					Instance.Reveal();
				}
			}
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

	private void Start()
	{
		image.enabled = HasRessurection;
	}

	private void Hide()
	{
		StopAllCoroutines();
		StartCoroutine(HideRoutine());
	}

	private IEnumerator HideRoutine()
	{
		yield return new WaitForSeconds(1f);
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(1f, 0f, Progress / Duration);
			image.rectTransform.localScale = new Vector3(num2, num2);
			yield return null;
		}
		image.enabled = false;
	}

	private void Reveal()
	{
		StartCoroutine(RevealRoutine());
	}

	private IEnumerator RevealRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		image.enabled = true;
		float Progress = 0f;
		float Duration = 0.5f;
		float num = 3f;
		float ScaleStart = num;
		float ScaleEnd = 1f;
		while (true)
		{
			float num2;
			Progress = (num2 = Progress + Time.deltaTime);
			if (num2 < Duration)
			{
				num = Mathf.Lerp(ScaleStart, ScaleEnd, curve.Evaluate(Progress / Duration));
				image.rectTransform.localScale = new Vector3(num, num);
				yield return null;
				continue;
			}
			break;
		}
	}
}
