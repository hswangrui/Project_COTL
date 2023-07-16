using System.Collections;
using DG.Tweening;
using MMBiomeGeneration;
using UnityEngine;

public class FinalBossIntroManager : MonoBehaviour
{
	[SerializeField]
	private Color backgroundCameraColor;

	[SerializeField]
	private Interaction_WeaponSelectionPodium[] weaponPodiums;

	[SerializeField]
	private Interaction_WeaponSelectionPodium[] cursePodiums;

	[SerializeField]
	private RoomLockController blockingDoor;

	private Camera camera;

	private bool weaponSelected;

	private bool curseSelected;

	private void Start()
	{
		Interaction_WeaponSelectionPodium[] array = weaponPodiums;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnInteraction += WeaponSelected;
		}
		array = cursePodiums;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnInteraction += CurseSelected;
		}
		if (DungeonSandboxManager.Active)
		{
			array = weaponPodiums;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
			array = cursePodiums;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(false);
			}
			RoomLockController.OpenAll();
		}
	}

	private void OnEnable()
	{
		StartCoroutine(Play());
	}

	private void WeaponSelected(StateMachine state)
	{
		weaponSelected = true;
		if (weaponSelected && curseSelected)
		{
			RoomLockController.OpenAll();
			blockingDoor.Completed = true;
			FaithAmmo.Reload();
		}
	}

	private void CurseSelected(StateMachine state)
	{
		curseSelected = true;
		if (weaponSelected && curseSelected)
		{
			RoomLockController.OpenAll();
			blockingDoor.Completed = true;
		}
	}

	private IEnumerator Play()
	{
		yield return new WaitForEndOfFrame();
		if (base.gameObject.activeInHierarchy)
		{
			DataManager.Instance.CameFromDeathCatFight = true;
			WeatherSystemController.Instance.EnteredBuilding();
			camera = Camera.main;
			BiomeGenerator.Instance.SpawnDemons();
			yield return new WaitForEndOfFrame();
			while (PlayerFarming.Instance == null)
			{
				yield return null;
			}
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/final-ritual-land", 0, false);
			PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, false, 0f);
			AudioManager.Instance.PlayOneShot("event:/pentagram/pentagram_teleport_segment", base.gameObject);
			yield return new WaitForSeconds(1f);
			camera.backgroundColor = backgroundCameraColor;
			yield return new WaitForSeconds(2f);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
			PlayerFarming.Instance.EndGoToAndStop();
			GameManager.GetInstance().OnConversationEnd();
			GameManager.GetInstance().AddPlayerToCamera();
			RoomLockController.CloseAll();
			GameManager.InitialDungeonEnter = true;
			StartCoroutine(BiomeGenerator.Instance.DelayActivateRoom(!DungeonSandboxManager.Active));
		}
	}

	public void CameraFocusOnDeathCat()
	{
		GameManager.GetInstance().CamFollowTarget.transform.DORotate(new Vector3(-60f, 0f, 0f), 3f).SetEase(Ease.InOutSine);
	}

	public void ResetCameraFocus()
	{
		GameManager.GetInstance().CamFollowTarget.transform.DORotate(new Vector3(-45f, 0f, 0f), 1f).SetEase(Ease.InOutSine);
	}
}
