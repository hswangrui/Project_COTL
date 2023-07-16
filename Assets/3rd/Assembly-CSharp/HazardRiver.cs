using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FMOD.Studio;
using UnityEngine;

public class HazardRiver : BaseMonoBehaviour
{
	private struct RiverPathPosition
	{
		public Vector3 position;

		public Vector3 direction;

		public float distance;

		public int id;
	}

	[SerializeField]
	private Transform riverPathTransform;

	[SerializeField]
	private float waterSpeed = 1f;

	[Range(0f, 1f)]
	private float _waterSpeedMultiplier = 1f;

	public LineRenderer riverPath;

	public LineRenderer bankPath;

	public LineRenderer bankEdgePath;

	public WaterInteractionRiver waterInteractionRiver;

	private bool _riverDriedUp;

	public bool generateEdgeDressing;

	public GameObject edgeDressingPrefab;

	public float dressingGap = 0.75f;

	public float dressingScaleMin = 0.8f;

	public float dressingScaleRange = 0.4f;

	public float dressingXRange = 0.1f;

	public float dressingYRange = 0.1f;

	public Vector3 dressingOffset = Vector3.zero;

	private EventInstance loopedSound;

	private RiverPathPosition[] riverPathPositions;

	public Animator waterScrollAnimator;

	public float colliderWidthMultiplier = 0.6f;

	private static readonly int AnimatedOffsetUVX1 = Shader.PropertyToID("AnimatedOffsetUV_X_1");

	private float MaxDistanceSFX = 15f;

	private float soundOffset;

	private bool foundPlayer;

	private bool inTrigger;

	public bool playerInWater;

	public float SFXIntensity;

	private float waterSpeedMultiplier
	{
		get
		{
			return _waterSpeedMultiplier;
		}
		set
		{
			if (value != _waterSpeedMultiplier)
			{
				riverPath.materials[2].SetFloat(AnimatedOffsetUVX1, value * waterSpeed * -5f);
			}
			_waterSpeedMultiplier = value;
		}
	}

	private bool riverDriedUp
	{
		get
		{
			return _riverDriedUp;
		}
		set
		{
			_riverDriedUp = value;
			if (!_riverDriedUp)
			{
				riverPath.materials[2].SetFloat(AnimatedOffsetUVX1, 0f);
			}
			else
			{
				riverPath.materials[2].SetFloat(AnimatedOffsetUVX1, waterSpeed * -5f);
			}
		}
	}

	private void OnEnable()
	{
		StartCoroutine(WaitForPlayerLoop());
	}

	private void OnDisable()
	{
		AudioManager.Instance.StopLoop(loopedSound);
	}

	private void Awake()
	{
		if (riverPath == null)
		{
			riverPath = riverPathTransform.GetComponent<LineRenderer>();
		}
		riverPathPositions = new RiverPathPosition[riverPath.positionCount];
		new List<Vector2>();
		for (int num = riverPath.positionCount - 1; num >= 0; num--)
		{
			RiverPathPosition riverPathPosition = default(RiverPathPosition);
			riverPathPosition.id = num;
			riverPathPosition.position = riverPath.GetPosition(num);
			if (num < riverPath.positionCount - 1)
			{
				riverPathPosition.direction = Vector3.Normalize(riverPathPositions[num + 1].position - riverPathPosition.position);
			}
			riverPathPositions[num] = riverPathPosition;
		}
		riverPath.materials[2].SetFloat(AnimatedOffsetUVX1, waterSpeed * -5f);
	}

	private void FixedUpdate()
	{
		if (!inTrigger && foundPlayer)
		{
			if (PlayerRelic.TimeFrozen)
			{
				waterSpeedMultiplier = 0f;
			}
			else
			{
				waterSpeedMultiplier = 1f;
			}
			if (playerInWater)
			{
				soundOffset = 0f;
			}
			else
			{
				soundOffset = 0.25f;
			}
			float num = Vector3.Distance(PlayerFarming.Instance.transform.position, base.transform.position) / MaxDistanceSFX * -1f + 1f;
			if (num < MaxDistanceSFX)
			{
				SFXIntensity = num - soundOffset;
				AudioManager.Instance.SetEventInstanceParameter(loopedSound, "river_intensity", SFXIntensity);
			}
		}
	}

	private void Start()
	{
		RoomLockController.OnRoomCleared += RoomLockController_OnRoomCleared;
	}

	private IEnumerator WaitForPlayerLoop()
	{
		while (PlayerFarming.Instance == null)
		{
			yield return null;
		}
		loopedSound = AudioManager.Instance.CreateLoop("event:/atmos/misc/river_loop", base.gameObject, true);
		foundPlayer = true;
	}

	private void OnDestroy()
	{
		AudioManager.Instance.StopLoop(loopedSound);
		RoomLockController.OnRoomCleared -= RoomLockController_OnRoomCleared;
	}

	private void RoomLockController_OnRoomCleared()
	{
		inTrigger = false;
		AudioManager.Instance.SetEventInstanceParameter(loopedSound, "river_intensity", 0f);
		riverPath.DOKill();
		DOTween.To(() => riverPath.widthMultiplier, delegate(float x)
		{
			riverPath.widthMultiplier = x;
		}, 0f, 2f).SetEase(Ease.InQuart).OnComplete(delegate
		{
			riverDriedUp = true;
			Object.Destroy(waterInteractionRiver);
			AudioManager.Instance.StopLoop(loopedSound);
		});
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!riverDriedUp)
		{
			UnitObject component = collision.GetComponent<UnitObject>();
			if (component != null && component.health != null && component.health.team == Health.Team.PlayerTeam)
			{
				playerInWater = false;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!riverDriedUp)
		{
			UnitObject component = collision.GetComponent<UnitObject>();
			if (component != null && component.health.team == Health.Team.PlayerTeam)
			{
				playerInWater = true;
			}
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (riverDriedUp)
		{
			return;
		}
		UnitObject unit = collision.GetComponent<UnitObject>();
		if (unit != null && !unit.isFlyingEnemy)
		{
			RiverPathPosition[] array = riverPathPositions.OrderBy((RiverPathPosition rp) => Vector3.Distance(rp.position, unit.transform.position)).ToArray();
			RiverPathPosition riverPathPosition = array[0];
			RiverPathPosition riverPathPosition2 = array[1];
			RiverPathPosition riverPathPosition3 = ((riverPathPosition.id >= riverPathPosition2.id) ? riverPathPosition2 : riverPathPosition);
			unit.transform.position += riverPathPosition3.direction * (waterSpeed * 3f * _waterSpeedMultiplier) * Time.deltaTime;
		}
	}
}
