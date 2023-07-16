using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UIKeyScreenOverlayController : UIMenuBase
	{
		private const string kFlashMaterialID = "_FillColor";

		private const string kFillColorLerpFadeID = "_FillColorLerpFade";

		[Header("Key Screen")]
		[SerializeField]
		private KeyPiece[] _keyPieces;

		[SerializeField]
		private RectTransform _keyContainer;

		[SerializeField]
		private Material _flashMaterial;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Complete Key")]
		[SerializeField]
		private RectTransform _completeKeyContainer;

		[SerializeField]
		private Image _completeKey;

		[SerializeField]
		private GameObject _outline;

		[SerializeField]
		private RectTransform _glow;

		[Header("Total Keys")]
		[SerializeField]
		private RectTransform _totalKeysContainer;

		[SerializeField]
		private Image _totalKeysImage;

		[SerializeField]
		private TextMeshProUGUI _totalKeys;

		private KeyPiece _targetPiece;

		private Material _flashMaterialInstance;

		private Color _flashColor = new Color(1f, 1f, 1f, 0f);

		private static readonly int FillColorLerpFade = Shader.PropertyToID("_FillColorLerpFade");

		private static readonly int FillColor = Shader.PropertyToID("_FillColor");

		private Color _flashColorImpl
		{
			get
			{
				return _flashColor;
			}
			set
			{
				_flashColor = value;
				_flashMaterialInstance.SetFloat(FillColorLerpFade, _flashColor.a);
				_flashMaterialInstance.SetColor(FillColor, _flashColor);
			}
		}

		protected override void OnShowStarted()
		{
			_controlPrompts.HideAcceptButton();
			int currentKeyPieces = DataManager.Instance.CurrentKeyPieces;
			if (currentKeyPieces != 0)
			{
				for (int i = 0; i < _keyPieces.Length; i++)
				{
					if (currentKeyPieces > i)
					{
						if (currentKeyPieces - 1 == i)
						{
							_targetPiece = _keyPieces[i];
						}
					}
					else
					{
						_keyPieces[i].gameObject.SetActive(false);
					}
				}
				_totalKeysContainer.gameObject.SetActive(Inventory.TempleKeys > 0);
				_totalKeys.text = string.Format("x{0}", Inventory.TempleKeys);
			}
			else
			{
				_targetPiece = _keyPieces.LastElement();
				if (Inventory.TempleKeys == 1)
				{
					_totalKeysContainer.gameObject.SetActive(false);
				}
				else
				{
					_totalKeys.text = string.Format("x{0}", Inventory.TempleKeys - 1);
				}
			}
			_flashMaterialInstance = new Material(_flashMaterial);
			_flashMaterialInstance.SetFloat(FillColorLerpFade, 0f);
			KeyPiece[] keyPieces = _keyPieces;
			for (int j = 0; j < keyPieces.Length; j++)
			{
				keyPieces[j].SetMaterial(_flashMaterialInstance);
			}
			_completeKey.material = _flashMaterialInstance;
			_targetPiece.PrepareForAttach();
		}

		protected override IEnumerator DoShowAnimation()
		{
			yield return _003C_003En__0();
			StartCoroutine(RunKeyScreen());
		}

		private IEnumerator RunKeyScreen()
		{
			yield return new WaitForSecondsRealtime(0.25f);
			yield return _targetPiece.Attach();
			_keyContainer.DOShakePosition(0.6f, _targetPiece.MoveVector * 25f).SetEase(Ease.OutSine).SetUpdate(true);
			Flash(0.35f);
			if (Inventory.KeyPieces == 0)
			{
				yield return new WaitForSecondsRealtime(1.5f);
				if (!_totalKeysContainer.gameObject.activeSelf)
				{
					_totalKeysContainer.gameObject.SetActive(true);
				}
				_flashMaterialInstance.color = Color.white;
				_completeKeyContainer.gameObject.SetActive(true);
				_glow.DOScale(Vector3.one * 1.25f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)
					.SetUpdate(true);
				_keyContainer.DOShakePosition(0.5f, Vector3.right * 35f).SetEase(Ease.OutSine).SetUpdate(true);
				_totalKeysImage.material = _flashMaterialInstance;
				UIManager.PlayAudio("event:/temple_key/become_whole");
				yield return new WaitForSecondsRealtime(0.4f);
				Flash(1.75f);
				_totalKeys.text = string.Format("x{0}", Inventory.TempleKeys);
				yield return new WaitForSecondsRealtime(1f);
			}
			else
			{
				yield return new WaitForSecondsRealtime(1f);
			}
			_controlPrompts.ShowAcceptButton();
			while (!InputManager.UI.GetAcceptButtonDown())
			{
				yield return null;
			}
			Hide();
		}

		private void Flash(float time)
		{
			_flashColorImpl = Color.white;
			StartCoroutine(DoFlash(time));
		}

		private IEnumerator DoFlash(float time)
		{
			float t = 0f;
			while (t < time)
			{
				t += Time.unscaledDeltaTime;
				_flashColorImpl = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t / time);
				yield return null;
			}
		}

		protected override void OnHideCompleted()
		{
			Object.Destroy(base.gameObject);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Object.Destroy(_flashMaterialInstance);
			_flashMaterialInstance = null;
		}

		[CompilerGenerated]
		[DebuggerHidden]
		private IEnumerator _003C_003En__0()
		{
			return base.DoShowAnimation();
		}
	}
}
