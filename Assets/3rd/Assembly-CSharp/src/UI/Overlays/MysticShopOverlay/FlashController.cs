using UnityEngine;
using UnityEngine.UI;

namespace src.UI.Overlays.MysticShopOverlay
{
	[ExecuteInEditMode]
	public class FlashController : MonoBehaviour
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _flash;

		[SerializeField]
		private MaskableGraphic[] _graphics;

		public float Flash
		{
			get
			{
				return _flash;
			}
			set
			{
				_flash = value;
				Update();
			}
		}

		private void Update()
		{
			Color white = Color.white;
			white.a = _flash;
			MaskableGraphic[] graphics = _graphics;
			for (int i = 0; i < graphics.Length; i++)
			{
				graphics[i].color = white;
			}
		}
	}
}
