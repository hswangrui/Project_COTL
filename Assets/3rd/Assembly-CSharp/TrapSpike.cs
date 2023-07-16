using Spine.Unity;
using UnityEngine;

public class TrapSpike : BaseMonoBehaviour
{
	[SerializeField]
	private SkeletonAnimation skele;

	[SerializeField]
	private SpriteRenderer crossSprite;

	[SerializeField]
	private SpriteRenderer topSprite;

	[SerializeField]
	private SpriteRenderer bottomSprite;

	[SerializeField]
	private Sprite TopSpriteRed;

	[SerializeField]
	private Sprite TopSpriteWhite;

	[SerializeField]
	private Sprite TopSpriteDark;

	private bool disabled;

	public void AnimateSpike(string animationName, Color color)
	{
		skele.AnimationState.SetAnimation(0, animationName, false);
		skele.skeleton.SetColor(color);
	}

	public void SetRedSprite()
	{
		if (disabled)
		{
			DisableSpike();
		}
		topSprite.sprite = TopSpriteRed;
	}

	public void SetWarningSprite()
	{
		if (disabled)
		{
			DisableSpike();
		}
		topSprite.sprite = TopSpriteWhite;
	}

	public void DisableSpike()
	{
		disabled = true;
		crossSprite.color = new Color(0.33f, 0.33f, 0.33f, 0.51f);
		topSprite.sprite = TopSpriteDark;
	}
}
