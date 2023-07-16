using System;
using System.Collections.Generic;
using DG.Tweening;
using src.UI.InfoCards;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI.KitchenMenu
{
	public class UIFollowerKitchenMenuController : UIMenuBase
	{
		public Action<InventoryItem.ITEM_TYPE> OnItemQueued;

		public Action<InventoryItem.ITEM_TYPE, int> OnItemRemovedFromQueue;

		[SerializeField]
		private RecipeInfoCardController _infoCardController;

		public RecipeItem recipeIconPrefab;

		public Transform Container;

		[SerializeField]
		private Transform queuedContainer;

		[SerializeField]
		private TMP_Text amountQueuedText;

		[SerializeField]
		private GameObject[] _vacantSlots;

		[SerializeField]
		private GameObject _addToQueuePrompt;

		[SerializeField]
		private GameObject _removeFromQueuePrompt;

		[SerializeField]
		private GameObject _queueFullPrompt;

		private List<RecipeItem> _recipeItems = new List<RecipeItem>();

		private List<RecipeItem> _queuedItems = new List<RecipeItem>();

		private Tween _queuedTextTween;

		private StructuresData _structureInfo;

		private Interaction_FollowerKitchen _interactionKitchen;

		private int kMaxItems
		{
			get
			{
				return 30;
			}
		}

		public void Show(StructuresData kitchenData, Interaction_FollowerKitchen interactionKitchen, bool instant = false)
		{
			_structureInfo = kitchenData;
			_interactionKitchen = interactionKitchen;
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			for (int i = 0; i < _vacantSlots.Length; i++)
			{
				_vacantSlots[i].SetActive(i < kMaxItems);
			}
			List<InventoryItem.ITEM_TYPE> list = new List<InventoryItem.ITEM_TYPE>();
			InventoryItem.ITEM_TYPE[] allMeals = CookingData.GetAllMeals();
			foreach (InventoryItem.ITEM_TYPE iTEM_TYPE in allMeals)
			{
				if ((iTEM_TYPE != InventoryItem.ITEM_TYPE.MEAL_POOP || DataManager.Instance.RecipesDiscovered.Contains(iTEM_TYPE)) && (iTEM_TYPE != InventoryItem.ITEM_TYPE.MEAL_GRASS || DataManager.Instance.RecipesDiscovered.Contains(iTEM_TYPE)))
				{
					if (CookingData.CanMakeMeal(iTEM_TYPE))
					{
						CookingData.TryDiscoverRecipe(iTEM_TYPE);
					}
					list.Add(iTEM_TYPE);
				}
			}
			list.Sort((InventoryItem.ITEM_TYPE a, InventoryItem.ITEM_TYPE b) => CookingData.HasRecipeDiscovered(b).CompareTo(CookingData.HasRecipeDiscovered(a)));
			foreach (InventoryItem.ITEM_TYPE item in list)
			{
				RecipeItem recipeItem = UnityEngine.Object.Instantiate(recipeIconPrefab, Container);
				recipeItem.OnRecipeChosen = (Action<RecipeItem>)Delegate.Combine(recipeItem.OnRecipeChosen, new Action<RecipeItem>(OnRecipeChosen));
				MMButton button = recipeItem.Button;
				button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnQueueableItemSelected));
				recipeItem.Configure(item);
				recipeItem.FadeIn((float)_recipeItems.Count * 0.03f);
				_recipeItems.Add(recipeItem);
			}
			OverrideDefault(_recipeItems[0].Button);
			ActivateNavigation();
			for (int k = 0; k < _structureInfo.QueuedMeals.Count; k++)
			{
				RecipeItem recipeItem2 = MakeQueuedItem(_structureInfo.QueuedMeals[k].MealType);
				_infoCardController.Card1.HungerRecipeController.AddRecipe(recipeItem2.Type);
				_infoCardController.Card2.HungerRecipeController.AddRecipe(recipeItem2.Type);
			}
			UpdateQueueText();
			UpdateQuantities();
		}

		private void UpdateQueueText()
		{
			Color colour = ((_queuedItems.Count > 0) ? StaticColors.GreenColor : StaticColors.RedColor);
			amountQueuedText.text = string.Format("{0}/{1}", _queuedItems.Count, kMaxItems).Colour(colour);
		}

		private void UpdateQuantities()
		{
			foreach (RecipeItem recipeItem in _recipeItems)
			{
				recipeItem.UpdateQuantity();
			}
			foreach (RecipeItem queuedItem in _queuedItems)
			{
				queuedItem.UpdateQuantity();
			}
		}

		private void OnRecipeChosen(RecipeItem item)
		{
			if (_structureInfo.QueuedResources.Count < kMaxItems)
			{
				if (_structureInfo.QueuedResources.Count >= kMaxItems || !CookingData.CanMakeMeal(item.Type))
				{
					ShowMaxQueued();
					item.Shake();
				}
				else
				{
					AddToQueue(item.Type);
				}
			}
		}

		private void OnQueueableItemSelected()
		{
			if (_structureInfo.QueuedResources.Count >= kMaxItems)
			{
				_addToQueuePrompt.SetActive(false);
				_removeFromQueuePrompt.SetActive(false);
				_queueFullPrompt.SetActive(true);
			}
			else
			{
				_addToQueuePrompt.SetActive(true);
				_removeFromQueuePrompt.SetActive(false);
				_queueFullPrompt.SetActive(false);
			}
		}

		private void OnQueuedItemSelected()
		{
			_addToQueuePrompt.SetActive(false);
			_removeFromQueuePrompt.SetActive(true);
			_queueFullPrompt.SetActive(false);
		}

		private void AddToQueue(InventoryItem.ITEM_TYPE meal)
		{
			_structureInfo.QueuedMeals.Add(new Interaction_Kitchen.QueuedMeal
			{
				MealType = meal,
				CookingDuration = CookingData.GetMealCookDuration(meal),
				CookedTime = 0f,
				Ingredients = CookingData.GetRecipeSimplified(meal)
			});
			foreach (InventoryItem item in CookingData.GetRecipe(meal)[0])
			{
				Inventory.ChangeItemQuantity(item.type, -item.quantity);
			}
			RecipeItem recipeItem = MakeQueuedItem(meal);
			Vector3 localScale = recipeItem.RectTransform.localScale;
			recipeItem.RectTransform.localScale = Vector3.one * 1.2f;
			recipeItem.RectTransform.DOScale(localScale, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
			_infoCardController.CurrentCard.Configure(meal);
			_infoCardController.Card1.HungerRecipeController.AddRecipe(meal);
			_infoCardController.Card2.HungerRecipeController.AddRecipe(meal);
			Action<InventoryItem.ITEM_TYPE> onItemQueued = OnItemQueued;
			if (onItemQueued != null)
			{
				onItemQueued(meal);
			}
			UpdateQueueText();
			UpdateQuantities();
			OnQueueableItemSelected();
			foreach (RecipeItem recipeItem2 in _recipeItems)
			{
				recipeItem2.Button.Confirmable = _queuedItems.Count < kMaxItems;
			}
		}

		private RecipeItem MakeQueuedItem(InventoryItem.ITEM_TYPE resource)
		{
			RecipeItem recipeItem = UnityEngine.Object.Instantiate(recipeIconPrefab, queuedContainer);
			recipeItem.Configure(resource, false, true);
			MMButton button = recipeItem.Button;
			button.OnSelected = (Action)Delegate.Combine(button.OnSelected, new Action(OnQueuedItemSelected));
			recipeItem.OnRecipeChosen = (Action<RecipeItem>)Delegate.Combine(recipeItem.OnRecipeChosen, new Action<RecipeItem>(RemoveFromQueue));
			_vacantSlots[_queuedItems.Count].SetActive(false);
			_queuedItems.Add(recipeItem);
			recipeItem.transform.SetSiblingIndex(_queuedItems.Count - 1);
			return recipeItem;
		}

		private void RemoveFromQueue(RecipeItem item)
		{
			if (_infoCardController.CurrentCard != null)
			{
				_infoCardController.CurrentCard.Configure(item.Type);
			}
			IMMSelectable iMMSelectable = item.Button.FindSelectableOnRight() as IMMSelectable;
			IMMSelectable iMMSelectable2 = item.Button.FindSelectableOnLeft() as IMMSelectable;
			if (_queuedItems.IndexOf(item) < _queuedItems.Count - 1 && iMMSelectable != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(iMMSelectable);
			}
			else if (_queuedItems.IndexOf(item) > 0 && iMMSelectable2 != null)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(iMMSelectable2);
			}
			else if (_queuedItems.Count - 1 > 0)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_queuedItems[_queuedItems.Count - 2].Button);
			}
			else
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_recipeItems[0].Button);
			}
			int num = _queuedItems.IndexOf(item);
			_structureInfo.QueuedMeals.RemoveAt(num);
			foreach (InventoryItem item2 in CookingData.GetRecipe(item.Type)[0])
			{
				Inventory.AddItem(item2.type, item2.quantity);
			}
			_queuedItems.Remove(item);
			_vacantSlots[_queuedItems.Count].SetActive(true);
			_infoCardController.Card1.HungerRecipeController.RemoveRecipe(item.Type);
			_infoCardController.Card2.HungerRecipeController.RemoveRecipe(item.Type);
			if (num == 0 && _queuedItems.Count > 0)
			{
				_queuedItems[0].Configure(_queuedItems[0].Type, false, true);
				_structureInfo.Progress = 0f;
			}
			Action<InventoryItem.ITEM_TYPE, int> onItemRemovedFromQueue = OnItemRemovedFromQueue;
			if (onItemRemovedFromQueue != null)
			{
				onItemRemovedFromQueue(item.Type, num);
			}
			UnityEngine.Object.Destroy(item.gameObject);
			UpdateQueueText();
			UpdateQuantities();
		}

		private void ShowMaxQueued()
		{
			if (_queuedTextTween != null && !_queuedTextTween.IsComplete())
			{
				_queuedTextTween.Complete();
				amountQueuedText.transform.localScale = Vector3.one;
			}
			_queuedTextTween = amountQueuedText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideStarted()
		{
			base.OnHideStarted();
			UIManager.PlayAudio("event:/ui/close_menu");
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
