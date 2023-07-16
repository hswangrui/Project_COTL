using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDoctrineUpgradeTile : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public enum UpgradeTileState
	{
		Locked,
		Unlocked,
		Bought,
		Hidden
	}

	public SermonCategory Category;

	public DoctrineUpgradeSystem.DoctrineType Type;

	public UpgradeTileState State;

	public UpgradeTileState PreviousState;

	public TextMeshProUGUI NameText;

	public Selectable Selectable;

	public RectTransform Container;

	public Image Icon;

	public Image BG;

	public Image SelectedIcon;

	public bool StartAvailable;

	public bool Revealed;

	public bool Disabled;

	public bool Upgrade;

	public CanvasGroup CanvasGroup;

	public UIDoctrineUpgradeTile PartnerTile;

	private bool _Paused;

	public bool AvailableToSelect = true;

	public bool Paused
	{
		get
		{
			return _Paused;
		}
		set
		{
			Debug.Log("CHANGE VALUE! " + value);
			_Paused = value;
		}
	}

	protected virtual void Start()
	{
		Init();
		ShowAvailability();
	}

	public IEnumerator UnPause(float Delay)
	{
		Debug.Log("DELAY: " + Delay);
		yield return new WaitForSecondsRealtime(Delay);
		Debug.Log("UNPAUSE!");
		Paused = false;
		ShowAvailability();
	}

	public void ReEnable()
	{
		Paused = true;
		Selectable.interactable = true;
		ShowAvailability();
	}

	private void ShowAvailability()
	{
		AvailableToSelect = false;
		if (StartAvailable || DoctrineUpgradeSystem.GetUnlocked(Type))
		{
			Selectable.interactable = true;
			Container.gameObject.SetActive(true);
			PreviousState = State;
			if (DoctrineUpgradeSystem.GetUnlocked(Type))
			{
				State = UpgradeTileState.Bought;
			}
			else
			{
				State = UpgradeTileState.Unlocked;
			}
			if (DoctrineUpgradeSystem.GetUnlocked(Type))
			{
				BG.color = Color.green;
			}
			else
			{
				BG.color = Color.white;
			}
			Icon.color = Color.white;
			if (!Revealed && !StartAvailable)
			{
				StartCoroutine(Selected(1.5f, 1f));
			}
			Revealed = true;
		}
		else if (NeighbourUnlocked() && !Upgrade)
		{
			SetLocked();
			if (!Revealed && !StartAvailable)
			{
				StartCoroutine(Selected(1.5f, 1f));
			}
		}
		else
		{
			Container.gameObject.SetActive(false);
			Selectable.interactable = false;
			State = UpgradeTileState.Hidden;
		}
	}

	private void SetLocked()
	{
		State = UpgradeTileState.Locked;
		Selectable.interactable = true;
		Container.gameObject.SetActive(true);
		NameText.gameObject.SetActive(false);
		BG.color = new Color(1f, 1f, 1f, 0.25f);
		Icon.color = new Color(1f, 1f, 1f, 0.25f);
		Revealed = true;
	}

	public IEnumerator RevealRoutine()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		float Progress = 0f;
		float Duration = 1f;
		BG.color = Color.green;
		Selectable.interactable = true;
		Container.gameObject.SetActive(true);
		Container.localScale = Vector3.zero;
		CanvasGroup.alpha = 0f;
		NameText.gameObject.SetActive(true);
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.Lerp(2f, 1f, Progress / Duration);
			Container.localScale = Vector3.one * num2;
			CanvasGroup.alpha = Mathf.Lerp(0f, 1f, Progress / Duration);
			yield return null;
		}
		CanvasGroup.alpha = 1f;
		yield return new WaitForSecondsRealtime(0.5f);
	}

	public IEnumerator LockedRevealRoutine()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		float Progress = 0f;
		float Duration = 1f;
		SetLocked();
		Selectable.interactable = true;
		Container.gameObject.SetActive(true);
		CanvasGroup.alpha = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			CanvasGroup.alpha = Mathf.Lerp(0f, 1f, Progress / Duration);
			yield return null;
		}
		CanvasGroup.alpha = 1f;
		yield return new WaitForSecondsRealtime(0.5f);
	}

	private bool NeighbourUnlocked()
	{
		if (PartnerTile != null)
		{
			return DoctrineUpgradeSystem.GetUnlocked(PartnerTile.Type);
		}
		return false;
	}

	public void Init()
	{
		SelectedIcon.enabled = false;
		Selectable.interactable = !Disabled;
		Container.gameObject.SetActive(!Disabled);
		NameText.text = DoctrineUpgradeSystem.GetLocalizedName(Type);
		base.gameObject.name = NameText.text;
		Icon.preserveAspect = true;
		Icon.sprite = DoctrineUpgradeSystem.GetIcon(Type);
		if (Icon.sprite == null)
		{
			Icon.enabled = false;
		}
	}

	public UIDoctrineUpgradeTile GetLockedNeighbour()
	{
		return PartnerTile;
	}

	public bool CallBack()
	{
		if (DoctrineUpgradeSystem.UnlockAbility(Type))
		{
			ShowAvailability();
			return true;
		}
		return false;
	}

	public void BecomeAvailable()
	{
		Init();
		ShowAvailability();
	}

	public void OnSelect(BaseEventData eventData)
	{
		SelectedIcon.enabled = true;
		StartCoroutine(Selected(Container.localScale.x, 1.3f));
	}

	public void OnDeselect(BaseEventData eventData)
	{
		SelectedIcon.enabled = false;
		StartCoroutine(DeSelected());
	}

	private IEnumerator Selected(float Starting, float Target)
	{
		while (Paused)
		{
			yield return null;
		}
		float Progress = 0f;
		float Duration = 0.2f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(Starting, Target, Progress / Duration);
			Container.localScale = Vector3.one * num2;
			yield return null;
		}
		Container.localScale = Vector3.one * Target;
	}

	private IEnumerator DeSelected()
	{
		while (Paused)
		{
			yield return null;
		}
		float Progress = 0f;
		float Duration = 0.3f;
		float StartingScale = Container.localScale.x;
		float TargetScale = 1f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.unscaledDeltaTime);
			if (!(num < Duration))
			{
				break;
			}
			float num2 = Mathf.SmoothStep(StartingScale, TargetScale, Progress / Duration);
			Container.localScale = Vector3.one * num2;
			yield return null;
		}
		Container.localScale = Vector3.one * TargetScale;
	}
}
