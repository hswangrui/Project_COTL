using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Inventory : BaseMonoBehaviour
{
	public GameObject icon;

	private float Timer;

	private List<Inventory_Icon> icons;

	public static Inventory_Icon CURRENT_SELECTION;

	public RectTransform cursor;

	public RectTransform pointer;

	private float PointerAngle;

	private float PointerDistance;

	private float Delay;

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		CURRENT_SELECTION = null;
		PointerAngle = (PointerDistance = (Timer = 0f));
		pointer.localPosition = Vector3.zero;
		icons = new List<Inventory_Icon>();
		for (int i = 0; i < Inventory.items.Count; i++)
		{
			Inventory_Icon component = UnityEngine.Object.Instantiate(icon, base.transform.parent).GetComponent<Inventory_Icon>();
			if (Inventory.items.Count == 1)
			{
				component.Angle = 90f;
			}
			else if (Inventory.items.Count == 2)
			{
				component.Angle = 180 * i + 90;
			}
			else if (Inventory.items.Count == 3)
			{
				switch (i)
				{
				case 0:
					component.Angle = 90f;
					break;
				case 1:
					component.Angle = 0f;
					break;
				case 2:
					component.Angle = 180f;
					break;
				}
			}
			else if (Inventory.items.Count == 4)
			{
				component.Angle = 90 * (Inventory.items.Count - 1 - i) + 180;
			}
			else
			{
				component.Angle = 45 * (Inventory.items.Count - 1 - i) + 270;
			}
			component.TargetDistance = 250f;
			component.Delay = (float)i * 0.05f;
			component.TargetLocation = new Vector3(component.TargetDistance * Mathf.Cos(component.Angle * ((float)Math.PI / 180f)), component.TargetDistance * Mathf.Sin(component.Angle * ((float)Math.PI / 180f)));
			component.SetImage(Inventory.items[i].type, Inventory.items[i].quantity);
			icons.Add(component);
		}
		Delay = (float)Inventory.items.Count * 0.05f;
		cursor.gameObject.GetComponent<Image>().enabled = false;
	}

	private void OnDisable()
	{
		foreach (Inventory_Icon icon in icons)
		{
			UnityEngine.Object.Destroy(icon.gameObject);
		}
		icons.Clear();
		icons = null;
	}

	private void Update()
	{
		if ((Timer += Time.deltaTime) > Delay)
		{
			cursor.localPosition = Vector3.Lerp(cursor.localPosition, new Vector3(250f * InputManager.UI.GetHorizontalAxis(), 250f * InputManager.UI.GetVerticalAxis()), 15f * Time.deltaTime);
			if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > 0.3f || Mathf.Abs(InputManager.UI.GetVerticalAxis()) > 0.3f)
			{
				foreach (Inventory_Icon icon in icons)
				{
					if (CURRENT_SELECTION == null)
					{
						CURRENT_SELECTION = icon;
					}
					else if (CURRENT_SELECTION != icon && Vector2.Distance(icon.TargetLocation, cursor.localPosition) < Vector2.Distance(CURRENT_SELECTION.TargetLocation, cursor.localPosition))
					{
						CURRENT_SELECTION = icon;
					}
				}
				if (PointerDistance <= 0f)
				{
					PointerAngle = Utils.GetAngle(pointer.localPosition, CURRENT_SELECTION.TargetLocation);
				}
			}
			if (CURRENT_SELECTION != null)
			{
				PointerAngle = Mathf.LerpAngle(PointerAngle, Utils.GetAngle(pointer.localPosition, CURRENT_SELECTION.TargetLocation), 15f * Time.deltaTime);
				PointerDistance = Mathf.Lerp(PointerDistance, 100f, 15f * Time.deltaTime);
				pointer.localPosition = new Vector3(PointerDistance * Mathf.Cos(PointerAngle * ((float)Math.PI / 180f)), PointerDistance * Mathf.Sin(PointerAngle * ((float)Math.PI / 180f)));
			}
		}
		if (InputManager.Gameplay.GetAttackButtonUp())
		{
			Close();
		}
	}

	private void Close()
	{
		base.gameObject.SetActive(false);
	}
}
