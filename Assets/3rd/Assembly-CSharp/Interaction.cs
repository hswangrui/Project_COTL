using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using I2.Loc;
using Lamb.UI;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityFx.Outline;

public abstract class Interaction : BaseMonoBehaviour
{
	public delegate void InteractionEvent(StateMachine state);

	private class RendererAndColor
	{
		public SpriteRenderer spriteRenderer;

		public Color color;

		public RendererAndColor(SpriteRenderer spriteRenderer, Color color)
		{
			this.spriteRenderer = spriteRenderer;
			this.color = color;
		}
	}

	private class RendererAndColorSpine
	{
		public SkeletonAnimation obj;

		public Color color;

		public RendererAndColorSpine(SkeletonAnimation obj, Color color)
		{
			this.obj = obj;
			this.color = color;
		}
	}

	protected OutlineEffect Outliner;

	public Transform LockPosition;

	public static List<Interaction> interactions = new List<Interaction>();

	[HideInInspector]
	public Vector3 Position;

	public StateMachine state;

	public Vector3 Offset = new Vector3(0f, 3.5f, 0f);

	public float PriorityWeight = 1f;

	public float ActivateDistance = 1f;

	public Vector3 ActivatorOffset = Vector3.zero;

	public bool HoldToInteract;

	[HideInInspector]
	public bool ContinuouslyHold;

	public bool AutomaticallyInteract;

	[HideInInspector]
	public float HoldProgress;

	[HideInInspector]
	public bool HoldBegun;

	public bool Interactable = true;

	public bool HasSecondaryInteraction;

	public bool SecondaryInteractable = true;

	public bool HasThirdInteraction;

	public bool ThirdInteractable = true;

	public bool HasFourthInteraction;

	public bool FourthInteractable = true;

	public UnityEvent CallbackStart;

	[HideInInspector]
	public bool IgnoreTutorial;

	[SerializeField]
	private string label;

	[SerializeField]
	private string secondaryLabel;

	[SerializeField]
	private string thirdLabel;

	[SerializeField]
	private string fourthLabel;

	private List<RendererAndColor> SpriteRendererAndColors = new List<RendererAndColor>();

	private List<RendererAndColorSpine> SpineRendererAndColors = new List<RendererAndColorSpine>();

	public GameObject OutlineTarget;

	public UnityEvent indicateHighlight;

	public UnityEvent indicateHighlightEnd;

	private EventInstance loopingSoundInstance;

	private string holdTime = "hold_time";

	private bool hasPlayed;

	public OutlineEffect OutlineEffect
	{
		get
		{
			return Outliner;
		}
	}

	public int Action
	{
		get
		{
			return 9;
		}
	}

	public int SecondaryAction
	{
		get
		{
			return 68;
		}
	}

	public int ThirdAction
	{
		get
		{
			return 67;
		}
	}

	public int FourthAction
	{
		get
		{
			return 66;
		}
	}

	public string Label
	{
		get
		{
			if (!IgnoreTutorial && !DataManager.Instance.AllowBuilding && BiomeBaseManager.Instance != null)
			{
				return "";
			}
			GetLabel();
			return label;
		}
		set
		{
			label = value;
		}
	}

	public bool HasChanged { get; set; }

	public string SecondaryLabel
	{
		get
		{
			if (!IgnoreTutorial && !DataManager.Instance.AllowBuilding)
			{
				return "";
			}
			GetSecondaryLabel();
			return secondaryLabel;
		}
		set
		{
			secondaryLabel = value;
		}
	}

	public string ThirdLabel
	{
		get
		{
			if (!IgnoreTutorial && !DataManager.Instance.AllowBuilding)
			{
				return "";
			}
			GetThirdLabel();
			return thirdLabel;
		}
		set
		{
			thirdLabel = value;
		}
	}

	public string FourthLabel
	{
		get
		{
			if (!IgnoreTutorial && !DataManager.Instance.AllowBuilding)
			{
				return "";
			}
			GetFourthLabel();
			return fourthLabel;
		}
		set
		{
			fourthLabel = value;
		}
	}

	public event InteractionEvent OnInteraction;

	public virtual void GetLabel()
	{
	}

	public virtual void GetSecondaryLabel()
	{
	}

	public virtual void GetThirdLabel()
	{
	}

	public virtual void GetFourthLabel()
	{
	}

