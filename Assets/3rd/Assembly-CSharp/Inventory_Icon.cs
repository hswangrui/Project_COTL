using System;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Icon : BaseMonoBehaviour
{
	private float scale;

	private float scaleSpeed;

	public float Angle;

	private float Distance;

	private float DistanceSpeed;

	public float TargetDistance;

	public float Delay;

	private float TargetScale = 1f;

	public RectTransform rectTransform;

	public Vector3 TargetLocation;

	public Text QuantityText;

	public InventoryItemDisplay ItemDisplay;

	private InventoryItem item;

	private void Start()
	{
		base.transform.localScale = Vector3.zero;
		rectTransform = GetComponent<RectTransform>();
	}

	public void SetImage(int i, int quantity)
	{
		ItemDisplay.SetImage((InventoryItem.ITEM_TYPE)i);
		QuantityText.text = quantity.ToString();
	}

	public void SetItem(InventoryItem item)
	{
		this.item = item;
	}

	private void Update()
	{
		if (!((Delay -= Time.deltaTime) > 0f))
		{
			scaleSpeed += (TargetScale - scale) * 0.3f;
			scale += (scaleSpeed *= 0.7f);
			base.transform.localScale = new Vector3(scale, scale, 1f);
			DistanceSpeed += (TargetDistance - Distance) * 0.4f;
			Distance += (DistanceSpeed *= 0.5f);
			rectTransform.localPosition = new Vector3(Distance * Mathf.Cos(Angle * ((float)Math.PI / 180f)), Distance * Mathf.Sin(Angle * ((float)Math.PI / 180f)), 0f);
			if (HUD_Inventory.CURRENT_SELECTION == this)
			{
				TargetScale = 1.2f;
			}
			else
			{
				TargetScale = 1f;
			}
			if (item != null)
			{
				SetImage(item.type, item.quantity);
			}
		}
	}
}
