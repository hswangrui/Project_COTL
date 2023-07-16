using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_SoulDoor : Interaction
{
	public enum DoorTag
	{
		BaseDoorNorthEast,
		BaseDoorNorthWest,
		BossForest,
		ForestTempleDoor,
		ShrineDoor
	}

	public DoorTag MyDoorTag;

	public SimpleSetCamera SetCamera;

	private bool Activating;

	private bool Closing;

	public int Cost = 5;

	public float OpeningTime = 1f;

	public BoxCollider2D Collider;

	public GameObject PlayerGoTo;

	public GameObject DevotionTarget;

	private GameObject Player;

	private float Delay;

	private int SoulsInTheAir;

	public float ShakeAmount = 0.2f;

	public float v1 = 0.4f;

	public float v2 = 0.7f;

	public Transform ShakeObject;

	public int CurrentCount
	{
		get
		{
			return (int)typeof(DataManager).GetField(string.Concat(MyDoorTag, "_Count")).GetValue(DataManager.Instance);
		}
		set
		{
			typeof(DataManager).GetField(string.Concat(MyDoorTag, "_Count")).SetValue(DataManager.Instance, value);
		}
	}

	private void Start()
	{
		ContinuouslyHold = true;
		if ((bool)typeof(DataManager).GetField(MyDoorTag.ToString()).GetValue(DataManager.Instance))
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override void GetLabel()
	{
		base.Label = ((CurrentCount >= Cost) ? "" : (ScriptLocalization.Interactions.OpenDoor + " <sprite name=\"icon_spirits\"> x" + (Cost - CurrentCount) + " (" + Inventory.Souls + ")"));
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activating && !Closing)
		{
			base.OnInteract(state);
			if (Inventory.Souls < Cost)
			{
				MonoSingleton<Indicator>.Instance.PlayShake();
			}
			else
			{
				PayResources();
			}
		}
	}

	private void GetSoul()
	{
		SoulsInTheAir--;
		CurrentCount++;
		if (CurrentCount >= Cost)
		{
			Closing = true;
			CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, state.transform.position));
			StopAllCoroutines();
			PlayerFarming.Instance.GoToAndStop(PlayerGoTo, ShakeObject.gameObject, false, false, Activate);
		}
	}

	private new void Update()
	{
		if (!((Player = GameObject.FindWithTag("Player")) == null) && !Closing && Activating)
		{
			if ((Delay -= Time.deltaTime) < 0f && Activating && CurrentCount + SoulsInTheAir < Cost)
			{
				SoulCustomTarget.Create(DevotionTarget, state.gameObject.transform.position, Color.white, GetSoul);
				PlayerFarming.Instance.GetSoul(-1);
				Delay = 0.2f;
				SoulsInTheAir++;
			}
			if (CurrentCount >= Cost || Inventory.Souls <= 0 || InputManager.Gameplay.GetInteractButtonUp() || Vector3.Distance(base.transform.position, Player.transform.position) > ActivateDistance)
			{
				Activating = false;
			}
		}
	}

	private void Activate()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 7f);
		StartCoroutine(UnlockDoorRoutine());
	}

	private void PayResources()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 7f);
		Closing = true;
		StopAllCoroutines();
		PlayerFarming.Instance.GoToAndStop(PlayerGoTo, ShakeObject.gameObject, false, false, Activate);
	}

	public virtual IEnumerator UnlockDoorRoutine()
	{
		Closing = true;
		int i = 0;
		if (Cost > 20)
		{
			Cost = 20;
		}
		while (true)
		{
			int num = i + 1;
			i = num;
			if (num > Cost)
			{
				break;
			}
			SoulCustomTarget.Create(ShakeObject.gameObject, state.gameObject.transform.position, Color.white, null);
			PlayerFarming.Instance.GetSoul(-1);
			yield return new WaitForSeconds(0.1f - 0.1f * (float)(i / Cost));
		}
		yield return new WaitForSeconds(0.5f);
		Debug.Log("ORIGINAL!");
		SetCamera.Play();
		yield return new WaitForSeconds(SetCamera.Duration + 0.5f);
		typeof(DataManager).GetField(MyDoorTag.ToString()).SetValue(DataManager.Instance, true);
		CameraManager.shakeCamera(0.3f, Utils.GetAngle(base.transform.position, state.transform.position));
		Collider.enabled = false;
		ShakeObject.gameObject.SetActive(false);
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		SetCamera.Reset();
		Object.Destroy(base.gameObject);
	}

	private IEnumerator DoShake()
	{
		float Timer = 0f;
		float ShakeSpeed2 = ShakeAmount;
		float Shake = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (num < 3f)
			{
				ShakeSpeed2 += (0f - Shake) * v1;
				float num2 = Shake;
				ShakeSpeed2 = (num = ShakeSpeed2 * v2);
				Shake = num2 + num;
				ShakeObject.localPosition = Vector3.left * Shake;
				yield return null;
				continue;
			}
			break;
		}
	}
}
