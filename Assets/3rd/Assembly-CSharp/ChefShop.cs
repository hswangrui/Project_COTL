using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ChefShop : MonoBehaviour
{
	[SerializeField]
	private GameObject snailWarningConvo_0;

	[SerializeField]
	private GameObject snailWarningConvo_1;

	[SerializeField]
	private GameObject snailWarningConvo_2;

	[SerializeField]
	private GameObject snailDoublePrices;

	[SerializeField]
	private shopKeeperManager shopKeeperManager_0;

	[SerializeField]
	private shopKeeperManager shopKeeperManager_1;

	[SerializeField]
	private Health snailhealth;

	[SerializeField]
	private Health shopKeeperHealth;

	[SerializeField]
	private AnimateOnCollision _animateOnCollision;

	[SerializeField]
	private List<MonoBehaviour> chefComponentsToEnable;

	[SerializeField]
	private List<MonoBehaviour> chefComponentsToDisable;

	[SerializeField]
	private List<MonoBehaviour> chefComponentsToDisableDead;

	[SerializeField]
	private SkeletonAnimation shopKeep;

	[SerializeField]
	private spineChangeAnimationSimple[] worms;

	private bool touchedSnail;

	private int warnings;

	private float attackedTimestamp;

	private void Start()
	{
		DataManager.Instance.HasMetChefShop = true;
		if (DataManager.Instance.ShopKeeperChefState == 1)
		{
			warnings = 4;
			foreach (MonoBehaviour item in chefComponentsToEnable)
			{
				item.enabled = true;
			}
			foreach (MonoBehaviour item2 in chefComponentsToDisable)
			{
				item2.enabled = false;
			}
			spineChangeAnimationSimple[] array = worms;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}
		else if (DataManager.Instance.ShopKeeperChefState == 2)
		{
			warnings = 4;
			foreach (MonoBehaviour item3 in chefComponentsToDisableDead)
			{
				item3.enabled = false;
			}
			if (shopKeep.AnimationState != null)
			{
				shopKeep.AnimationState.SetAnimation(0, "scared", true);
			}
		}
		snailhealth.OnHit += HitSnail;
	}

	private IEnumerator EnragedIE()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(shopKeep.gameObject, 5f);
		yield return new WaitForSeconds(0.75f);
		shopKeep.AnimationState.SetAnimation(0, "furious", true);
		AudioManager.Instance.PlayOneShot("event:/dialogue/shop_shrimp_rakshasa/angry_rakshasa", shopKeep.gameObject);
		yield return new WaitForSeconds(2.6f);
		GameManager.GetInstance().OnConversationEnd();
		foreach (MonoBehaviour item in chefComponentsToEnable)
		{
			item.enabled = true;
		}
		foreach (MonoBehaviour item2 in chefComponentsToDisable)
		{
			item2.enabled = false;
		}
		DataManager.Instance.ShopKeeperChefState = 1;
		DataManager.Instance.ShopKeeperChefEnragedDay = TimeManager.CurrentDay + 1;
		shopKeeperHealth.OnDie += ChefDied;
		checkMusic();
		spineChangeAnimationSimple[] array = worms;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	private void ChefDied(GameObject Attacker, Vector3 AttackLocation, Health Victim, Health.AttackTypes AttackType, Health.AttackFlags AttackFlags)
	{
		AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.Shop);
	}

	private void OnEnable()
	{
		if (shopKeeperHealth.enabled)
		{
			shopKeeperHealth.OnDie += ChefDied;
		}
		checkMusic();
	}

	private void OnDisable()
	{
		shopKeeperHealth.OnDie -= ChefDied;
	}

	private void checkMusic()
	{
		if (DataManager.Instance.ShopKeeperChefState == 1)
		{
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.SpecialCombat);
		}
		else
		{
			AudioManager.Instance.SetMusicRoomID(SoundConstants.RoomID.Shop);
		}
	}

	private void OnDestroy()
	{
		snailhealth.OnHit -= HitSnail;
	}

	public void TouchSnail()
	{
		if (!touchedSnail)
		{
			touchedSnail = true;
			snailWarningConvo_0.SetActive(true);
			warnings++;
			CameraManager.instance.ShakeCameraForDuration(1f, 1.2f, 0.2f);
		}
	}

	private void HitSnail(GameObject Attacker, Vector3 AttackLocation, Health.AttackTypes AttackType, bool FromBehind = false)
	{
		if (!(Time.time < attackedTimestamp))
		{
			_animateOnCollision.PushPlayer();
			if (warnings == 0)
			{
				snailWarningConvo_0.SetActive(true);
			}
			else if (warnings == 1)
			{
				snailWarningConvo_1.SetActive(true);
			}
			else if (warnings == 2)
			{
				snailWarningConvo_2.SetActive(true);
			}
			else if (warnings == 3)
			{
				StartCoroutine(EnragedIE());
			}
			attackedTimestamp = Time.time + 1.5f;
			warnings++;
		}
	}

	public void DefeatedChef()
	{
		DataManager.Instance.ShopKeeperChefState = 2;
	}

	private void UpdateShop()
	{
		shopKeeperManager_0.DoublePrices();
		shopKeeperManager_1.DoublePrices();
	}
}
