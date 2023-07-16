using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using MMBiomeGeneration;
using UnityEngine;

public class Interactor : BaseMonoBehaviour
{
	public static Interaction CurrentInteraction;

	public static Interaction PreviousInteraction;

	private float CurrentDist;

	private float CurrentPriorityWeight;

	private float TestDist;

	private PlayerFarming player;

	private Indicator Indicator;

	private RectTransform IndicatorRT;

	private StateMachine state;

	private float Delay;

	private int defaultButton = 9;

	private int secondaryButton = 68;

	private int tertiaryButton = 67;

	private int quaternaryButton = 66;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private Vector2 _axes;

	public static bool UseRegions = false;

	public static Dictionary<Vector3Int, List<Interaction>> InteractionRegions = new Dictionary<Vector3Int, List<Interaction>>();

	public const int RegionSize = 3;

	private void Start()
	{
		state = base.gameObject.GetComponent<StateMachine>();
		player = base.gameObject.GetComponent<PlayerFarming>();
		StartCoroutine(WaitForIndicator());
		FrameIntervalOffset = Random.Range(0, UpdateInterval);
	}

	private IEnumerator WaitForIndicator()
	{
		while (Indicator == null)
		{
			Indicator = MonoSingleton<Indicator>.Instance;
			yield return null;
		}
		IndicatorRT = Indicator.RectTransform;
		ResetPrompts();
	}

	private void ResetPrompts()
	{
		Indicator.primaryControlPrompt.Category = 0;
		Indicator.primaryControlPrompt.Action = defaultButton;
		Indicator.secondaryControlPrompt.Category = 0;
		Indicator.secondaryControlPrompt.Action = secondaryButton;
		Indicator.thirdControlPrompt.Category = 0;
		Indicator.thirdControlPrompt.Action = tertiaryButton;
		Indicator.fourthControlPrompt.Category = 0;
		Indicator.fourthControlPrompt.Action = quaternaryButton;
	}

	private void OnEnable()
	{
		if (state == null)
		{
			state = base.gameObject.GetComponent<StateMachine>();
		}
		if (player == null)
		{
			player = base.gameObject.GetComponent<PlayerFarming>();
		}
	}

