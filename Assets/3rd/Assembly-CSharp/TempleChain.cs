using System.Collections;
using MMTools;
using UnityEngine;

public class TempleChain : BaseMonoBehaviour
{
	public DataManager.Chains Chain;

	private void OnEnable()
	{
		switch (Chain)
		{
		case DataManager.Chains.Chain1:
			base.gameObject.SetActive(!DataManager.Instance.Chain1);
			break;
		case DataManager.Chains.Chain2:
			base.gameObject.SetActive(!DataManager.Instance.Chain2);
			break;
		case DataManager.Chains.Chain3:
			base.gameObject.SetActive(!DataManager.Instance.Chain3);
			break;
		}
	}

	public void Play()
	{
		StartCoroutine(PlayRoutine());
		StartCoroutine(FadeRoutine());
	}

	private IEnumerator PlayRoutine()
	{
		yield return new WaitForSeconds(0.5f);
		GameManager.GetInstance().OnConversationNext(base.gameObject, 10f);
		float Progress = 0f;
		float Duration = 5f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (num < Duration)
			{
				base.transform.localPosition = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.1f, 0.1f), -0.01f);
				GameManager.GetInstance().CameraSetZoom(10f - 4f * Progress / Duration);
				CameraManager.shakeCamera(0.1f + 0.6f * (Progress / Duration));
				yield return null;
				continue;
			}
			break;
		}
	}

	private IEnumerator FadeRoutine()
	{
		yield return new WaitForSeconds(2.5f);
		switch (Chain)
		{
		case DataManager.Chains.Chain1:
			DataManager.Instance.Chain1 = true;
			break;
		case DataManager.Chains.Chain2:
			DataManager.Instance.Chain2 = true;
			break;
		case DataManager.Chains.Chain3:
			DataManager.Instance.Chain3 = true;
			break;
		}
		GameManager.ToShip("Base Biome 1", 3f, MMTransition.Effect.WhiteFade);
	}
}
