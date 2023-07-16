using System.Collections;
using I2.Loc;
using MMBiomeGeneration;
using UnityEngine;

public class Interaction_DungeonKey : Interaction
{
	private string sLabelName;

	private bool Activated;

	public SpriteRenderer Shadow;

	public SpriteRenderer Image;

	public ParticleSystem GodRays;

	public GameObject PlayerPosition;

	private Vector3 BookTargetPosition;

	private float Timer;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sLabelName = ScriptLocalization.Interactions.KeyPiece;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sLabelName);
	}

	public override void OnInteract(StateMachine state)
	{
		if (!Activated)
		{
			Activated = true;
			base.OnInteract(state);
			StartCoroutine(PlayerPickUpBook());
		}
	}

	private IEnumerator PlayerPickUpBook()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 6f);
		if (BiomeGenerator.Instance != null)
		{
			BiomeGenerator.Instance.HasKey = true;
		}
		PlayerFarming.Instance.GoToAndStop(PlayerPosition, base.gameObject);
		while (PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.2f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = component.ItemImage.transform.position;
		Shadow.enabled = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		while ((Timer += Time.deltaTime) < 1f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		GodRays.Stop();
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}
}
