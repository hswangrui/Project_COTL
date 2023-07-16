using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Heart : BaseMonoBehaviour
{
	public enum HeartType
	{
		Red,
		Blue,
		Spirit,
		Black
	}

	public enum HeartState
	{
		None,
		HeartFull,
		HeartHalfFull,
		HeartHalf,
		HeartContainer,
		HalfHeartContainer
	}

	public Image Circle;

	public RectTransform rectTransform;

	public SkeletonGraphic Spine;

	[SpineSlot("", "", false, true, false, dataField = "Spine")]
	public string Slot;

	[SpineAttachment(true, false, false, "", "", "", true, false, dataField = "Spine")]
	public string Attachment = "";

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string RedHeartSkin = "";

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string BlueHeartSkin = "";

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string BlackHeartSkin = "";

	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	public string halfSkin = "";

	private float _Shake;

	private float ShakeSpeed;

	private float _Scale;

	private float ScaleSpeed;

	private bool wasFull;

	private bool isEffectActivated;

	public HeartType MyHeartType;

	public HeartState MyState;

	private bool Shaking;

	private float ShakeTimer;

	private float Timer;

	private float Shake
	{
		get
		{
			return _Shake;
		}
		set
		{
			_Shake = value;
			Spine.rectTransform.localPosition = new Vector3(0f, Shake);
		}
	}

	private float Scale
	{
		get
		{
			return _Scale;
		}
		set
		{
			_Scale = value;
			Spine.rectTransform.localScale = Vector3.one * Scale;
		}
	}

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
	}

	public void Activate(bool Active, bool DoEffects)
	{
		if (!Active && DoEffects)
		{
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(DeactivateWithEffect());
			}
			return;
		}
		if (DoEffects || isEffectActivated)
		{
			ClearCoroutines();
		}
		base.gameObject.SetActive(Active);
	}

	public void ActivateAndScale(float Delay)
	{
		base.gameObject.SetActive(true);
		ClearCoroutines();
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(DoScale(Delay));
		}
	}

	private IEnumerator DeactivateWithEffect()
	{
		isEffectActivated = true;
		Spine.AnimationState.SetAnimation(0, "disappear", false);
		yield return StartCoroutine(DoCircle());
		yield return new WaitForSeconds(0.1f);
		base.gameObject.SetActive(false);
		isEffectActivated = false;
	}

	public void SetSprite(HeartState NewState, bool DoEffects = false, HeartType NewHeartType = HeartType.Red)
	{
		if (Spine == null || Spine.Skeleton == null)
		{
			return;
		}
		switch (NewHeartType)
		{
		case HeartType.Black:
			Spine.Skeleton.SetSkin(BlackHeartSkin);
			Spine.Skeleton.SetAttachment(Slot, null);
			break;
		case HeartType.Blue:
			Spine.Skeleton.SetSkin(BlueHeartSkin);
			break;
		case HeartType.Spirit:
			Spine.Skeleton.SetSkin(RedHeartSkin);
			Spine.Skeleton.SetAttachment(Slot, Attachment);
			if ((NewState == HeartState.HeartHalf && !wasFull) || (NewState == HeartState.HeartContainer && !wasFull) || (NewState == HeartState.HalfHeartContainer && !wasFull))
			{
				Spine.Skeleton.SetSkin(halfSkin);
			}
			if (NewState == HeartState.HeartFull)
			{
				wasFull = true;
			}
			break;
		default:
			if (Spine != null)
			{
				Spine.Skeleton.SetSkin(RedHeartSkin);
				Spine.Skeleton.SetAttachment(Slot, null);
				if (NewState == HeartState.HeartHalf || NewState == HeartState.HalfHeartContainer)
				{
					Spine.Skeleton.SetSkin(halfSkin);
				}
				if (NewState == HeartState.HeartFull)
				{
					wasFull = true;
				}
			}
			break;
		}
		switch (NewState)
		{
		case HeartState.HeartFull:
			switch (MyState)
			{
			case HeartState.HeartFull:
				Spine.AnimationState.SetAnimation(0, "full", false);
				break;
			case HeartState.HeartHalfFull:
			case HeartState.HeartHalf:
				Spine.AnimationState.SetAnimation(0, "fill-half-right", false);
				Spine.AnimationState.AddAnimation(0, "full", true, 0f);
				break;
			default:
				Spine.AnimationState.SetAnimation(0, "fill-whole", false);
				Spine.AnimationState.AddAnimation(0, "full", true, 0f);
				break;
			}
			break;
		case HeartState.HeartHalfFull:
		case HeartState.HeartHalf:
			switch (MyState)
			{
			case HeartState.HeartHalf:
				Spine.AnimationState.AddAnimation(0, "half-full", true, 0f);
				break;
			case HeartState.HeartFull:
			case HeartState.HeartHalfFull:
				if (MyState != NewState)
				{
					Spine.AnimationState.SetAnimation(0, "lose-half", false);
				}
				Spine.AnimationState.AddAnimation(0, "half-full", true, 0f);
				break;
			default:
				Spine.AnimationState.SetAnimation(0, "fill-half-left", false);
				Spine.AnimationState.AddAnimation(0, "half-full", true, 0f);
				break;
			}
			break;
		case HeartState.HeartContainer:
		case HeartState.HalfHeartContainer:
			switch (MyState)
			{
			case HeartState.HeartFull:
				Spine.AnimationState.SetAnimation(0, "lose-whole", false);
				Spine.AnimationState.AddAnimation(0, "empty", true, 0f);
				break;
			case HeartState.HeartHalfFull:
			case HeartState.HeartHalf:
				Spine.AnimationState.SetAnimation(0, "lose-whole", false);
				Spine.AnimationState.AddAnimation(0, "empty", true, 0f);
				break;
			default:
				Spine.AnimationState.AddAnimation(0, "empty", true, 0f);
				break;
			}
			break;
		}
		MyState = NewState;
		MyHeartType = NewHeartType;
		Spine.UpdateMesh();
	}

	private void ClearCoroutines()
	{
		Shake = 0f;
		Scale = 1f;
		StopAllCoroutines();
		isEffectActivated = false;
	}

	private IEnumerator DoShake()
	{
		StartCoroutine(DoCircle());
		Shaking = true;
		ShakeTimer = 0f;
		ShakeSpeed = 10f;
		while ((double)(ShakeTimer += Time.deltaTime) < 1.5)
		{
			ShakeSpeed += (0f - Shake) * 0.3f;
			Shake += (ShakeSpeed *= 0.9f);
			yield return null;
		}
		Shake = 0f;
		Shaking = false;
	}

	private IEnumerator DoCircle()
	{
		Color color = new Color(1f, 1f, 1f, 1f);
		Color colorOff = new Color(1f, 1f, 1f, 0f);
		Circle.transform.localScale = Vector3.zero;
		Circle.DOKill();
		Circle.color = color;
		Circle.DOColor(colorOff, 1f);
		Circle.transform.DOScale(Vector3.one * 2f, 1f);
		yield return new WaitForSeconds(1f);
		Circle.transform.localScale = Vector3.zero;
		Circle.color = colorOff;
	}

	private IEnumerator DoScale(float Delay)
	{
		Scale = 0f;
		while (true)
		{
			float num;
			Delay = (num = Delay - Time.deltaTime);
			if (!(num > 0f))
			{
				break;
			}
			yield return null;
		}
		Timer = 0f;
		Scale = 0f;
		while ((Timer += Time.deltaTime) < 15f)
		{
			ScaleSpeed += (1f - Scale) * 0.2f;
			Scale += (ScaleSpeed *= 0.8f);
			yield return null;
		}
		Scale = 1f;
	}
}
