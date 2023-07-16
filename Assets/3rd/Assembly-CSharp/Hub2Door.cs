using System.Collections;
using UnityEngine;

public class Hub2Door : BaseMonoBehaviour
{
	public BoxCollider2D Collider;

	public GameObject ShakeObject;

	private void Start()
	{
		Debug.Log("DataManager.Instance.BaseDoorEast " + DataManager.Instance.BaseDoorEast);
		Debug.Log("DataManager.Instance.Chain1 " + DataManager.Instance.Chain1);
		if (!DataManager.Instance.BaseDoorEast && DataManager.Instance.Chain1)
		{
			StartCoroutine(PlayRoutine());
		}
		else if (DataManager.Instance.BaseDoorEast)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator PlayRoutine()
	{
		Debug.Log("PLAY ROUTINE!");
		yield return new WaitForSeconds(3f);
		SimpleSetCamera.DisableAll();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		yield return new WaitForSeconds(2f);
		CameraManager.shakeCamera(0.5f);
		Collider.enabled = false;
		ShakeObject.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		GameManager.GetInstance().OnConversationEnd();
		SimpleSetCamera.EnableAll();
		DataManager.Instance.BaseDoorEast = true;
		Object.Destroy(base.gameObject);
	}
}
