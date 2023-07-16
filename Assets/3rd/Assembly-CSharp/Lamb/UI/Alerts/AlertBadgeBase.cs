using System.Collections;
using UnityEngine;

namespace Lamb.UI.Alerts
{
	public class AlertBadgeBase : BaseMonoBehaviour
	{
		private Vector3 _wobble;

		private bool _performedCTA;

		private void OnEnable()
		{
			StopAllCoroutines();
			StartCoroutine(WobbleAlert());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private IEnumerator CTA()
		{
			_performedCTA = true;
			float progress = 0f;
			float duration = 0.3f;
			float targetScale = base.transform.localScale.x;
			while (true)
			{
				float num;
				progress = (num = progress + Time.unscaledDeltaTime);
				if (!(num < duration))
				{
					break;
				}
				base.transform.localScale = Vector3.one * Mathf.Lerp(1f, targetScale, progress / duration);
				yield return null;
			}
			base.transform.localScale = Vector3.one * targetScale;
		}

		private IEnumerator WobbleAlert()
		{
			if (!_performedCTA)
			{
				yield return CTA();
			}
			float wobble = Random.Range(0, 360);
			float wobbleSpeed = 4f;
			while (true)
			{
				wobble += wobbleSpeed * Time.unscaledDeltaTime;
				_wobble.z = 15f * Mathf.Cos(wobble);
				base.transform.eulerAngles = _wobble;
				yield return null;
			}
		}
	}
}
