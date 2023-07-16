using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI;
using MMTools;
using Spine;
using Spine.Unity;
using UnityEngine;

public class WormBossIntroRitual : BaseMonoBehaviour
{
	[SerializeField]
	private GameObject cultLeader;

	[SerializeField]
	private GameObject cameraTarget;

	[SerializeField]
	private SkeletonAnimation cultLeaderSpine;

	[SerializeField]
	private Renderer[] blood;

	[SerializeField]
	private GameObject bloodParticle;

	[SerializeField]
	private Renderer symbols;

	[SerializeField]
	private AnimationCurve absorbSoulCurve;

	[SerializeField]
	private Texture2D lutTexture;

	[SerializeField]
	private Transform distortionObject;

	[Space]
	[SerializeField]
	private DeadBodySliding enemyBody;

	[SerializeField]
	private GameObject deathParticlePrefab;

	[SerializeField]
	private GameObject[] bloodSprays;

	[SerializeField]
	private SkeletonAnimation[] enemySpines;

	[SerializeField]
	private SkeletonAnimation[] surroundingSpines;

	[Space]
	[SerializeField]
	private MiniBossController frogBoss;

	[SerializeField]
	private GameObject[] environmentTraps;

	private LongGrass[] surroundingGrass;

	public GameObject BloodPortalEffect;

	private Texture originalLut;

	private Texture originalHighlightLut;

	private int camZoom = 10;

	private bool triggered;

	public Color bloodColor = new Color(0.47f, 0.11f, 0.11f, 1f);

	private bool skippable;

	private bool skipped;

	private List<Tween> tweens = new List<Tween>();

	private Camera mainCamera;

	private AmplifyColorEffect amplifyColorEffect;

	public GameObject offsetObject;

	private string term1
	{
		get
		{
			if (GameManager.Layer2)
			{
				return "Conversation_NPC/Story/Dungeon1/Leader1/Boss1_Layer2";
			}
			return "Conversation_NPC/Story/Dungeon1/Leader1/Boss1";
		}
	}

	private string term2
	{
		get
		{
			if (GameManager.Layer2)
			{
				return "Conversation_NPC/Story/Dungeon1/Leader1/Boss2_Layer2";
			}
			return "Conversation_NPC/Story/Dungeon1/Leader1/Boss2";
		}
	}