	private void Update()
	{
		if (player != null && player.PickedUpFollower != null)
		{
			return;
		}
		Indicator.primaryControlPrompt.PrioritizeMouse = BiomeGenerator.Instance == null;
		Indicator.secondaryControlPrompt.PrioritizeMouse = BiomeGenerator.Instance == null;
		Indicator.thirdControlPrompt.PrioritizeMouse = BiomeGenerator.Instance == null;
		Indicator.fourthControlPrompt.PrioritizeMouse = BiomeGenerator.Instance == null;
		if (PlacementObject.Instance != null)
		{
			Indicator.Reset();
			Indicator.gameObject.SetActive(true);
			Indicator.Interactable = true;
			Indicator.HideTopInfo();
			Indicator.primaryControlPrompt.Category = 0;
			Indicator.primaryControlPrompt.Action = 69;
			Indicator.secondaryControlPrompt.Category = 0;
			Indicator.secondaryControlPrompt.Action = 70;
			Indicator.thirdControlPrompt.Category = 0;
			Indicator.thirdControlPrompt.Action = 67;
			Indicator.HasFourthInteraction = true;
			Indicator.fourthControlPrompt.Category = 1;
			Indicator.fourthControlPrompt.Action = 39;
			Indicator.Fourthtext.text = ScriptLocalization.Interactions.Cancel;
			_axes.x = InputManager.Gameplay.GetHorizontalAxis();
			_axes.y = InputManager.Gameplay.GetVerticalAxis();
			Indicator.primaryControlPrompt.PrioritizeMouse = _axes.magnitude <= 0f && InputManager.General.MouseInputActive;
			Indicator.secondaryControlPrompt.PrioritizeMouse = _axes.magnitude <= 0f && InputManager.General.MouseInputActive;
			Indicator.thirdControlPrompt.PrioritizeMouse = _axes.magnitude <= 0f && InputManager.General.MouseInputActive;
			Indicator.fourthControlPrompt.PrioritizeMouse = _axes.magnitude <= 0f && InputManager.General.MouseInputActive;
			if (PlacementObject.Instance.StructureType == StructureBrain.TYPES.EDIT_BUILDINGS)
			{
				Structure hoveredStructure = PlacementRegion.Instance.GetHoveredStructure();
				Indicator.HasSecondaryInteraction = true;
				Indicator.HasFourthInteraction = true;
				Indicator.HasThirdInteraction = true;
				if (hoveredStructure != null && (hoveredStructure.Brain.Data.CanBeMoved || hoveredStructure.Brain.Data.IsDeletable))
				{
					StructureBrain.TYPES tYPES = hoveredStructure.Type;
					if (tYPES == StructureBrain.TYPES.TEMPLE_II || tYPES == StructureBrain.TYPES.TEMPLE_III || tYPES == StructureBrain.TYPES.TEMPLE_IV)
					{
						tYPES = StructureBrain.TYPES.TEMPLE;
					}
					Indicator.text.text = (hoveredStructure.Brain.Data.CanBeMoved ? (ScriptLocalization.Interactions.MoveStructure + " <color=yellow>" + StructuresData.LocalizedName(tYPES) + "</color>") : "");
					Indicator.SecondaryText.text = (hoveredStructure.Brain.Data.IsDeletable ? (ScriptLocalization.Interactions.Remove + " <color=yellow>" + StructuresData.LocalizedName(tYPES) + "</color>") : "");
					if (PlacementRegion.Instance.GetPathAtPosition() != 0)
					{
						Indicator.Thirdtext.text = ScriptLocalization.Interactions.Remove + " <color=yellow>" + StructuresData.LocalizedName(PlacementRegion.Instance.GetPathAtPosition()) + "</color>";
					}
					else
					{
						Indicator.Thirdtext.text = string.Empty;
					}
				}
				else if (PlacementRegion.Instance.GetPathAtPosition() != 0)
				{
					Indicator.SecondaryText.text = "";
					Indicator.Thirdtext.text = ScriptLocalization.Interactions.Remove + " <color=yellow>" + StructuresData.LocalizedName(PlacementRegion.Instance.GetPathAtPosition()) + "</color>";
				}
				else
				{
					Indicator.text.text = "";
					Indicator.SecondaryText.text = "";
					Indicator.Thirdtext.text = "";
				}
				return;
			}
			if (StructureBrain.IsPath(PlacementObject.Instance.StructureType))
			{
				if (PlacementRegion.Instance.GetPathAtPosition() != 0)
				{
					Indicator.HasSecondaryInteraction = true;
					Indicator.SecondaryText.text = ScriptLocalization.Interactions.Remove + " <color=yellow>" + StructuresData.LocalizedName(PlacementRegion.Instance.GetPathAtPosition()) + "</color>";
				}
				else
				{
					Indicator.HasSecondaryInteraction = false;
					Indicator.SecondaryText.text = string.Empty;
				}
			}
			else
			{
				Indicator.SecondaryInteractable = StructuresData.CanBeFlipped(PlacementObject.Instance.StructureType);
				Indicator.SecondaryText.text = (StructuresData.CanBeFlipped(PlacementObject.Instance.StructureType) ? ScriptLocalization.Interactions.Flip : "");
				if (PlacementObject.Instance.StructureType == StructureBrain.TYPES.SLEEPING_BAG || PlacementObject.Instance.StructureType == StructureBrain.TYPES.BED || PlacementObject.Instance.StructureType == StructureBrain.TYPES.BED_2 || PlacementObject.Instance.StructureType == StructureBrain.TYPES.BED_3 || PlacementObject.Instance.StructureType == StructureBrain.TYPES.SHARED_HOUSE)
				{
					Indicator.ShowTopInfo("<sprite name=\"icon_House\">" + StructureManager.GetTotalHomesCount(true) + " <sprite name=\"icon_Followers\">" + DataManager.Instance.Followers.Count);
				}
			}
			Indicator.HasSecondaryInteraction = true;
			if (PlacementRegion.Instance.CurrentMode == PlacementRegion.Mode.Upgrading && PlacementObject.Instance.StructureType != StructureBrain.TYPES.SHRINE)
			{
				string upgradeBuilding = ScriptLocalization.Interactions.UpgradeBuilding;
				upgradeBuilding = upgradeBuilding.Replace("{0}", StructuresData.LocalizedName(PlacementObject.Instance.StructureType));
				Indicator.text.text = upgradeBuilding;
				return;
			}
			StructureBrain.TYPES tYPES2 = PlacementRegion.Instance.StructureType;
			if (tYPES2 == StructureBrain.TYPES.TEMPLE_II || tYPES2 == StructureBrain.TYPES.TEMPLE_III || tYPES2 == StructureBrain.TYPES.TEMPLE_IV)
			{
				tYPES2 = StructureBrain.TYPES.TEMPLE;
			}
			Indicator.text.text = ScriptLocalization.Interactions.PlaceBuilding + " <color=yellow>" + StructuresData.LocalizedName(tYPES2);
			return;
		}
		ResetPrompts();
		if (state.CURRENT_STATE == StateMachine.State.Idle_CarryingBody || state.CURRENT_STATE == StateMachine.State.Moving_CarryingBody)
		{
			Indicator.gameObject.SetActive(true);
			Indicator.Interactable = true;
			Indicator.HasSecondaryInteraction = false;
			Indicator.SecondaryText.text = "";
			if (PlayerFarming.Instance.NearGrave)
			{
				if (PlayerFarming.Instance.NearStructure.IsFull)
				{
					Indicator.text.text = ScriptLocalization.Interactions.Full;
					Indicator.Interactable = false;
				}
				else if (PlayerFarming.Instance.CarryingDeadFollowerID == -1)
				{
					Indicator.text.text = LocalizationManager.GetTranslation("Interactions/Bury");
				}
				else
				{
					Indicator.text.text = ScriptLocalization.Interactions.BuryBody;
				}
			}
			else if (PlayerFarming.Instance.NearCompostBody)
			{
				Indicator.text.text = ScriptLocalization.Interactions.CompostBody;
			}
			else
			{
				Indicator.text.text = ScriptLocalization.Interactions.Drop;
			}
			Indicator.Reset();
		}
		else if (state != null && player != null && !IsInStateToInteract())
		{
			if (Indicator != null && Indicator.gameObject != null && Indicator.gameObject.activeSelf)
			{
				Indicator.Deactivate();
			}
		}
		else
		{
			if (Time.timeScale == 0f)
			{
				return;
			}
			if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval == 0)
			{
				GetClosestInteraction();
			}
			bool flag = PreviousInteraction != CurrentInteraction || (CurrentInteraction != null && CurrentInteraction.HasChanged) || PlayerFollowerSelection.IsPlaying;
			if (PlayerFollowerSelection.IsPlaying)
			{
				CurrentInteraction = null;
			}
			if (flag)
			{
				if (PreviousInteraction != null)
				{
					PreviousInteraction.OnBecomeNotCurrent();
				}
				if (CurrentInteraction != null)
				{
					CurrentInteraction.OnBecomeCurrent();
				}
				Indicator.Reset();
			}
			if (PlayerFollowerSelection.IsPlaying)
			{
				Indicator.gameObject.SetActive(true);
				Indicator.Interactable = false;
				Indicator.HasSecondaryInteraction = false;
				Indicator.SecondaryText.text = "";
				Indicator.HasThirdInteraction = false;
				Indicator.Thirdtext.text = "";
				Indicator.HasFourthInteraction = false;
				Indicator.Fourthtext.text = "";
				Indicator.text.text = ((PlayerFollowerSelection.Instance.SelectedFollowers.Count > 0) ? "Release to issue your <color=red>Command</color>" : "Select <color=yellow>Followers</color> to Command");
				Indicator.Reset();
				return;
			}
			if (CurrentInteraction == null)
			{
				if (PreviousInteraction != null)
				{
					PreviousInteraction.OnHoldProgressStop();
				}
				PreviousInteraction = null;
				if (Indicator.gameObject.activeSelf)
				{
					Indicator.Deactivate();
				}
				return;
			}
			if (CurrentInteraction == PreviousInteraction && !Indicator.gameObject.activeSelf)
			{
				Indicator.gameObject.SetActive(true);
			}
			Vector3 position = ((CurrentInteraction.LockPosition == null) ? (CurrentInteraction.transform.position + CurrentInteraction.Offset) : CurrentInteraction.LockPosition.transform.position);
			Indicator.SetPosition(position);
			if (CurrentInteraction.ContinuouslyHold)
			{
				Indicator.text.text = CurrentInteraction.Label;
				if (string.IsNullOrWhiteSpace(Indicator.text.text) && string.IsNullOrWhiteSpace(CurrentInteraction.SecondaryLabel) && string.IsNullOrWhiteSpace(CurrentInteraction.ThirdLabel) && string.IsNullOrWhiteSpace(CurrentInteraction.FourthLabel))
				{
					Indicator.ContainerImage.enabled = false;
				}
				Indicator.Interactable = CurrentInteraction.Interactable;
				Indicator.HasSecondaryInteraction = CurrentInteraction.HasSecondaryInteraction;
				Indicator.SecondaryText.text = CurrentInteraction.SecondaryLabel;
				Indicator.HasThirdInteraction = CurrentInteraction.HasThirdInteraction;
				Indicator.Thirdtext.text = CurrentInteraction.ThirdLabel;
				Indicator.HasFourthInteraction = CurrentInteraction.HasFourthInteraction;
				Indicator.Fourthtext.text = CurrentInteraction.FourthLabel;
				Indicator.Reset();
			}
			if (flag)
			{
				if (PreviousInteraction != null)
				{
					PreviousInteraction.OnHoldProgressStop();
				}
				Indicator.gameObject.SetActive(false);
				Indicator.HoldToInteract = CurrentInteraction.HoldToInteract && SettingsManager.Settings.Accessibility.HoldActions;
				Indicator.Interactable = CurrentInteraction.Interactable;
				Indicator.HasSecondaryInteraction = CurrentInteraction.HasSecondaryInteraction;
				Indicator.SecondaryInteractable = CurrentInteraction.SecondaryInteractable;
				Indicator.HasThirdInteraction = CurrentInteraction.HasThirdInteraction;
				Indicator.ThirdInteractable = CurrentInteraction.ThirdInteractable;
				Indicator.HasFourthInteraction = CurrentInteraction.HasFourthInteraction;
				Indicator.FourthInteractable = CurrentInteraction.FourthInteractable;
				Indicator.text.text = CurrentInteraction.Label;
				Indicator.gameObject.SetActive(true);
				if (string.IsNullOrWhiteSpace(Indicator.text.text) && string.IsNullOrWhiteSpace(CurrentInteraction.SecondaryLabel) && string.IsNullOrWhiteSpace(CurrentInteraction.ThirdLabel) && string.IsNullOrWhiteSpace(CurrentInteraction.FourthLabel))
				{
					Indicator.ContainerImage.enabled = false;
				}
				Indicator.SecondaryText.text = CurrentInteraction.SecondaryLabel;
				Indicator.Thirdtext.text = CurrentInteraction.ThirdLabel;
				Indicator.Fourthtext.text = CurrentInteraction.FourthLabel;
				PreviousInteraction = CurrentInteraction;
				Indicator.Reset();
			}
			if (CurrentInteraction.Interactable)
			{
				if (CurrentInteraction.HoldToInteract && SettingsManager.Settings.Accessibility.HoldActions)
				{
					if (InputManager.Gameplay.GetInteractButtonDown())
					{
						CurrentInteraction.OnHoldProgressDown();
					}
					if (InputManager.Gameplay.GetInteractButtonUp())
					{
						CurrentInteraction.OnHoldProgressRelease();
					}
					if (InputManager.Gameplay.GetInteractButtonHeld())
					{
						CurrentInteraction.HoldProgress += 1f * Time.deltaTime;
						CurrentInteraction.HoldBegun = true;
						Indicator.Progress = CurrentInteraction.HoldProgress / 1f;
						if (CurrentInteraction.HoldProgress >= 1f)
						{
							CurrentInteraction.OnHoldProgressStop();
							CurrentInteraction.OnInteract(state);
							Delay = 0.25f;
						}
					}
					else
					{
						if (!CurrentInteraction.HoldBegun && CurrentInteraction.HoldProgress > 0f)
						{
							CurrentInteraction.HoldProgress -= 0.5f * Time.deltaTime;
							Indicator.Progress = CurrentInteraction.HoldProgress / 1f;
						}
						else if (CurrentInteraction.HoldBegun && CurrentInteraction.HoldProgress < 0.2f)
						{
							CurrentInteraction.HoldProgress += 1f * Time.deltaTime;
							Indicator.Progress = CurrentInteraction.HoldProgress / 1f;
							if (CurrentInteraction.HoldProgress >= 0.2f)
							{
								CurrentInteraction.HoldBegun = false;
							}
						}
						if (CurrentInteraction.HoldProgress >= 0.2f)
						{
							CurrentInteraction.HoldBegun = false;
						}
					}
				}
				else if (CurrentInteraction.AutomaticallyInteract)
				{
					CurrentInteraction.OnInteract(state);
					HideIndicator();
					Delay = 0.25f;
				}
				else if (InputManager.Gameplay.GetInteractButtonDown() && IsInStateToInteract())
				{
					if (!CurrentInteraction.ContinuouslyHold)
					{
						StartCoroutine(WaitForEndOfFrameAndInteract());
						Delay = 0.25f;
					}
					else
					{
						CurrentInteraction.OnInteract(state);
						Indicator.text.text = CurrentInteraction.Label;
					}
				}
			}
			if (CurrentInteraction.HasSecondaryInteraction && CurrentInteraction.SecondaryInteractable && InputManager.Gameplay.GetInteract2ButtonDown() && IsInStateToInteract())
			{
				CurrentInteraction.OnSecondaryInteract(state);
				Delay = 0.25f;
			}
			if (CurrentInteraction.HasThirdInteraction && CurrentInteraction.ThirdInteractable && InputManager.Gameplay.GetInteract3ButtonDown() && IsInStateToInteract())
			{
				CurrentInteraction.OnThirdInteract(state);
				Delay = 0.25f;
			}
			if (CurrentInteraction.HasFourthInteraction && CurrentInteraction.FourthInteractable && InputManager.Gameplay.GetInteract4ButtonDown() && IsInStateToInteract())
			{
				CurrentInteraction.OnFourthInteract(state);
				Delay = 0.25f;
			}
		}
	}

	private bool IsInStateToInteract()
	{
		if (state.CURRENT_STATE != StateMachine.State.InActive && state.CURRENT_STATE != StateMachine.State.CustomAnimation && state.CURRENT_STATE != StateMachine.State.Heal && state.CURRENT_STATE != StateMachine.State.Meditate && state.CURRENT_STATE != StateMachine.State.Map && state.CURRENT_STATE != StateMachine.State.Teleporting && state.CURRENT_STATE != StateMachine.State.Grapple && state.CURRENT_STATE != StateMachine.State.CustomAction0 && !LetterBox.IsPlaying && state.CURRENT_STATE != StateMachine.State.TimedAction && state.CURRENT_STATE != StateMachine.State.Idle_CarryingBody && state.CURRENT_STATE != StateMachine.State.Moving_CarryingBody && (!player || !player.GoToAndStopping) && state.CURRENT_STATE != StateMachine.State.Aiming)
		{
			return state.CURRENT_STATE != StateMachine.State.Casting;
		}
		return false;
	}

	private IEnumerator WaitForEndOfFrameAndInteract()
	{
		yield return new WaitForEndOfFrame();
		CurrentInteraction.OnInteract(state);
	}

	private IEnumerator HideAndRevealIndicator()
	{
		HideIndicator();
		yield return new WaitForSeconds(0.3f);
		if (CurrentInteraction != null && !string.IsNullOrWhiteSpace(CurrentInteraction.Label))
		{
			PreviousInteraction = null;
		}
	}

	public void HideIndicator()
	{
		if (Indicator.gameObject.activeSelf)
		{
			Indicator.Deactivate();
		}
	}

	private void GetClosestInteraction()
	{
		if (InputManager.Gameplay.GetInteractButtonDown())
		{
			return;
		}
		CurrentInteraction = null;
		CurrentDist = 2.1474836E+09f;
		CurrentPriorityWeight = 0f;
		Interaction interaction = null;
		if (UseRegions)
		{
			Vector3 position = PlayerFarming.Instance.transform.position;
			if (!InteractionRegions.ContainsKey(PositionToRegions(position)))
			{
				return;
			}
			foreach (Interaction item in InteractionRegions[PositionToRegions(position)])
			{
				if (!item.gameObject.activeInHierarchy)
				{
					continue;
				}
				TestDist = Vector3.Distance(item.gameObject.transform.position + item.ActivatorOffset, base.gameObject.transform.position);
				if (TestDist < item.ActivateDistance)
				{
					if (interaction == null || item.PriorityWeight >= interaction.PriorityWeight)
					{
						interaction = item;
					}
					if (TestDist < CurrentDist && (!string.IsNullOrWhiteSpace(item.Label) || !string.IsNullOrWhiteSpace(item.SecondaryLabel)))
					{
						CurrentInteraction = item;
						CurrentDist = TestDist;
						CurrentPriorityWeight = item.PriorityWeight;
					}
				}
			}
			if (CurrentInteraction != null)
			{
				if (interaction != null && interaction.PriorityWeight > CurrentInteraction.PriorityWeight)
				{
					CurrentInteraction = interaction;
				}
				if (CurrentInteraction is interaction_FollowerInteraction && PlayerFarming.Instance.playerController.speed <= 0f && PreviousInteraction != null)
				{
					CurrentInteraction = PreviousInteraction;
				}
			}
			return;
		}
		foreach (Interaction interaction2 in Interaction.interactions)
		{
			if (!(interaction2 != null) || !interaction2.gameObject.activeInHierarchy)
			{
				continue;
			}
			TestDist = Vector3.Distance(interaction2.gameObject.transform.position + interaction2.ActivatorOffset, base.gameObject.transform.position);
			if (TestDist < interaction2.ActivateDistance && (!string.IsNullOrWhiteSpace(interaction2.Label) || !string.IsNullOrWhiteSpace(interaction2.SecondaryLabel) || !string.IsNullOrWhiteSpace(interaction2.ThirdLabel)))
			{
				if (interaction == null || interaction2.PriorityWeight >= interaction.PriorityWeight)
				{
					interaction = interaction2;
				}
				if (TestDist < CurrentDist)
				{
					CurrentInteraction = interaction2;
					CurrentDist = TestDist;
					CurrentPriorityWeight = interaction2.PriorityWeight;
				}
			}
		}
		if (CurrentInteraction != null)
		{
			if (interaction != null && interaction.PriorityWeight > CurrentInteraction.PriorityWeight)
			{
				CurrentInteraction = interaction;
			}
			if (CurrentInteraction is interaction_FollowerInteraction && PlayerFarming.Instance.playerController.speed <= 0f && PreviousInteraction != null)
			{
				CurrentInteraction = PreviousInteraction;
			}
		}
	}

	public static void AddToRegion(Interaction l)
	{
		Vector3Int key = PositionToRegions(l.Position);
		if (!InteractionRegions.ContainsKey(key))
		{
			InteractionRegions.Add(key, new List<Interaction> { l });
		}
		else
		{
			InteractionRegions[key].Add(l);
		}
	}

	public static void RemoveFromRegion(Interaction l)
	{
		Vector3Int key = PositionToRegions(l.Position);
		if (InteractionRegions.ContainsKey(key))
		{
			InteractionRegions[key].Remove(l);
		}
	}

	private static Vector3Int PositionToRegions(Vector3 Position)
	{
		return Vector3Int.FloorToInt(Position) / 3;
	}
}
