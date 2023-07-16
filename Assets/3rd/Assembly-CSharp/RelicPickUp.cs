using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using Lamb.UI;
using src.Extensions;
using src.UI.Prompts;
using UnityEngine;

public class RelicPickUp : Interaction
{
	public static List<RelicPickUp> RelicPickUps = new List<RelicPickUp>();

	[SerializeField]
	private SpriteRenderer icon;

	private RelicData relicData;

	private UIRelicPickupPromptController _relicPickupUI;

	public RelicData RelicData
	{
		get
		{
			return relicData;
		}
	}

	private void Start()
	{
		ConfigureRandomise();
		RelicPickUps.Add(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RelicPickUps.Remove(this);
	}

	public void Configure(RelicData relicData)
	{
		this.relicData = relicData;
		icon.sprite = EquipmentManager.GetRelicIcon(relicData.RelicType);
		icon.transform.localScale /= 2f;
		base.transform.position += Vector3.forward * -1f;
		base.transform.localScale = Vector3.zero;
		base.transform.DOScale(1f, 0.25f);
		if (relicData.RelicType.ToString().Contains("Blessed") && DataManager.Instance.ForceBlessedRelic)
		{
			DataManager.Instance.ForceBlessedRelic = false;
		}
		else if (relicData.RelicType.ToString().Contains("Dammed") && DataManager.Instance.ForceDammedRelic)
		{
			DataManager.Instance.ForceDammedRelic = false;
		}
		if (!DataManager.Instance.SpawnedRelicsThisRun.Contains(relicData.RelicType))
		{
			DataManager.Instance.SpawnedRelicsThisRun.Add(relicData.RelicType);
		}
	}

	public void ConfigureRandomise()
	{
		if (!(relicData != null))
		{
			Configure(EquipmentManager.GetRandomRelicData(false));
		}
	}

	public override void GetLabel()
	{
		base.Label = ScriptLocalization.Interactions.PickUp + " <color=#FFD201>" + RelicData.GetTitleLocalisation(relicData.RelicType);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		PlayerFarming.Instance.playerRelic.EquipRelic(relicData, true, true);
		MonoSingleton<Indicator>.Instance.HideTopInfo();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void IndicateHighlighted()
	{
		base.IndicateHighlighted();
		if (_relicPickupUI == null)
		{
			_relicPickupUI = MonoSingleton<UIManager>.Instance.RelicPickupPromptControllerTemplate.Instantiate();
			UIRelicPickupPromptController relicPickupUI = _relicPickupUI;
			relicPickupUI.OnHidden = (Action)Delegate.Combine(relicPickupUI.OnHidden, (Action)delegate
			{
				_relicPickupUI = null;
			});
		}
		_relicPickupUI.Show(relicData, PlayerFarming.Instance.playerRelic.CurrentRelic);
	}

	public override void EndIndicateHighlighted()
	{
		base.EndIndicateHighlighted();
		if (_relicPickupUI != null)
		{
			_relicPickupUI.Hide();
		}
	}
}
