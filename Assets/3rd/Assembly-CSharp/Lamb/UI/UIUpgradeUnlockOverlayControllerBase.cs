using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using FMOD.Studio;
using src.UI.InfoCards;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIUpgradeUnlockOverlayControllerBase : UIMenuBase
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass15_0
		{
			public UIUpgradeUnlockOverlayControllerBase _003C_003E4__this;

			public ParticleSystem.EmissionModule particleSystemEmission;

			public bool cancel;

			internal void _003CRunMenu_003Eg__UpdateProgress_007C0(float progress)
			{
				float num = 1f - 0.25f * progress;
				_003C_003E4__this.infoCardBase.RectTransform.localScale = new Vector3(num, num, num);
				_003C_003E4__this.infoCardBase.RectTransform.localPosition = UnityEngine.Random.insideUnitCircle * progress * _003C_003E4__this._holdInteraction.HoldTime * 2f;
				if (_003C_003E4__this._redOutline.gameObject.activeSelf != progress > 0f)
				{
					_003C_003E4__this._redOutline.gameObject.SetActive(progress > 0f);
				}
				_003C_003E4__this._redOutline.localScale = Vector3.Lerp(new Vector3(1f, 1f, 1f), new Vector3(1.2f, 1.2f, 1.2f), progress);
				if (InputManager.UI.GetAcceptButtonHeld())
				{
					particleSystemEmission.rateOverTime = 5f + progress * 9f;
					if (!_003C_003E4__this._particleSystem.isPlaying)
					{
						_003C_003E4__this._particleSystem.Play();
					}
					MMVibrate.RumbleContinuous(progress * 0.2f, progress * 0.2f);
					if (!_003C_003E4__this._loopingSound.HasValue)
					{
						_003C_003E4__this._loopingSound = AudioManager.Instance.CreateLoop("event:/hearts_of_the_faithful/draw_power_loop", true);
					}
				}
				else
				{
					MMVibrate.StopRumble();
					_003C_003E4__this._particleSystem.Stop();
				}
				ref EventInstance? loopingSound = ref _003C_003E4__this._loopingSound;
				if (loopingSound.HasValue)
				{
					loopingSound.GetValueOrDefault().setParameterByName("power", progress);
				}
			}

			internal void _003CRunMenu_003Eg__Cancel_007C1()
			{
				if (!_003C_003E4__this._tooLateToCancel)
				{
					cancel = true;
					MMVibrate.StopRumble();
				}
			}
		}

		public Action OnUnlocked;

		[SerializeField]
		private UpgradeTreeInfoCard infoCardBase;

		[SerializeField]
		private RectTransform _redOutline;

		[SerializeField]
		private UIHoldInteraction _holdInteraction;

		[SerializeField]
		private ParticleSystem _particleSystem;

		[SerializeField]
		private ParticleSystem _particleSystemExplode;

		[SerializeField]
		private Image _redFlash;

		protected UpgradeTreeNode _node;

		protected UpgradeSystem.Type _upgrade;

		private EventInstance? _loopingSound;

		private bool _tooLateToCancel;

		public ParticleSystem.EmitParams emitParams;

		public void Show(UpgradeTreeNode node, bool instant = false)
		{
			_node = node;
			MonoSingleton<UINavigatorNew>.Instance.Clear();
			infoCardBase.Show(node);
			_holdInteraction.gameObject.SetActive(NodeAvailable() && IsAvailable());
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			base.OnShowStarted();
			_particleSystem.Clear();
		}

		protected override void OnShowCompleted()
		{
			if (NodeAvailable() && IsAvailable())
			{
				StartCoroutine(RunMenu());
			}
		}

		private IEnumerator RunMenu()
		{
			_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
			_003C_003Ec__DisplayClass15_._003C_003E4__this = this;
			_003C_003Ec__DisplayClass15_.particleSystemEmission = _particleSystem.emission;
			_003C_003Ec__DisplayClass15_.cancel = false;
			_tooLateToCancel = false;
			yield return _holdInteraction.DoHoldInteraction(_003C_003Ec__DisplayClass15_._003CRunMenu_003Eg__UpdateProgress_007C0, _003C_003Ec__DisplayClass15_._003CRunMenu_003Eg__Cancel_007C1);
			if (_loopingSound.HasValue)
			{
				AudioManager.Instance.StopLoop(_loopingSound.Value);
			}
			MMVibrate.StopRumble();
			if (!_003C_003Ec__DisplayClass15_.cancel)
			{
				_tooLateToCancel = true;
				_holdInteraction.gameObject.SetActive(false);
				UIManager.PlayAudio("event:/hearts_of_the_faithful/draw_power_end");
				_redFlash.gameObject.SetActive(true);
				_redFlash.DOFade(0f, 0.5f).SetUpdate(true).SetDelay(0.3f);
				_redOutline.DOScale(Vector3.zero, 0.5f).SetUpdate(true).SetEase(Ease.OutSine);
				infoCardBase.RectTransform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBack);
				base.CanvasGroup.DOFade(0f, 0.5f).SetUpdate(true).SetDelay(0.8f);
				_particleSystem.Clear();
				_particleSystem.Stop();
				_particleSystemExplode.Play();
				yield return new WaitForSecondsRealtime(1.3f);
			}
			if (_003C_003Ec__DisplayClass15_.cancel)
			{
				yield return new WaitForSecondsRealtime(0.3f);
				infoCardBase.RectTransform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetUpdate(true).SetEase(Ease.OutSine);
			}
			if (_003C_003Ec__DisplayClass15_.cancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			else
			{
				Action onUnlocked = OnUnlocked;
				if (onUnlocked != null)
				{
					onUnlocked();
				}
			}
			Hide();
		}

		public override void OnCancelButtonInput()
		{
			if (!_tooLateToCancel)
			{
				if (_loopingSound.HasValue)
				{
					AudioManager.Instance.StopLoop(_loopingSound.Value);
				}
				if (_canvasGroup.interactable)
				{
					Hide();
				}
			}
		}

		protected override void OnHideStarted()
		{
			_redFlash.gameObject.SetActive(false);
			_particleSystem.Stop();
			_particleSystem.Clear();
			_redOutline.gameObject.SetActive(false);
			infoCardBase.Hide();
		}

		protected override void OnHideCompleted()
		{
			if (_loopingSound.HasValue)
			{
				AudioManager.Instance.StopLoop(_loopingSound.Value);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private bool NodeAvailable()
		{
			if (_node.State != UpgradeTreeNode.NodeState.Available || UpgradeSystem.GetUnlocked(_upgrade))
			{
				return false;
			}
			return true;
		}

		protected abstract bool IsAvailable();
	}
}
