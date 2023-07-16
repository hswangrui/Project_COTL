using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_PermanentHeart : Interaction
{
	private Vector3 BookTargetPosition;

	private float Timer;

	public SpriteRenderer Shadow;

	public ParticleSystem Particles;

	public SpriteRenderer Image;

	private void Start()
	{
		UpdateLocalisation();
		Particles.Stop();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		base.Label = ScriptLocalization.Inventory.RED_HEART;
	}

	private IEnumerator PlayerPickUpBook()
	{
		Timer = 0f;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		BookTargetPosition = new Vector3(component.ItemImage.transform.position.x, component.ItemImage.transform.position.y, -1f);
		Shadow.enabled = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		Particles.Play();
		AudioManager.Instance.PlayOneShot("event:/Stings/Choir", base.transform.position);
		while ((Timer += Time.deltaTime) < 2.5f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		base.transform.position = BookTargetPosition;
		Particles.Stop();
		Image.enabled = false;
		state.CURRENT_STATE = StateMachine.State.Idle;
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(0.5f);
		IncreaseHP();
		Object.Destroy(base.gameObject);
	}

	private void IncreaseHP()
	{
		HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
		component.totalHP += 1f;
		component.HP = component.totalHP;
		DataManager.Instance.RedHeartShrineLevel++;
		DataManager.Instance.PLAYER_HEALTH_MODIFIED++;
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		CameraManager.shakeCamera(1f, Random.Range(0, 360));
		BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
		BiomeConstants.Instance.EmitBloodImpact(base.transform.position, 0f, "red", "BloodImpact_Large_0");
		StartCoroutine(PlayerPickUpBook());
		Interactable = false;
	}
}
