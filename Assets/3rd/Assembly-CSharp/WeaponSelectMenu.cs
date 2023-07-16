using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectMenu : BaseMonoBehaviour
{
	private float Angle;

	public GameObject PointerRotator;

	public GameObject Pointer;

	private float PointerAngle;

	private GameObject CurrentGameObject;

	public Text text;

	public List<GameObject> Nodes;

	private int CURRENT_SELECTION;

	public void Show()
	{
		base.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		GameManager.SetTimeScale(0.1f);
		CURRENT_SELECTION = Inventory.CURRENT_WEAPON;
		CurrentGameObject = Nodes[CURRENT_SELECTION];
		PointerAngle = Utils.GetAngle(CurrentGameObject.transform.localPosition, Vector3.zero) + 90f;
		PointerRotator.transform.eulerAngles = new Vector3(0f, 0f, PointerAngle);
	}

	private void OnDisable()
	{
		GameManager.SetTimeScale(1f);
	}

	private void Update()
	{
		if (Mathf.Abs(InputManager.UI.GetHorizontalAxis()) > 0.2f || Mathf.Abs(InputManager.UI.GetVerticalAxis()) > 0.2f)
		{
			Angle = Utils.GetAngle(new Vector3(InputManager.UI.GetHorizontalAxis(), InputManager.UI.GetVerticalAxis()), Vector3.zero) + 90f;
		}
		else if (CurrentGameObject != null)
		{
			Angle = Utils.GetAngle(CurrentGameObject.transform.localPosition, Vector3.zero) + 90f;
		}
		PointerAngle += Mathf.Atan2(Mathf.Sin((Angle - PointerAngle) * ((float)Math.PI / 180f)), Mathf.Cos((Angle - PointerAngle) * ((float)Math.PI / 180f))) * 57.29578f / 3f;
		PointerRotator.transform.eulerAngles = new Vector3(0f, 0f, PointerAngle);
		float num = float.MaxValue;
		for (int i = 0; i < Nodes.Count; i++)
		{
			GameObject gameObject = Nodes[i];
			gameObject.transform.localScale = new Vector3(1f, 1f);
			float num2 = Vector2.Distance(Pointer.transform.position, gameObject.transform.position);
			if (CurrentGameObject == null)
			{
				CurrentGameObject = gameObject;
				num = num2;
				CURRENT_SELECTION = i;
			}
			if (num2 < num)
			{
				CurrentGameObject = gameObject;
				num = num2;
				CURRENT_SELECTION = i;
			}
		}
		CurrentGameObject.transform.localScale = new Vector3(1.1f, 1.1f);
		Inventory.CURRENT_WEAPON = CURRENT_SELECTION;
		text.text = Inventory.weapons[CURRENT_SELECTION].name + "\n" + Inventory.weapons[CURRENT_SELECTION].GetQuantity();
	}
}
