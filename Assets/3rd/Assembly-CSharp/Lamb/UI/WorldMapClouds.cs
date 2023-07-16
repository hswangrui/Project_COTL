using System.Collections;
using UnityEngine;

namespace Lamb.UI
{
	public class WorldMapClouds : MonoBehaviour
	{
		[SerializeField]
		private FollowerLocation _location;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		public FollowerLocation Location
		{
			get
			{
				return _location;
			}
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
		}

		public IEnumerator DoHide()
		{
			while (_canvasGroup.alpha > 0f)
			{
				_canvasGroup.alpha -= Time.unscaledDeltaTime;
				yield return null;
			}
		}
	}
}
