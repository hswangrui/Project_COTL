using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UpgradeTreeNodeInfoCard : UIInfoCardBase<UpgradeTreeNode>
	{
		[SerializeField]
		private TextMeshProUGUI _nodeNameText;

		public override void Configure(UpgradeTreeNode node)
		{
			_nodeNameText.text = UpgradeSystem.GetLocalizedName(node.Upgrade);
			Vector2 anchoredPosition = node.RectTransform.anchoredPosition;
			anchoredPosition.y -= node.RectTransform.rect.height * node.RectTransform.localScale.y * 0.5f;
			anchoredPosition.y -= 20f;
			base.RectTransform.anchoredPosition = anchoredPosition;
		}

		protected override void DoShow(bool instant)
		{
			base.CanvasGroup.DOKill();
			if (instant)
			{
				base.CanvasGroup.alpha = 1f;
			}
			else
			{
				base.CanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
			}
		}

		protected override void DoHide(bool instant)
		{
			base.CanvasGroup.DOKill();
			if (instant)
			{
				base.CanvasGroup.alpha = 0f;
			}
			else
			{
				base.CanvasGroup.DOFade(0f, 0.25f).SetUpdate(true);
			}
		}
	}
}