	protected virtual void OnEnable()
	{
		interactions.Add(this);
		OnEnableInteraction();
		LocalizationManager.OnLocalizeEvent -= UpdateLocalisation;
		LocalizationManager.OnLocalizeEvent += UpdateLocalisation;
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(AddToRegion());
		}
		if (Outliner == null && Camera.main != null)
		{
			Outliner = Camera.main.GetComponent<OutlineEffect>();
		}
	}

	private IEnumerator AddToRegion()
	{
		yield return new WaitForSeconds(0.1f);
		Position = base.transform.position;
		Interactor.AddToRegion(this);
	}

	public virtual void UpdateLocalisation()
	{
		HasChanged = true;
	}

	public virtual void OnEnableInteraction()
	{
	}

	protected virtual void OnDestroy()
	{
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		LocalizationManager.OnLocalizeEvent -= UpdateLocalisation;
		interactions.Remove(this);
		Interactor.RemoveFromRegion(this);
	}

	protected virtual void OnDisable()
	{
		interactions.Remove(this);
		OnDisableInteraction();
		AudioManager.Instance.StopLoop(loopingSoundInstance);
		Interactor.RemoveFromRegion(this);
	}

	public virtual void OnDisableInteraction()
	{
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	public virtual void OnBecomeCurrent()
	{
		HasChanged = false;
		if (!AutomaticallyInteract)
		{
			IndicateHighlighted();
		}
	}

	public virtual void IndicateHighlighted()
	{
		if (CheatConsole.HidingUI)
		{
			return;
		}
		if (Outliner != null)
		{
			Outliner.OutlineLayers[0].Add((OutlineTarget == null) ? base.gameObject : OutlineTarget);
			UnityEvent unityEvent = indicateHighlight;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
		else
		{
			Debug.Log("Outliner = null");
		}
	}

	public virtual void OnBecomeNotCurrent()
	{
		if (!AutomaticallyInteract)
		{
			EndIndicateHighlighted();
			AudioManager.Instance.StopLoop(loopingSoundInstance);
		}
	}

	public virtual void EndIndicateHighlighted()
	{
		if (Outliner != null && Outliner.OutlineLayers.Count > 0 && Outliner.OutlineLayers[0] != null)
		{
			Outliner.OutlineLayers[0].Remove((OutlineTarget == null) ? base.gameObject : OutlineTarget);
			Outliner.RemoveGameObject((OutlineTarget == null) ? base.gameObject : OutlineTarget);
			UnityEvent unityEvent = indicateHighlightEnd;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
		}
		SpriteRendererAndColors.Clear();
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	public virtual void OnHoldProgressDown()
	{
		MonoSingleton<Indicator>.Instance.ControlPromptContainer.DOKill();
		MonoSingleton<Indicator>.Instance.ControlPromptContainer.localScale = Vector3.one;
		MonoSingleton<Indicator>.Instance.ControlPromptContainer.DOPunchScale(new Vector3(0.2f, 0.2f), 0.2f);
		loopingSoundInstance = AudioManager.Instance.CreateLoop("event:/ui/hold_button_loop", base.gameObject, true);
		if (HoldProgress > 0.95f)
		{
			AudioManager.Instance.StopLoop(loopingSoundInstance);
		}
	}

	protected virtual void Update()
	{
		if (HoldProgress > 0f)
		{
			hasPlayed = true;
			loopingSoundInstance.setParameterByName(holdTime, HoldProgress);
			if (HoldBegun)
			{
				MMVibrate.RumbleContinuous(HoldProgress * 0.2f, HoldProgress * 0.2f);
			}
		}
		else if (hasPlayed)
		{
			AudioManager.Instance.StopLoop(loopingSoundInstance);
			hasPlayed = false;
		}
	}

	public virtual void OnHoldProgressRelease()
	{
		MMVibrate.StopRumble();
	}

	public virtual void OnHoldProgressStop()
	{
		HoldBegun = false;
		HoldProgress = 0f;
		MMVibrate.StopRumble();
		AudioManager.Instance.StopLoop(loopingSoundInstance);
	}

	public void OnInteractGetStaticState()
	{
		state = PlayerFarming.Instance.state;
		OnInteract(state);
	}

	public virtual void OnInteract(StateMachine state)
	{
		EndIndicateHighlighted();
		this.state = state;
		if (CallbackStart != null)
		{
			CallbackStart.Invoke();
		}
		InteractionEvent onInteraction = this.OnInteraction;
		if (onInteraction != null)
		{
			onInteraction(state);
		}
		if (HoldToInteract && SettingsManager.Settings.Accessibility.HoldActions)
		{
			AudioManager.Instance.PlayOneShot("event:/ui/hold_activate", base.transform.position);
			MMVibrate.StopRumble();
		}
		else
		{
			UIManager.PlayAudio("event:/ui/open_menu");
		}
	}

	public virtual void OnSecondaryInteract(StateMachine state)
	{
		EndIndicateHighlighted();
		this.state = state;
	}

	public virtual void OnThirdInteract(StateMachine state)
	{
		EndIndicateHighlighted();
		this.state = state;
	}

	public virtual void OnFourthInteract(StateMachine state)
	{
		EndIndicateHighlighted();
		this.state = state;
	}

	public virtual void OnEndInteraction()
	{
	}

	public virtual void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivatorOffset, ActivateDistance, Color.white);
	}
}
