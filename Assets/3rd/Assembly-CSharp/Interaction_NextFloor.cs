using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_NextFloor : Interaction
{
	public SpriteRenderer spriteRenderer;

	private bool AlreadyUnlocked;

	private int Cost = 1;

	public GameObject darkness;

	public GoopFade GoopFade;

	private string sNextLayer;

	public GameObject BlockingCollider;

	private bool _playedSFX;

	private bool Activating;

	private void Start()
	{
		GoopFade.gameObject.SetActive(false);
		AutomaticallyInteract = true;
		UpdateLocalisation();
		AlreadyUnlocked = false;
	}

	private void DestroyEverything()
	{
		Object.Destroy(darkness);
		Object.Destroy(base.gameObject);
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sNextLayer = ScriptLocalization.Interactions.UnlockDoor;
	}

	public override void GetLabel()
	{
		base.Label = (Activating ? "" : sNextLayer);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		BlockingCollider.SetActive(false);
		GoopFade.gameObject.SetActive(true);
		GoopFade.FadeIn();
		AudioManager.Instance.PlayOneShot("event:/enter_leave_buildings/enter_building", base.transform.position);
		DestroyEverything();
	}

	public override void OnEnableInteraction()
	{
		base.OnEnableInteraction();
		StartCoroutine(PlayOneShot());
	}

	private IEnumerator PlayOneShot()
	{
		yield return new WaitForSeconds(0.33f);
		if (!_playedSFX)
		{
			AudioManager.Instance.PlayOneShot("event:/Stings/end_floor", PlayerFarming.Instance.gameObject);
			_playedSFX = true;
		}
	}

	private IEnumerator PaySoulsRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		CameraManager.shakeCamera(0.5f);
		BlockingCollider.SetActive(false);
		DestroyEverything();
		GameManager.GetInstance().OnConversationEnd();
	}
}
