using System.Collections;
using I2.Loc;
using UnityEngine;

public class Interaction_CultDoor : Interaction
{
	public Transform ShakeObject;

	public GameObject KeyToShow;

	public Collider2D collider;

	public float ShakeAmount = 0.2f;

	public float v1 = 0.4f;

	public float v2 = 0.7f;

	private bool Activated;

	private string sOpenDoor;

	private void Start()
	{
		UpdateLocalisation();
	}

	public override void UpdateLocalisation()
	{
		base.UpdateLocalisation();
		sOpenDoor = ScriptLocalization.Interactions.OpenDoor;
	}

	public override void GetLabel()
	{
		base.Label = (Activated ? "" : sOpenDoor);
	}

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		StopAllCoroutines();
	}

	private IEnumerator OpenDoor()
	{
		Activated = true;
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(state.gameObject, 5f);
		PlayerSimpleInventory component = state.gameObject.GetComponent<PlayerSimpleInventory>();
		Vector3 BookTargetPosition = component.ItemImage.transform.position;
		state.CURRENT_STATE = StateMachine.State.FoundItem;
		KeyToShow.SetActive(true);
		float Timer = 0f;
		while (true)
		{
			float num;
			Timer = (num = Timer + Time.deltaTime);
			if (!(num < 2.5f))
			{
				break;
			}
			KeyToShow.transform.position = Vector3.Lerp(KeyToShow.transform.position, BookTargetPosition, 5f * Time.deltaTime);
			yield return null;
		}
		state.CURRENT_STATE = StateMachine.State.InActive;
		state.facingAngle = 90f;
		KeyToShow.SetActive(false);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 6f);
		yield return new WaitForSeconds(0.5f);
		float Progress = 0f;
		float Duration = 3f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			CameraManager.shakeCamera(Random.Range(0.15f, 0.2f), Random.Range(0, 360));
			Vector3 localPosition = ShakeObject.localPosition;
			localPosition.z = 2f * (Progress / Duration);
			ShakeObject.localPosition = localPosition;
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		state.CURRENT_STATE = StateMachine.State.Idle;
		collider.enabled = false;
		GameManager.GetInstance().OnConversationEnd();
		DataManager.Instance.SetVariable(DataManager.Variables.Goat_Guardian_Door_Open, true);
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
