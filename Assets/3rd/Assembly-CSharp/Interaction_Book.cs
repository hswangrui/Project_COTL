using System.Collections;
using UnityEngine;

public class Interaction_Book : Interaction
{
	public ParticleSystem Particles;

	public SpriteRenderer Shadow;

	public GameObject BuildMenuTutorial;

	public GameObject BookImage;

	private Vector3 BookTargetPosition;

	private float Timer;

	private void Start()
	{
		Particles.Stop();
		if (DataManager.Instance.BuildingTome)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		base.Label = "";
		StartCoroutine(PlayerPickUpBook());
	}

	private IEnumerator PlayerPickUpBook()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		BookTargetPosition = component.ItemImage.transform.position;
		Shadow.enabled = false;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		Particles.Play();
		while ((Timer += Time.deltaTime) < 2.5f)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		Particles.Stop();
		End();
	}

	private void End()
	{
		StartCoroutine(EndCoroutine());
	}

	private IEnumerator EndCoroutine()
	{
		yield return new WaitForSeconds(0.5f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		BookImage.SetActive(false);
		yield return new WaitForSeconds(0.5f);
		DataManager.Instance.BuildingTome = true;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Canvas");
		Object.Instantiate(BuildMenuTutorial, gameObject.transform).GetComponent<BuildMenuTutorial>().Target = state.transform;
		Object.Destroy(base.gameObject);
	}
}
