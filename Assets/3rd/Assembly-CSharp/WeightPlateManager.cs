using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeightPlateManager : BaseMonoBehaviour
{
	public delegate void PlayerActivatingWeightPlate(List<WeightPlate> WeightPlates);

	public static PlayerActivatingWeightPlate OnPlayerActivatingWeightPlate;

	public List<WeightPlate> WeightPlates = new List<WeightPlate>();

	public float ActivateRange = 4f;

	public float DeactivateRange = 6f;

	private bool PlayerInRange;

	public UnityEvent ActivatedCallback;

	public UnityEvent DeactivatedCallback;

	public int _ActivatedCount;

	public int ActivatedCount
	{
		get
		{
			return _ActivatedCount;
		}
		set
		{
			if (_ActivatedCount != value)
			{
				if (value == WeightPlates.Count)
				{
					UnityEvent activatedCallback = ActivatedCallback;
					if (activatedCallback != null)
					{
						activatedCallback.Invoke();
					}
				}
				if (_ActivatedCount == WeightPlates.Count && value < WeightPlates.Count)
				{
					UnityEvent deactivatedCallback = DeactivatedCallback;
					if (deactivatedCallback != null)
					{
						deactivatedCallback.Invoke();
					}
				}
			}
			_ActivatedCount = value;
		}
	}

	private void GetPlates()
	{
		WeightPlates = new List<WeightPlate>(GetComponentsInChildren<WeightPlate>());
	}

	private void Start()
	{
		foreach (WeightPlate weightPlate in WeightPlates)
		{
			weightPlate.WeightPlateManager = this;
		}
	}

	private void Update()
	{
		if (PlayerFarming.Instance == null)
		{
			return;
		}
		if (!PlayerInRange && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < ActivateRange)
		{
			Debug.Log("SEND MESSAGE!");
			PlayerActivatingWeightPlate onPlayerActivatingWeightPlate = OnPlayerActivatingWeightPlate;
			if (onPlayerActivatingWeightPlate != null)
			{
				onPlayerActivatingWeightPlate(WeightPlates);
			}
			PlayerInRange = true;
		}
		if (PlayerInRange && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) > DeactivateRange)
		{
			PlayerInRange = false;
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position, ActivateRange, Color.green);
		Utils.DrawCircleXY(base.transform.position, DeactivateRange, Color.red);
	}
}
