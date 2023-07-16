using System;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using UnityEngine;

public class Interaction_FarmPlotSign : Interaction
{
	private Color FadeOut = new Color(1f, 1f, 1f, 0f);

	public SpriteRenderer RangeSprite;

	public Structure structure;

	public InventoryItemDisplay icon;

	private Vector3 _updatePos;

	private float DistanceRadius = 1f;

	private int FrameIntervalOffset;

	private int UpdateInterval = 2;

	private bool distanceChanged;

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Combine(obj.OnBrainAssigned, new Action(OnBrainAssigned));
		if (GetComponentInParent<PlacementObject>() == null)
		{
			RangeSprite.DOColor(FadeOut, 0f).SetUpdate(true);
		}
		RangeSprite.size = new Vector2(5f, 5f);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		Structure obj = structure;
		obj.OnBrainAssigned = (Action)Delegate.Remove(obj.OnBrainAssigned, new Action(OnBrainAssigned));
	}

	private void OnBrainAssigned()
	{
		if (structure.Structure_Info.SignPostItem != 0)
		{
			icon.SetImage(structure.Structure_Info.SignPostItem, false);
		}
	}

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.SetIcon;
	}

	protected override void Update()
	{
		base.Update();
		if ((Time.frameCount + FrameIntervalOffset) % UpdateInterval == 0 && !(PlayerFarming.Instance == null))
		{
			if (!GameManager.overridePlayerPosition)
			{
				_updatePos = PlayerFarming.Instance.transform.position;
				DistanceRadius = 1f;
			}
			else
			{
				_updatePos = PlacementRegion.Instance.PlacementPosition;
				DistanceRadius = 5f;
			}
			if (Vector3.Distance(_updatePos, base.transform.position) < DistanceRadius)
			{
				RangeSprite.gameObject.SetActive(true);
				RangeSprite.DOKill();
				RangeSprite.DOColor(StaticColors.OffWhiteColor, 0.5f).SetUpdate(true);
				distanceChanged = true;
			}
			else if (distanceChanged)
			{
				RangeSprite.DOKill();
				RangeSprite.DOColor(FadeOut, 0.5f).SetUpdate(true);
				distanceChanged = false;
			}
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().AddPlayerToCamera();
		CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
		cameraFollowTarget.SetOffset(new Vector3(0f, 2.5f, 2f));
		cameraFollowTarget.AddTarget(base.gameObject, 1f);
		state.CURRENT_STATE = StateMachine.State.CustomAction0;
		UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(InventoryItem.AllPlantables, new ItemSelector.Params
		{
			Key = "farm_plot",
			Context = ItemSelector.Context.SetLabel,
			Offset = new Vector2(0f, 150f),
			ShowEmpty = true,
			RequiresDiscovery = false,
			HideOnSelection = true,
			HideQuantity = true
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController = itemSelector;
		uIItemSelectorOverlayController.OnItemChosen = (Action<InventoryItem.ITEM_TYPE>)Delegate.Combine(uIItemSelectorOverlayController.OnItemChosen, (Action<InventoryItem.ITEM_TYPE>)delegate(InventoryItem.ITEM_TYPE chosenItem)
		{
			icon.SetImage(chosenItem, false);
			structure.Structure_Info.SignPostItem = chosenItem;
		});
		UIItemSelectorOverlayController uIItemSelectorOverlayController2 = itemSelector;
		uIItemSelectorOverlayController2.OnHidden = (Action)Delegate.Combine(uIItemSelectorOverlayController2.OnHidden, (Action)delegate
		{
			state.CURRENT_STATE = StateMachine.State.Idle;
			itemSelector = null;
			Interactable = true;
			base.HasChanged = true;
			GameManager.GetInstance().OnConversationEnd();
			cameraFollowTarget.RemoveTarget(base.gameObject);
			cameraFollowTarget.SetOffset(Vector2.zero);
		});
	}
}
