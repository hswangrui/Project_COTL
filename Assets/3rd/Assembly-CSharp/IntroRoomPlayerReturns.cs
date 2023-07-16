using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using Spine.Unity;
using UnityEngine;

public class IntroRoomPlayerReturns : BaseMonoBehaviour
{
	public IntroRoomMusicController musicController;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject PlayerPrefab;

	private SimpleSpineAnimator simpleSpineAnimator;

	public AnimateOnCallBack GoatSpine;

	public CanvasGroup ControlsHUD;

	public List<GameObject> DestroyObjects = new List<GameObject>();

	public List<SkeletonAnimation> AnimateCultists = new List<SkeletonAnimation>();

	public GameObject ActivateEnemies;

	public GameObject InvisibleEnemyWall;

	public GameObject LightingOverride;

	public float LightningTransitionDurationOverride = 0.2f;

	private void Start()
	{
		ActivateEnemies.SetActive(false);
		ControlsHUD.alpha = 0f;
	}

	public void Play()
	{
		StartCoroutine(PlayRoutine());
	}

	private IEnumerator PlayRoutine()
	{
		ApplyLightningOverrideTransitionDuration();
		BiomeGenerator.Instance.CurrentRoom.generateRoom.SetColliderAndUpdatePathfinding();
		musicController.PlayAmbientNature();
		foreach (GameObject destroyObject in DestroyObjects)
		{
			Object.Destroy(destroyObject);
		}
		ActivateEnemies.SetActive(true);
		GameObject Player = GameObject.FindWithTag("Player");
		if (Player != null)
		{
			Object.Destroy(Player);
		}
		GameObject NewPlayer = Object.Instantiate(PlayerPrefab, new Vector3(0f, Player.transform.position.y, Player.transform.position.z), Quaternion.identity, GameObject.FindGameObjectWithTag("Unit Layer").transform);
		BiomeGenerator.Instance.Player = NewPlayer;
		NewPlayer.GetComponent<Health>().DamageModifier = 0f;
		StateMachine component = NewPlayer.GetComponent<StateMachine>();
		component.facingAngle = Player.GetComponent<StateMachine>().facingAngle;
		component.CURRENT_STATE = StateMachine.State.CustomAnimation;
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().AddToCamera(NewPlayer);
		SimpleSetCamera.Reset();
		GameManager.GetInstance().CamFollowTarget.TargetCamera = null;
		GameManager.GetInstance().AddPlayerToCamera();
		GameManager.GetInstance().OnConversationNew(false, true);
		GameManager.GetInstance().OnConversationNext(NewPlayer, 4f);
		GameManager.GetInstance().CameraSetZoom(4f);
		simpleSpineAnimator = NewPlayer.GetComponentInChildren<SimpleSpineAnimator>();
		simpleSpineAnimator.Animate("intro/dead", 0, true).MixDuration = 0f;
		simpleSpineAnimator.SetSkin("Lamb_Intro");
		Object.Destroy(Player);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(NewPlayer, 5f);
		CameraManager.shakeCamera(0.3f);
		simpleSpineAnimator.OnSpineEvent += OnSpineEvent;
		simpleSpineAnimator.Animate("intro/rebirth", 0, false);
		simpleSpineAnimator.AddAnimate("idle", 0, true, 0f);
		yield return new WaitForSeconds(3f);
		ApplyLightningOverrideTransitionDuration();
		LightingOverride.SetActive(false);
		yield return new WaitForSeconds(1.5f);
		GameManager.GetInstance().OnConversationNext(NewPlayer.gameObject, 6f);
		PlayerFarming.Instance.playerWeapon.SetWeapon(EquipmentType.Sword, 0);
		yield return new WaitForSeconds(2.5f);
		GameManager.GetInstance().OnConversationEnd();
		GameManager.GetInstance().AddPlayerToCamera();
		HUD_Manager.Instance.Hide(true);
		EnemySwordsman[] array = Object.FindObjectsOfType<EnemySwordsman>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
		FormationFighter[] array2 = Object.FindObjectsOfType<FormationFighter>();
		foreach (FormationFighter obj in array2)
		{
			obj.transform.localScale = Vector3.one;
			obj.enabled = true;
		}
		Object.FindObjectOfType<IntroGuards>().Wall.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().CameraSetZoom(12f);
		GameManager.GetInstance().CachedZoom = 12f;
		float Progress2 = 0f;
		float Duration = 0.5f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.deltaTime);
			if (num < Duration)
			{
				ControlsHUD.alpha = Mathf.Lerp(0f, 1f, Progress2 / Duration);
				yield return null;
				continue;
			}
			break;
		}
		while (NewPlayer.transform.position.y > InvisibleEnemyWall.transform.position.y)
		{
			yield return null;
		}
		Progress2 = 0f;
		while (true)
		{
			float num;
			Progress2 = (num = Progress2 + Time.deltaTime);
			if (num < Duration)
			{
				ControlsHUD.alpha = Mathf.Lerp(1f, 0f, Progress2 / Duration);
				yield return null;
				continue;
			}
			break;
		}
	}

	private void ApplyLightningOverrideTransitionDuration()
	{
		LightingManagerVolume component = LightingOverride.GetComponent<LightingManagerVolume>();
		if (component != null)
		{
			component.transitionDurationMultiplierAdjustment = LightningTransitionDurationOverride;
		}
	}

	private void OnSpineEvent(string EventName)
	{
		switch (EventName)
		{
		case "change-skin":
			simpleSpineAnimator.SetSkin("Lamb_0");
			GameManager.GetInstance().CameraSetTargetZoom(7f);
			CameraManager.shakeCamera(0.5f);
			foreach (SkeletonAnimation animateCultist in AnimateCultists)
			{
				AudioManager.Instance.PlayOneShot("event:/enemy/vocals/humanoid/warning", animateCultist.gameObject);
				animateCultist.AnimationState.SetAnimation(0, "scared", false);
				animateCultist.AnimationState.AddAnimation(0, "scared-loop", true, 0f);
			}
			musicController.PlayCombatMusic();
			break;
		case "sfxTrigger":
			AudioManager.Instance.PlayOneShot("event:/player/resurrect", PlayerFarming.Instance.gameObject);
			break;
		case "intro-sword":
			AudioManager.Instance.PlayOneShot("event:/player/weapon_unlocked", PlayerFarming.Instance.gameObject);
			break;
		}
	}

	private void OnDisable()
	{
		if (simpleSpineAnimator != null)
		{
			simpleSpineAnimator.OnSpineEvent -= OnSpineEvent;
		}
	}
}
