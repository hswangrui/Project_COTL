using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lamb.UI;
using MMTools;
using Spine.Unity;
using UnityEngine;

public class SpiderBossIntroRitual : BaseMonoBehaviour
{
	[SerializeField]
	private bool skipIntro;

	[Space]
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

	[SerializeField]
	private GameObject transformParticles;

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
	private MiniBossController spiderBoss;

	[SerializeField]
	private Health spiderBossHealth;

	[SerializeField]
	private GameObject[] environmentTraps;

	private LongGrass[] surroundingGrass;

	public GameObject BloodPortalEffect;

	private Texture originalLut;

	private Texture originalHighlightLut;

	private AmplifyColorEffect amplifyColorEffect;

	private int camZoom = 10;

	private bool triggered;

	public Color bloodColor = new Color(0.47f, 0.11f, 0.11f, 1f);

	private bool skippable;

	private bool skipped;

	private List<Tween> tweens = new List<Tween>();

	public GameObject offsetObject;

	private string term1
	{
		get
		{
			if (GameManager.Layer2)
			{
				return "Conversation_NPC/Story/Dungeon4/Leader1/Boss1_Layer2";
			}
			return "Conversation_NPC/Story/Dungeon1/Leader4/Boss1";
		}
	}

	private string term2
	{
		get
		{
			if (GameManager.Layer2)
			{
				return "Conversation_NPC/Story/Dungeon4/Leader1/Boss2_Layer2";
			}
			return "Conversation_NPC/Story/Dungeon1/Leader4/Boss2";
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
		surroundingGrass = base.transform.parent.GetComponentsInChildren<LongGrass>();
		amplifyColorEffect = Camera.main.GetComponent<AmplifyColorEffect>();
		string skin = (GameManager.Layer2 ? "Beaten" : "Mask");
		cultLeaderSpine.Skeleton.SetSkin(skin);
		spiderBossHealth.untouchable = true;
		array2 = environmentTraps;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetActive(GameManager.Layer2);
		}
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
		MMConversation mmConversation = MMConversation.mmConversation;
		if ((object)mmConversation != null)
		{
			mmConversation.Close();
		}
		LetterBox.Instance.HideSkipPrompt();
		cultLeaderSpine.AnimationState.SetAnimation(0, "mutate", false).TrackTime = 10.25f;
		AudioManager.Instance.PlayOneShot("event:/boss/spider/roar");
		GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		GameManager.GetInstance().StartCoroutine(IntroDone());
		skipped = true;
		spiderBossHealth.untouchable = false;
		spiderBoss.EnemiesToTrack[0].enabled = true;
	}

