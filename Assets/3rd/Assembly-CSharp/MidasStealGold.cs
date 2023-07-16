using System.Collections;
using DG.Tweening;
using MMTools;
using Spine.Unity;
using Unify;
using UnityEngine;

public class MidasStealGold : MonoBehaviour
{
	[SerializeField]
	private SkeletonAnimation spine;

	[SerializeField]
	private SimpleSpineFlash simpleSpineFlash;

	[SerializeField]
	private Interaction_SimpleConversation[] conversations;

	[SerializeField]
	private SimpleBark bark;

	private bool goldStolen;

	private int stolenGold = -1;

	private void Awake()
	{
		GetComponent<Health>().OnDamaged += MidasStealGold_OnDamaged;
		for (int i = 0; i < conversations.Length; i++)
		{
			conversations[i].gameObject.SetActive(DataManager.Instance.MidasSpecialEncounter == i);
		}
		DataManager.Instance.ShowSpecialMidasRoom = false;
	}

	private void MidasStealGold_OnDamaged(GameObject attacker, Vector3 attackLocation, float damage, Health.AttackTypes attackType, Health.AttackFlags attackFlag)
	{
		simpleSpineFlash.FlashFillRed();
		if (stolenGold == -1)
		{
			stolenGold = DataManager.Instance.MidasTotalGoldStolen;
		}
		if (DataManager.Instance.MidasTotalGoldStolen > 0)
		{
			if (spine.AnimationState.GetCurrent(0).Animation.Name != "drop-money")
			{
				spine.AnimationState.SetAnimation(0, "drop-money", false);
				spine.AnimationState.AddAnimation(0, "idle", true, 0f);
				foreach (ConversationEntry entry in bark.Entries)
				{
					entry.Animation = "drop-money";
					entry.LoopAnimation = false;
				}
			}
			int num = Mathf.Min(25, DataManager.Instance.MidasTotalGoldStolen);
			DataManager.Instance.MidasTotalGoldStolen -= Mathf.RoundToInt((float)stolenGold / 4f);
			for (int i = 0; i < num; i++)
			{
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, base.transform.position, 0f).SetInitialSpeedAndDiraction(5f, Random.Range(0, 360));
			}
		}
		else
		{
			DataManager.Instance.MidasBeaten = true;
			AchievementsWrapper.UnlockAchievement(Achievements.Instance.Lookup("BEAT_UP_MIDAS"));
			Debug.Log("ACHIEVEMENT GOT : BEAT_UP_MIDAS");
			spine.Skeleton.SetSkin("Beaten");
			foreach (ConversationEntry entry2 in bark.Entries)
			{
				entry2.Animation = "talk";
				entry2.LoopAnimation = false;
			}
		}
		if (!bark.gameObject.activeSelf)
		{
			bark.gameObject.SetActive(true);
		}
		else if (MMConversation.CURRENT_CONVERSATION == null)
		{
			bark.Show();
		}
	}

	public void StealGold()
	{
		if (!goldStolen)
		{
			goldStolen = true;
			StartCoroutine(StealGoldIE());
		}
	}

	private IEnumerator StealGoldIE()
	{
		DataManager.Instance.MidasSpecialEncounteredLocations.Add(PlayerFarming.Location);
		DataManager.Instance.MidasSpecialEncounter++;
		spine.AnimationState.SetAnimation(0, "steal-money", true);
		Vector3 startingPosition = base.transform.position;
		base.transform.DOMove(PlayerFarming.Instance.transform.position + Vector3.right * 1.5f, 1f).SetEase(Ease.InSine);
		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(0.25f);
		base.transform.DOMove(PlayerFarming.Instance.transform.position, 0.15f).SetEase(Ease.Linear);
		AudioManager.Instance.PlayOneShot("event:/player/gethit", base.transform.position);
		BiomeConstants.Instance.EmitHitVFX(PlayerFarming.Instance.transform.position, Quaternion.identity.z, "HitFX_Blocked");
		PlayerFarming.Instance.simpleSpineAnimator.FlashRedTint();
		CameraManager.instance.ShakeCameraForDuration(1.5f, 1.7f, 0.2f);
		PlayerFarming.Instance.state.facingAngle = Utils.GetAngle(PlayerFarming.Instance.transform.position, base.transform.position);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.HitThrown;
		yield return new WaitForSeconds(0.15f);
		base.transform.DOMove(PlayerFarming.Instance.transform.position + Vector3.right * 1.5f, 0.15f).SetEase(Ease.Linear);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "reactions/react-angry", false, 0f);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "idle", false, 0f);
		yield return new WaitForSeconds(0.15f);
		int itemQuantity = Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD);
		int num = ((itemQuantity >= 1000) ? Mathf.CeilToInt((float)itemQuantity / 1.5f) : Mathf.CeilToInt((float)itemQuantity / 10f));
		DataManager.Instance.MidasTotalGoldStolen += num;
		Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -num);
		for (int i = 0; i < 5; i++)
		{
			PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, PlayerFarming.Instance.transform.position, 0f);
			pickUp.SetInitialSpeedAndDiraction(5f, Random.Range(0, 360));
			pickUp.Player = base.gameObject;
			pickUp.MagnetToPlayer = true;
			pickUp.AddToInventory = false;
			yield return new WaitForSeconds(0.05f);
		}
		AudioManager.Instance.PlayOneShot("event:/dialogue/midas/standard_midas", base.transform.position);
		AudioManager.Instance.PlayOneShot("event:/Stings/gamble_lose", base.transform.position);
		yield return new WaitForSeconds(0.5f);
		spine.transform.localScale = new Vector3(-1f, 1f, 1f);
		spine.AnimationState.SetAnimation(0, "run", true);
		base.transform.DOMove(startingPosition, 0.5f).SetEase(Ease.Linear);
		yield return new WaitForSeconds(0.5f);
		if (DataManager.Instance.MidasSpecialEncounter >= 3)
		{
			spine.transform.localScale = new Vector3(1f, 1f, 1f);
			spine.AnimationState.SetAnimation(0, "searching", true);
			GetComponent<Health>().enabled = true;
		}
		else
		{
			spine.AnimationState.SetAnimation(0, "exit", false);
			yield return new WaitForSeconds(1f);
			spine.gameObject.SetActive(false);
		}
		GameManager.GetInstance().OnConversationEnd();
	}
}
