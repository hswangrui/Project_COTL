using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStructureEffectTile : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public StructureEffectManager.EffectType Type;

	public TextMeshProUGUI Text;

	public TextMeshProUGUI AvailabilityText;

	public Selectable Selectable;

	public RectTransform Container;

	public StructureEffectManager.State State;

	public Image CoolDownProgressIcon;

	private Canvas canvas;

	private Coroutine cShakeRoutine;

	public void Init(StructureEffectManager.EffectType Type, int ID)
	{
		canvas = GetComponentInParent<Canvas>();
		this.Type = Type;
		Text.text = Type.ToString();
		CoolDownProgressIcon.gameObject.SetActive(false);
		switch (State = StructureEffectManager.GetEffectAvailability(ID, Type))
		{
		case StructureEffectManager.State.DoesntExist:
			AvailabilityText.text = "Available";
			break;
		case StructureEffectManager.State.Cooldown:
			AvailabilityText.text = "Cooling Down...";
			CoolDownProgressIcon.gameObject.SetActive(true);
			CoolDownProgressIcon.fillAmount = 1f - StructureEffectManager.GetEffectCoolDownProgress(ID, Type);
			break;
		case StructureEffectManager.State.Active:
			AvailabilityText.text = "Currently Active";
			break;
		}
	}

	public void Shake()
	{
		if (cShakeRoutine != null)
		{
			StopCoroutine(cShakeRoutine);
		}
		cShakeRoutine = StartCoroutine(ShakeRoutine());
	}

	private IEnumerator ShakeRoutine()
	{
		float Progress = 0f;
		float Duration = 2f;
		float Speed = 100f * canvas.scaleFactor;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Container.localPosition = Vector3.right * Utils.BounceLerpUnscaledDeltaTime(0f, Container.localPosition.x, ref Speed);
			yield return null;
		}
		Container.localPosition = Vector3.zero;
	}

	public void OnSelect(BaseEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(Selected(Container.localScale.x, 1.3f));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(DeSelected());
	}

	private IEnumerator Selected(float Starting, float Target)
	{
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
			float num2 = Mathf.SmoothStep(Starting, Target, Progress / Duration);
			Container.localScale = Vector3.one * num2;
			yield return null;
		}
		Container.localScale = Vector3.one * Target;
	}

	private IEnumerator DeSelected()
	{
		float Progress = 0f;
		float Duration = 0.3f;
		float StartingScale = Container.localScale.x;
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
			Container.localScale = Vector3.one * num2;
			yield return null;
		}
		Container.localScale = Vector3.one * TargetScale;
	}
}
