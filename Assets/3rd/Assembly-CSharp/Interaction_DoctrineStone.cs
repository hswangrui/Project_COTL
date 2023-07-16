using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;

public class Interaction_DoctrineStone : Interaction
{
	public static int SpritePiece;

	public SpriteRenderer SpriteRenderer;

	public List<Sprite> Sprites = new List<Sprite>();

	public UnityEvent Callback;

	public static List<Interaction_DoctrineStone> DoctrineStones = new List<Interaction_DoctrineStone>();

	public bool EnableAutoCollect = true;

	private string sDoctrine;

	private string sPickUp;

	public Action OnCollect;

	private void Start()
	{
		ActivateDistance = 2f;
		UpdateLocalisation();
		SetSprite();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		DoctrineStones.Add(this);
	}

	public override void OnDisableInteraction()
	{
		base.OnDisableInteraction();
		DoctrineStones.Remove(this);
	}

	protected override void Update()
	{
		base.Update();
		if (EnableAutoCollect && PlayerFarming.Instance != null && Vector3.Distance(base.transform.position, PlayerFarming.Instance.transform.position) < ActivateDistance / 3f && Interactable)
		{
			OnInteract(PlayerFarming.Instance.state);
		}
	}

	private void SetSprite()
	{
		SpriteRenderer.sprite = Sprites[SpritePiece];
		if (++SpritePiece >= 3)
		{
			SpritePiece = 0;
		}
	}

	public override void UpdateLocalisation()
	{
		sDoctrine = ScriptLocalization.Inventory.DOCTRINE_STONE;
		sPickUp = ScriptLocalization.Interactions.PickUp;
	}

	public override void GetLabel()
	{
		base.Label = sPickUp + " " + sDoctrine;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		Collect();
	}

	private void Collect()
	{
		DataManager.Instance.DoctrineStoneTotalCount++;
		CameraManager.instance.ShakeCameraForDuration(0.7f, 0.9f, 0.3f);
		PlayerDoctrineStone.Play(1);
		UnityEvent callback = Callback;
		if (callback != null)
		{
			callback.Invoke();
		}
		Action onCollect = OnCollect;
		if (onCollect != null)
		{
			onCollect();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void MagnetToPlayer()
	{
		StartCoroutine(IMagnetToPlayer());
		Debug.Log("magnet to player".Colour(Color.green));
	}

	private IEnumerator IMagnetToPlayer()
	{
		yield return new WaitForSeconds(0.5f);
		PickUp component = GetComponent<PickUp>();
		component.MagnetDistance = 100f;
		component.AddToInventory = false;
		component.MagnetToPlayer = true;
		component.Callback.AddListener(Collect);
		AutomaticallyInteract = true;
	}
}
