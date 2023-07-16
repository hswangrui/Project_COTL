using System.Collections;
using FMOD.Studio;
using I2.Loc;
using UnityEngine;

public class Interaction_Cook : Interaction
{
	public Interaction_Cauldron cauldron;

	public Structure structure;

	public float CookingDuration = 0.5f;

	[HideInInspector]
	public string sCook;

	[HideInInspector]
	public string sCancel;

	private Coroutine cookingRoutine;

	[HideInInspector]
	public EventInstance loopingSoundInstance;

	private void Start()
	{
		HoldToInteract = true;
		UpdateLocalisation();
		HasSecondaryInteraction = true;
	}

	public override void GetLabel()
	{
		base.Label = sCook;
		Interactable = true;
	}

	public override void GetSecondaryLabel()
	{
		SecondaryInteractable = true;
		base.SecondaryLabel = ((cauldron is Interaction_Kitchen && structure.Structure_Info.QueuedMeals.Count < 5) ? LocalizationManager.GetTranslation("UI/Queue") : "");
	}

	public override void OnSecondaryInteract(StateMachine state)
	{
		if (cauldron is Interaction_Kitchen && structure.Structure_Info.QueuedMeals.Count < 5)
		{
			for (int num = structure.Structure_Info.Inventory.Count - 1; num >= 0; num--)
			{
			}
			cauldron.enabled = true;
			base.enabled = false;
		}
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sCook = ScriptLocalization.Interactions.Cook;
		sCancel = ScriptLocalization.Interactions.Cancel;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		if (cookingRoutine == null)
		{
			cookingRoutine = StartCoroutine(CookFood());
		}
	}

	public void DoCook()
	{
		StartCoroutine(CookFood());
	}

	public override void OnDisableInteraction()
	{
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	public virtual IEnumerator CookFood()
	{
		CookingData.CookedMeal(InventoryItem.ITEM_TYPE.NONE);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/cooking/cooking_loop", base.gameObject, true);
		yield return new WaitForEndOfFrame();
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		state.facingAngle = Utils.GetAngle(state.transform.position, base.transform.position);
		CameraManager.instance.ShakeCameraForDuration(0.1f, 0.2f, 0.5f);
		if (CookingDuration > 0.5f)
		{
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		}
		yield return new WaitForSeconds(CookingDuration);
		AudioManager.Instance.PlayOneShot("event:/cooking/meal_cooked", base.transform.position);
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		structure.Structure_Info.Inventory.Clear();
		structure.Structure_Info.Fuel -= 2;
		cauldron.enabled = true;
		base.enabled = false;
		DataManager.Instance.MealsCooked++;
		ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
		Debug.Log("BBB");
		if (!DataManager.Instance.CookedFirstFood)
		{
			DataManager.Instance.CookedFirstFood = true;
		}
		foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
		{
			if (!FollowerManager.FollowerLocked(allBrain.Info.ID, true))
			{
				allBrain.CheckChangeTask();
			}
		}
		ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CookFirstMeal);
		cookingRoutine = null;
	}
}
