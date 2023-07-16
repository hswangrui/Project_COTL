using System;
using DG.Tweening;
using Lamb.UI;
using src.UI.Overlays.TutorialOverlay;
using TMPro;
using UnityEngine;

public class HUDDoctrineStoneCount : MonoBehaviour
{
	public static HUDDoctrineStoneCount Instance;

	public TextMeshProUGUI IconText;

	public TextMeshProUGUI CountText;

	public Transform Container;

	public TextMeshProUGUI IconToMove;

	private int previousCount;

	private Vector3 TargetPositon;

	private void OnEnable()
	{
		UpdateCount();
		PlayerDoctrineStone.OnIncreaseCount = (Action)Delegate.Combine(PlayerDoctrineStone.OnIncreaseCount, new Action(Fly));
		PlayerDoctrineStone.OnDecreaseCount = (Action)Delegate.Combine(PlayerDoctrineStone.OnDecreaseCount, new Action(UpdateCount));
		PlayerDoctrineStone.OnCachePosition = (Action)Delegate.Combine(PlayerDoctrineStone.OnCachePosition, new Action(CachePosition));
		Instance = this;
		previousCount = DataManager.Instance.CompletedDoctrineStones;
	}

	private void OnDisable()
	{
		PlayerDoctrineStone.OnIncreaseCount = (Action)Delegate.Remove(PlayerDoctrineStone.OnIncreaseCount, new Action(Fly));
		PlayerDoctrineStone.OnDecreaseCount = (Action)Delegate.Remove(PlayerDoctrineStone.OnDecreaseCount, new Action(UpdateCount));
		PlayerDoctrineStone.OnCachePosition = (Action)Delegate.Remove(PlayerDoctrineStone.OnCachePosition, new Action(CachePosition));
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void CachePosition()
	{
		TargetPositon = IconText.transform.position;
	}

	private void Fly()
	{
		IconToMove.text = "<sprite name=\"icon_DoctrineStone\">";
		IconToMove.transform.localScale = Vector3.one * 2f;
		IconToMove.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack);
		IconToMove.transform.position = Camera.main.WorldToScreenPoint(PlayerDoctrineStone.Instance.gameObject.transform.position);
		IconToMove.transform.DOMove(TargetPositon, 1.25f).SetEase(Ease.InBack).OnComplete(delegate
		{
			if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.CommandmentStone))
			{
				RoomLockController.RoomCompleted();
				UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.CommandmentStone);
				uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, (Action)delegate
				{
					ObjectiveManager.Add(new Objectives_Custom("Objectives/GroupTitles/DeclareDoctrine", Objectives.CustomQuestTypes.DeclareDoctrine), true);
				});
			}
			UpdateCount();
		});
	}

	private void UpdateCount()
	{
		IconToMove.text = "";
		if (DataManager.Instance.CompletedDoctrineStones <= 0)
		{
			CountText.text = "";
			IconText.text = "";
			return;
		}
		IconText.text = "<sprite name=\"icon_DoctrineStone\">";
		CountText.text = DataManager.Instance.CompletedDoctrineStones.ToString();
		if (previousCount != DataManager.Instance.CompletedDoctrineStones)
		{
			CountText.transform.DOKill();
			CameraManager instance = CameraManager.instance;
			if ((object)instance != null)
			{
				instance.ShakeCameraForDuration(0.7f, 0.8f, 0.2f);
			}
			CountText.transform.localScale = Vector3.one * 1.2f;
			CountText.transform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);
		}
		previousCount = DataManager.Instance.CompletedDoctrineStones;
	}
}
