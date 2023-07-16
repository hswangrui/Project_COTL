using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIRadialMenuBase<T, U> : UIMenuBase where T : UIRadialWheelItem
	{
        private class SelectItem
	{
		public UIRadialMenuBase<T, U> item;

		public bool itemChosen;
    }

		private SelectItem selectItem=new SelectItem();

	 private const string kXPositionProperty = "_XPosition";

		private const string kYPositionProperty = "_YPosition";

		private const float kPointerSpeed = 10f;

		private const float kAbsTargetStickThreshold = 0.15f;

		public Action<U> OnItemChosen;

		public new Action OnCancel;

		[SerializeField]
		protected UIMenuControlPrompts _controlPrompts;

		[Header("Radial")]
		[SerializeField]
		protected MMUIRadialGraphic _radialGraphic;

		[Header("Eye")]
		[SerializeField]
		private RectTransform _eye;

		[SerializeField]
		private RectTransform _pupil;

		[Header("Selectables")]
		[SerializeField]
		protected List<T> _wheelItems;

		[Header("Text")]
		[SerializeField]
		private TextMeshProUGUI _itemName;

		[SerializeField]
		private CanvasGroup _itemNameCanvasGroup;

		[SerializeField]
		private TextMeshProUGUI _itemDescription;

		[SerializeField]
		private CanvasGroup _itemDescriptionCanvasGroup;

		protected bool _finalizedSelection;

		protected Material _radialMaterialInstance;

		private float _radiusThreshold;

		private Vector2 _targetVector;

		private T _pointerSelectedOption;

		private bool _didCancel;

		protected abstract bool SelectOnHighlight { get; }

		protected virtual void Start()
		{
			_radialMaterialInstance = new Material(_radialGraphic.material);
			_radialGraphic.material = _radialMaterialInstance;
			_itemNameCanvasGroup.alpha = 0f;
			_itemDescriptionCanvasGroup.alpha = 0f;
			_radiusThreshold = _radialGraphic.Radius * 0.5f;
		}

		protected override void OnShowCompleted()
		{
			StartCoroutine(DoWheelLoop());
		}

		protected virtual IEnumerator DoWheelLoop()
		{
			foreach (T wheelItem in _wheelItems)
			{
				MMButton button = wheelItem.Button;
				
				button.OnPointerEntered = (Action)Delegate.Combine(button.OnPointerEntered, (Action)delegate
				{
                    PointerSelect(wheelItem);
                });
				MMButton button2 = wheelItem.Button;
				button2.OnPointerExited = (Action)Delegate.Combine(button2.OnPointerExited, new Action(PointerDeselect));
				if (wheelItem.IsValidOption())
				{
					wheelItem.Button.onClick.AddListener(delegate
					{
                        ChooseItem(wheelItem);
					});
				}
			}
			bool itemChosen = false;
			T cachedSelection = null;
			while (!itemChosen)
			{
				T val = null;
				if (_targetVector.Abs().magnitude > 0.15f)
				{
					float num = float.MinValue;
					foreach (T wheelItem2 in _wheelItems)
					{
						if (wheelItem2.Button.interactable && wheelItem2.Visible())
						{
							float num2 = Vector2.Dot(wheelItem2.Vector, _targetVector);
							if (num2 > num && num2 > 0.75f)
							{
								num = num2;
								val = wheelItem2;
							}
						}
					}
				}
				if ((UnityEngine.Object)val != (UnityEngine.Object)cachedSelection)
				{
					foreach (T wheelItem3 in _wheelItems)
					{
						if ((UnityEngine.Object)wheelItem3 == (UnityEngine.Object)val)
						{
							wheelItem3.DoSelected();
							if (SelectOnHighlight)
							{
								Button.ButtonClickedEvent onClick = wheelItem3.Button.onClick;
								if (onClick != null)
								{
									onClick.Invoke();
								}
							}
						}
						else
						{
							wheelItem3.DoDeselected();
						}
					}
					cachedSelection = val;
				}
				if ((UnityEngine.Object)val != (UnityEngine.Object)null)
				{
					_controlPrompts.ShowAcceptButton();
					_itemName.text = val.GetTitle();
					if ((bool)_itemDescription)
					{
						_itemDescription.text = val.GetDescription();
					}
					if (_itemNameCanvasGroup.alpha < 1f)
					{
						_itemNameCanvasGroup.alpha += Time.unscaledDeltaTime * 3f;
					}
					if (_itemDescriptionCanvasGroup != null && _itemDescriptionCanvasGroup.alpha < 1f)
					{
						_itemDescriptionCanvasGroup.alpha += Time.unscaledDeltaTime * 3f;
					}
				}
				else
				{
					_controlPrompts.HideAcceptButton();
					if (_itemNameCanvasGroup.alpha > 0f)
					{
						_itemNameCanvasGroup.alpha -= Time.unscaledDeltaTime * 3f;
					}
					if (_itemDescriptionCanvasGroup != null && _itemDescriptionCanvasGroup.alpha > 0f)
					{
						_itemDescriptionCanvasGroup.alpha -= Time.unscaledDeltaTime * 3f;
					}
				}
				if (!SelectOnHighlight && (UnityEngine.Object)val != (UnityEngine.Object)null && InputManager.UI.GetAcceptButtonDown())
				{
					Button.ButtonClickedEvent onClick2 = val.Button.onClick;
					if (onClick2 != null)
					{
						onClick2.Invoke();
					}
				}
				yield return null;
			}
			yield return null;
			CleanupWheelLoop();
			OnChoiceFinalized();
		}

        void ChooseItem(T item)
        {
			Debug.Log("ChooseItem");
			selectItem.item.MakeChoice(item);
				if (!selectItem.item.SelectOnHighlight)
				{
					selectItem.itemChosen = true;
				}

			
			//	((SelectItem)(object)this).item.MakeChoice(item);
   //         if (!((SelectItem)(object)this).item.SelectOnHighlight)
			//{
   //             ((SelectItem)(object)this).itemChosen = true;
   //         }
        }
        void PointerDeselect()
        {
			Debug.Log("PointerDeselect");
			if (selectItem.item!= null)
			{
				selectItem.item._pointerSelectedOption = null;
			}


				//((SelectItem)(object)this).item._pointerSelectedOption = null;
        }
        void PointerSelect(T wheelItem)
        {
			Debug.Log("PointerSelect");
			selectItem.item = this;
			selectItem.item._pointerSelectedOption = wheelItem;


			//((SelectItem)(object)this).item._pointerSelectedOption = wheelItem;
		}






        private void Update()
		{
			if (_finalizedSelection)
			{
				return;
			}
			if (_canvasGroup.interactable)
			{
				if ((UnityEngine.Object)_pointerSelectedOption != (UnityEngine.Object)null)
				{
					_targetVector = _pointerSelectedOption.Vector;
				}
				else
				{
					_targetVector = new Vector2(InputManager.UI.GetHorizontalAxis(), InputManager.UI.GetVerticalAxis()).normalized;
				}
			}
			else
			{
				_targetVector.x = (_targetVector.y = 0f);
			}
			Vector2 b = _targetVector * _radiusThreshold;
			_eye.localPosition = Vector2.Lerp(_eye.localPosition, b, Time.unscaledDeltaTime * 10f);
			_pupil.transform.localPosition = _eye.localPosition / 4f;
			Vector2 vector = default(Vector2);
			vector.x = Mathf.Clamp(_eye.localPosition.x / _radiusThreshold, -1f, 1f);
			vector.y = Mathf.Clamp(_eye.localPosition.y / _radiusThreshold, -1f, 1f);
			Vector2 vector2 = vector;
			_radialMaterialInstance.SetFloat("_XPosition", vector2.x);
			_radialMaterialInstance.SetFloat("_YPosition", vector2.y);
		}

		protected void CleanupWheelLoop()
		{
			foreach (T wheelItem in _wheelItems)
			{
				wheelItem.Button.OnPointerEntered = null;
				wheelItem.Button.OnPointerExited = null;
				wheelItem.Button.onClick.RemoveAllListeners();
			}
		}

		protected abstract void OnChoiceFinalized();

		protected abstract void MakeChoice(T item);

		protected override IEnumerator DoHideAnimation()
		{
			if (!_finalizedSelection)
			{
				float time = 0f;
				while (time < 0.2f)
				{
					time += Time.unscaledDeltaTime;
					_eye.localPosition = Vector2.Lerp(_eye.localPosition, Vector3.zero, time / 0.2f);
					_pupil.transform.localPosition = _eye.localPosition / 4f;
					yield return null;
				}
			}
			yield return base.DoHideAnimation();
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_didCancel = true;
				StopAllCoroutines();
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (_radialMaterialInstance != null)
			{
				UnityEngine.Object.Destroy(_radialMaterialInstance);
				_radialMaterialInstance = null;
			}
		}
	}
}
