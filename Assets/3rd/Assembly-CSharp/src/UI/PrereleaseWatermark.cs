using UnityEngine;

namespace src.UI
{
	public class PrereleaseWatermark : MonoBehaviour
	{
		private enum Position
		{
			TopLeft,
			TopRight,
			BottomRight,
			BottomLeft
		}

		private GUIStyle _cachedGUIStyle;

		public static bool Hidden;

		private string _username;

		private Position _position;

		private float _timer;

		private string _message;

		private const float kMaxTime = 10f;

		private GUIStyle CachedGUIStyle
		{
			get
			{
				if (_cachedGUIStyle == null)
				{
					_cachedGUIStyle = new GUIStyle(GUI.skin.box)
					{
						fontSize = Screen.currentResolution.height / 75,
						alignment = TextAnchor.MiddleLeft,
						normal = 
						{
							textColor = new Color(1f, 1f, 1f, 0.5f)
						}
					};
				}
				return _cachedGUIStyle;
			}
		}
	}
}
