using System.Collections;
using DG.Tweening;
using I2.Loc;
using MMBiomeGeneration;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class Interaction_TempleBossDoor : Interaction
{
	public ParticleSystem doorParticles;

	public ParticleSystem doorParticlesLong;

	public GameObject PlayerPosition;

	public SimpleSetCamera SimpleSetCamera;

	public GameObject DoorToMove;

	private Vector3 OpenDoorPosition = new Vector3(0f, -2.5f, 4.5f);

	public string SceneName;

	public GameObject TeleportToBase;

	public SkeletonAnimation SealSpine;

	public SimpleSpineFlash SimpleSpineFlash;

	private Coroutine sealCoroutine;

	[SerializeField]
	private SkeletonRendererCustomMaterials spineMaterialOverride;

	[SerializeField]
	private ParticleSystem doorcloseParticleSystem;

	private bool spawnedExtraHearts;

	public BoxCollider2D CollideForDoor;

	private bool Unlocked;

	private bool Used;

	private Tween moveTween;

	private void Start()
	{
		UpdateLocalisation();
		if ((bool)TeleportToBase)
		{
			TeleportToBase.SetActive(!DataManager.Instance.DungeonBossFight || Unlocked);
			if (DungeonSandboxManager.Active)
			{
				TeleportToBase.gameObject.SetActive(true);
				base.gameObject.SetActive(false);
			}
		}
	}

	public void OpenTheDoor()
	{
		Unlocked = true;
		OpenDoor();
		Interactable = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Interactable = false;
		Used = false;
		spineMaterialOverride.enabled = false;
		Debug.Log("DataManager.Instance.UnlockedBossTempleDoor.Contains(MMBiomeGeneration.BiomeGenerator.Instance.DungeonLocation) " + DataManager.Instance.UnlockedBossTempleDoor.Contains(BiomeGenerator.Instance.DungeonLocation).ToString() + "  " + BiomeGenerator.Instance.DungeonLocation);
		Unlocked = DataManager.Instance.UnlockedBossTempleDoor.Contains(BiomeGenerator.Instance.DungeonLocation) || DungeonSandboxManager.Active;
		if ((Unlocked && (DataManager.Instance.ShownInitialTempleDoorSeal || DungeonSandboxManager.Active)) || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_1 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_2 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_3 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_4)
		{
			Unlocked = true;
			DoorToMove.transform.localPosition = OpenDoorPosition;
			DoorToMove.SetActive(false);
			StartCoroutine(SpawnHeartOnPreviousDeaths());
		}
		else
		{
			DoorToMove.transform.localPosition = Vector3.zero;
			if (SealSpine != null)
			{
				sealCoroutine = StartCoroutine(SealRoutine());
			}
		}
		OpenDoor();
		BiomeGenerator.OnBiomeChangeRoom += OnBiomeChangeRoom;
	}

	private IEnumerator SpawnHeartOnPreviousDeaths()
	{
		yield return new WaitForEndOfFrame();
		if (spawnedExtraHearts || !base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		if (DifficultyManager.AssistModeEnabled && DataManager.Instance.playerDeathsInARowFightingLeader > 2)
		{
			switch (DifficultyManager.PrimaryDifficulty)
			{
			case DifficultyManager.Difficulty.Easy:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 2, TeleportToBase.gameObject.transform.position);
				break;
			case DifficultyManager.Difficulty.Medium:
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, TeleportToBase.gameObject.transform.position);
				break;
			}
		}
		spawnedExtraHearts = true;
	}

	public override void GetLabel()
	{
		base.GetLabel();
		base.Label = ((!Unlocked) ? ScriptLocalization.Interactions.Locked : "");
	}

	private IEnumerator SealRoutine()
	{
		int dungeonLayer = GameManager.CurrentDungeonLayer;
		if (GameManager.Layer2)
		{
			dungeonLayer = DataManager.GetGodTearNotches(PlayerFarming.Location);
			if (DataManager.Instance.DungeonBossFight)
			{
				dungeonLayer = 4;
			}
		}
		string anim3 = Mathf.Clamp(dungeonLayer - 1, 0, int.MaxValue).ToString();
		while (SealSpine.AnimationState == null)
		{
			yield return null;
		}
		SealSpine.AnimationState.SetAnimation(0, anim3, true);
		if (PlayerFarming.Instance != null && (dungeonLayer == 4 || !DataManager.Instance.ShownInitialTempleDoorSeal))
		{
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
			float t = 0f;
			while (true)
			{
				float num;
				t = (num = t + Time.deltaTime);
				if (num < 1f)
				{
					PlayerFarming.Instance.GoToAndStop(new Vector3(-0.5f, 0.5f, 0f));
					yield return null;
					continue;
				}
				break;
			}
			while (PlayerFarming.Instance.GoToAndStopping)
			{
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			spineMaterialOverride.enabled = true;
			GameManager.GetInstance().OnConversationNext(SealSpine.gameObject, 7f);
			yield return new WaitForSeconds(2.5f);
		}
		else
		{
			yield return new WaitForSeconds(1.5f);
		}
		anim3 = dungeonLayer + "-activate";
		if (anim3 == "4-activate")
		{
			AudioManager.Instance.PlayOneShot("event:/door/boss_door_sequence", base.gameObject);
		}
		else
		{
			AudioManager.Instance.PlayOneShot("event:/door/boss_door_piece", base.gameObject);
		}
		SealSpine.AnimationState.SetAnimation(0, anim3, false);
		anim3 = dungeonLayer.ToString();
		if (dungeonLayer != 4)
		{
			SealSpine.AnimationState.AddAnimation(0, anim3, true, 0f);
		}
		doorParticles.Play();
		SimpleSpineFlash.FlashFillRed();
		if (dungeonLayer == 4)
		{
			doorParticlesLong.Play();
			yield return new WaitForSeconds(4.5f);
			doorParticles.Play();
			OnInteract(null);
			if (!DataManager.Instance.UnlockedBossTempleDoor.Contains(BiomeGenerator.Instance.DungeonLocation))
			{
				Debug.Log("ADD ME! " + BiomeGenerator.Instance.DungeonLocation);
				DataManager.Instance.UnlockedBossTempleDoor.Add(BiomeGenerator.Instance.DungeonLocation);
			}
		}
		else if (!DataManager.Instance.ShownInitialTempleDoorSeal)
		{
			yield return new WaitForSeconds(3f);
			GameManager.GetInstance().OnConversationEnd();
		}
		DataManager.Instance.ShownInitialTempleDoorSeal = true;
		TeleportToBase.GetComponent<Interaction_TeleportHome>().HasChanged = true;
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
		if (sealCoroutine != null)
		{
			StopCoroutine(sealCoroutine);
		}
	}

	private void OnBiomeChangeRoom()
	{
		BiomeGenerator.OnBiomeChangeRoom -= OnBiomeChangeRoom;
	}

	private void OpenDoor()
	{
		if (Unlocked)
		{
			CollideForDoor.enabled = true;
		}
		else
		{
			CollideForDoor.enabled = false;
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		SimpleSetCamera.Play();
		StartCoroutine(EnterTemple());
	}

	private IEnumerator EnterTemple()
	{
		if (GameManager.CurrentDungeonLayer != 4)
		{
			AudioManager.Instance.PlayOneShot("event:/door/boss_door_piece", base.gameObject);
		}
		yield return new WaitForSeconds(1f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(0.2f);
		AudioManager.Instance.PlayOneShot("event:/door/door_lower", base.gameObject);
		doorcloseParticleSystem.Play();
		float Progress = 0f;
		float Duration = 3f;
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, Duration);
		Vector3 StartingPosition = DoorToMove.transform.position;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			DoorToMove.transform.position = Vector3.Lerp(StartingPosition, StartingPosition + OpenDoorPosition, Mathf.SmoothStep(0f, 1f, Progress / Duration));
			yield return null;
		}
		AudioManager.Instance.PlayOneShot("event:/door/door_done", base.gameObject);
		doorcloseParticleSystem.Stop();
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		SimpleSetCamera.Reset();
		Unlocked = true;
		OpenDoor();
		OnBiomeChangeRoom();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && !Used)
		{
			Used = true;
			MMTransition.StopCurrentTransition();
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", ChangeRoom);
		}
	}

	private void ChangeRoom()
	{
		Debug.Log("HIT");
		if (BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_1 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_2 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_3 || BiomeGenerator.Instance.DungeonLocation == FollowerLocation.Boss_4)
		{
			BiomeGenerator.ChangeRoom(0, 1);
		}
		else
		{
			BiomeGenerator.ChangeRoom(BiomeGenerator.BossCoords.x, BiomeGenerator.BossCoords.y);
		}
	}

	private void FadeSave()
	{
	}
}
