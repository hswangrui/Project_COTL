using System.Collections.Generic;
using Lamb.UI;
using MMBiomeGeneration;
using Rewired;
using src.Extensions;

public class RewiredGameplayInputSource : CategoryInputSource
{
	protected override int Category
	{
		get
		{
			return 0;
		}
	}

	public static int[] AllBindings
	{
		get
		{
			return new int[21]
			{
				1, 0, 64, 2, 16, 13, 71, 93, 9, 68,
				67, 66, 73, 69, 70, 59, 23, 58, 31, 17,
				26
			};
		}
	}

	private bool OverlapsWithInteractionBinding(int action)
	{
		if (base._rewiredPlayer == null)
		{
			return false;
		}
		if (MonoSingleton<Indicator>.Instance != null && Interactor.CurrentInteraction != null)
		{
			List<int> list = new List<int>();
			if (Interactor.CurrentInteraction.Interactable)
			{
				list.Add(9);
			}
			if (MonoSingleton<Indicator>.Instance.HasSecondaryInteraction)
			{
				list.Add(68);
			}
			if (MonoSingleton<Indicator>.Instance.HasThirdInteraction)
			{
				list.Add(67);
			}
			if (MonoSingleton<Indicator>.Instance.HasFourthInteraction)
			{
				list.Add(66);
			}
			if (list.Count <= 0)
			{
				return false;
			}
			return OverlapsWithBinding(action, list.ToArray());
		}
		return false;
	}

	private bool OverlapsWithBinding(int action, params int[] overlapCheck)
	{
		Controller lastActiveController = InputManager.General.GetLastActiveController();
		if (lastActiveController == null)
		{
			return false;
		}
		ControllerMap controllerMapForCategory = InputManager.General.GetControllerMapForCategory(0, lastActiveController.type);
		if (controllerMapForCategory == null)
		{
			return false;
		}
		ActionElementMap actionElementMap = controllerMapForCategory.GetActionElementMap(action, Pole.Positive);
		if (actionElementMap == null)
		{
			return false;
		}
		foreach (int action2 in overlapCheck)
		{
			ActionElementMap actionElementMap2 = controllerMapForCategory.GetActionElementMap(action2, Pole.Positive);
			if (actionElementMap2 != null && actionElementMap2.elementIdentifierId == actionElementMap.elementIdentifierId)
			{
				return true;
			}
		}
		return false;
	}

	public float GetHorizontalAxis()
	{
		return GetAxis(1);
	}

	public float GetVerticalAxis()
	{
		return GetAxis(0);
	}

	public bool GetPauseButtonDown()
	{
		return GetButtonDown(17);
	}

	public bool GetMenuButtonDown()
	{
		return GetButtonDown(26);
	}

	public bool GetMenuButtonHeld()
	{
		return GetButtonHeld(26);
	}

