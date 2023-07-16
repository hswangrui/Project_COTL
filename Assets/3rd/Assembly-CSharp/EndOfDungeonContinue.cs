using System.Collections;
using System.Collections.Generic;
using Lamb.UI;
using Map;
using MMBiomeGeneration;
using MMRoomGeneration;
using Spine.Unity;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;

public class EndOfDungeonContinue : MonoBehaviour
{
	[SerializeField]
	private GameObject door;

	[SerializeField]
	private GameObject chest;

	[SerializeField]
	private GameObject teleporter;

	[SerializeField]
	private List<GameObject> DoorObjectsToHide = new List<GameObject>();

	[SerializeField]
	private ParticleSystemRenderer doorParticle;

	[SerializeField]
	private Material oldMaterial;

	[SerializeField]
	private Material newMaterial;

	[SerializeField]
	private SkeletonRendererCustomMaterials[] torchCustomMaterials;

	private void Awake()
	{
		if (DungeonSandboxManager.Active)
		{
			base.enabled = false;
			if (MapManager.Instance != null && MapManager.Instance.CurrentMap.GetFinalBossNode() == MapManager.Instance.CurrentNode)
			{
				teleporter.SetActive(true);
				chest.gameObject.SetActive(false);
				HideDoor();
			}
			else
			{
				teleporter.SetActive(false);
			}
			return;
		}
		if ((DataManager.Instance.DungeonCompleted(BiomeGenerator.Instance.DungeonLocation, GameManager.Layer2) || DungeonSandboxManager.Active) && MapManager.Instance != null && MapManager.Instance.CurrentNode != null && MapManager.Instance.CurrentNode == MapManager.Instance.CurrentMap.GetFinalBossNode())
		{
			chest.SetActive(false);
			teleporter.SetActive(true);
			if (GameManager.DungeonEndlessLevel >= 3 || DungeonSandboxManager.Active)
			{
				HideDoor();
			}
		}
		else
		{
			chest.SetActive(true);
			teleporter.SetActive(false);
		}
		doorParticle.material = oldMaterial;
	}

	private void HideDoor()
	{
		Debug.Log("GameManager.DungeonEndlessLevel:" + GameManager.DungeonEndlessLevel);
		Debug.Log("BiomeGenerator.MAX_ENDLESS_LEVELS: " + 3);
		foreach (GameObject item in DoorObjectsToHide)
		{
			item.SetActive(false);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(MovePlayerToMiddle());
	}

	private void SetEverythingGreen()
	{
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.MediumImpact);
		SkeletonRendererCustomMaterials[] array = torchCustomMaterials;
		foreach (SkeletonRendererCustomMaterials skeletonRendererCustomMaterials in array)
		{
			AudioManager.Instance.PlayOneShot("event:/cooking/fire_start", skeletonRendererCustomMaterials.gameObject);
			skeletonRendererCustomMaterials.enabled = true;
		}
		doorParticle.material = newMaterial;
	}

	private IEnumerator MovePlayerToMiddle()
	{
		yield return new WaitForEndOfFrame();
		if (BiomeGenerator.Instance == null || BiomeGenerator.Instance.CurrentRoom == null || BiomeGenerator.Instance.CurrentRoom.generateRoom != GetComponentInParent<GenerateRoom>() || MapManager.Instance == null || MapManager.Instance.CurrentNode == null || MapManager.Instance.CurrentNode.nodeType != NodeType.MiniBossFloor)
		{
			yield break;
		}
		bool shownTutorial = !DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.ContinueAdventureMap);
		if (shownTutorial)
		{
			SetEverythingGreen();
			yield break;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		float t = 0f;
		while (true)
		{
			float num;
			t = (num = t + Time.deltaTime);
			if (!(num < 1.8f))
			{
				break;
			}
			PlayerFarming.Instance.GoToAndStop(new Vector3(-0.5f, 2.5f, 0f));
			yield return null;
		}
		UITutorialOverlayController tutorialOverlay = null;
		if (!shownTutorial)
		{
			tutorialOverlay = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.ContinueAdventureMap);
		}
		while (tutorialOverlay != null)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		while (UIMenuBase.ActiveMenus.Count > 0)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(door);
		yield return new WaitForSeconds(1.5f);
		SkeletonRendererCustomMaterials[] array = torchCustomMaterials;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].enabled = true;
		}
		Material i = doorParticle.material;
		float time = 0f;
		while (true)
		{
			float num;
			time = (num = time + Time.deltaTime);
			if (!(num < 1f))
			{
				break;
			}
			doorParticle.material.Lerp(i, newMaterial, time / 1f);
			yield return null;
		}
		yield return new WaitForSeconds(1.5f);
		GameManager.GetInstance().OnConversationEnd();
	}
}
