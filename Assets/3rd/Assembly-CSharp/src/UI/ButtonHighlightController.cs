using UnityEngine;
using UnityEngine.UI;

namespace src.UI
{
	public class ButtonHighlightController : MonoBehaviour
	{
		[Header("Components")]
		[SerializeField]
		private Image _image;

		[Header("Textures")]
		[SerializeField]
		private Sprite _blackSprite;

		[SerializeField]
		private Sprite _redSprite;

		[SerializeField]
		private Sprite _twitchSprite;

		public Image Image
		{
			get
			{
				return _image;
			}
		}

		public void SetAsBlack()
		{
			_image.sprite = _blackSprite;
		}

		public void SetAsRed()
		{
			_image.sprite = _redSprite;
		}

		public void SetAsTwitch()
		{
			_image.sprite = _twitchSprite;
		}
	}
}
