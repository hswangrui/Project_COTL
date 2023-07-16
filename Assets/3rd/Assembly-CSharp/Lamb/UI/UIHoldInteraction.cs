using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

namespace Lamb.UI
{
	public class UIHoldInteraction : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _controlPrompt;

		[SerializeField]
		private RadialProgress _radialProgress;

		[SerializeField]
		private float _holdTime = 3f;

		public string LoopSound = "event:/unlock_building/unlock_hold";

		public float HoldTime
		{
			get
			{
				return _holdTime;
			}
		}

		private void Start()
		{
			Reset();
		}

		public void Reset()
		{
			_radialProgress.Progress = 0f;
			_radialProgress.gameObject.SetActive(SettingsManager.Settings.Accessibility.HoldActions);
		}

		public IEnumerator DoHoldInteraction(Action<float> onUpdate, Action onCancel)
		{
			EventInstance? loopingSoundInstance = null;
			bool cancelled = false;
			float progress2 = 0f;
			while (SettingsManager.Settings.Accessibility.HoldActions)
			{
				if (InputManager.UI.GetAcceptButtonDown() || InputManager.Gameplay.GetInteractButtonDown())
				{
					_controlPrompt.localScale = Vector3.one;
					_controlPrompt.DOKill();
					_controlPrompt.DOPunchScale(new Vector3(0.2f, 0.2f), 0.2f).SetUpdate(true);
				}
				else if (InputManager.UI.GetAcceptButtonHeld() || InputManager.Gameplay.GetInteractButtonHeld())
				{
					if (!loopingSoundInstance.HasValue)
					{
						loopingSoundInstance = AudioManager.Instance.CreateLoop(LoopSound, true);
					}
					progress2 += Time.unscaledDeltaTime;
					if (progress2 >= _holdTime)
					{
						break;
					}
				}
				else
				{
					if (loopingSoundInstance.HasValue)
					{
						AudioManager.Instance.StopLoop(loopingSoundInstance.Value);
						loopingSoundInstance = null;
					}
					if (progress2 > 0f)
					{
						progress2 -= Time.unscaledDeltaTime * 5f;
						progress2 = Mathf.Max(progress2, 0f);
					}
				}
				if (InputManager.UI.GetCancelButtonDown())
				{
					cancelled = true;
					break;
				}
				if (onUpdate != null)
				{
					onUpdate(progress2 / _holdTime);
				}
				_radialProgress.Progress = progress2 / _holdTime;
				yield return null;
			}
			while (!SettingsManager.Settings.Accessibility.HoldActions && !InputManager.UI.GetAcceptButtonDown() && !InputManager.Gameplay.GetInteractButtonDown())
			{
				if (InputManager.UI.GetCancelButtonDown())
				{
					cancelled = true;
					break;
				}
				yield return null;
			}
			if (loopingSoundInstance.HasValue)
			{
				AudioManager.Instance.StopLoop(loopingSoundInstance.Value);
			}
			if (cancelled)
			{
				if (onCancel != null)
				{
					onCancel();
				}
			}
		}
	}
}
