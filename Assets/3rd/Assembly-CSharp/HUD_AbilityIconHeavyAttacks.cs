using System;
using DG.Tweening;
using Lamb.UI;
using MMBiomeGeneration;
using UnityEngine;

public class HUD_AbilityIconHeavyAttacks : MonoBehaviour
{
	public Transform Container;

	private void OnEnable()
	{
		Container.gameObject.SetActive(false);
		EnemyHasShield.OnTutorialShown = (Action)Delegate.Combine(EnemyHasShield.OnTutorialShown, new Action(DrawAttention));
		BiomeGenerator.OnTutorialShown = (Action)Delegate.Combine(BiomeGenerator.OnTutorialShown, new Action(DrawAttention));
		Interaction_WeaponSelectionPodium.OnTutorialShown = (Action)Delegate.Combine(Interaction_WeaponSelectionPodium.OnTutorialShown, new Action(DrawAttention));
	}

	private void OnDisable()
	{
		EnemyHasShield.OnTutorialShown = (Action)Delegate.Remove(EnemyHasShield.OnTutorialShown, new Action(DrawAttention));
		BiomeGenerator.OnTutorialShown = (Action)Delegate.Remove(BiomeGenerator.OnTutorialShown, new Action(DrawAttention));
		Interaction_WeaponSelectionPodium.OnTutorialShown = (Action)Delegate.Remove(Interaction_WeaponSelectionPodium.OnTutorialShown, new Action(DrawAttention));
	}

	private void DrawAttention()
	{
		Vector3 localPosition = new Vector3(0f, -250f);
		if (!Container.gameObject.activeSelf)
		{
			Container.transform.localPosition = localPosition;
		}
		Container.gameObject.SetActive(true);
		Sequence s = DOTween.Sequence();
		s.AppendCallback(delegate
		{
			UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
		});
		s.Append(Container.transform.DOLocalMove(new Vector3(0f, 200f), 1f).SetEase(Ease.OutBack));
		s.AppendCallback(delegate
		{
			UIManager.PlayAudio("event:/ui/objective_group_complete");
		});
		s.Append(Container.transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack));
		s.Append(Container.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack));
		s.AppendInterval(1f);
		s.AppendCallback(delegate
		{
			UIManager.PlayAudio("event:/ui/level_node_end_screen_ui_appear");
		});
		s.Append(Container.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InBack));
	}
}
