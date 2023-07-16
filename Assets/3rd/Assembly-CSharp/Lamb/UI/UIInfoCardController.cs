using System;
using src.UINavigator;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public abstract class UIInfoCardController<T, U> : MonoBehaviour where T : UIInfoCardBase<U>
	{
		public Action<T> OnInfoCardShown;

		public Action OnInfoCardsHidden;

		[SerializeField]
		private T _card1;

		[SerializeField]
		private T _card2;

		private T _currentCard;

		private U _currentShowParam;

		public T CurrentCard
		{
			get
			{
				return _currentCard;
			}
		}

		public T Card1
		{
			get
			{
				return _card1;
			}
		}

		public T Card2
		{
			get
			{
				return _card2;
			}
		}

		private void Start()
		{
			_currentShowParam = DefaultShowParam();
			_card1.Hide(true);
			_card2.Hide(true);
		}

		protected virtual void OnEnable()
		{
			UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
			instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Combine(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
			UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
			instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Combine(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
			UINavigatorNew instance3 = MonoSingleton<UINavigatorNew>.Instance;
			instance3.OnClear = (Action)Delegate.Combine(instance3.OnClear, new Action(OnClear));
		}

		protected virtual void OnDisable()
		{
			if (MonoSingleton<UINavigatorNew>.Instance != null)
			{
				UINavigatorNew instance = MonoSingleton<UINavigatorNew>.Instance;
				instance.OnSelectionChanged = (Action<Selectable, Selectable>)Delegate.Remove(instance.OnSelectionChanged, new Action<Selectable, Selectable>(OnSelectionChanged));
				UINavigatorNew instance2 = MonoSingleton<UINavigatorNew>.Instance;
				instance2.OnDefaultSetComplete = (Action<Selectable>)Delegate.Remove(instance2.OnDefaultSetComplete, new Action<Selectable>(OnSelection));
				UINavigatorNew instance3 = MonoSingleton<UINavigatorNew>.Instance;
				instance3.OnClear = (Action)Delegate.Remove(instance3.OnClear, new Action(OnClear));
			}
		}

		protected abstract bool IsSelectionValid(Selectable selectable, out U showParam);

		private void OnSelection(Selectable current)
		{
			U showParam;
			if (!IsSelectionValid(current, out showParam))
			{
				if ((UnityEngine.Object)_currentCard != (UnityEngine.Object)null)
				{
					_currentCard.Hide();
					_currentCard = null;
					Action onInfoCardsHidden = OnInfoCardsHidden;
					if (onInfoCardsHidden != null)
					{
						onInfoCardsHidden();
					}
				}
				_currentShowParam = DefaultShowParam();
			}
			else
			{
				ShowCardWithParam(showParam);
			}
		}

		public void ShowCardWithParam(U showParam)
		{
			if (_currentShowParam == null || !_currentShowParam.Equals(showParam))
			{
				_currentShowParam = showParam;
				if ((UnityEngine.Object)_currentCard == (UnityEngine.Object)null)
				{
					_currentCard = _card1;
					_card1.Show(showParam);
					_card2.Hide(true);
				}
				else if ((UnityEngine.Object)_currentCard == (UnityEngine.Object)_card2)
				{
					_card1.Show(showParam);
					_card2.Hide();
					_currentCard = _card1;
				}
				else
				{
					_card2.Show(showParam);
					_card1.Hide();
					_currentCard = _card2;
				}
				Action<T> onInfoCardShown = OnInfoCardShown;
				if (onInfoCardShown != null)
				{
					onInfoCardShown(_currentCard);
				}
			}
		}

		protected virtual U DefaultShowParam()
		{
			return default(U);
		}

		private void OnSelectionChanged(Selectable current, Selectable previous)
		{
			OnSelection(current);
		}

		private void OnClear()
		{
			if ((UnityEngine.Object)_currentCard != (UnityEngine.Object)null)
			{
				_currentCard.Hide();
			}
			_currentShowParam = DefaultShowParam();
		}

		public void ForceCurrentCard(T card, U param)
		{
			_currentCard = card;
			_currentShowParam = param;
		}
	}
}