	private void Start()
	{
		if (DungeonSandboxManager.Active)
		{
			cultLeaderSpine.AnimationState.SetAnimation(0, "idle-boss", true);
			SkeletonAnimation[] array = enemySpines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
			array = surroundingSpines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
			spiderBoss.EnemiesToTrack[0].enabled = false;
		}
		else
		{
			cultLeaderSpine.AnimationState.SetAnimation(0, "idle", true);
		}
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
		if (DungeonSandboxManager.Active)
		{
			skippable = true;
			yield break;
		}
		SkeletonAnimation[] array;
		if (!skipIntro)
		{
			if (!DungeonSandboxManager.Active)
			{
				cultLeaderSpine.AnimationState.SetAnimation(0, "talk", true);
			}
			AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_intro_zoom");
			List<ConversationEntry> list = new List<ConversationEntry>
			{
				new ConversationEntry(offsetObject, term1),
				new ConversationEntry(offsetObject, term2)
			};
			list[0].CharacterName = "NAMES/CultLeaders/Dungeon4";
			list[0].Offset = new Vector3(0f, 2f, 0f);
			list[0].Animation = "talk";
			list[0].SkeletonData = cultLeaderSpine;
			list[1].CharacterName = "NAMES/CultLeaders/Dungeon4";
			list[1].Offset = new Vector3(0f, 2f, 0f);
			list[1].Animation = "talk";
			list[1].SkeletonData = cultLeaderSpine;
			foreach (ConversationEntry item in list)
			{
				item.soundPath = "event:/dialogue/dun4_cult_leader_shamura/standard_shamura";
			}
			MMConversation.Play(new ConversationObject(list, null, null), false, true, false, true, true, false, false);
			MMConversation.mmConversation.SpeechBubble.ScreenOffset = 350f;
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
			cultLeaderSpine.AnimationState.SetAnimation(0, "idle", true);
			Object.Destroy(offsetObject);
			yield return new WaitForEndOfFrame();
			AudioManager.Instance.PlayOneShot("event:/boss/frog/transition_zoom_back");
			AudioManager.Instance.PlayOneShot("event:/boss/frog/after_intro_grunt");
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(cameraTarget, 12f);
			yield return new WaitForSeconds(1f);
			AudioManager.Instance.PlayOneShot("event:/boss/frog/cultist_sequence");
			array = enemySpines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].GetComponentInChildren<WorshipperBubble>(true).Play(WorshipperBubble.SPEECH_TYPE.BOSSCROWN4, 4.5f, Random.Range(0f, 0.3f));
			}
			array = enemySpines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AnimationState.SetAnimation(0, "dance", true).TrackTime = Random.Range(0f, 0.3f);
			}
			yield return new WaitForSeconds(3f);
			array = enemySpines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AnimationState.SetAnimation(0, "ritual/sacrifice", false).TrackTime = Random.Range(0f, 0.3f);
			}
			yield return new WaitForSeconds(1f);
			transformParticles.SetActive(true);
			cultLeaderSpine.AnimationState.SetAnimation(0, "mutate", true);
			AudioManager.Instance.PlayOneShot("event:/boss/spider/transform_sequence", base.gameObject);
			yield return new WaitForSeconds(0.3f);
			array = surroundingSpines;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AnimationState.SetAnimation(0, "worship", true).TrackTime = Random.Range(0f, 0.4f);
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
			GameManager.GetInstance().OnConversationNext(cameraTarget, 10f);
			tweens.Add(DOTween.To(() => GameManager.GetInstance().CamFollowTarget.targetDistance, delegate(float x)
			{
				GameManager.GetInstance().CamFollowTarget.targetDistance = x;
			}, 6f, 6f).SetEase(Ease.InSine));
			Camera.main.GetComponent<AmplifyColorEffect>().BlendTo(lutTexture, 6f, null);
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
			yield return new WaitForSeconds(4f);
			BloodPortalEffect.transform.DOScale(Vector3.zero, 2f);
		}
		CameraManager.instance.StopAllCoroutines();
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNext(cultLeader, 12f);
		distortionObject.DOScale(50f, 5f).SetEase(Ease.Linear).OnComplete(delegate
		{
			Object.Destroy(distortionObject.gameObject);
		});
		AmplifyColorEffect component = Camera.main.GetComponent<AmplifyColorEffect>();
		component.LutHighlightTexture = originalHighlightLut;
		component.LutTexture = originalLut;
		CameraManager.instance.ShakeCameraForDuration(1.5f, 2f, 0.1f);
		BiomeConstants.Instance.ImpactFrameForDuration();
		MMVibrate.Haptic(MMVibrate.HapticTypes.HeavyImpact);
		yield return new WaitForSeconds(0.1f);
		array = enemySpines;
		foreach (SkeletonAnimation skeletonAnimation in array)
		{
			string[] array4 = new string[3] { "BloodImpact_0", "BloodImpact_1", "BloodImpact_2" };
			int num = Random.Range(0, array4.Length - 1);
			if (array4[num] != null)
			{
				BiomeConstants.Instance.EmitBloodImpact(skeletonAnimation.transform.position + Vector3.back * 0.5f, Random.Range(0, 360), "black", array4[num]);
			}
			BiomeConstants.Instance.EmitBloodSplatterGroundParticles(skeletonAnimation.transform.position, Vector3.zero, bloodColor);
			skeletonAnimation.gameObject.SetActive(false);
			SpawnDeadBody(skeletonAnimation.transform.position, skeletonAnimation.transform.localScale);
		}
		yield return new WaitForSeconds(0.15f);
		array = surroundingSpines;
		foreach (SkeletonAnimation skeletonAnimation2 in array)
		{
			string[] array5 = new string[3] { "BloodImpact_0", "BloodImpact_1", "BloodImpact_2" };
			int num2 = Random.Range(0, array5.Length - 1);
			if (array5[num2] != null)
			{
				BiomeConstants.Instance.EmitBloodImpact(skeletonAnimation2.transform.position + Vector3.back * 0.5f, Random.Range(0, 360), "black", array5[num2]);
			}
			BiomeConstants.Instance.EmitBloodSplatterGroundParticles(skeletonAnimation2.transform.position, Vector3.zero, bloodColor);
			skeletonAnimation2.gameObject.SetActive(false);
			SpawnDeadBody(skeletonAnimation2.transform.position, skeletonAnimation2.transform.localScale);
		}
		yield return new WaitForSeconds(0.15f);
		LongGrass[] array6 = surroundingGrass;
		foreach (LongGrass obj in array6)
		{
			obj.StartCoroutine(obj.ShakeGrassRoutine(base.gameObject, 2f));
		}
		yield return new WaitForSeconds(2.3f);
		StartCoroutine(IntroDone());
	}

	private IEnumerator IntroDone()
	{
		skippable = false;
		LetterBox.Instance.HideSkipPrompt();
		GameManager.GetInstance().OnConversationNext(cultLeader, 16f);
		CameraManager.instance.ShakeCameraForDuration(1.5f, 2f, 0.7f);
		cultLeaderSpine.AnimationState.AddAnimation(0, "idle-boss", true, 0f);
		yield return new WaitForSeconds(2.5f);
		yield return new WaitForEndOfFrame();
		spiderBoss.Play();
		spiderBossHealth.untouchable = false;
		yield return new WaitForSeconds(0.1f);
		GameManager.GetInstance().OnConversationEnd();
		spiderBoss.GetComponentInChildren<EnemySpiderMonster>().Play();
		if (!DataManager.Instance.BossesEncountered.Contains(FollowerLocation.Dungeon1_4))
		{
			DataManager.Instance.BossesEncountered.Add(FollowerLocation.Dungeon1_4);
		}
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

	private void SpawnDeadBody(Vector3 pos, Vector3 scale)
	{
		Object.Instantiate(deathParticlePrefab, pos, Quaternion.identity, base.transform.parent);
		DeadBodySliding deadBodySliding = Object.Instantiate(enemyBody, pos, Quaternion.identity, base.transform.parent);
		deadBodySliding.Init(base.gameObject, Random.Range(0f, 360f), Random.Range(500, 1000));
		deadBodySliding.transform.localScale = scale;
	}
}
