using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using src.UI;
using src.UI.InfoCards;
using src.UINavigator;
using TMPro;
using UnityEngine;

namespace Lamb.UI.KitchenMenu
{
	public class UICookingFireMenuController : UIMenuBase
	{
		public Action OnConfirm;

		[Header("Kitchen Menu")]
		[SerializeField]
		private RecipesMenu _recipesMenu;

		[SerializeField]
		private RecipeQueue _recipeQueue;

		[SerializeField]
		private MMButton _cookButton;

		[SerializeField]
		private RectTransform _cookButtonRectTransform;

		[SerializeField]
		private ButtonHighlightController _highlightController;

		[SerializeField]
		private RecipeInfoCardController _infoCardController;

		[Header("Hunger Bar")]
		[SerializeField]
		private TextMeshProUGUI _followerCount;

		protected StructuresData _kitchenData;

		private bool _didConfirm;

		public virtual void Show(StructuresData kitchenData, bool instant = false)
		{
			_kitchenData = kitchenData;
			_recipesMenu.Configure(kitchenData);
			_recipeQueue.Configure(kitchenData);
			foreach (Interaction_Kitchen.QueuedMeal queuedMeal in _kitchenData.QueuedMeals)
			{
				_infoCardController.Card1.HungerRecipeController.AddRecipe(queuedMeal.MealType);
				_infoCardController.Card2.HungerRecipeController.AddRecipe(queuedMeal.MealType);
			}
			RecipesMenu recipesMenu = _recipesMenu;
			recipesMenu.OnRecipeChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(recipesMenu.OnRecipeChosen, new Action<InventoryItem.ITEM_TYPE>(OnRecipeChosen));
			_recipeQueue.RequestSelectable = ProvideFallback;
			RecipeQueue recipeQueue = _recipeQueue;
			recipeQueue.OnRecipeRemoved = (Action<InventoryItem.ITEM_TYPE, int>)Delegate.Combine(recipeQueue.OnRecipeRemoved, new Action<InventoryItem.ITEM_TYPE, int>(OnRecipeRemoved));
			_cookButton.onClick.AddListener(ConfirmCooking);
			MMButton cookButton = _cookButton;
			cookButton.OnConfirmDenied = (Action)Delegate.Combine(cookButton.OnConfirmDenied, new Action(ShakeCookButton));
			MMButton cookButton2 = _cookButton;
			cookButton2.OnSelected = (Action)Delegate.Combine(cookButton2.OnSelected, new Action(OnCookButtonSelected));
			MMButton cookButton3 = _cookButton;
			cookButton3.OnDeselected = (Action)Delegate.Combine(cookButton3.OnDeselected, new Action(OnCookButtonDeselected));
			_cookButton.Confirmable = kitchenData.QueuedMeals.Count > 0;
			_highlightController.SetAsRed();
			if (MonoSingleton<UINavigatorNew>.Instance.CurrentSelectable == null)
			{
				StartCoroutine(Wait());
			}
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			UIManager.PlayAudio("event:/ui/open_menu");
			base.OnShowStarted();
			StartCoroutine(Wait());
		}

		private void Update()
		{
			if (_canvasGroup.interactable && InputManager.UI.GetCookButtonDown())
			{
				if (_kitchenData.QueuedMeals.Count > 0)
				{
					ConfirmCooking();
				}
				else
				{
					ShakeCookButton();
				}
			}
		}

		private void ConfirmCooking()
		{
			if (_kitchenData.QueuedMeals.Count > 0)
			{
				_didConfirm = true;
				Hide();
			}
		}

		private void ShakeCookButton()
		{
			_cookButtonRectTransform.DOKill();
			_cookButtonRectTransform.anchoredPosition = Vector2.zero;
			_cookButtonRectTransform.DOShakePosition(1f, new Vector3(10f, 0f)).SetUpdate(true);
		}

		private IEnumerator Wait()
		{
			yield return null;
			DetermineSelectable();
		}

		private void DetermineSelectable()
		{
			IMMSelectable iMMSelectable = null;
			iMMSelectable = _recipesMenu.ProvideFirstSelectable();
			if (iMMSelectable != null)
			{
				OverrideDefault(iMMSelectable.Selectable);
			}
			ActivateNavigation();
		}

		private void OnCookButtonSelected()
		{
			_highlightController.Image.color = new Color(1f, 1f, 1f, 1f);
			_highlightController.transform.DOKill();
			_highlightController.transform.DOShakeScale(0.1f, new Vector3(-0.1f, 0.1f, 1f), 5, 90f, false).SetUpdate(true);
		}

		private void OnCookButtonDeselected()
		{
			_highlightController.Image.color = new Color(0f, 0.5f, 1f, 1f);
		}

		private void OnRecipeChosen(InventoryItem.ITEM_TYPE recipe)
		{
			UIManager.PlayAudio("event:/cooking/add_food_ingredient");
			_highlightController.SetAsRed();
			List<InventoryItem> list = CookingData.GetRecipe(recipe)[0];
			foreach (InventoryItem item in list)
			{
				Inventory.ChangeItemQuantity(item.type, -item.quantity);
			}
			_recipesMenu.UpdateQuantities();
			_infoCardController.CurrentCard.Configure(recipe);
			_kitchenData.Inventory.Clear();
			_kitchenData.QueuedMeals.Add(new Interaction_Kitchen.QueuedMeal
			{
				MealType = recipe,
				CookingDuration = CookingData.GetMealCookDuration(recipe),
				Ingredients = list
			});
			_recipeQueue.AddRecipe(recipe);
			_infoCardController.Card1.HungerRecipeController.AddRecipe(recipe);
			_infoCardController.Card2.HungerRecipeController.AddRecipe(recipe);
			if ((_kitchenData.QueuedMeals.Count == RecipeLimit() || _recipesMenu.ReadilyAvailableMeals() == 0) && !InputManager.General.MouseInputActive)
			{
				MonoSingleton<UINavigatorNew>.Instance.NavigateToNew(_cookButton);
			}
			_cookButton.Confirmable = _kitchenData.QueuedMeals.Count > 0;
		}

		private void OnRecipeRemoved(InventoryItem.ITEM_TYPE recipe, int index)
		{
			if (_infoCardController.CurrentCard != null)
			{
				_infoCardController.CurrentCard.Configure(recipe);
			}
			foreach (InventoryItem ingredient in _kitchenData.QueuedMeals[index].Ingredients)
			{
				Inventory.AddItem(ingredient.type, ingredient.quantity);
			}
			_recipesMenu.UpdateQuantities();
			_kitchenData.QueuedMeals.RemoveAt(index);
			_infoCardController.Card1.HungerRecipeController.RemoveRecipe(recipe);
			_infoCardController.Card2.HungerRecipeController.RemoveRecipe(recipe);
			_cookButton.Confirmable = _kitchenData.QueuedMeals.Count > 0;
		}

		private IMMSelectable ProvideFallback()
		{
			return _recipesMenu.ProvideSelectable();
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
			if (_didConfirm)
			{
				Action onConfirm = OnConfirm;
				if (onConfirm != null)
				{
					onConfirm();
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private int RecipeLimit()
		{
			return 12 + ((_kitchenData.Type == StructureBrain.TYPES.KITCHEN_II) ? 5 : 0);
		}
	}
}
