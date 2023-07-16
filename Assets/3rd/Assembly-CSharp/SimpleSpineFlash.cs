using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

public class SimpleSpineFlash : BaseMonoBehaviour
{
	public enum FollowAngleMode
	{
		LookingAngle,
		FacingAngle
	}

	public enum SetFacingMode
	{
		None,
		Normal,
		Reversed,
		Ignore
	}

	public FollowAngleMode FollowAngle;

	public SetFacingMode SetFacing;

	public float FacingAngle;

	private StateMachine state;

	[HideInInspector]
	public bool isFillWhite;

	private MaterialPropertyBlock BlockFlash;

	private MeshRenderer _meshRenderer;

	private int fillAlpha;

	private int fillColor;

	private int tintColor;

	private Color WarningColour = Color.white;

	private Color baseColor = new Color(0f, 0f, 0f, 0f);

	private bool FlashingRed;

	private Coroutine cFlashFillRoutine;

	private float FlashRedMultiplier = 0.01f;

	private int NumColorToApply;

	private Color ColorToApply;

	private int _Dir;

	private Coroutine fadeRoutine;

	public SkeletonAnimation Spine { get; private set; }

	private MeshRenderer meshRenderer
	{
		get
		{
			if (_meshRenderer == null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}
			return _meshRenderer;
		}
	}

	private int Dir
	{
		set
		{
			if (_Dir != value)
			{
				Spine.skeleton.ScaleX = (_Dir = value);
			}
		}
	}

	private void OnEnable()
	{
		Spine = GetComponent<SkeletonAnimation>();
		if (state == null)
		{
			state = GetComponentInParent<StateMachine>();
		}
	}

	private void Start()
	{
		fillColor = Shader.PropertyToID("_FillColor");
		fillAlpha = Shader.PropertyToID("_FillAlpha");
		tintColor = Shader.PropertyToID("_Color");
		BlockFlash = new MaterialPropertyBlock();
		meshRenderer.GetPropertyBlock(BlockFlash);
		if (baseColor == new Color(0f, 0f, 0f, 0f))
		{
			baseColor = BlockFlash.GetColor(fillColor);
		}
		baseColor.a = BlockFlash.GetFloat(fillAlpha);
	}

	public void FlashMeWhite(float alpha = 0.5f, int frameCount = 5)
	{
		if (Time.frameCount % frameCount == 0)
		{
			FlashWhite(!isFillWhite, alpha);
		}
	}

	public void FlashMeWhite()
	{
		if (Time.frameCount % 5 == 0)
		{
			FlashWhite(!isFillWhite);
		}
	}

