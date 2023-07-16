using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class HUD_AmmoIcon : BaseMonoBehaviour
{
	public enum Mode
	{
		ON,
		EMPTY,
		OFF,
		ON_SPIRIT,
		EMPTY_SPIRIT
	}

	public Sprite On;

	public Sprite Empty;

	public Sprite SpiritOn;

	public Sprite SpiritEmpty;

	private RectTransform _rectTransform;

	public SkeletonGraphic Spine;

	[SpineSlot("", "", false, true, false, dataField = "Spine")]
	public string Slot;

	[SpineAttachment(true, false, false, "", "", "", true, false, dataField = "Spine")]
	public string Attachment = "images/AmmoSpiritOutline";

	private Image _Image;

	private Mode mode;

	private Mode PrevMode;

	private Coroutine A;

	private Coroutine D;

	private Coroutine AE;

	private float ScaleTimer;

	private float _Scale;

	private float ScaleSpeed;

	private bool Shaking;

	private float ShakeTimer;

	private float Shake;

	private float ShakeSpeed;

	private Vector3 InitPos;

	private RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
		set
		{
			rectTransform = value;
		}
	}

	private Image Image
	{
		get
		{
			if (_Image == null)
			{
				_Image = GetComponent<Image>();
			}
			return _Image;
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

	public void SetMode(Mode mode, float Delay)
	{
		if (A != null)
		{
			StopCoroutine(A);
		}
		if (D != null)
		{
			StopCoroutine(D);
		}
		if (AE != null)
		{
			StopCoroutine(AE);
		}
		this.mode = mode;
		switch (mode)
		{
		case Mode.ON:
		case Mode.ON_SPIRIT:
			Spine.gameObject.SetActive(true);
			A = StartCoroutine(Activate(Delay));
			break;
		case Mode.EMPTY:
		case Mode.EMPTY_SPIRIT:
			Spine.gameObject.SetActive(true);
			AE = StartCoroutine(ActivateEmpty((PrevMode == Mode.OFF) ? Delay : 0f));
			break;
		case Mode.OFF:
			D = StartCoroutine(Deactivate(Delay));
			break;
		}
		PrevMode = mode;
	}

	private IEnumerator Activate(float Delay)
	{
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
		switch (mode)
		{
		case Mode.ON:
			Spine.Skeleton.SetAttachment(Slot, null);
			if (Spine.AnimationState.GetCurrent(0).Animation.Name != "full")
			{
				Spine.AnimationState.SetAnimation(0, "fill", false);
				Spine.AnimationState.AddAnimation(0, "full", true, 0f);
			}
			break;
		case Mode.ON_SPIRIT:
			if (Spine.AnimationState.GetCurrent(0).Animation.Name != "full")
			{
				Spine.AnimationState.SetAnimation(0, "fill", false);
				Spine.AnimationState.AddAnimation(0, "full", true, 0f);
			}
			Spine.Skeleton.SetAttachment(Slot, Attachment);
			StartCoroutine(DoScale());
			break;
		}
	}

	private IEnumerator ActivateEmpty(float Delay)
	{
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
		if (Spine != null)
		{
			if (Spine.AnimationState.GetCurrent(0).Animation.Name != "empty")
			{
				Spine.AnimationState.SetAnimation(0, "shoot", false);
				Spine.AnimationState.AddAnimation(0, "empty", true, 0f);
			}
			switch (mode)
			{
			case Mode.EMPTY:
				Spine.Skeleton.SetAttachment(Slot, null);
				break;
			case Mode.EMPTY_SPIRIT:
				Spine.Skeleton.SetAttachment(Slot, Attachment);
				break;
			}
		}
	}

	private IEnumerator Deactivate(float Delay)
	{
		if (!Spine.gameObject.activeSelf)
		{
			yield break;
		}
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
		Spine.gameObject.SetActive(false);
	}

	private IEnumerator DoScale()
	{
		ScaleTimer = 0f;
		Scale = 1.3f;
		while ((ScaleTimer += Time.deltaTime) < 15f)
		{
			ScaleSpeed += (1f - Scale) * 0.2f;
			Scale += (ScaleSpeed *= 0.8f);
			yield return null;
		}
		Scale = 1f;
	}

	public void StartShake()
	{
		if (!Shaking)
		{
			StartCoroutine(DoShake());
		}
	}

	private IEnumerator DoShake()
	{
		Shaking = true;
		InitPos = Spine.rectTransform.localPosition;
		ShakeTimer = 0f;
		Shake = 0f;
		ShakeSpeed = 10f;
		while ((ShakeTimer += Time.deltaTime) < 1.2f)
		{
			ShakeSpeed += (0f - Shake) * 0.3f;
			Shake += (ShakeSpeed *= 0.9f);
			Spine.rectTransform.localPosition = InitPos + new Vector3(0f, Shake);
			yield return null;
		}
		Shake = 0f;
		Spine.rectTransform.localPosition = InitPos + new Vector3(0f, Shake);
		Shaking = false;
	}
}
