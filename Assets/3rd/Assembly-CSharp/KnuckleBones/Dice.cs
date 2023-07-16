using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace KnuckleBones
{
	public class Dice : MonoBehaviour
	{
		private RectTransform rectTransform;

		public List<Sprite> DiceFaces = new List<Sprite>();

		public Image image;

		public int Num;

		private Canvas canvas;

		public bool matched;

		private Vector2 ShakeMultiplier = new Vector2(20f, 5f);

		private void OnEnable()
		{
			rectTransform = GetComponent<RectTransform>();
			canvas = GetComponentInParent<Canvas>();
		}

		public void Roll(float Duration)
		{
			StartCoroutine(RollRoutine(Duration));
		}

		public IEnumerator RollRoutine(float Duration)
		{
			float x = Random.Range(-115, 115);
			float y = Random.Range(-15, 15);
			Vector3 vector = new Vector3(x, y, 0f);
			rectTransform.DOMove(rectTransform.transform.position + vector * 1f, 1f).SetEase(Ease.OutBounce).SetUpdate(true);
			float Progress = 0f;
			while (Progress < Duration)
			{
				Num = Random.Range(1, 7);
				image.sprite = DiceFaces[Num - 1];
				Progress += 0.1f;
				yield return new WaitForSecondsRealtime(0.01f + 0.04f * (Progress / Duration));
			}
			Num = Random.Range(1, 7);
			image.sprite = DiceFaces[Num - 1];
		}

		public IEnumerator GoToLocationRoutine(Vector3 Position)
		{
			float Progress = 0f;
			float Duration = 0.3f;
			Vector3 StartPosition = rectTransform.position;
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.unscaledDeltaTime);
				if (!(num < Duration))
				{
					break;
				}
				rectTransform.position = Vector3.Lerp(StartPosition, Position, Mathf.SmoothStep(0f, 1f, Progress / Duration));
				yield return null;
			}
			rectTransform.position = Position;
			AudioManager.Instance.PlayOneShot("event:/knuckle_bones/die_place");
		}

		public void Shake()
		{
			StartCoroutine(ShakeRoutine());
		}

		public IEnumerator ShakeRoutine()
		{
			float Shake = 0.5f;
			Vector3 Position = image.rectTransform.position;
			while (true)
			{
				float num;
				Shake = (num = Shake - Time.unscaledDeltaTime);
				if (!(num > 0f))
				{
					break;
				}
				image.rectTransform.position = Position + new Vector3(ShakeMultiplier.x * Random.Range(0f - Shake, Shake), ShakeMultiplier.y * Random.Range(0f - Shake, Shake));
				image.color = Color.Lerp(Color.white, Color.red, Shake / 0.5f);
				image.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Shake / 0.5f);
				yield return null;
			}
			image.rectTransform.position = Position;
		}

		public void Scale()
		{
			StartCoroutine(ScaleRoutine());
		}

		public IEnumerator ScaleRoutine()
		{
			float Progress = 0f;
			float Duration = 1f;
			Vector3 Scale = Vector3.one;
			Vector3 ScaleSpeed = new Vector3(0.5f, 0.5f);
			while (true)
			{
				float num;
				Progress = (num = Progress + Time.unscaledDeltaTime);
				if (!(num < Duration))
				{
					break;
				}
				ScaleSpeed.x += (1f - Scale.x) * 0.2f;
				Scale.x += (ScaleSpeed.x *= 0.6f);
				ScaleSpeed.y += (1f - Scale.y) * 0.2f;
				Scale.y += (ScaleSpeed.y *= 0.6f);
				rectTransform.localScale = Scale;
				yield return null;
			}
			rectTransform.localScale = Vector3.one;
		}
	}
}