	public void FlashWhite(bool toggle, float alpha = 0.5f)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights && !FlashingRed)
		{
			Color warningColour = WarningColour;
			warningColour.a = (toggle ? alpha : 0f);
			SetColor(warningColour);
			isFillWhite = toggle;
		}
	}

	public void FlashWhite(float amt)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights && !FlashingRed)
		{
			isFillWhite = amt > 0f;
			amt *= 0.44f;
			Color warningColour = WarningColour;
			warningColour.a = Mathf.Lerp(0f, 1f, amt);
			SetColor(warningColour);
		}
	}

	public void FlashRed(float amt)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			Color red = Color.red;
			red.a = Mathf.Lerp(0f, 1f, amt);
			SetColor(red);
		}
	}

	public void FlashFillRed(float opacity = 0.5f)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			FlashingRed = true;
			if (cFlashFillRoutine != null)
			{
				StopCoroutine(cFlashFillRoutine);
			}
			if (base.gameObject.activeSelf)
			{
				cFlashFillRoutine = StartCoroutine(FlashOnHitRoutine(opacity));
			}
		}
	}

	private IEnumerator FlashOnHitRoutine(float opacity)
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.SetPropertyBlock(propertyBlock);
		SetColor(new Color(1f, 1f, 1f, opacity));
		yield return new WaitForSeconds(6f * FlashRedMultiplier);
		SetColor(new Color(0f, 0f, 0f, opacity));
		yield return new WaitForSeconds(3f * FlashRedMultiplier);
		SetColor(new Color(1f, 0f, 0f, opacity));
		yield return new WaitForSeconds(2f * FlashRedMultiplier);
		SetColor(new Color(0f, 0f, 0f, opacity));
		yield return new WaitForSeconds(2f * FlashRedMultiplier);
		SetColor(new Color(1f, 0f, 0f, opacity));
		yield return new WaitForSeconds(2f * FlashRedMultiplier);
		SetColor(new Color(1f, 0f, 0f, 0f));
		FlashingRed = false;
		meshRenderer.receiveShadows = true;
		meshRenderer.shadowCastingMode = ShadowCastingMode.On;
	}

	private IEnumerator DoFlashFillRed()
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(propertyBlock);
		SetColor(Color.red);
		yield return new WaitForSeconds(0.1f);
		float Progress = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress - 0.05f * Spine.timeScale);
			if (num >= 0f)
			{
				if (Progress <= 0f)
				{
					Progress = 0f;
				}
				SetColor(new Color(1f, 0f, 0f, Progress));
				yield return null;
				continue;
			}
			break;
		}
	}

	public void FlashFillGreen()
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			StopCoroutine(DoFlashFillGreen());
			StartCoroutine(DoFlashFillGreen());
		}
	}

	private IEnumerator DoFlashFillGreen()
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(propertyBlock);
		SetColor(Color.green);
		yield return new WaitForSeconds(0.1f);
		float Progress = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress - 0.05f * Spine.timeScale);
			if (num >= 0f)
			{
				if (Progress <= 0f)
				{
					Progress = 0f;
				}
				SetColor(new Color(0f, 0.5f, 0f, Progress));
				yield return null;
				continue;
			}
			break;
		}
	}

	public void FlashFillBlack(bool ignoreSpineTimescale = false)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			StopCoroutine(DoFlashFillBlack(ignoreSpineTimescale));
			StartCoroutine(DoFlashFillBlack(ignoreSpineTimescale));
		}
	}

	private IEnumerator DoFlashFillBlack(bool ignoreSpineTimescale)
	{
		MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
		meshRenderer.SetPropertyBlock(propertyBlock);
		SetColor(Color.black);
		yield return new WaitForSeconds(0.1f);
		float Progress = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress - 0.05f * (ignoreSpineTimescale ? 1f : Spine.timeScale));
			if (num >= 0f)
			{
				if (Progress <= 0f)
				{
					Progress = 0f;
				}
				SetColor(new Color(0f, 0f, 0f, Progress));
				yield return null;
				continue;
			}
			break;
		}
	}

	private void FlashRedTint()
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			StopCoroutine(DoFlashTintRed());
			StartCoroutine(DoFlashTintRed());
		}
	}

	private IEnumerator DoFlashTintRed()
	{
		float Progress = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * Spine.timeScale);
			if (!(num <= Duration))
			{
				break;
			}
			SetColor(new Color(1f, 0f, 0f, 1f - Progress / Duration));
			yield return null;
		}
		SetColor(new Color(0f, 0f, 0f, 0f));
	}

	public void SetColor(Color color)
	{
		ColorToApply += color;
		NumColorToApply++;
	}

	public void Tint(Color color)
	{
		if (SettingsManager.Settings.Accessibility.FlashingLights)
		{
			Color color2 = ((!(color == Color.white) || !(baseColor != new Color(0f, 0f, 0f, 0f))) ? color : new Color(baseColor.r, baseColor.g, baseColor.b));
			if (fadeRoutine != null)
			{
				StopCoroutine(fadeRoutine);
			}
			fadeRoutine = StartCoroutine(FadeTintAway(color2));
		}
	}

	private IEnumerator FadeTintAway(Color color)
	{
		if (BlockFlash == null)
		{
			BlockFlash = new MaterialPropertyBlock();
		}
		Color currentColor = BlockFlash.GetColor(tintColor);
		float duration = 0.1f;
		float t = 0f;
		while (true)
		{
			float num;
			t = (num = t + Time.deltaTime * Spine.timeScale);
			if (!(num < duration))
			{
				break;
			}
			float t2 = t / duration;
			BlockFlash.SetColor(tintColor, Color.Lerp(currentColor, color, t2));
			meshRenderer.SetPropertyBlock(BlockFlash);
			yield return null;
		}
		BlockFlash.SetColor(tintColor, color);
		meshRenderer.SetPropertyBlock(BlockFlash);
		fadeRoutine = null;
	}

	public void OverrideBaseColor(Color color)
	{
		baseColor = color;
	}

	private void LateUpdate()
	{
		if (state == null)
		{
			return;
		}
		if (Spine.timeScale == 0.0001f)
		{
			NumColorToApply = 0;
			ColorToApply = baseColor;
			return;
		}
		if (NumColorToApply > 0)
		{
			if (BlockFlash == null)
			{
				BlockFlash = new MaterialPropertyBlock();
			}
			BlockFlash.SetColor(fillColor, ColorToApply / NumColorToApply);
			BlockFlash.SetFloat(fillAlpha, ColorToApply.a / (float)NumColorToApply);
			meshRenderer.SetPropertyBlock(BlockFlash);
			NumColorToApply = 0;
			ColorToApply = baseColor;
		}
		if (SetFacing != SetFacingMode.Ignore)
		{
			FacingAngle = ((FollowAngle == FollowAngleMode.LookingAngle) ? state.LookAngle : state.facingAngle);
			Dir = ((FacingAngle > 90f && FacingAngle < 270f) ? 1 : (-1)) * ((SetFacing != SetFacingMode.Reversed) ? 1 : (-1));
		}
	}

	public void SetFacingType(int mode)
	{
		SetFacing = (SetFacingMode)mode;
	}
}
