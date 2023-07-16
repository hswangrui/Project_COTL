using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using Unify.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINavigator : BaseMonoBehaviour
{
	public delegate void ChangeSelection(Buttons Button);

	public delegate void ChangeSelectionUnity(Selectable NewSelectable, Selectable PrevSelectable);

	public delegate void Deselect(Buttons Button);

	public delegate void Close();

	public delegate void ButtonDown(Buttons CurrentButton);

	public bool isCardNavigator;

	public CanvasGroup canvasGroup;

	public Scrollbar scrollBar;

	public bool MoveAButton;

	public GameObject buttonToMove;

	public bool CallBackOnChange;

	public UnityEvent Callback;

	public bool Is2DArray;

	public int arrayWidth;

	public string HorizontalNavAxisName = "Horizontal";

	public string VerticalNavAxisName = "Vertical";

	private Vector3 velocity = Vector3.zero;

	private float startingScrollBarValue;

	private float SelectionDelay;

	public int _CurrentSelection;

	public float scrollValue;

	private bool canvasGroupOff;

	private Animator nextButtonAnim;

	private Animator prevButtonAnim;

	public bool updateButtons;

	public List<Buttons> Buttons = new List<Buttons>();

	public List<Buttons> dynamicButtons = new List<Buttons>();

	public List<Buttons> list = new List<Buttons>();

	public Selectable startingItem;

	public ScrollRect scrollRect;

	public Selectable _selectable;

	private Selectable newSelect;

	public bool useUnityNavigation;

	public bool focusOnSelected;

	public RectTransform focusMovementRect;

	public bool ControlsEnabled = true;

	public ChangeSelection OnChangeSelection;

	public ChangeSelectionUnity OnChangeSelectionUnity;

	public Deselect OnDeselect;

	public Close OnClose;

	public Action OnSelectDown;

	public int oldSelection = -1;

	private Animator animator;

	public bool released;

	private RectTransform rect;

	private RectTransform thisRect;

	public Vector3 focusOffset = new Vector3(0f, 0f, 0f);

	private float ButtonDownDelay;

	public ButtonDown OnButtonDown;

	private Player player
	{
		get
		{
			return RewiredInputManager.MainPlayer;
		}
	}

	public Selectable selectable
	{
		get
		{
			return _selectable;
		}
		set
		{
			if (_selectable != value)
			{
				ChangeSelectionUnity onChangeSelectionUnity = OnChangeSelectionUnity;
				if (onChangeSelectionUnity != null)
				{
					onChangeSelectionUnity(_selectable, value);
				}
				_selectable = value;
				_selectable.Select();
			}
		}
	}

	public int CurrentSelection
	{
		get
		{
			return _CurrentSelection;
		}
		set
		{
			if (Callback != null)
			{
				UnityEvent callback = Callback;
				if (callback != null)
				{
					callback.Invoke();
				}
			}
			if (list.Count <= 0)
			{
				return;
			}
			AudioManager.Instance.PlayOneShot("event:/ui/change_selection");
			if (selectable != null)
			{
				Deselect onDeselect = OnDeselect;
				if (onDeselect != null)
				{
					onDeselect(list[_CurrentSelection]);
				}
			}
			SelectionDelay = 0.5f;
			oldSelection = _CurrentSelection;
			_CurrentSelection = value;
			if (_CurrentSelection < 0)
			{
				_CurrentSelection = list.Count - 1;
			}
			if (_CurrentSelection > list.Count - 1)
			{
				_CurrentSelection = 0;
			}
			if (list[_CurrentSelection] != null)
			{
				GameObject button = list[_CurrentSelection].Button;
				if (button == null)
				{
					return;
				}
				animator = button.GetComponent<Animator>();
				if (animator != null && oldSelection != _CurrentSelection)
				{
					animator.SetTrigger("Selected");
				}
				if (oldSelection != -1 && oldSelection != _CurrentSelection)
				{
					animator = list[oldSelection].Button.GetComponent<Animator>();
					if (animator != null)
					{
						animator.SetTrigger("Normal");
					}
				}
				selectable = list[_CurrentSelection].Button.GetComponent<Selectable>();
				if (selectable != null)
				{
					selectable.Select();
				}
				else
				{
					Button component = list[_CurrentSelection].Button.GetComponent<Button>();
					if (component != null)
					{
						component.Select();
					}
				}
			}
			MoveButton();
			ChangeSelection onChangeSelection = OnChangeSelection;
			if (onChangeSelection != null)
			{
				onChangeSelection(list[_CurrentSelection]);
			}
		}
	}

	public void updateIndex()
	{
		for (int i = 0; i < Buttons.Count; i++)
		{
			Buttons[i].Index = i;
		}
	}

	private IEnumerator ScrollToTop()
	{
		if (scrollRect != null)
		{
			scrollRect.verticalNormalizedPosition = 1f;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)scrollRect.transform);
		}
		yield break;
	}

	private void Start()
	{
		released = false;
		setDefault();
		checkScrollBar();
		if (updateButtons)
		{
			newList();
			list = dynamicButtons;
		}
		else
		{
			list = Buttons;
		}
		StartCoroutine(ScrollToTop());
	}

	private void checkScrollBar()
	{
		if (!(scrollBar != null))
		{
			return;
		}
		CanvasGroup component = scrollBar.GetComponent<CanvasGroup>();
		if ((double)scrollBar.size >= 0.95)
		{
			if (component != null)
			{
				component.alpha = 0f;
			}
		}
		else if (component != null)
		{
			component.alpha = 1f;
		}
	}

	public void setDefault()
	{
		checkScrollBar();
		canvasGroupOff = false;
		if (!useUnityNavigation)
		{
			if (list.Count > 0 && list[0] != null)
			{
				oldSelection = -1;
				_CurrentSelection = -1;
				CurrentSelection = 0;
				for (int i = 0; i < list.Count - 1 && !list[i].Button.activeInHierarchy; i++)
				{
					CurrentSelection++;
				}
			}
		}
		else if (startingItem != null)
		{
			selectable = startingItem;
			newSelect = startingItem;
			unityNavigation();
		}
	}

	public void Update()
	{
		if (player != null && canvasGroup != null)
		{
			if (canvasGroup.alpha == 1f)
			{
				Controls();
			}
			else if (!canvasGroupOff)
			{
				canvasGroupOff = true;
			}
		}
	}

	public void newList()
	{
		dynamicButtons.Clear();
		for (int i = 0; i < Buttons.Count; i++)
		{
			if (Buttons[i].Button.activeSelf)
			{
				dynamicButtons.Add(Buttons[i]);
			}
		}
		setDefault();
	}

	private void MoveButton()
	{
		if (MoveAButton)
		{
			StopAllCoroutines();
			StartCoroutine(MoveButtonRoutine());
		}
	}

	private IEnumerator MoveButtonRoutine()
	{
		if (!canvasGroup.interactable)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		Vector3 targetLocalPosition;
		Vector3 currentLocalPosition;
		if (!useUnityNavigation)
		{
			Vector3 position = list[_CurrentSelection].Button.transform.position;
			targetLocalPosition = buttonToMove.transform.parent.InverseTransformPoint(position);
			currentLocalPosition = buttonToMove.transform.localPosition;
		}
		else
		{
			Vector3 position = selectable.transform.position;
			targetLocalPosition = buttonToMove.transform.parent.InverseTransformPoint(position);
			currentLocalPosition = buttonToMove.transform.localPosition;
		}
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime * 5f);
			if (!(num <= 1f))
			{
				break;
			}
			buttonToMove.transform.localPosition = Vector3.SmoothDamp(targetLocalPosition, currentLocalPosition, ref velocity, Progress);
			yield return null;
		}
		yield return null;
	}

	private void changeSetting(int i, int valueToIncrease)
	{
	}

	private void updateScrollBar()
	{
		StopAllCoroutines();
		StartCoroutine(MoveScrollBar());
	}

	private IEnumerator MoveScrollBar()
	{
		if (!canvasGroup.interactable)
		{
			yield return null;
		}
		float Progress = 0f;
		startingScrollBarValue = scrollBar.value;
		float num = _CurrentSelection;
		float num2 = list.Count - 1;
		scrollValue = num / num2;
		scrollValue -= 1f;
		scrollValue *= -1f;
		while (true)
		{
			float num3;
			Progress = (num3 = Progress + Time.unscaledDeltaTime * 5f);
			if (num3 <= 1f)
			{
				scrollBar.value = Mathf.SmoothStep(startingScrollBarValue, scrollValue, Progress);
				yield return null;
				continue;
			}
			break;
		}
	}

	private void updateScrollBarHorizontal()
	{
		StopAllCoroutines();
		StartCoroutine(MoveScrollBarHorizontal());
	}

	private IEnumerator MoveScrollBarHorizontal()
	{
		if (!canvasGroup.interactable)
		{
			yield return null;
		}
		float Progress = 0f;
		startingScrollBarValue = scrollBar.value;
		float num = _CurrentSelection;
		float num2 = list.Count;
		scrollValue = num / num2;
		while (true)
		{
			float num3;
			Progress = (num3 = Progress + Time.unscaledDeltaTime * 5f);
			if (num3 <= 1f)
			{
				scrollBar.value = Mathf.SmoothStep(startingScrollBarValue, scrollValue, Progress);
				yield return null;
				continue;
			}
			break;
		}
	}

	private void unityNavigation()
	{
		if (newSelect != null && newSelect.gameObject.activeSelf)
		{
			SelectionDelay = 0.5f;
			animator = newSelect.gameObject.GetComponent<Animator>();
			if (animator != null)
			{
				animator.SetTrigger("Selected");
			}
			selectable = newSelect;
			MoveButton();
			selectable.Select();
			if (scrollBar != null)
			{
				updateScrollBar();
			}
		}
	}

	private void focusOnSelectedObject()
	{
		rect = selectable.gameObject.GetComponent<RectTransform>();
		if (focusMovementRect == null)
		{
			focusMovementRect = base.gameObject.GetComponent<RectTransform>();
		}
		float num = 10f;
		focusMovementRect.position = Vector3.Lerp(focusMovementRect.position, -rect.position + focusOffset, num * Time.unscaledDeltaTime);
	}

	private void DeselectButton()
	{
		if (newSelect != null)
		{
			animator = newSelect.gameObject.GetComponent<Animator>();
		}
		if (animator != null)
		{
			animator.SetTrigger("Normal");
		}
	}

	private void Controls()
	{
		if (focusOnSelected)
		{
			focusOnSelectedObject();
		}
		if (!ControlsEnabled || !canvasGroup.interactable || canvasGroup.alpha == 0f)
		{
			return;
		}
		if (canvasGroupOff)
		{
			setDefault();
		}
		SelectionDelay -= Time.unscaledDeltaTime;
		if (SelectionDelay < 0f)
		{
			if (!useUnityNavigation)
			{
				if (Is2DArray)
				{
					if (canvasGroup.interactable)
					{
						if (scrollBar != null)
						{
							updateScrollBarHorizontal();
						}
						if (player.GetAxis(HorizontalNavAxisName) > 0.2f)
						{
							int currentSelection = CurrentSelection + 1;
							CurrentSelection = currentSelection;
						}
						if (player.GetAxis(HorizontalNavAxisName) < -0.2f)
						{
							int currentSelection = CurrentSelection - 1;
							CurrentSelection = currentSelection;
						}
					}
					else
					{
						if (player.GetAxis(HorizontalNavAxisName) > 0.2f && Mathf.Abs(player.GetAxis(VerticalNavAxisName)) < 0.35f)
						{
							changeSetting(CurrentSelection, 1);
						}
						if (player.GetAxis(HorizontalNavAxisName) < -0.2f && Mathf.Abs(player.GetAxis(VerticalNavAxisName)) < 0.35f)
						{
							changeSetting(CurrentSelection, -1);
						}
					}
				}
				else if (list.Count > 0 && list[0].isSetting)
				{
					if (player.GetAxis(HorizontalNavAxisName) > 0.2f && Mathf.Abs(player.GetAxis(VerticalNavAxisName)) < 0.35f)
					{
						changeSetting(CurrentSelection, 1);
					}
					if (player.GetAxis(HorizontalNavAxisName) < -0.2f && Mathf.Abs(player.GetAxis(VerticalNavAxisName)) < 0.35f)
					{
						changeSetting(CurrentSelection, -1);
					}
				}
			}
			else
			{
				if (player.GetAxis(HorizontalNavAxisName) > 0.2f)
				{
					DeselectButton();
					newSelect = selectable.FindSelectableOnRight();
					unityNavigation();
				}
				if (player.GetAxis(HorizontalNavAxisName) < -0.2f)
				{
					DeselectButton();
					newSelect = selectable.FindSelectableOnLeft();
					unityNavigation();
				}
			}
		}
		if (SelectionDelay < 0f && list.Count > 0)
		{
			if (!useUnityNavigation)
			{
				if (player.GetAxis(VerticalNavAxisName) < -0.35f)
				{
					if (Is2DArray)
					{
						CurrentSelection += arrayWidth;
					}
					else
					{
						for (int i = Mathf.Min(CurrentSelection + 1, list.Count - 1); i < list.Count; i++)
						{
							if (list[i].Button.activeInHierarchy)
							{
								CurrentSelection = i;
								break;
							}
						}
					}
					if (scrollBar != null)
					{
						updateScrollBar();
					}
				}
				if (player.GetAxis(VerticalNavAxisName) > 0.35f)
				{
					if (Is2DArray)
					{
						CurrentSelection -= arrayWidth;
					}
					else
					{
						for (int num = Mathf.Max(CurrentSelection - 1, 0); num >= 0; num--)
						{
							if (list[num].Button.activeInHierarchy)
							{
								CurrentSelection = num;
								break;
							}
						}
					}
					if (scrollBar != null)
					{
						updateScrollBar();
					}
				}
			}
			else
			{
				if (player.GetAxis(VerticalNavAxisName) < -0.35f)
				{
					DeselectButton();
					newSelect = selectable.FindSelectableOnDown();
					unityNavigation();
				}
				if (player.GetAxis(VerticalNavAxisName) > 0.35f)
				{
					DeselectButton();
					newSelect = selectable.FindSelectableOnUp();
					unityNavigation();
				}
			}
		}
		if (SelectionDelay > 0.1f && Mathf.Abs(player.GetAxis(HorizontalNavAxisName)) <= 0.2f && Mathf.Abs(player.GetAxis(VerticalNavAxisName)) <= 0.35f)
		{
			SelectionDelay = 0.1f;
		}
		if ((ButtonDownDelay -= Time.deltaTime) < 0f && released && InputManager.UI.GetAcceptButtonHeld())
		{
			if (!useUnityNavigation)
			{
				if (list.Count > 0)
				{
					ButtonDown onButtonDown = OnButtonDown;
					if (onButtonDown != null)
					{
						onButtonDown(list[CurrentSelection]);
					}
				}
			}
			else
			{
				StopAllCoroutines();
				selectable.GetComponent<Button>().onClick.Invoke();
			}
			ButtonDownDelay = 0.2f;
		}
		if (!released && !InputManager.UI.GetAcceptButtonHeld())
		{
			released = true;
		}
		if (useUnityNavigation)
		{
			return;
		}
		if (list.Count > 0 && InputManager.UI.GetAcceptButtonDown() && canvasGroup.interactable)
		{
			if (buttons.Toggle == list[CurrentSelection].buttonTypes)
			{
				changeSetting(CurrentSelection, 1);
			}
			else
			{
				Button component = list[CurrentSelection].Button.GetComponent<Button>();
				if (component != null)
				{
					component.onClick.Invoke();
				}
			}
		}
		if (InputManager.UI.GetAcceptButtonDown())
		{
			Action onSelectDown = OnSelectDown;
			if (onSelectDown != null)
			{
				onSelectDown();
			}
		}
		if (SelectionDelay <= 0f && InputManager.UI.GetCancelButtonUp() && (!isCardNavigator || (isCardNavigator && UIWeaponCardSoul.uIWeaponCardSouls.Count <= 0)))
		{
			Close onClose = OnClose;
			if (onClose != null)
			{
				onClose();
			}
		}
	}
}
