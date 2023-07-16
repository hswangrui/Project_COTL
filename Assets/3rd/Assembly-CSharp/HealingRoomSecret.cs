using System.Collections;
using DG.Tweening;
using UnityEngine;

public class HealingRoomSecret : MonoBehaviour
{
	public const int Spirit = 5;

	public const int Collector = 3;

	[SerializeField]
	private Interaction_SimpleConversation wrongHeartConvo;

	[SerializeField]
	private Interaction_SimpleConversation rightHeartConvo;

	[SerializeField]
	private Interaction_SimpleConversation gaveHeartConvo;

	[SerializeField]
	private SimpleBark bark;

	[SerializeField]
	private SimpleBark barkAlternate;

	[SerializeField]
	private Transform _moveTarget;

	[SerializeField]
	private RatooCheckSkin _ratooCheckSkin;

	private bool _gotCollector;

	private bool _gotSpirit;

	private Demon_Collector demon;

	private void OnEnable()
	{
		CheckStatus();
		if (DataManager.Instance.EncounteredHealingRoom)
		{
			if ((bool)barkAlternate)
			{
				barkAlternate.gameObject.SetActive(DataManager.Instance.RatauKilled && (bool)barkAlternate);
			}
			if ((bool)bark)
			{
				bark.gameObject.SetActive(!DataManager.Instance.RatauKilled || !barkAlternate);
			}
		}
		else
		{
			if ((bool)bark)
			{
				bark.gameObject.SetActive(false);
			}
			if ((bool)barkAlternate)
			{
				barkAlternate.gameObject.SetActive(false);
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)demon)
		{
			demon.CanCollect = true;
		}
	}

	private void CheckStatus()
	{
		if (DataManager.Instance.RatooGivenHeart || DungeonSandboxManager.Active)
		{
			return;
		}
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			if ((bool)demon.GetComponent<Demon_Collector>())
			{
				this.demon = demon.GetComponent<Demon_Collector>();
				this.demon.CanCollect = false;
			}
		}
		if (DataManager.Instance.EncounteredHealingRoom)
		{
			UpdateStatus();
		}
	}

	public void UpdateStatus()
	{
		if (DataManager.Instance.RatooGivenHeart)
		{
			return;
		}
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			if ((bool)demon.GetComponent<Demon_Collector>() && demon.GetComponent<Demon_Collector>().FollowerInfo != null)
			{
				_gotCollector = true;
				break;
			}
			if ((bool)demon.GetComponent<Demon_Spirit>() && !_gotCollector && demon.GetComponent<Demon_Spirit>().FollowerInfo != null)
			{
				_gotSpirit = true;
			}
		}
		if (_gotCollector && !DataManager.Instance.RatooGivenHeart)
		{
			rightHeartConvo.gameObject.SetActive(true);
			if ((bool)bark)
			{
				bark.gameObject.SetActive(false);
			}
			if ((bool)barkAlternate)
			{
				barkAlternate.gameObject.SetActive(false);
			}
		}
		if (_gotSpirit && !_gotCollector && !DataManager.Instance.RatooMentionedWrongHeart)
		{
			wrongHeartConvo.gameObject.SetActive(true);
			if ((bool)bark)
			{
				bark.gameObject.SetActive(false);
			}
			if ((bool)barkAlternate)
			{
				barkAlternate.gameObject.SetActive(false);
			}
		}
	}

	public void GiveDemon()
	{
		foreach (GameObject demon in Demon_Arrows.Demons)
		{
			Demon_Collector component = demon.GetComponent<Demon_Collector>();
			if (component != null)
			{
				StartCoroutine(GiveDemonRoutine(component));
				break;
			}
		}
	}

	public void GiveReward()
	{
		StartCoroutine(GiveHeartRoutine());
	}

	private IEnumerator GiveHeartRoutine()
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		bool waiting = true;
		PermanentHeart_CustomTarget.Create(_moveTarget.transform.position, PlayerFarming.Instance.gameObject.transform.position, 2f, delegate
		{
			waiting = false;
		});
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
		while (waiting)
		{
			yield return null;
		}
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForSeconds(1f);
		if (DataManager.Instance.RatauKilled && (bool)barkAlternate)
		{
			barkAlternate.gameObject.SetActive(true);
		}
		else if ((bool)bark)
		{
			bark.gameObject.SetActive(true);
		}
	}

	private IEnumerator GiveDemonRoutine(Demon_Collector demon)
	{
		yield return new WaitForEndOfFrame();
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		int count = 0;
		foreach (int followers_Demons_Type in DataManager.Instance.Followers_Demons_Types)
		{
			if (followers_Demons_Type == 3)
			{
				int iD = DataManager.Instance.Followers_Demons_IDs[count];
				DataManager.Instance.Followers_Demons_Types.RemoveAt(count);
				DataManager.Instance.Followers_Demons_IDs.RemoveAt(count);
				FollowerManager.RemoveFollowerBrain(iD);
				break;
			}
			count++;
			yield return null;
		}
		demon.enabled = false;
		yield return demon.spine.state.SetAnimation(0, "action", false);
		demon.spine.transform.DOMove(_moveTarget.position + Vector3.back * 0.5f, 3f).SetEase(Ease.OutQuart);
		yield return new WaitForSeconds(3f);
		BiomeConstants.Instance.EmitSmokeExplosionVFX(_moveTarget.position);
		BiomeConstants.Instance.ShakeCamera();
		CameraManager.instance.ShakeCameraForDuration(0f, 1f, 0.5f);
		demon.gameObject.SetActive(false);
		DataManager.Instance.RatooGivenHeart = true;
		_ratooCheckSkin.CheckSkin();
		GameManager.GetInstance().OnConversationEnd();
		yield return new WaitForEndOfFrame();
		gaveHeartConvo.gameObject.SetActive(true);
		gaveHeartConvo.OnInteract(PlayerFarming.Instance.state);
	}
}
