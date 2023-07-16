using DG.Tweening;
using UnityEngine;

namespace Map
{
	public class ScrollNonUI : MonoBehaviour
	{
		public float tweenBackDuration = 0.3f;

		public Ease tweenBackEase;

		public bool freezeX;

		public FloatMinMax xConstraints = new FloatMinMax();

		public bool freezeY;

		public FloatMinMax yConstraints = new FloatMinMax();

		private Vector2 offset;

		private Vector3 pointerDisplacement;

		private float zDisplacement;

		private bool dragging;

		private Camera mainCamera;

		private void Awake()
		{
			mainCamera = Camera.main;
			zDisplacement = 0f - mainCamera.transform.position.z + base.transform.position.z;
		}

		public void OnMouseDown()
		{
			pointerDisplacement = -base.transform.position + MouseInWorldCoords();
			base.transform.DOKill();
			dragging = true;
		}

		public void OnMouseUp()
		{
			dragging = false;
			TweenBack();
		}

		private void Update()
		{
			if (dragging)
			{
				Vector3 vector = MouseInWorldCoords();
				base.transform.position = new Vector3(freezeX ? base.transform.position.x : (vector.x - pointerDisplacement.x), freezeY ? base.transform.position.y : (vector.y - pointerDisplacement.y), base.transform.position.z);
			}
		}

		private Vector3 MouseInWorldCoords()
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = zDisplacement;
			return mainCamera.ScreenToWorldPoint(mousePosition);
		}

		private void TweenBack()
		{
			if (freezeY)
			{
				if (!(base.transform.localPosition.x >= xConstraints.min) || !(base.transform.localPosition.x <= xConstraints.max))
				{
					float endValue = ((base.transform.localPosition.x < xConstraints.min) ? xConstraints.min : xConstraints.max);
					base.transform.DOLocalMoveX(endValue, tweenBackDuration).SetEase(tweenBackEase);
				}
			}
			else if (freezeX && (!(base.transform.localPosition.y >= yConstraints.min) || !(base.transform.localPosition.y <= yConstraints.max)))
			{
				float endValue2 = ((base.transform.localPosition.y < yConstraints.min) ? yConstraints.min : yConstraints.max);
				base.transform.DOLocalMoveY(endValue2, tweenBackDuration).SetEase(tweenBackEase);
			}
		}
	}
}