	public bool GetAttackButtonDown()
	{
		if (GetButtonDown(2))
		{
			if (OverlapsWithInteractionBinding(2))
			{
				Interaction currentInteraction = Interactor.CurrentInteraction;
				Interaction_EntranceShrine interaction_EntranceShrine;
				Interaction_EntrySignPost interaction_EntrySignPost;
				Interaction_Woodcutting interaction_Woodcutting;
				Interaction_Berries interaction_Berries;
				if ((object)currentInteraction != null && ((object)(interaction_EntranceShrine = currentInteraction as Interaction_EntranceShrine) != null || (object)(interaction_EntrySignPost = currentInteraction as Interaction_EntrySignPost) != null || (object)(interaction_Woodcutting = currentInteraction as Interaction_Woodcutting) != null || (object)(interaction_Berries = currentInteraction as Interaction_Berries) != null))
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool GetAttackButtonHeld()
	{
		return GetButtonHeld(2);
	}

	public bool GetAttackButtonUp()
	{
		return GetButtonUp(2);
	}

	public bool GetDodgeButtonDown()
	{
		if (OverlapsWithInteractionBinding(16))
		{
			return false;
		}
		if (GetButtonDown(16) && !MonoSingleton<UIManager>.Instance.MenusBlocked)
		{
			return true;
		}
		return false;
	}

	public bool GetDodgeButtonHeld()
	{
		if (OverlapsWithInteractionBinding(16))
		{
			return false;
		}
		return GetButtonHeld(16);
	}

	public bool GetDodgeRollButtonDown()
	{
		return GetButtonUp(16);
	}

	public bool GetCurseButtonDown()
	{
		if (OverlapsWithInteractionBinding(13))
		{
			return false;
		}
		return GetButtonDown(13);
	}

	public bool GetCurseButtonHeld()
	{
		return GetButtonHeld(13);
	}

	public bool GetCurseButtonUp()
	{
		return GetButtonUp(13);
	}

	public bool GetInteractButtonDown()
	{
		if (BiomeGenerator.Instance != null)
		{
			Controller lastActiveController = InputManager.General.GetLastActiveController();
			if (lastActiveController != null && lastActiveController.type == ControllerType.Mouse)
			{
				return false;
			}
		}
		return GetButtonDown(9);
	}

	public bool GetInteractButtonHeld()
	{
		if (BiomeGenerator.Instance != null)
		{
			Controller lastActiveController = InputManager.General.GetLastActiveController();
			if (lastActiveController != null && lastActiveController.type == ControllerType.Mouse)
			{
				return false;
			}
		}
		return GetButtonHeld(9);
	}

	public bool GetInteractButtonUp()
	{
		if (BiomeGenerator.Instance != null)
		{
			Controller lastActiveController = InputManager.General.GetLastActiveController();
			if (lastActiveController != null && lastActiveController.type == ControllerType.Mouse)
			{
				return false;
			}
		}
		return GetButtonUp(9);
	}

	public bool GetInteract2ButtonDown()
	{
		if (BiomeGenerator.Instance != null)
		{
			Controller lastActiveController = InputManager.General.GetLastActiveController();
			if (lastActiveController != null && lastActiveController.type == ControllerType.Mouse)
			{
				return false;
			}
		}
		return GetButtonDown(68);
	}

	public bool GetInteract2ButtonHeld()
	{
		return GetButtonHeld(68);
	}

	public bool GetInteract3ButtonHeld()
	{
		return GetButtonHeld(67);
	}

	public bool GetInteract3ButtonDown()
	{
		return GetButtonDown(67);
	}

	public bool GetInteract4ButtonDown()
	{
		return GetButtonDown(66);
	}

	public bool GetPlaceMoveUpgradeButtonDown()
	{
		return GetButtonDown(69);
	}

	public bool GetPlaceMoveUpgradeButtonHeld()
	{
		return GetButtonHeld(69);
	}

	public bool GetRemoveFlipButtonDown()
	{
		return GetButtonDown(70);
	}

	public bool GetRemoveFlipButtonHeld()
	{
		return GetButtonHeld(70);
	}

	public bool GetTrackQuestButtonDown()
	{
		return GetButtonDown(31);
	}

	public bool GetTrackQuestButtonHeld()
	{
		return GetButtonHeld(31);
	}

	public bool GetReturnToBaseButtonHeld()
	{
		if (GetButtonHeld(23))
		{
			return !MonoSingleton<UIManager>.Instance.MenusBlocked;
		}
		return false;
	}

	public bool GetAbilityButtonHeld()
	{
		return GetButtonHeld(23);
	}

	public bool GetMeditateButtonDown()
	{
		if (OverlapsWithInteractionBinding(59))
		{
			return false;
		}
		if (MonoSingleton<UIManager>.Instance.MenusBlocked)
		{
			return false;
		}
		if (PlayerFarming.Instance != null && PlayerFarming.Instance.state.CURRENT_STATE == StateMachine.State.CustomAnimation)
		{
			return false;
		}
		return GetButtonDown(59);
	}

	public bool GetMeditateButtonHeld()
	{
		return GetButtonHeld(59);
	}

	public bool GetAdvanceDialogueButtonDown()
	{
		return GetButtonDown(64);
	}

	public bool GetBleatButtonDown()
	{
		return GetButtonDown(58);
	}

	public bool GetBleatButtonHeld()
	{
		return GetButtonHeld(58);
	}

	public bool GetBleatButtonUp()
	{
		return GetButtonUp(58);
	}

	public bool GetRelicButtonHeld()
	{
		return GetButtonHeld(71);
	}

	public bool GetCancelFishingButtonDown()
	{
		return GetButtonDown(73);
	}

	public bool GetRelicButtonDown()
	{
		if (MonoSingleton<UIManager>.Instance.MenusBlocked)
		{
			return false;
		}
		return GetButtonDown(71);
	}

	public bool GetFleeceAbilityButtonDown()
	{
		return GetButtonDown(93);
	}

	public bool GetFleeceAbilityButtonHeld()
	{
		return GetButtonHeld(93);
	}

	public bool GetHeavyAttackButtonDown()
	{
		return GetButtonDown(94);
	}

	public bool GetHeavyAttackButtonHeld()
	{
		return GetButtonHeld(94);
	}
}
