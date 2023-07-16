using System.Collections;
using UnityEngine;

namespace src.UI
{
	public class UIConfirmationCountdownWindow : UIMenuConfirmationWindow
	{
		private string _cachedBody;

		public void Configure(string header, string body, int seconds)
		{
			_cachedBody = body;
			_headerText.text = header;
			_bodyText.text = string.Format(_cachedBody, seconds);
			StartCoroutine(DoCountdown(seconds));
		}

		private IEnumerator DoCountdown(int seconds)
		{
			while (seconds > 0)
			{
				yield return new WaitForSecondsRealtime(1f);
				seconds--;
				_bodyText.text = string.Format(_cachedBody, seconds);
			}
			OnCancelClicked();
		}
	}
}
