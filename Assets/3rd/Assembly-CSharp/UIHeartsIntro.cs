using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;

public class UIHeartsIntro : MonoBehaviour
{
	[SerializeField]
	private SkeletonGraphic[] hearts;

	public IEnumerator HeartRoutine()
	{
		int heartsCount = DataManager.Instance.PLAYER_STARTING_HEALTH / 2;
		for (int k = 0; k < hearts.Length; k++)
		{
			hearts[k].transform.localScale = Vector3.zero;
			if (k >= heartsCount)
			{
				hearts[k].gameObject.SetActive(false);
			}
		}
		while (HUD_Hearts.Instance == null)
		{
			yield return null;
		}
		HUD_Hearts.Instance.GetComponent<CanvasGroup>().alpha = 0f;
		while (PlayerFarming.Instance == null || PlayerFarming.Instance.GoToAndStopping)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.LookAngle = 0f;
		PlayerFarming.Instance.state.facingAngle = 0f;
		GameManager.GetInstance().OnConversationNew(true, false, false);
		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(1f);
		for (int j = 0; j < heartsCount; j++)
		{
			hearts[j].transform.localScale = Vector3.one * 2.7f;
			hearts[j].AnimationState.SetAnimation(0, "fill-whole", false);
			AudioManager.Instance.PlayOneShot("event:/player/collect_heart");
			yield return new WaitForSeconds(0.25f);
		}
		HUD_Manager.Instance.Show(0);
		for (int j = 0; j < heartsCount; j++)
		{
			StartCoroutine(AnimateHeart(hearts[j], j));
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForSeconds(1f);
		SkeletonGraphic[] array = hearts;
		for (int l = 0; l < array.Length; l++)
		{
			array[l].gameObject.SetActive(false);
		}
		HUD_Hearts.Instance.GetComponent<CanvasGroup>().alpha = 1f;
		GameManager.GetInstance().OnConversationEnd();
		Object.Destroy(base.gameObject);
	}

	private IEnumerator AnimateHeart(SkeletonGraphic heart, int index)
	{
		heart.transform.DOScale(heart.transform.localScale * 1.25f, 0.5f).SetEase(Ease.OutBack);
		yield return new WaitForSeconds(0.1f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
		yield return new WaitForSeconds(0.2f);
		heart.transform.DOMove(HUD_Hearts.Instance.HeartIcons[index].transform.position, 0.5f).SetEase(Ease.InOutSine);
		yield return new WaitForSeconds(0.6f);
		heart.transform.DOScale(Vector3.one * 0.8f, 0.5f).SetEase(Ease.InOutBack);
		yield return new WaitForSeconds(0.1f);
		AudioManager.Instance.PlayOneShot("event:/ui/level_node_beat_level");
		yield return new WaitForSeconds(0.3f);
		CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.3f);
	}
}
