using System.Collections.Generic;
using UnityEngine;

public class WeightPlate : BaseMonoBehaviour
{
	public bool Reserved;

	public WeightPlateManager WeightPlateManager;

	public SpriteRenderer DoorWeightSignalColor;

	public SpriteRenderer WeightColor;

	private List<Collider2D> CurrentCollisions = new List<Collider2D>();

	public void OnTriggerEnter2D(Collider2D other)
	{
		if (CurrentCollisions.Count <= 0)
		{
			WeightPlateManager weightPlateManager = WeightPlateManager;
			int activatedCount = weightPlateManager.ActivatedCount + 1;
			weightPlateManager.ActivatedCount = activatedCount;
			DoorWeightSignalColor.color = Color.green;
			WeightColor.color = Color.green;
		}
		CurrentCollisions.Add(other);
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		CurrentCollisions.Remove(other);
		if (CurrentCollisions.Count <= 0)
		{
			WeightPlateManager weightPlateManager = WeightPlateManager;
			int activatedCount = weightPlateManager.ActivatedCount - 1;
			weightPlateManager.ActivatedCount = activatedCount;
			DoorWeightSignalColor.color = Color.red;
			WeightColor.color = Color.red;
		}
	}
}
