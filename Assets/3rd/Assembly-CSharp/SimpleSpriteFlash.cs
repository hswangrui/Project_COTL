using UnityEngine;

public class SimpleSpriteFlash : BaseMonoBehaviour
{
	private MaterialPropertyBlock BlockFlash;

	private SpriteRenderer _spriteRenderer;

	private int fillAlpha;

	private int fillColor;

	private Color WarningColour = Color.white;

	[HideInInspector]
	public bool isFillWhite;

	private int NumColorToApply;

	private Color ColorToApply;

	private SpriteRenderer spriteRenderer
	{
		get
		{
			if (_spriteRenderer == null)
			{
				_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			return _spriteRenderer;
		}
	}

	private void Start()
	{
		fillColor = Shader.PropertyToID("_FillColor");
		fillAlpha = Shader.PropertyToID("_FillAlpha");
		BlockFlash = new MaterialPropertyBlock();
	}

	public void FlashMeWhite()
	{
		if (Time.frameCount % 5 == 0)
		{
			FlashWhite(!isFillWhite);
		}
	}

	public void FlashWhite(bool toggle)
	{
		Color warningColour = WarningColour;
		warningColour.a = (toggle ? 0.5f : 0f);
		SetColor(warningColour);
		isFillWhite = toggle;
	}

	public void FlashWhite(float amt)
	{
		isFillWhite = amt > 0f;
		Color warningColour = WarningColour;
		warningColour.a = Mathf.Lerp(0f, 1f, amt);
		SetColor(warningColour);
	}

	public void SetColor(Color color)
	{
		ColorToApply += color;
		NumColorToApply++;
	}

	private void LateUpdate()
	{
		if (NumColorToApply > 0)
		{
			if (BlockFlash == null)
			{
				BlockFlash = new MaterialPropertyBlock();
			}
			Texture2D texture = spriteRenderer.sprite.texture;
			BlockFlash.SetTexture("_MainTex", texture);
			BlockFlash.SetColor(fillColor, ColorToApply / NumColorToApply);
			BlockFlash.SetFloat(fillAlpha, ColorToApply.a / (float)NumColorToApply);
			spriteRenderer.SetPropertyBlock(BlockFlash);
			NumColorToApply = 0;
			ColorToApply = new Color(0f, 0f, 0f, 0f);
		}
	}
}
