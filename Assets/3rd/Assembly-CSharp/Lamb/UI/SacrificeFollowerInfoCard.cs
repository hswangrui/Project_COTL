using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class SacrificeFollowerInfoCard : UIInfoCardBase<FollowerInfo>
	{
		[SerializeField]
		private TextMeshProUGUI _headerText;

		[SerializeField]
		private TextMeshProUGUI SermonXPText;

		[SerializeField]
		private TextMeshProUGUI DeltaText;

		[SerializeField]
		private Image ProgressBar;

		[SerializeField]
		private Image InstantBar;

		[SerializeField]
		private GameObject GreenGlow;

		public override void Configure(FollowerInfo config)
		{
			_headerText.text = ScriptLocalization.Interactions.Sermon;
			float xPBySermon = DoctrineUpgradeSystem.GetXPBySermon(SermonCategory.PlayerUpgrade);
			float xPTargetBySermon = DoctrineUpgradeSystem.GetXPTargetBySermon(SermonCategory.PlayerUpgrade);
			float num = xPTargetBySermon * ((float)RitualSacrifice.GetDevotionGain(config.XPLevel) / 100f);
			SermonXPText.text = Mathf.RoundToInt(xPBySermon * 10f) + "/" + Mathf.RoundToInt(xPTargetBySermon * 10f);
			DeltaText.text = "(+" + Mathf.Ceil(num * 10f) + ")";
			ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(xPBySermon / xPTargetBySermon, 0f, 1f), 1f);
			InstantBar.transform.localScale = ProgressBar.transform.localScale;
			InstantBar.transform.DOKill();
			InstantBar.transform.DOScale(new Vector3(Mathf.Clamp((xPBySermon + num) / xPTargetBySermon, 0f, 1f), 1f), 0.5f).SetEase(Ease.OutSine);
			GreenGlow.SetActive(xPBySermon + num > xPTargetBySermon);
			if (GreenGlow.activeSelf)
			{
				GreenGlow.transform.DOKill();
				GreenGlow.transform.localScale = Vector3.one * 1.3f;
				GreenGlow.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutSine);
			}
		}
	}
}