	private void Awake()
	{
		Renderer[] array = blood;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].material.SetFloat("_BlotchMultiply", 0f);
		}
		symbols.material.SetFloat("_BlotchMultiply", 0f);
		GameObject[] array2 = bloodSprays;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetActive(false);
		}
		cultLeaderSpine.AnimationState.Event += LeaderEvent;
		surroundingGrass = base.transform.parent.GetComponentsInChildren<LongGrass>();
		amplifyColorEffect = Camera.main.GetComponent<AmplifyColorEffect>();
		string skin = (GameManager.Layer2 ? "Beaten" : "Normal");
		frogBoss.BossIntro.GetComponentInChildren<SkeletonAnimation>().Skeleton.SetSkin(skin);
		cultLeaderSpine.Skeleton.SetSkin(skin);
		if (DungeonSandboxManager.Active)
		{
			cultLeader.gameObject.SetActive(false);
			frogBoss.gameObject.SetActive(true);
			frogBoss.BossIntro.GetComponentInChildren<SkeletonAnimation>().AnimationState.AddAnimation(0, "idle", true, 0f);
			SkeletonAnimation[] array3 = enemySpines;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].gameObject.SetActive(false);
			}
			array3 = surroundingSpines;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].gameObject.SetActive(false);
			}
			frogBoss.EnemiesToTrack[0].enabled = false;
		}
		array2 = environmentTraps;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetActive(GameManager.Layer2);
		}
		mainCamera = Camera.main;
		amplifyColorEffect = mainCamera.GetComponent<AmplifyColorEffect>();
	}

	private void Update()
	{
		if (!(PlayerFarming.Instance != null) || skipped || !skippable || MonoSingleton<UIManager>.Instance.MenusBlocked || (!InputManager.Gameplay.GetAttackButtonDown() && !DungeonSandboxManager.Active))
		{
			return;
		}
		StopAllCoroutines();
		base.gameObject.SetActive(false);
		CameraManager.instance.Stopshake();
		foreach (Tween tween in tweens)
		{
			tween.Kill();
		}
		frogBoss.gameObject.SetActive(true);
		frogBoss.Play(true);
		AudioManager.Instance.PlayOneShot("event:/boss/worm/roar", frogBoss.gameObject);
		MMConversation mmConversation = MMConversation.mmConversation;
		if ((object)mmConversation != null)
		{
			mmConversation.Close();
		}
		LetterBox.Instance.HideSkipPrompt();
		AmplifyColorEffect obj = amplifyColorEffect;
		obj.LutHighlightTexture = originalHighlightLut;
		obj.LutTexture = originalLut;
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		skipped = true;
		frogBoss.EnemiesToTrack[0].enabled = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!triggered && !skipped && collision.tag == "Player")
		{
			StartCoroutine(RitualRoutine());
		}
	}

	private IEnumerator RitualRoutine()
	{
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		SimulationManager.Pause();
		HUD_Manager.Instance.HideTopRight();
		originalHighlightLut = amplifyColorEffect.LutHighlightTexture;
		originalLut = amplifyColorEffect.LutTexture;
		amplifyColorEffect.LutHighlightBlendTexture = lutTexture;
		triggered = true;
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom");
		if (DungeonSandboxManager.Active)
		{
			skippable = true;
			yield break;
		}
		List<ConversationEntry> list = new List<ConversationEntry>();
		list.Add(new ConversationEntry(offsetObject, term1));
		list.Add(new ConversationEntry(offsetObject, term2));
		list[0].CharacterName = "NAMES/CultLeaders/Dungeon1";
		list[0].Offset = new Vector3(0f, 2f, 0f);
		list[0].soundPath = "event:/dialogue/dun1_cult_leader_leshy/standard_leshy";
		list[0].Animation = "talk";
		list[0].SkeletonData = cultLeaderSpine;
		list[1].CharacterName = "NAMES/CultLeaders/Dungeon1";
		list[1].Offset = new Vector3(0f, 2f, 0f);
		list[1].soundPath = "event:/dialogue/dun1_cult_leader_leshy/standard_leshy";
		list[1].Animation = "talk";
		list[1].SkeletonData = cultLeaderSpine;
		cultLeaderSpine.AnimationState.SetAnimation(0, "talk", true);
		MMConversation.Play(new ConversationObject(list, null, null), false, true, false, true, true, false, false);
		MMConversation.mmConversation.SpeechBubble.ScreenOffset = 200f;
		yield return new WaitForSeconds(1f);
		skippable = DataManager.Instance.BossesEncountered.Contains(PlayerFarming.Location);
		if (skippable && !skipped)
		{
			LetterBox.Instance.ShowSkipPrompt();
		}
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		Object.Destroy(offsetObject);
		yield return new WaitForEndOfFrame();
		AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_zoom_back");
		AudioManager.Instance.PlayOneShot("event:/boss/frog/after_intro_grunt");
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(cultLeaderSpine.gameObject, 12f);
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/boss/frog/cultist_sequence");
		SkeletonAnimation[] array = enemySpines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponentInChildren<WorshipperBubble>(true).Play(WorshipperBubble.SPEECH_TYPE.BOSSCROWN1, 4.5f, Random.Range(0f, 0.3f));
		}
		array = enemySpines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AnimationState.SetAnimation(0, "ritual/preach2", true).TrackTime = Random.Range(0f, 0.15f);
		}
		yield return new WaitForSeconds(2.8f);
		array = enemySpines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AnimationState.SetAnimation(0, "ritual/sacrifice2", false).TrackTime = Random.Range(0f, 0.15f);
		}
		yield return new WaitForSeconds(1f);
		cultLeaderSpine.AnimationState.SetAnimation(0, "transform", false);
		AudioManager.Instance.PlayOneShot("event:/boss/worm/transform");
		yield return new WaitForSeconds(0.3f);
		array = surroundingSpines;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AnimationState.SetAnimation(0, "ritual/preach", true).TrackTime = Random.Range(0f, 0.4f);
		}
		GameObject[] array2 = bloodSprays;
		foreach (GameObject gameObject in array2)
		{
			gameObject.SetActive(true);
			AudioManager.Instance.PlayOneShot("event:/enemy/impact_squishy", gameObject.gameObject);
			float z = Vector3.Angle(gameObject.transform.position, cultLeader.transform.position);
			BiomeConstants.Instance.EmitBloodSplatter(gameObject.transform.position, new Vector3(0f, 0f, z), Color.black);
		}
		Renderer[] array3 = blood;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].material.DOFloat(4f, "_BlotchMultiply", 4f).SetEase(Ease.InSine);
		}
		GameManager.GetInstance().OnConversationNext(cultLeaderSpine.gameObject, 10f);
		tweens.Add(DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
		{
			GameManager.GetInstance().CamFollowTarget.targetDistance = x;
		}, 6f, 6f).SetEase(Ease.InSine));
		tweens.Add(DOTween.To(() => GameManager.GetInstance().CamFollowTarget.TargetOffset, delegate(Vector3 x)
		{
			GameManager.GetInstance().CamFollowTarget.TargetOffset = x;
		}, Vector3.forward * -2f, 6f).SetEase(Ease.InSine));
		amplifyColorEffect.BlendTo(lutTexture, 6f, null);
		for (int j = 0; j < enemySpines.Length; j++)
		{
			StartCoroutine(SpawnSouls(enemySpines[j].transform.position));
		}
		yield return new WaitForSeconds(1f);
		bloodParticle.SetActive(true);
		yield return new WaitForSeconds(2f);
		symbols.material.DOFloat(5f, "_BlotchMultiply", 5f);
		BloodPortalEffect.transform.DOScale(new Vector3(5.3f, 2f, 5.3f), 2f);
		CameraManager.instance.ShakeCameraForDuration(1f, 1.5f, 10f);
		BiomeConstants.Instance.ImpactFrameForDuration();
		MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact);
		yield return new WaitForSeconds(4f);
		BloodPortalEffect.transform.DOScale(Vector3.zero, 2f);
	}

	private IEnumerator SpawnSouls(Vector3 position)
	{
		float delay = 0.3f;
		int ParticleCount = 50;
		for (int i = 0; i < ParticleCount; i++)
		{
			float time = (float)i / (float)ParticleCount;
			delay *= 1f - absorbSoulCurve.Evaluate(time);
			SoulCustomTarget.Create(cameraTarget, new Vector3(position.x, position.y, position.z + 1f), Color.red, null, 0.2f, 100f * (1f + absorbSoulCurve.Evaluate(time))).transform.parent = base.transform;
			yield return new WaitForSeconds(delay);
		}
	}

	private IEnumerator BossTransformed()
	{
		skippable = false;
		cultLeader.SetActive(false);
		frogBoss.gameObject.SetActive(true);
		frogBoss.Play();
		GameManager.GetInstance().CamFollowTarget.TargetOffset = Vector3.zero;
		distortionObject.DOScale(50f, 5f).SetEase(Ease.Linear).OnComplete(delegate
		{
			Object.Destroy(distortionObject.gameObject);
		});
		AmplifyColorEffect obj = amplifyColorEffect;
		obj.LutHighlightTexture = originalHighlightLut;
		obj.LutTexture = originalLut;
		CameraManager.instance.StopAllCoroutines();
		yield return new WaitForEndOfFrame();
		CameraManager.instance.ShakeCameraForDuration(1.5f, 2f, 0.3f);
		yield return new WaitForSeconds(0.1f);
		SkeletonAnimation[] array = enemySpines;
		foreach (SkeletonAnimation skeletonAnimation in array)
		{
			string[] array2 = new string[3] { "BloodImpact_0", "BloodImpact_1", "BloodImpact_2" };
			int num = Random.Range(0, array2.Length - 1);
			if (array2[num] != null)
			{
				BiomeConstants.Instance.EmitBloodImpact(skeletonAnimation.transform.position + Vector3.back * 0.5f, Random.Range(0, 360), "black", array2[num]);
			}
			BiomeConstants.Instance.EmitBloodSplatterGroundParticles(skeletonAnimation.transform.position, Vector3.zero, bloodColor);
			skeletonAnimation.gameObject.SetActive(false);
			SpawnDeadBody(skeletonAnimation.transform.position, skeletonAnimation.transform.localScale);
		}
		yield return new WaitForSeconds(0.15f);
		array = surroundingSpines;
		foreach (SkeletonAnimation skeletonAnimation2 in array)
		{
			string[] array3 = new string[3] { "BloodImpact_0", "BloodImpact_1", "BloodImpact_2" };
			int num2 = Random.Range(0, array3.Length - 1);
			if (array3[num2] != null)
			{
				BiomeConstants.Instance.EmitBloodImpact(skeletonAnimation2.transform.position + Vector3.back * 0.5f, Random.Range(0, 360), "black", array3[num2]);
			}
			BiomeConstants.Instance.EmitBloodSplatterGroundParticles(skeletonAnimation2.transform.position, Vector3.zero, bloodColor);
			skeletonAnimation2.gameObject.SetActive(false);
			SpawnDeadBody(skeletonAnimation2.transform.position, skeletonAnimation2.transform.localScale);
		}
		yield return new WaitForSeconds(0.15f);
		LongGrass[] array4 = surroundingGrass;
		foreach (LongGrass obj2 in array4)
		{
			obj2.StartCoroutine(obj2.ShakeGrassRoutine(base.gameObject, 2f));
		}
	}

	private void LeaderEvent(TrackEntry trackEntry, Spine.Event e)
	{
		if (e.Data.Name == "transform")
		{
			StartCoroutine(BossTransformed());
		}
	}

	private void SpawnDeadBody(Vector3 pos, Vector3 scale)
	{
		Object.Instantiate(deathParticlePrefab, pos, Quaternion.identity, base.transform.parent);
		DeadBodySliding deadBodySliding = Object.Instantiate(enemyBody, pos, Quaternion.identity, base.transform.parent);
		deadBodySliding.Init(base.gameObject, Random.Range(0f, 360f), Random.Range(500, 1000));
		deadBodySliding.transform.localScale = scale;
	}
}
