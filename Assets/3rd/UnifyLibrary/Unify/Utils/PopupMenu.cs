using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unify.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unify.Utils
{
	public class PopupMenu : MonoBehaviour
	{
		public bool dontRemove;

		public int priority;

		public bool Modal;

		private static List<PopupMenu> stack;

		private GameObject lastSelectable;

		public GameObject defaultSelectable;

		private GameObject parentSelectable;

		public void Open()
		{
			base.gameObject.SetActive(value: true);
		}

		public void Close()
		{
			base.gameObject.SetActive(value: false);
		}

		private void OnEnable()
		{
			Logger.Log("PopupMenuFocus:OnEnable: " + base.gameObject.name);
			if (stack == null)
			{
				stack = new List<PopupMenu>();
			}
			parentSelectable = EventSystem.current.currentSelectedGameObject;
			Logger.Log("PopupMenuFocus:parentSelectable: " + ((parentSelectable != null) ? parentSelectable.gameObject.name : "null"));
			if (!stack.Contains(this))
			{
				Logger.Log("PopupMenuFocus:Add Menu: " + base.gameObject.name);
				stack.Add(this);
				UpdateIteraction();
				if (Modal)
				{
					InputManager.InputEnabled = false;
				}
			}
			else
			{
				Logger.LogError("PushFocus: Menu Exists: " + base.gameObject.name);
			}
		}

		private void OnDisable()
		{
			Logger.Log("PopupMenuFocus:OnDisable: " + base.gameObject.name);
			if (!dontRemove)
			{
				Logger.Log("PopupMenuFocus:Remove Menu: " + base.gameObject.name);
				stack.Remove(this);
				UpdateIteraction();
				if (parentSelectable != null)
				{
					Logger.Log("PopupMenuFocus:Restoring parentSelectable: " + parentSelectable.gameObject.name);
					parentSelectable.GetComponent<Selectable>().Select();
				}
				if (Modal)
				{
					InputManager.InputEnabled = true;
				}
			}
		}

		private void Update()
		{
			if (CurrentMenu().gameObject != base.gameObject)
			{
				return;
			}
			GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			if (currentSelectedGameObject == null)
			{
				if (lastSelectable != null)
				{
					Logger.Log("PopupMenuFocus:Update: force a selection: " + SafeName(lastSelectable));
					lastSelectable.GetComponent<Selectable>().Select();
				}
				else
				{
					Logger.Log("PopupMenuFocus:Update: force  default: " + SafeName(defaultSelectable));
					defaultSelectable.GetComponent<Selectable>().Select();
				}
			}
			else if (!currentSelectedGameObject.transform.IsChildOf(base.transform))
			{
				if (lastSelectable != null)
				{
					Logger.Log("PopupMenuFocus:Update: override child selection: " + SafeName(lastSelectable));
					lastSelectable.GetComponent<Selectable>().Select();
				}
				else
				{
					Logger.Log("PopupMenuFocus:Update: override default selection: " + SafeName(defaultSelectable));
					defaultSelectable.GetComponent<Selectable>().Select();
				}
			}
			else
			{
				if (lastSelectable != currentSelectedGameObject)
				{
					Logger.Log("PopupMenuFocus:Update: last selection: " + SafeName(currentSelectedGameObject));
				}
				lastSelectable = currentSelectedGameObject;
			}
		}

		private PopupMenu CurrentMenu()
		{
			if (stack.Count > 0)
			{
				return stack[stack.Count - 1];
			}
			return null;
		}

		private PopupMenu PreviousMenu()
		{
			if (stack.Count > 1)
			{
				return stack[stack.Count - 2];
			}
			return null;
		}

		private void UpdateIteraction()
		{
			stack = stack.OrderBy((PopupMenu menu) => menu.priority).ToList();
			foreach (PopupMenu item in stack)
			{
				if (!(item != null))
				{
					continue;
				}
				CanvasGroup component = item.gameObject.GetComponent<CanvasGroup>();
				if (component != null)
				{
					if (item != CurrentMenu())
					{
						StartCoroutine(DisableCanvasAfterDelay(component));
					}
					else
					{
						component.interactable = true;
					}
				}
			}
		}

		private IEnumerator DisableCanvasAfterDelay(CanvasGroup cg)
		{
			yield return new WaitForSeconds(0.1f);
			cg.interactable = false;
		}

		private string SafeName(GameObject gameObject)
		{
			if (!(gameObject != null))
			{
				return "none";
			}
			return gameObject.name;
		}
	}
}
