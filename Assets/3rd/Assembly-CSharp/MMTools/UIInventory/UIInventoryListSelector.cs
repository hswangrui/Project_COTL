using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace MMTools.UIInventory
{
	public class UIInventoryListSelector : BaseMonoBehaviour
	{
		public delegate void ListSelectorAction(InventoryItem Item);

		public ListSelectorAction OnMove;

		public ListSelectorAction OnSelect;

		private UIInventoryList List;

		public Image Selector;

		private float selectionDelay;

		public Vector3 SelectorTargetPosition;

		private int _CurrentSelection;

		private int CURRENT_SELECTION
		{
			get
			{
				return _CurrentSelection;
			}
			set
			{
				selectionDelay = 0.25f;
				_CurrentSelection = value;
				if (List.Items.Count > 0)
				{
					while (_CurrentSelection > List.Items.Count - 1)
					{
						_CurrentSelection -= List.Items.Count;
					}
					while (_CurrentSelection < 0)
					{
						_CurrentSelection += List.Items.Count;
					}
					SelectorTargetPosition = List.Items[CURRENT_SELECTION].rectTransform.position;
					ListSelectorAction onMove = OnMove;
					if (onMove != null)
					{
						onMove(List.Items[CURRENT_SELECTION].Item);
					}
				}
			}
		}

		private void OnEnable()
		{
			Selector.enabled = false;
			List = GetComponent<UIInventoryList>();
		}

		public void SetActive(int Selection)
		{
			CURRENT_SELECTION = Selection;
			Selector.enabled = true;
			Selector.transform.position = SelectorTargetPosition;
		}

		private void Update()
		{
			Selector.transform.position = Vector3.Lerp(Selector.transform.position, SelectorTargetPosition, 35f * Time.unscaledDeltaTime);
		}

		public void DoUpdate(Player RewiredController)
		{
			selectionDelay -= Time.unscaledDeltaTime;
			if (InputManager.UI.GetHorizontalAxis() > 0.3f && selectionDelay < 0f)
			{
				int cURRENT_SELECTION = CURRENT_SELECTION + 1;
				CURRENT_SELECTION = cURRENT_SELECTION;
			}
			if (InputManager.UI.GetHorizontalAxis() < -0.3f && selectionDelay < 0f)
			{
				int cURRENT_SELECTION = CURRENT_SELECTION - 1;
				CURRENT_SELECTION = cURRENT_SELECTION;
			}
			if (InputManager.UI.GetVerticalAxis() > 0.3f && selectionDelay < 0f)
			{
				CURRENT_SELECTION -= List.GridSize.x;
			}
			if (InputManager.UI.GetVerticalAxis() < -0.3f && selectionDelay < 0f)
			{
				CURRENT_SELECTION += List.GridSize.x;
			}
			if (InputManager.UI.GetVerticalAxis() >= -0.3f && InputManager.UI.GetVerticalAxis() <= 0.3f && InputManager.Gameplay.GetHorizontalAxis() < 0.3f && InputManager.Gameplay.GetHorizontalAxis() > -0.3f)
			{
				selectionDelay = 0f;
			}
			if (InputManager.UI.GetAcceptButtonDown() && List.Items[CURRENT_SELECTION].Item != null && List.Items[CURRENT_SELECTION].Item.type != 0)
			{
				ListSelectorAction onSelect = OnSelect;
				if (onSelect != null)
				{
					onSelect(List.Items[CURRENT_SELECTION].Item);
				}
			}
		}
	}
}
