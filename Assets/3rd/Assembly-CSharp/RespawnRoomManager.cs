using System;
using System.Collections;
using System.Collections.Generic;
using Beffio.Dithering;
using DG.Tweening;
using FMOD.Studio;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using UnityEngine;

public class RespawnRoomManager : BaseMonoBehaviour
{
	public static RespawnRoomManager Instance;

	private BiomeGenerator biomeGenerator;

	private GenerateRoom generateRoom;

	private GameObject Player;

	public GameObject PlayerPrefab;

	public GameObject LightingOverride;

	[SerializeField]
	private Interaction_SimpleConversation conversation;

	[SerializeField]
	private AnimationCurve absorbSoulCurve;

	[SerializeField]
	private GameObject teleporter;

	private AstarPath astarPath;

	private Stylizer cameraStylizer;

	public Follower FollowerPrefab;

	public Transform FollowerPosition;

	private List<FollowerManager.SpawnedFollower> followers = new List<FollowerManager.SpawnedFollower>();

	private EventInstance LoopInstance;

	public static float HP;

	public static float SpiritHearts;

	public static float BlueHearts;

	public static float BlackHearts;

	public bool Respawning { get; private set; }

	private void OnEnable()
	{
		Instance = this;
		generateRoom = GetComponent<GenerateRoom>();
		cameraStylizer = Camera.main.gameObject.GetComponent<Stylizer>();
		if (cameraStylizer == null)
		{
			Debug.Log("Camera null");
		}
        if (cameraStylizer)
            cameraStylizer.enabled = true;
		if (DataManager.Instance.PermadeDeathActive)
		{
			teleporter.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public void Init(BiomeGenerator biomeGenerator)
	{
		this.biomeGenerator = biomeGenerator;
	}

	public static void Play()
	{
		Debug.Log("PLAY");
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", delegate
		{
			Time.timeScale = 1f;
			Instance.gameObject.SetActive(true);
			Instance.StartCoroutine(Instance.PlayRoutine());
		});
	}

	public void SpawnFollowers()
	{
		astarPath = AstarPath.active;
		AstarPath.active = null;
		List<SimFollower> list = FollowerManager.SimFollowersAtLocation(FollowerLocation.Base);
		for (int i = 0; i < list.Count; i++)
		{
			FollowerManager.SpawnedFollower f = FollowerManager.SpawnCopyFollower(list[i].Brain._directInfoAccess, base.transform.position, FollowerPosition, PlayerFarming.Location);
			followers.Add(f);
			f.Follower.GetComponentInChildren<UIFollowerName>(true).Show();
			f.Follower.transform.position = GetCirclePosition(list.Count, i);
			f.Follower.Interaction_FollowerInteraction.enabled = false;
			f.FollowerFakeBrain.HardSwapToTask(new FollowerTask_AwaitConsuming());
			f.Follower.State.LookAngle = (f.Follower.State.facingAngle = Utils.GetAngle(f.Follower.transform.position, FollowerPosition.transform.position));
			f.Follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
			f.Follower.SetBodyAnimation("pray", true);
			Interaction_ConsumeFollower interaction_ConsumeFollower = f.Follower.gameObject.AddComponent<Interaction_ConsumeFollower>();
			interaction_ConsumeFollower.followerSpine = f.Follower.Spine;
			interaction_ConsumeFollower.Play(list[i].Brain._directInfoAccess, delegate(int HP, int SpiritHearts, int BlueHearts, int BlackHearts)
			{
				RespawnRoomManager.HP = HP;
				RespawnRoomManager.SpiritHearts = SpiritHearts;
				RespawnRoomManager.BlueHearts = BlueHearts;
				RespawnRoomManager.BlackHearts = BlackHearts;
				StartCoroutine(ConsumefollowerRoutine(f));
			});
		}
	}

	private Vector3 GetCirclePosition(int followersCount, int index, float forceTargetDistance = -1f)
	{
		float num = Mathf.Max(Mathf.Clamp(followersCount / 3, 2f, 3.5f), forceTargetDistance);
		float f = (float)index * (360f / (float)followersCount) * ((float)Math.PI / 180f);
		Vector3 result = FollowerPosition.position + new Vector3(num * Mathf.Cos(f), num * Mathf.Sin(f));
		result.x *= 1.2f;
		result.y /= 1.2f;
		result.y += 1f;
		return result;
	}

	private IEnumerator ConsumefollowerRoutine(FollowerManager.SpawnedFollower sacraficeFollower)
	{
		HUD_Manager.Instance.Hide(false);
		generateRoom.RoomTransform.gameObject.SetActive(false);
		sacraficeFollower.FollowerFakeBrain.CompleteCurrentTask();
		FollowerTask_ManualControl Task = new FollowerTask_ManualControl();
		sacraficeFollower.FollowerFakeBrain.HardSwapToTask(Task);
		followers.Remove(sacraficeFollower);
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = false;
		if (DataManager.Instance.First_Dungeon_Resurrecting)
		{
			for (int i = 0; i < followers.Count; i++)
			{
				FollowerTask_ManualControl followerTask_ManualControl = new FollowerTask_ManualControl();
				Follower f = followers[i].Follower;
				followers[i].FollowerFakeBrain.HardSwapToTask(followerTask_ManualControl);
				followerTask_ManualControl.GoToAndStop(f, GetCirclePosition(followers.Count, i, 4f), delegate
				{
					StartCoroutine(SpectatorCheer(f));
				});
			}
			int num = ((!(sacraficeFollower.Follower.transform.position.x > PlayerFarming.Instance.transform.position.x)) ? 1 : (-1));
			bool waitForFollower = true;
			PlayerFarming.Instance.GoToAndStop(FollowerPosition.transform.position + new Vector3(num, 0f, 0f), sacraficeFollower.Follower.gameObject, false, false, delegate
			{
				PlayerFarming.Instance.state.LookAngle = (PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, FollowerPosition.position));
			});
			Task.GoToAndStop(sacraficeFollower.Follower, FollowerPosition.transform.position + new Vector3(-num, 0f, 0f), delegate
			{
				sacraficeFollower.Follower.State.LookAngle = (sacraficeFollower.Follower.State.facingAngle = Utils.GetAngle(sacraficeFollower.Follower.transform.position, FollowerPosition.position));
				waitForFollower = false;
			});
			while (waitForFollower)
			{
				yield return null;
			}
		}
		foreach (FollowerManager.SpawnedFollower follower in followers)
		{
			if (follower.Follower.Spine != null)
			{
				follower.Follower.SetBodyAnimation("cheer", true);
			}
		}
		if (DataManager.Instance.First_Dungeon_Resurrecting)
		{
			GameManager.GetInstance().OnConversationNext(sacraficeFollower.Follower.gameObject, 8f);
			GameManager.GetInstance().AddPlayerToCamera();
			yield return new WaitForSeconds(1f);
		}
		GameManager.GetInstance().OnConversationNext(sacraficeFollower.Follower.gameObject, 10f);
		GameManager.GetInstance().AddPlayerToCamera();
		if (DataManager.Instance.First_Dungeon_Resurrecting)
		{
			AudioManager.Instance.PlayOneShot("event:/followers/consume_start", base.gameObject);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("sacrifice-long", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("warp-out-down", 0, true, 0f);
			sacraficeFollower.Follower.SetBodyAnimation("sacrifice-long", false);
			sacraficeFollower.Follower.State.LookAngle = (sacraficeFollower.Follower.State.facingAngle = Utils.GetAngle(base.transform.position, PlayerFarming.Instance.transform.position));
			DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
			{
				GameManager.GetInstance().CamFollowTarget.targetDistance = x;
			}, 6f, 2f).SetEase(Ease.InOutSine);
			yield return new WaitForSeconds(2f);
			LoopInstance = AudioManager.Instance.CreateLoop("event:/followers/consume_loop", base.gameObject, true);
			yield return new WaitForSeconds(1f);
			GameManager.GetInstance().CamFollowTarget.targetDistance += 2f;
			StartCoroutine(SpawnSouls(sacraficeFollower.Follower.transform.position, 0.2f, 0.05f));
			yield return new WaitForSeconds(3f);
			PlayerFarming.Instance.transform.DOMove(FollowerPosition.position, 0.5f).SetEase(Ease.OutSine);
		}
		else
		{
			bool waitForPlayer = true;
			int num2 = ((!(sacraficeFollower.Follower.transform.position.x > PlayerFarming.Instance.transform.position.x)) ? 1 : (-1));
			PlayerFarming.Instance.GoToAndStop(sacraficeFollower.Follower.transform.position + new Vector3(num2, 0f, 0f) * 2f, sacraficeFollower.Follower.gameObject, false, false, delegate
			{
				waitForPlayer = false;
				PlayerFarming.Instance.state.LookAngle = (PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, sacraficeFollower.Follower.transform.position));
			});
			while (waitForPlayer)
			{
				yield return null;
			}
			yield return new WaitForEndOfFrame();
			AudioManager.Instance.PlayOneShot("event:/followers/consume_start", base.gameObject);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			float duration = PlayerFarming.Instance.simpleSpineAnimator.Animate("sacrifice-short", 0, false).Animation.Duration;
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("warp-out-down", 0, true, 0f);
			sacraficeFollower.Follower.SetBodyAnimation("sacrifice", false);
			sacraficeFollower.Follower.State.LookAngle = (sacraficeFollower.Follower.State.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position));
			DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
			{
				GameManager.GetInstance().CamFollowTarget.targetDistance = x;
			}, 6f, 2f).SetEase(Ease.InOutSine);
			yield return new WaitForSeconds((duration - 1.5f) / 2f);
			LoopInstance = AudioManager.Instance.CreateLoop("event:/followers/consume_loop", base.gameObject, true);
			yield return new WaitForSeconds((duration - 1.5f) / 2f);
			StartCoroutine(SpawnSouls(sacraficeFollower.Follower.transform.position, 0.025f, 0.015f));
			GameManager.GetInstance().CamFollowTarget.targetDistance += 2f;
		}
		yield return new WaitForSeconds(0.5f);
		AudioManager.Instance.StopLoop(LoopInstance);
		AudioManager.Instance.PlayOneShot("event:/followers/consume_end", base.gameObject);
		UnityEngine.Object.Destroy(sacraficeFollower.Follower.gameObject);
		StartCoroutine(FollowerConsumed());
		DataManager.Instance.First_Dungeon_Resurrecting = false;
	}

	private IEnumerator SpectatorCheer(Follower follower)
	{
		yield return new WaitForEndOfFrame();
		follower.State.LookAngle = (follower.State.facingAngle = Utils.GetAngle(follower.transform.position, FollowerPosition.transform.position));
		follower.SetBodyAnimation("cheer", true);
	}

	private IEnumerator FollowerConsumed()
	{
		yield return new WaitForSeconds(0.5f);
		AstarPath.active = astarPath;
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 1f, "", delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			GameManager.GetInstance().StartCoroutine(RespawnRoutine());
		});
	}

	public void Respawn()
	{
		GameManager.GetInstance().StartCoroutine(RespawnRoutine());
	}

	private IEnumerator RespawnRoutine()
	{
		AudioManager.Instance.PlayOneShot("event:/player/resurrect");
		Respawning = true;
		AstarPath.active = astarPath;
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().CachedCamTargets = new List<CameraFollowTarget.Target>();
		AudioManager.Instance.PlayMusic(BiomeGenerator.Instance.biomeMusicPath);
		AudioManager.Instance.SetMusicRoomID(BiomeGenerator.Instance.CurrentRoom.generateRoom.roomMusicID);
		Instance = null;
		UnityEngine.Object.Destroy(Player);
		Player = null;
		biomeGenerator.gameObject.SetActive(true);
		biomeGenerator.Player.SetActive(true);
		biomeGenerator.Player.GetComponent<StateMachine>().CURRENT_STATE = StateMachine.State.Resurrecting;
		yield return null;
		GameManager.GetInstance().CameraSnapToPosition(biomeGenerator.Player.transform.position);
		GameManager.GetInstance().AddToCamera(PlayerFarming.Instance.CameraBone);
		yield return null;
		LightingOverride.SetActive(false);
		GameManager.GetInstance().OnConversationNew(false);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 6f);
		biomeGenerator.CurrentRoom.generateRoom.SetColliderAndUpdatePathfinding();
		biomeGenerator.SpawnDemons();
		HUD_Manager.Instance.Show(0);
		base.gameObject.SetActive(false);
		FaithAmmo.Reload();
		PlayerRelic.Reload();
		Interaction_Chest instance = Interaction_Chest.Instance;
		if ((object)instance != null && instance.MyState == Interaction_Chest.State.Hidden)
		{
			RoomLockController.CloseAll();
		}
	}

	private IEnumerator SpawnSouls(Vector3 fromPosition, float startingDelay, float min)
	{
		float delay = startingDelay;
		for (int i = 0; i < 30; i++)
		{
			float time = (float)i / 30f;
			delay = Mathf.Clamp(delay * (1f - absorbSoulCurve.Evaluate(time)), min, float.MaxValue);
			SoulCustomTarget.Create(PlayerFarming.Instance.gameObject, fromPosition, Color.red, null, 0.2f, 100f * (1f + absorbSoulCurve.Evaluate(time)));
			yield return new WaitForSeconds(delay);
		}
	}

	public void ResetPathFinding()
	{
		AstarPath.active = astarPath;
	}

	private void OnDisable()
	{
        if (cameraStylizer)
            cameraStylizer.enabled = false;
		foreach (FollowerManager.SpawnedFollower follower in followers)
		{
			FollowerManager.CleanUpCopyFollower(follower);
		}
	}

	private IEnumerator PlayRoutine()
	{
		GameManager.GetInstance().RemoveAllFromCamera();
		GameManager.GetInstance().CachedCamTargets = new List<CameraFollowTarget.Target>();
		Instance.generateRoom.SetColliderAndUpdatePathfinding();
		Instance.biomeGenerator.gameObject.SetActive(false);
		Instance.biomeGenerator.Player.SetActive(false);
		yield return null;
		HealthPlayer.ResetHealthData = false;
		Player = UnityEngine.Object.Instantiate(PlayerPrefab, FollowerPosition.position, Quaternion.identity, base.transform);
		Player.GetComponent<Health>().untouchable = true;
		GameManager.GetInstance().CameraSnapToPosition(Player.transform.position);
		GameManager.GetInstance().AddToCamera(PlayerFarming.Instance.CameraBone);
		yield return null;
		GameManager.GetInstance().OnConversationNew(false, true);
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone, 8f);
		StateMachine component = Player.GetComponent<StateMachine>();
		component.facingAngle = 85f;
		component.CURRENT_STATE = StateMachine.State.SpawnIn;
		PlayerFarming.Instance.playerController.SpawnInShowHUD = false;
		SpawnFollowers();
		yield return new WaitForSeconds(1f);
		if (DataManager.Instance.First_Dungeon_Resurrecting)
		{
			conversation.enabled = true;
		}
		yield return null;
		GameManager.GetInstance().CamFollowTarget.DisablePlayerLook = true;
		yield return new WaitForSeconds(3f);
		yield return null;
	}
}
