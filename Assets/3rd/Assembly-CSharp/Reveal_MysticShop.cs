using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using MMTools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class Reveal_MysticShop : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> ObjectsToMove = new List<GameObject>();

	private List<float> ObjectsToMoveZ = new List<float>();

	[SerializeField]
	private List<GameObject> ObjectsToDisable = new List<GameObject>();

	[SerializeField]
	private GameObject ritualFX;

	[SerializeField]
	private SimpleSetCamera simpleCamera;

	[SerializeField]
	private SimpleSetCamera doorCamera;

	[SerializeField]
	private SkeletonAnimation mysticSeller;

	[SerializeField]
	private SkeletonRendererCustomMaterials mysticSellerMaterialOverride;

	[SerializeField]
	private Transform playerPosition;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image image;

	[SerializeField]
	private GameObject rift;

	[SerializeField]
	private GameObject cameraPos;

	[SerializeField]
	private GameObject revealDoorsCameraPos;

	[SerializeField]
	private Interaction_SimpleConversation introConversation;

	[SerializeField]
	private Interaction_BaseDungeonDoor[] doors;

	[SerializeField]
	private Interaction_BaseDungeonDoor[] newDoors;

	public ParticleSystem doorParticles1;

	public ParticleSystem doorParticles2;

	public ParticleSystem doorParticles3;

	public ParticleSystem doorParticles4;

	public ParticleSystem rockParticles1;

	public ParticleSystem rockParticles2;

	private Vector3 mysticSellerStartingPos;

	private EventInstance LoopedSound;

	private static readonly int FillAlpha = Shader.PropertyToID("_FillAlpha");

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(LoopedSound);
	}

	private void Start()
	{
		ritualFX.SetActive(false);
		canvas.gameObject.SetActive(false);
		mysticSeller.gameObject.SetActive(DataManager.Instance.OnboardedMysticShop);
		rift.gameObject.SetActive(DataManager.Instance.OnboardedMysticShop);
		introConversation.enabled = false;
		simpleCamera.gameObject.SetActive(!DataManager.Instance.OnboardedMysticShop);
		doorCamera.gameObject.SetActive(!DataManager.Instance.OnboardedMysticShop);
		Interaction_BaseDungeonDoor[] array = doors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(!DataManager.Instance.OnboardedMysticShop);
		}
		array = newDoors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(DataManager.Instance.OnboardedMysticShop);
		}
		if (!DataManager.Instance.OnboardedMysticShop)
		{
			return;
		}
		foreach (GameObject item in ObjectsToDisable)
		{
			item.SetActive(false);
		}
	}

	public void Reveal()
	{
		StartCoroutine(RevealRoutine());
	}

	private IEnumerator RevealRoutine()
	{
		ritualFX.SetActive(false);
		Vector3 position = (mysticSellerStartingPos = mysticSeller.transform.position);
		position -= new Vector3(0f, -2f, 0f);
		mysticSeller.transform.position = position;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 15f);
		bool Waiting = true;
		PlayerFarming.Instance.GoToAndStop(playerPosition.position + new Vector3(0f, -2f), playerPosition.gameObject, false, false, delegate
		{
			PlayerFarming.Instance.transform.position = playerPosition.transform.position + new Vector3(0f, -2f);
			Waiting = false;
		});
		while (Waiting)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNext(mysticSeller.gameObject, 20f);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.PlayOneShot("event:/material/earthquake", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom");
		MMVibrate.RumbleContinuous(1f, 5f);
		StartCoroutine(ShakeCameraWithRampUp());
		foreach (GameObject item in ObjectsToMove)
		{
			if (item.activeSelf)
			{
				item.transform.DOShakeRotation(4f, 20f).SetEase(Ease.InBack);
				item.GetComponent<SpriteRenderer>().sortingOrder = 0;
			}
		}
		yield return new WaitForSeconds(4f);
		CameraManager.instance.ShakeCameraForDuration(2f, 5f, 0.5f);
		BiomeConstants.Instance.ImpactFrameForDuration();
		LoopedSound = AudioManager.Instance.CreateLoop("event:/door/eye_beam_door_open", true);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_zoom_back");
		BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 0.75f);
		AudioManager.Instance.SetMusicPsychedelic(1f);
		MMVibrate.StopRumble();
		mysticSeller.gameObject.SetActive(true);
		ritualFX.SetActive(true);
		mysticSeller.GetComponent<MeshRenderer>().materials[0].color = Color.black;
		mysticSeller.GetComponent<MeshRenderer>().materials[0].DOColor(Color.white, 2f).SetDelay(1f);
		mysticSeller.transform.DOMove(mysticSellerStartingPos, 2f).SetEase(Ease.OutBack);
		ObjectsToMoveZ.Clear();
		foreach (GameObject item2 in ObjectsToMove)
		{
			if (item2.activeSelf)
			{
				ObjectsToMoveZ.Add(item2.transform.position.z);
				item2.transform.DOMoveZ(Random.Range(-5, -10), 2f).SetEase(Ease.OutCirc);
				item2.transform.DOShakeRotation(6f, 20f).SetEase(Ease.OutBack);
			}
		}
		GameManager.GetInstance().OnConversationNext(cameraPos, 15f);
		yield return new WaitForSeconds(4f);
		GameManager.GetInstance().OnConversationNext(cameraPos, 12f);
		canvas.gameObject.SetActive(true);
		image.color = new Color(1f, 1f, 1f, 0f);
		image.DOColor(StaticColors.RedColor, 2f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom", base.transform.gameObject);
		yield return new WaitForSeconds(1.5f);
		rockParticles1.Play();
		rockParticles2.Play();
		yield return new WaitForSeconds(0.5f);
		foreach (GameObject item3 in ObjectsToDisable)
		{
			item3.SetActive(false);
		}
		canvas.gameObject.SetActive(false);
		rift.SetActive(true);
		BiomeConstants.Instance.ChromaticAbberationTween(1f, 0.75f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
		AudioManager.Instance.StopLoop(LoopedSound);
		ritualFX.SetActive(false);
		GameManager.GetInstance().OnConversationNext(cameraPos, 15f);
		int num = 0;
		int num2 = 0;
		foreach (GameObject item4 in ObjectsToMove)
		{
			if (item4.activeSelf)
			{
				float num3 = Random.Range(0.5f, 1f);
				item4.transform.DOMoveZ(ObjectsToMoveZ[num], num3).SetEase(Ease.OutBounce);
				if (num2 == 3)
				{
					AudioManager.Instance.PlayOneShotDelayed("event:/material/stone_break", num3 - num3 / 5f, item4.transform);
					num2 = 0;
				}
				num2++;
				num++;
			}
		}
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_zoom_back", base.transform.gameObject);
		yield return new WaitForSeconds(2f);
		introConversation.Play();
		yield return new WaitForEndOfFrame();
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		introConversation.enabled = false;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNext(revealDoorsCameraPos, 20f);
		yield return new WaitForSeconds(2f);
		DataManager.Instance.UnlockedDungeonDoor.Clear();
		DataManager.Instance.UnlockedDungeonDoor.Add(FollowerLocation.Dungeon1_1);
		Interaction_BaseDungeonDoor component = doors[0].GetComponent<Interaction_BaseDungeonDoor>();
		component.doorInnerBlack.DOColor(Color.white, 0.1f);
		component.doorLightSource.SetActive(true);
		SpriteRenderer[] componentsInChildren = component.gameObject.GetComponentsInChildren<SpriteRenderer>();
		Material newFillMat = new Material(componentsInChildren[0].material);
		newFillMat.SetFloat(FillAlpha, 1f);
		SpriteRenderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = newFillMat;
		}
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
		yield return new WaitForSeconds(0.33f);
		doorParticles1.Play();
		doors[0].gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(newDoors[0].transform.position - Vector3.forward, Vector3.one * 5f);
		newDoors[0].gameObject.SetActive(true);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		CameraManager.shakeCamera(0.1f);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", newDoors[0].transform.position);
		yield return new WaitForSeconds(0.5f);
		Interaction_BaseDungeonDoor component2 = doors[1].GetComponent<Interaction_BaseDungeonDoor>();
		component2.doorInnerBlack.DOColor(Color.white, 0.1f);
		component2.doorLightSource.SetActive(true);
		componentsInChildren = component2.gameObject.GetComponentsInChildren<SpriteRenderer>();
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = newFillMat;
		}
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
		yield return new WaitForSeconds(0.33f);
		doorParticles2.Play();
		doors[1].gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(newDoors[1].transform.position - Vector3.forward, Vector3.one * 5f);
		newDoors[1].gameObject.SetActive(true);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		CameraManager.shakeCamera(0.2f);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", newDoors[1].transform.position);
		yield return new WaitForSeconds(0.5f);
		Interaction_BaseDungeonDoor component3 = doors[2].GetComponent<Interaction_BaseDungeonDoor>();
		component3.doorInnerBlack.DOColor(Color.white, 0.1f);
		component3.doorLightSource.SetActive(true);
		componentsInChildren = component3.gameObject.GetComponentsInChildren<SpriteRenderer>();
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = newFillMat;
		}
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
		yield return new WaitForSeconds(0.33f);
		doorParticles3.Play();
		doors[2].gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(newDoors[2].transform.position - Vector3.forward, Vector3.one * 5f);
		newDoors[2].gameObject.SetActive(true);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		CameraManager.shakeCamera(0.3f);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", newDoors[2].transform.position);
		yield return new WaitForSeconds(0.5f);
		Interaction_BaseDungeonDoor component4 = doors[3].GetComponent<Interaction_BaseDungeonDoor>();
		component4.doorInnerBlack.DOColor(Color.white, 0.1f);
		component4.doorLightSource.SetActive(true);
		componentsInChildren = component4.gameObject.GetComponentsInChildren<SpriteRenderer>();
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material = newFillMat;
		}
		AudioManager.Instance.PlayOneShot("event:/Stings/generic_positive", base.transform.position);
		yield return new WaitForSeconds(0.33f);
		doorParticles4.Play();
		doors[3].gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeInteractionVFX(newDoors[3].transform.position - Vector3.forward, Vector3.one * 5f);
		newDoors[3].gameObject.SetActive(true);
		MMVibrate.Haptic(MMVibrate.HapticTypes.LightImpact);
		CameraManager.shakeCamera(0.4f);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/door/door_done", newDoors[3].transform.position);
		yield return new WaitForSeconds(2f);
		MMVibrate.Haptic(MMVibrate.HapticTypes.Success);
		AudioManager.Instance.PlayOneShot("event:/Stings/boss_door_complete", base.transform.gameObject);
		AudioManager.Instance.SetMusicPsychedelic(0f);
		GameManager.GetInstance().OnConversationEnd();
		ObjectiveManager.Add(new Objectives_CollectItem("Objectives/GroupTitles/MysticShop", InventoryItem.ITEM_TYPE.GOD_TEAR, 1)
		{
			CustomTerm = "Objectives/CollectItem/DivineCrystals"
		}, true);
		GetComponent<Interaction_MysticShop>().StopMusic();
	}

	private IEnumerator ShakeCameraWithRampUp()
	{
		float t = 0f;
		while (true)
		{
			float num;
			t = (num = t + Time.deltaTime);
			if (!(num < 3.9f))
			{
				break;
			}
			float t2 = t / 3.9f;
			CameraManager.instance.ShakeCameraForDuration(Mathf.Lerp(0f, 0.5f, t2), Mathf.Lerp(0f, 1.5f, t2), 3.9f, false);
			yield return null;
		}
		CameraManager.instance.Stopshake();
	}
}
