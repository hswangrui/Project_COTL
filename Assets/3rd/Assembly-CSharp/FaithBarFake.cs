using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FaithBarFake : MonoBehaviour
{
	[SerializeField]
	private BarController barController;

	[SerializeField]
	private GameObject lockParent;

	[SerializeField]
	private GameObject lockIcon;

	private bool hiding;

	public static void Play(float faithAmount)
	{
		float newFaithNormalised = (CultFaithManager.CurrentFaith + faithAmount) / 85f;
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/UI/Faith Bar Fake.prefab", GameObject.FindGameObjectWithTag("Canvas").transform);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			FaithBarFake component = obj.Result.GetComponent<FaithBarFake>();
			component.transform.localScale = Vector3.zero;
			component.transform.DOScale(1f, 0.5f).SetEase(Ease.InOutBack).SetUpdate(true);
			component.barController.SetBarSize(FollowerBrainStats.BrainWashed ? 1f : CultFaithManager.CultFaithNormalised, false, true);
			component.StartCoroutine(component.SequenceIE(newFaithNormalised));
			component.lockParent.SetActive(FollowerBrainStats.BrainWashed);
		};
	}

	private void Update()
	{
		if (!HUD_Manager.Instance.Hidden && !hiding)
		{
			StopAllCoroutines();
			StartCoroutine(SequenceOut());
			hiding = true;
		}
	}

	private IEnumerator SequenceOut()
	{
		base.transform.DOScale(0f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
		yield return new WaitForSecondsRealtime(1f);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator SequenceIE(float newFaithNormalised)
	{
		yield return new WaitForSecondsRealtime(1f);
		if (lockParent.activeSelf)
		{
			lockIcon.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
		}
		else
		{
			barController.SetBarSize(newFaithNormalised, true, true);
		}
		yield return new WaitForSecondsRealtime(2f);
		base.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
		yield return new WaitForSecondsRealtime(1f);
		Object.Destroy(base.gameObject);
	}
}
