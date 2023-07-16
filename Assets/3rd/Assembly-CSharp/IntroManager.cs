using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MMBiomeGeneration;
using MMTools;
using Spine;
using UnityEngine;

public class IntroManager : BaseMonoBehaviour
{
	public List<GameObject> GameScene;

	public GameObject DeathScene;

	public DungeonLocationManager DungeonLocationManager;

	public GameObject PlayerPrefab;

	public BiomeGenerator BiomeGenerator;

	public GameObject distortionObject;

	private void Start()
	{
		distortionObject.gameObject.SetActive(false);
		GameManager.NewRun("", false);
	}

	private void OnEnable()
	{
		StartCoroutine(StopMusic());
		if (WeatherSystemController.Instance != null)
		{
			WeatherSystemController.Instance.EnteredBuilding();
		}
	}

	private void OnDisable()
	{
	}

	private IEnumerator StopMusic()
	{
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.StopCurrentMusic();
		AudioManager.Instance.StopCurrentAtmos();
	}

	public void ToggleGameScene()
	{
		DeviceLightingManager.StopAll();
		DeviceLightingManager.UpdateLocation();
		DungeonLocationManager.PlayerPrefab = PlayerPrefab;
		BiomeGenerator.DoFirstArrivalRoutine = true;
		DataManager.Instance.dungeonRunDuration = Time.time;
		foreach (GameObject item in GameScene)
		{
			item.SetActive(true);
		}
		DeathScene.SetActive(false);
		Object.FindObjectOfType<IntroRoomPlayerReturns>().Play();
		GameManager.setDefaultGlobalShaders();
		RoomLockController.RoomCompleted();
		WeatherSystemController.Instance.ExitedBuilding();
		PlayerFarming.Instance.Spine.AnimationState.Event += AnimationState_Event;
	}

	private void AnimationState_Event(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "change-skin")
		{
			PulseDisplacementObject();
		}
	}

	public void ToggleDeathScene()
	{
		DeviceLightingManager.TransitionLighting(Color.black, new Color(0.7f, 0.65f, 0.1f, 1f), 0f);
		AudioManager.Instance.StopCurrentMusic();
		AudioManager.Instance.StopCurrentAtmos(false);
		foreach (GameObject item in GameScene)
		{
			item.SetActive(false);
		}
		DeathScene.SetActive(true);
		MMTransition.ResumePlay();
		WeatherSystemController.Instance.EnteredBuilding();
	}

	public void DisableBoth()
	{
		AudioManager.Instance.StopCurrentAtmos(false);
		foreach (GameObject item in GameScene)
		{
			item.SetActive(false);
		}
		DeathScene.SetActive(false);
	}

	public void PulseDisplacementObject()
	{
		if (distortionObject.gameObject.activeSelf)
		{
			distortionObject.transform.localScale = Vector3.zero;
			distortionObject.transform.DORestart();
			distortionObject.transform.DOPlay();
			return;
		}
		distortionObject.SetActive(true);
		distortionObject.transform.localScale = Vector3.zero;
		distortionObject.transform.DOScale(9f, 0.9f).SetEase(Ease.Linear).OnComplete(delegate
		{
			distortionObject.SetActive(false);
		});
	}
}
