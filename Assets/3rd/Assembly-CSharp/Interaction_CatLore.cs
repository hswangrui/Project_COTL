using System.Collections;
using DG.Tweening;
using MMTools;
using Spine;
using Spine.Unity;
using Unify;
using UnityEngine;

public class Interaction_CatLore : MonoBehaviour
{
	[SerializeField]
	private Interaction_SimpleConversation loreConvo;

	[SerializeField]
	private Interaction_SimpleConversation returnFirstChildConvo;

	[SerializeField]
	private Interaction_SimpleConversation returnFirstChildConvoB;

	[SerializeField]
	private Interaction_SimpleConversation returnSecondChildConvo;

	[SerializeField]
	private Interaction_SimpleConversation returnSecondChildConvoB;

	[SerializeField]
	private Interaction_SimpleConversation returnedAfterChildren;

	[SerializeField]
	private GameObject bark;

	[SerializeField]
	private SkeletonAnimation catSpine;

	[SerializeField]
	private SkeletonAnimation baalSpine;

	[SerializeField]
	private SkeletonAnimation aymSpine;

	[SerializeField]
	private GameObject[] podiums;

	private void Start()
	{
		if (DataManager.Instance.ForneusLore || !DataManager.Instance.BeatenDungeon2)
		{
			Object.Destroy(loreConvo.gameObject);
		}
		if (DungeonSandboxManager.Active)
		{
			Object.Destroy(returnFirstChildConvo.gameObject);
			Object.Destroy(returnFirstChildConvoB.gameObject);
			Object.Destroy(returnSecondChildConvo.gameObject);
			Object.Destroy(returnSecondChildConvoB.gameObject);
			Object.Destroy(returnedAfterChildren.gameObject);
			Object.Destroy(baalSpine.gameObject);
			Object.Destroy(aymSpine.gameObject);
			return;
		}
		returnedAfterChildren.gameObject.SetActive(false);
		if (DataManager.Instance.HasReturnedAym && DataManager.Instance.HasReturnedBaal)
		{
			if (!DataManager.Instance.HasReturnedBoth)
			{
				DataManager.Instance.HasReturnedBoth = true;
				returnedAfterChildren.gameObject.SetActive(true);
			}
			Object.Destroy(baalSpine.gameObject);
			Object.Destroy(aymSpine.gameObject);
		}
		bool flag = DataManager.Instance.Followers_Demons_Types.Contains(6);
		bool flag2 = DataManager.Instance.Followers_Demons_Types.Contains(7);
		if (DataManager.Instance.HasReturnedBaal || !flag || DataManager.Instance.HasReturnedAym || !flag2)
		{
			if (GetChildrenReturned() >= 1)
			{
				Object.Destroy(returnFirstChildConvo.gameObject);
				Object.Destroy(returnFirstChildConvoB.gameObject);
				returnFirstChildConvo = null;
			}
			if (GetChildrenReturned() >= 2)
			{
				Object.Destroy(returnSecondChildConvo.gameObject);
				Object.Destroy(returnSecondChildConvoB.gameObject);
				returnSecondChildConvo = null;
			}
		}
		if (!flag && !flag2)
		{
			if (returnFirstChildConvo != null)
			{
				Object.Destroy(returnFirstChildConvo.gameObject);
				Object.Destroy(returnFirstChildConvoB.gameObject);
				returnFirstChildConvo = null;
			}
			if (returnSecondChildConvo != null)
			{
				Object.Destroy(returnSecondChildConvo.gameObject);
				Object.Destroy(returnSecondChildConvoB.gameObject);
				returnSecondChildConvo = null;
			}
		}
		baalSpine.gameObject.SetActive(DataManager.Instance.HasReturnedBaal);
		baalSpine.AnimationState.AddAnimation(0, "Forneus/idle-baal", true, 0f);
		aymSpine.gameObject.SetActive(DataManager.Instance.HasReturnedAym);
		aymSpine.AnimationState.AddAnimation(0, "Forneus/idle-aym", true, 0f);
		foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance.GetColourData("Boss Baal").SlotAndColours[0].SlotAndColours)
		{
			Slot slot = baalSpine.Skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
		foreach (WorshipperData.SlotAndColor slotAndColour2 in WorshipperData.Instance.GetColourData("Boss Aym").SlotAndColours[0].SlotAndColours)
		{
			Slot slot2 = aymSpine.Skeleton.FindSlot(slotAndColour2.Slot);
			if (slot2 != null)
			{
				slot2.SetColor(slotAndColour2.color);
			}
		}
		if (returnFirstChildConvo != null && (bool)returnSecondChildConvo)
		{
			returnSecondChildConvo.gameObject.SetActive(false);
			returnSecondChildConvoB.gameObject.SetActive(false);
		}
		if (returnFirstChildConvo != null || returnSecondChildConvo != null)
		{
			GameObject[] array = podiums;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
		}
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			Demon_Spirit component = demon.GetComponent<Demon_Spirit>();
			if (component != null && (component.FollowerInfo.ID == FollowerManager.BaalID || component.FollowerInfo.ID == FollowerManager.AymID))
			{
				component.ForceMove = true;
			}
		}
	}

	private int GetChildrenReturned()
	{
		int num = 0;
		if (DataManager.Instance.HasReturnedAym)
		{
			num++;
		}
		if (DataManager.Instance.HasReturnedBaal)
		{
			num++;
		}
		return num;
	}

	public void GiveChild()
	{
		bark.gameObject.SetActive(false);
		if (DataManager.Instance.Followers_Demons_Types.Contains(6) && !DataManager.Instance.HasReturnedBaal)
		{
			DataManager.Instance.HasReturnedBaal = true;
			StartCoroutine(GiveDemon(true));
		}
		else if (DataManager.Instance.Followers_Demons_Types.Contains(7) && !DataManager.Instance.HasReturnedAym)
		{
			DataManager.Instance.HasReturnedAym = true;
			StartCoroutine(GiveDemon(false));
		}
	}

	private IEnumerator GiveDemon(bool isBaal)
	{
		yield return new WaitForEndOfFrame();
		int num = (isBaal ? 6 : 7);
		int num2 = (isBaal ? FollowerManager.BaalID : FollowerManager.AymID);
		Demon_Spirit demon = null;
		foreach (GameObject demon2 in Demon_Arrows.Demons)
		{
			Demon_Spirit component = demon2.GetComponent<Demon_Spirit>();
			if (component != null && component.FollowerInfo.ID == num2)
			{
				demon = component;
				break;
			}
		}
		int num3 = 0;
		foreach (int followers_Demons_Type in DataManager.Instance.Followers_Demons_Types)
		{
			if (followers_Demons_Type == num)
			{
				int iD = DataManager.Instance.Followers_Demons_IDs[num3];
				DataManager.Instance.Followers_Demons_Types.RemoveAt(num3);
				DataManager.Instance.Followers_Demons_IDs.RemoveAt(num3);
				FollowerManager.RemoveFollowerBrain(iD);
				break;
			}
			num3++;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(catSpine.gameObject);
		demon.enabled = false;
		demon.spine.transform.DOMove(isBaal ? baalSpine.transform.position : aymSpine.transform.position, 1f).SetEase(Ease.Linear);
		AudioManager.Instance.PlayOneShot("event:/Stings/refuse_kneel_sting");
		yield return new WaitForSeconds(1f);
		AudioManager.Instance.PlayOneShot("event:/door/goop_door_unlock");
		demon.gameObject.SetActive(false);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(isBaal ? baalSpine.transform.position : aymSpine.transform.position);
		BiomeConstants.Instance.ShakeCamera();
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
		if (isBaal)
		{
			baalSpine.gameObject.SetActive(true);
			yield return new WaitForEndOfFrame();
			baalSpine.AnimationState.SetAnimation(0, "Forneus/greet-baal", false);
			baalSpine.AnimationState.AddAnimation(0, "Forneus/idle-baal", true, 0f);
		}
		else
		{
			aymSpine.gameObject.SetActive(true);
			yield return new WaitForEndOfFrame();
			aymSpine.AnimationState.SetAnimation(0, "Forneus/greet-aym", false);
			aymSpine.AnimationState.AddAnimation(0, "Forneus/idle-aym", true, 0f);
		}
		catSpine.AnimationState.SetAnimation(0, "greet", false);
		catSpine.AnimationState.AddAnimation(0, "animation", true, 0f);
		GameManager.GetInstance().OnConversationNext(isBaal ? baalSpine.gameObject : aymSpine.gameObject, 5f);
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/shop_cat_forneus/buy_forneus");
		yield return new WaitForSeconds(1.5f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/shop_cat_forneus/buy_forneus");
		yield return new WaitForSeconds(2f);
		AudioManager.Instance.PlayOneShot("event:/dialogue/shop_cat_forneus/buy_forneus");
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(1f);
		while (MMConversation.CURRENT_CONVERSATION != null)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject, 7f);
		bool waiting = true;
		RelicType relicType = (isBaal ? RelicType.HeartConversion_Blessed : RelicType.HeartConversion_Dammed);
		RelicCustomTarget.Create(catSpine.transform.position, PlayerFarming.Instance.transform.position, 1f, relicType, delegate
		{
			waiting = false;
		});
		while (waiting)
		{
			yield return null;
		}
		GameManager.GetInstance().OnConversationEnd();
		if (GetChildrenReturned() < 2 && ((DataManager.Instance.Followers_Demons_Types.Contains(6) && !DataManager.Instance.HasReturnedBaal) || (DataManager.Instance.Followers_Demons_Types.Contains(7) && !DataManager.Instance.HasReturnedAym)))
		{
			returnSecondChildConvo.gameObject.SetActive(true);
			returnSecondChildConvoB.gameObject.SetActive(true);
		}
		else
		{
			GameObject[] array = podiums;
			foreach (GameObject gameObject in array)
			{
				gameObject.gameObject.SetActive(true);
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 2f);
				gameObject.transform.DOLocalMoveZ(0f, 1f);
			}
		}
		if (DataManager.Instance.HasReturnedAym && DataManager.Instance.HasReturnedBaal)
		{
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("RETURN_BAAL_AYM"));
			Debug.Log("ACHIEVEMENT GOT : RETURN_BAAL_AYM");
		}
	}
}
