using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CodeStage.AdvancedFPSCounter.Labels
{
	internal class DrawableLabel
	{
		// ----------------------------------------------------------------------------
		// internal fields
		// ----------------------------------------------------------------------------

		internal GameObject container;
		internal LabelAnchor anchor;
		internal Text uiText;
		internal RectTransform uiTextTransform;
		internal StringBuilder newText;
		internal bool dirty;

		// ----------------------------------------------------------------------------
		// private methods
		// ----------------------------------------------------------------------------

		private GameObject labelGameObject;
		private Vector2 pixelOffset;
		private Font font;
		private int fontSize;
		private float lineSpacing;

		// ----------------------------------------------------------------------------
		// constructor
		// ----------------------------------------------------------------------------

		internal DrawableLabel(GameObject container, LabelAnchor anchor, Vector2 pixelOffset, Font font, int fontSize, float lineSpacing)
		{
			this.container = container;
			this.anchor = anchor;
			this.pixelOffset = pixelOffset;
			this.font = font;
			this.fontSize = fontSize;
			this.lineSpacing = lineSpacing;

			NormalizeOffset();

			newText = new StringBuilder(1000);
		}

		// ----------------------------------------------------------------------------
		// internal methods
		// ----------------------------------------------------------------------------

		internal void CheckAndUpdate()
		{
			if (newText.Length > 0)
			{
				if (uiText == null)
				{
					/* create label game object and configure it */
					labelGameObject = new GameObject(anchor.ToString());

					labelGameObject.layer = container.layer;
					labelGameObject.tag = container.tag;
					labelGameObject.transform.SetParent(container.transform, false);

					/* create UI Text component and apply settings */
					uiText = labelGameObject.AddComponent<Text>();
					uiTextTransform = uiText.rectTransform;

					uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
					uiText.verticalOverflow = VerticalWrapMode.Overflow;

					ApplyFont();
                    uiText.fontSize = fontSize;
					uiText.lineSpacing = lineSpacing;

					UpdateTextPosition();
				}

				if (dirty)
				{
					uiText.text = newText.ToString();
					dirty = false;
				}
				newText.Length = 0;
			}
			else if (uiText != null)
			{
				Object.DestroyImmediate(uiText.gameObject);
			}
		}

		internal void Clear()
		{
			newText.Length = 0;
			if (uiText != null)
			{
				Object.Destroy(uiText.gameObject);
				uiTextTransform = null;
				uiText = null;
			}
		}

		internal void Dispose()
		{
			Clear();
			newText = null;
		}

		internal void ChangeFont(Font labelsFont)
		{
			font = labelsFont;
			ApplyFont();
		}

		internal void ChangeFontSize(int newSize)
		{
			fontSize = newSize;
			if (uiText != null) uiText.fontSize = fontSize;
		}

		internal void ChangeOffset(Vector2 newPixelOffset)
		{
			pixelOffset = newPixelOffset;
			NormalizeOffset();

			if (uiText != null)
			{
				uiTextTransform.anchoredPosition = pixelOffset;
			}
		}

		internal void ChangeLineSpacing(float newValueLineSpacing)
		{
			lineSpacing = newValueLineSpacing;

			if (uiText != null)
			{
				uiText.lineSpacing = newValueLineSpacing;
			}
		}

		// ----------------------------------------------------------------------------
		// private methods
		// ----------------------------------------------------------------------------

		private void UpdateTextPosition()
		{
            uiTextTransform.localRotation = Quaternion.identity;
            uiTextTransform.sizeDelta = Vector2.zero;
			uiTextTransform.anchoredPosition3D = pixelOffset;

			if (anchor == LabelAnchor.UpperLeft)
			{
				uiText.alignment = TextAnchor.UpperLeft;
				uiTextTransform.anchorMin = Vector2.up;
				uiTextTransform.anchorMax = Vector2.up;
			}
			else if (anchor == LabelAnchor.UpperRight)	
			{
				uiText.alignment = TextAnchor.UpperRight;
				uiTextTransform.anchorMin = Vector2.one;
				uiTextTransform.anchorMax = Vector2.one;
			}
			else if (anchor == LabelAnchor.LowerLeft)
			{
				uiText.alignment = TextAnchor.LowerLeft;
				uiTextTransform.anchorMin = Vector2.zero;
				uiTextTransform.anchorMax = Vector2.zero;
			}
			else if (anchor == LabelAnchor.LowerRight)
			{
				uiText.alignment = TextAnchor.LowerRight;
				uiTextTransform.anchorMin = Vector2.right;
				uiTextTransform.anchorMax = Vector2.right;
			}
			else if (anchor == LabelAnchor.UpperCenter)
			{
				uiText.alignment = TextAnchor.UpperCenter;
				uiTextTransform.anchorMin = new Vector2(0.5f, 1f);
				uiTextTransform.anchorMax = new Vector2(0.5f, 1f);
			}
			else if (anchor == LabelAnchor.LowerCenter)
			{
				uiText.alignment = TextAnchor.LowerCenter;
				uiTextTransform.anchorMin = new Vector2(0.5f, 0f);
				uiTextTransform.anchorMax = new Vector2(0.5f, 0f);
			}
			else
			{
				Debug.LogWarning("[AFPSCounter] Unknown label anchor!");
				uiText.alignment = TextAnchor.UpperLeft;
				uiTextTransform.anchorMin = Vector2.up;
				uiTextTransform.anchorMax = Vector2.up;
			}
		}

		private void NormalizeOffset()
		{
			if (anchor == LabelAnchor.UpperLeft)
			{
				pixelOffset.y = -pixelOffset.y;
			}
			else if (anchor == LabelAnchor.UpperRight)
			{
				pixelOffset.x = -pixelOffset.x;
				pixelOffset.y = -pixelOffset.y;
			}
			else if (anchor == LabelAnchor.LowerLeft)
			{
				// it's fine
			}
			else if (anchor == LabelAnchor.LowerRight)
			{
				pixelOffset.x = -pixelOffset.x;
			}
			else if (anchor == LabelAnchor.UpperCenter)
			{
				pixelOffset.y = -pixelOffset.y;
				pixelOffset.x = 0;
			}
			else if (anchor == LabelAnchor.LowerCenter)
			{
				pixelOffset.x = 0;
			}
			else
			{
				pixelOffset.y = -pixelOffset.y;
			}
		}

		private void ApplyFont()
		{
			if (uiText == null) return;

			if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			uiText.font = font;
		}
	}
}