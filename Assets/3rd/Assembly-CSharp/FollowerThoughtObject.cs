using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FollowerThoughtObject : BaseMonoBehaviour
{
	public TextMeshProUGUI Title;

	public TextMeshProUGUI Description;

	public Image FaithEffect;

	public Sprite FaithDoubleDown;

	public Sprite FaithDown;

	public Sprite FaithUp;

	public Sprite FaithDoubleUp;

	public Color FaithDoubleDownColor;

	public Color FaithDownColor;

	public Color FaithUpColor;

	public Color FaithDoubleUpColor;

	public Image Holder;

	public CanvasGroup _CanvasGroup;

	public CanvasGroup _CanvasGroupThought;

	private float ExpiryIndicator;

	public void Init(ThoughtData t)
	{
		if (t != null)
		{
			_CanvasGroupThought.alpha = 1f;
			_CanvasGroup.alpha = 1f;
			_CanvasGroup.interactable = true;
			Description.text = FollowerThoughts.GetLocalisedDescription(t.ThoughtType);
			Title.text = FollowerThoughts.GetLocalisedName(t.ThoughtType);
			if (t.Quantity > 1)
			{
				TextMeshProUGUI description = Description;
				description.text = description.text + " x" + t.Quantity;
			}
			if (t.Modifier <= -7f)
			{
				FaithEffect.sprite = FaithDoubleDown;
				Holder.color = FaithDoubleDownColor;
			}
			else if (t.Modifier < 0f)
			{
				FaithEffect.sprite = FaithDown;
				Holder.color = FaithDownColor;
			}
			else if (t.Modifier >= 7f)
			{
				FaithEffect.sprite = FaithDoubleUp;
				Holder.color = FaithDoubleUpColor;
			}
			else if (t.Modifier >= 0f)
			{
				FaithEffect.sprite = FaithUp;
				Holder.color = FaithUpColor;
			}
		}
		else
		{
			_CanvasGroupThought.alpha = 0f;
			_CanvasGroup.alpha = 1f;
			_CanvasGroup.interactable = false;
		}
	}
}
