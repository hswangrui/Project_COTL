using System;
using System.Collections;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class UIRecruitScreen : BaseMonoBehaviour
{
	public Animator Animator;

	public UINavigator uINavigator;

	private Action Callback;

	private Follower follower;

	public TextMeshProUGUI NameText;

	public TextMeshProUGUI FactionText;

	public TMP_InputField ChangeName;

	private readonly int NumSkins = 3;

	private void Start()
	{
		UINavigator obj = uINavigator;
		obj.OnButtonDown = (UINavigator.ButtonDown)Delegate.Combine(obj.OnButtonDown, new UINavigator.ButtonDown(OnButtonDown));
		GameManager.GetInstance().CameraSetOffset(new Vector3(-1.25f, 0f, 0.5f));
	}

	public void Play(Action Callback, Follower f)
	{
		this.Callback = Callback;
		follower = f;
		NameText.text = follower.Brain.Info.Name;
		FactionText.text = "Level " + follower.Brain.Info.XPLevel.ToNumeral();
		ChangeName.onValueChanged.AddListener(delegate
		{
			OnNameChanged();
		});
	}

	public void OnNameChanged()
	{
		follower.Brain.Info.Name = ChangeName.text;
		NameText.text = follower.Brain.Info.Name;
	}

	public void NextSkin()
	{
		follower.Brain.Info.NextSkin();
		Skin skin = follower.Spine.Skeleton.Data.FindSkin(follower.Brain.Info.SkinName);
		string outfitSkinName = follower.Outfit.GetOutfitSkinName(follower.Brain.Info.Outfit);
		skin.AddSkin(follower.Spine.Skeleton.Data.FindSkin(outfitSkinName));
		follower.Spine.Skeleton.SetSkin(skin);
	}

	public void PrevSkin()
	{
		follower.Brain.Info.PrevSkin();
		Skin skin = follower.Spine.Skeleton.Data.FindSkin(follower.Brain.Info.SkinName);
		string outfitSkinName = follower.Outfit.GetOutfitSkinName(follower.Brain.Info.Outfit);
		skin.AddSkin(follower.Spine.Skeleton.Data.FindSkin(outfitSkinName));
		follower.Spine.Skeleton.SetSkin(skin);
	}

	public void NextColour()
	{
		follower.Brain.Info.NextSkinColor();
		foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance.GetColourData(follower.Brain.Info.SkinName).SlotAndColours[follower.Brain.Info.SkinColour].SlotAndColours)
		{
			Slot slot = follower.Spine.skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
	}

	public void PrevColour()
	{
		follower.Brain.Info.PrevSkinColor();
		foreach (WorshipperData.SlotAndColor slotAndColour in WorshipperData.Instance.GetColourData(follower.Brain.Info.SkinName).SlotAndColours[follower.Brain.Info.SkinColour].SlotAndColours)
		{
			Slot slot = follower.Spine.skeleton.FindSlot(slotAndColour.Slot);
			if (slot != null)
			{
				slot.SetColor(slotAndColour.color);
			}
		}
	}

	private void OnDisable()
	{
		UINavigator obj = uINavigator;
		obj.OnButtonDown = (UINavigator.ButtonDown)Delegate.Remove(obj.OnButtonDown, new UINavigator.ButtonDown(OnButtonDown));
		ChangeName.onValueChanged.RemoveAllListeners();
	}

	private void OnButtonDown(Buttons CurrentButton)
	{
		Debug.Log("close!");
	}

	public void Close()
	{
		GameManager instance = GameManager.GetInstance();
		if ((object)instance != null)
		{
			instance.CameraSetOffset(Vector3.zero);
		}
		StartCoroutine(CloseRoutine());
	}

	private IEnumerator CloseRoutine()
	{
		Animator.Play("Base Layer.Out");
		yield return new WaitForSeconds(0.5f);
		Action callback = Callback;
		if (callback != null)
		{
			callback();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
