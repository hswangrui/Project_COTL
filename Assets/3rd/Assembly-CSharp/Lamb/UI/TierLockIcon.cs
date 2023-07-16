using System;
using System.Collections;
using DG.Tweening;
using Lamb.UI.Assets;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class TierLockIcon : MonoBehaviour
	{
		private const float kLineBuffer = 125f;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private TextMeshProUGUI _tierCountText;

		[SerializeField]
		private UpgradeTreeNode.TreeTier _tier;

		[SerializeField]
		private UpgradeTreeConfiguration _config;

		[SerializeField]
		private RectTransform _lockContainer;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private MMUILineRenderer _leftLine;

		[SerializeField]
		private MMUILineRenderer _rightLine;

		private UpgradeTreeConfiguration.TreeTierConfig _tierConfig;

		public UpgradeTreeNode.TreeTier Tier
		{
			get
			{
				return _tier;
			}
		}

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public void Configure(UpgradeTreeNode.TreeTier tier)
		{
			if (_tierConfig == null)
			{
				_tierConfig = _config.GetConfigForTier(_tier);
			}
			if (tier >= _tier)
			{
				base.gameObject.SetActive(false);
				return;
			}
			int num = _config.NumRequiredNodesForTier(_tier);
			if (_config.NumUnlockedUpgrades() >= num)
			{
				_lockContainer.gameObject.SetActive(false);
				_leftLine.Fill = 1f - 125f / _leftLine.Root.TotalLength / 2f;
				_rightLine.Fill = _leftLine.Fill;
			}
			else
			{
				UpdateText();
				UpgradeSystem.OnAbilityUnlocked = (Action<UpgradeSystem.Type>)Delegate.Combine(UpgradeSystem.OnAbilityUnlocked, new Action<UpgradeSystem.Type>(OnAbilityUnlocked));
			}
		}

		private void OnDisable()
		{
			UpgradeSystem.OnAbilityUnlocked = (Action<UpgradeSystem.Type>)Delegate.Remove(UpgradeSystem.OnAbilityUnlocked, new Action<UpgradeSystem.Type>(OnAbilityUnlocked));
		}

		private void OnAbilityUnlocked(UpgradeSystem.Type type)
		{
			UpdateText();
		}

		private void UpdateText()
		{
			_tierCountText.text = string.Format("{0} / {1}", _config.NumUnlockedUpgrades(), _config.NumRequiredNodesForTier(_tier));
		}

		public IEnumerator DestroyTierLock()
		{
			yield return ShrinkLock();
			UIManager.PlayAudio("event:/upgrade_statue/upgrade_unlock");
			float size = 125f / _leftLine.Root.TotalLength / 2f;
			float t = 0f;
			float tt = 0.5f;
			while (t < tt)
			{
				t += Time.unscaledDeltaTime;
				_leftLine.Fill = 1f - size * (t / tt);
				_rightLine.Fill = _leftLine.Fill;
				yield return null;
			}
		}

		private IEnumerator ShrinkLock()
		{
			UIManager.PlayAudio("event:/door/door_unlock");
			MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
			_lockContainer.DOScale(Vector3.one * 1.25f, 0.25f).SetEase(Ease.OutSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			_lockContainer.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InSine).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.25f);
			_lockContainer.gameObject.SetActive(false);
		}

		public IEnumerator RevealTier()
		{
			if (_lockContainer.gameObject.activeSelf)
			{
				yield return ShrinkLock();
			}
			while (_leftLine.Fill > 0f)
			{
				_leftLine.Fill -= Time.unscaledDeltaTime * 1.5f;
				_rightLine.Fill = _leftLine.Fill;
				yield return null;
			}
		}
	}
}
