using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class KeyPiece : BaseMonoBehaviour
	{
		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _startPosition;

		[SerializeField]
		private Image _image;

		private Vector2 _idlePosition;

		private Vector2 _attachFromPosition;

		private Vector2 _moveVector;

		public Vector2 MoveVector
		{
			get
			{
				return _moveVector;
			}
		}

		private void Start()
		{
			_idlePosition = _rectTransform.anchoredPosition;
			_attachFromPosition = _startPosition.anchoredPosition;
			_moveVector = (_attachFromPosition - _idlePosition).normalized;
		}

		public void SetMaterial(Material material)
		{
			_image.material = material;
		}

		public void PrepareForAttach()
		{
			_rectTransform.localScale = Vector3.zero;
			_rectTransform.anchoredPosition = _attachFromPosition;
		}

		public IEnumerator Attach()
		{
			UIManager.PlayAudio("event:/temple_key/fragment_move");
			_rectTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.15f);
			_rectTransform.DOAnchorPos(_idlePosition, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
			yield return new WaitForSecondsRealtime(0.45f);
			UIManager.PlayAudio("event:/temple_key/fragment_into_place");
		}
	}
}
