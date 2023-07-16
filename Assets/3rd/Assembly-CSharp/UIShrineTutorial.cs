using System;
using TMPro;
using UnityEngine;

public class UIShrineTutorial : BaseMonoBehaviour
{
	public TextMeshProUGUI Text;

	private Canvas canvas;

	public Vector3 Offset = new Vector3(0f, 0f, -2f);

	private float Delay;

	public AnimationCurve bounceCurve;

	private Interaction_PlacementRegion structure;

	private Vector2 Scale;

	private Vector2 ScaleSpeed;

	private int Stone;

	private int Logs;

	private int CurrentStone;

	private int CurrentLog;

	private string sStone;

	private string sLog;

	private bool Activated;

	public void UpdateText(string String, Vector3 Position)
	{
		Text.text = String;
		base.transform.position = Camera.main.WorldToScreenPoint(Position + (Offset + new Vector3(0f, 0f, 0.25f * bounceCurve.Evaluate(Time.time * 0.5f % 1f)) * canvas.scaleFactor));
	}

	private void OnEnable()
	{
		Scale = (ScaleSpeed = Vector2.zero);
		base.transform.localScale = Scale;
		canvas = GetComponentInParent<Canvas>();
		Delay = 0.3f;
		structure = UnityEngine.Object.FindObjectOfType<Interaction_PlacementRegion>();
		foreach (StructuresData.ItemCost item in StructuresData.GetCost(StructureBrain.TYPES.COOKING_FIRE))
		{
			if (item.CostItem == InventoryItem.ITEM_TYPE.LOG)
			{
				Logs = item.CostValue;
			}
			if (item.CostItem == InventoryItem.ITEM_TYPE.STONE)
			{
				Stone = item.CostValue;
			}
		}
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Combine(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(Close));
		base.transform.SetAsFirstSibling();
	}

	private void OnDisable()
	{
		StructureManager.OnStructureAdded = (StructureManager.StructureChanged)Delegate.Remove(StructureManager.OnStructureAdded, new StructureManager.StructureChanged(Close));
	}

	private void Close(StructuresData structure)
	{
		Debug.Log(string.Concat(structure.Type, "  ", structure.ToBuildType));
		if (structure.Type == StructureBrain.TYPES.BUILD_SITE && structure.ToBuildType == StructureBrain.TYPES.COOKING_FIRE)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void LateUpdate()
	{
		if (structure == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		CurrentStone = Inventory.GetItemQuantity(2);
		sStone = "<sprite name=\"icon_stone\"> " + ((CurrentStone < Stone) ? "<color=red>" : "") + Inventory.GetItemQuantity(2) + " " + ((CurrentStone < Stone) ? "</color>" : "") + "/ " + Stone.ToString();
		CurrentLog = Inventory.GetItemQuantity(1);
		sLog = "<sprite name=\"icon_wood\"> " + ((CurrentLog < Logs) ? "<color=red>" : "") + Inventory.GetItemQuantity(1) + " " + ((CurrentLog < Logs) ? "</color>" : "") + "/ " + Logs.ToString();
		UpdateText(sLog + "\n " + sStone, structure.transform.position);
		if (!Activated)
		{
			if (PlayerFarming.Instance != null && Vector3.Distance(structure.transform.position, PlayerFarming.Instance.transform.position) < 8f)
			{
				Activated = true;
			}
		}
		else if (Time.timeScale <= 0f || LetterBox.IsPlaying || HUDManager.isHiding)
		{
			Scale = (ScaleSpeed = Vector2.zero);
			base.transform.localScale = Scale;
			Delay = 0.5f;
		}
		else
		{
			Delay -= Time.deltaTime;
			float num = 0f;
		}
	}

	private void FixedUpdate()
	{
		if (Delay <= 0f)
		{
			ScaleSpeed.x += (1f - Scale.x) * 0.5f;
			Scale.x += (ScaleSpeed.x *= 0.6f);
			ScaleSpeed.y += (1f - Scale.y) * 0.4f;
			Scale.y += (ScaleSpeed.y *= 0.5f);
			base.transform.localScale = Scale;
		}
	}
}
