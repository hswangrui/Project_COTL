using UnityEngine;

public class TargetWarning : BaseMonoBehaviour
{
	private Vector3 TargetScale;

	public SpriteRenderer TargetSprite;

	public SpriteRenderer TargetWarningSprite;

	private bool SizeSet;

	private float Scale;

	private float Rotation;

	private float RotationSpeed = 50f;

	public Color Color1;

	public Color Color2;

	private Color CurrentColor;

	private float flashTickTimer;

	private void OnEnable()
	{
		if (!SizeSet)
		{
			TargetScale = TargetSprite.transform.localScale;
		}
		SizeSet = true;
		Scale = 0f;
		TargetSprite.transform.localScale = Vector3.one * Scale;
		TargetWarningSprite.transform.localScale = Vector3.one * Scale;
	}

	private void Update()
	{
		if ((Scale += Time.deltaTime * 20f) <= TargetScale.x)
		{
			TargetSprite.transform.localScale = Vector3.one * Scale;
			TargetWarningSprite.transform.localScale = Vector3.one * Scale;
		}
		TargetSprite.transform.eulerAngles = new Vector3(0f, 0f, Rotation += Time.deltaTime * RotationSpeed);
		if (flashTickTimer >= 0.12f && BiomeConstants.Instance.IsFlashLightsActive)
		{
			ToggleColor();
			flashTickTimer = 0f;
		}
		flashTickTimer += Time.deltaTime;
	}

	private void ToggleColor()
	{
		TargetSprite.material.SetColor("_Color", CurrentColor = ((CurrentColor == Color1) ? Color2 : Color1));
		TargetWarningSprite.material.SetColor("_Color", CurrentColor);
	}
}
