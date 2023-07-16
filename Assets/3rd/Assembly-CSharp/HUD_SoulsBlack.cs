using TMPro;
using UnityEngine;

public class HUD_SoulsBlack : BaseMonoBehaviour
{
	public TextMeshProUGUI Count;

	public TextMeshProUGUI Delta;

	private int _currentCount;

	private int _currentdelta;

	private float Delay;

	private float FadeDelay;

	private Color DeltaColor;

	private Color DeltaFade;

	private int CurrentCount
	{
		get
		{
			return _currentCount;
		}
		set
		{
			_currentCount = value;
			Count.text = "<sprite name=\"icon_blackSoul\"> " + _currentCount;
		}
	}

	public int CurrentDelta
	{
		get
		{
			return _currentdelta;
		}
		set
		{
			_currentdelta = value;
			Delta.text = ((_currentdelta > 0) ? "+" : "") + _currentdelta;
		}
	}

	private void OnEnable()
	{
		PlayerFarming.OnGetBlackSoul += OnGetSoul;
		CurrentCount = Inventory.BlackSouls;
		Delta.text = "";
		DeltaColor = Delta.color;
		DeltaFade = Delta.color - new Color(0f, 0f, 0f, 1f);
	}

	private void OnResetBlackSouls(int DeltaValue)
	{
		CurrentDelta = 0;
		CurrentCount = Inventory.BlackSouls;
		Delta.text = "";
	}

	private void OnDisable()
	{
		PlayerFarming.OnGetBlackSoul -= OnGetSoul;
	}

	private void OnGetSoul(int DeltaValue)
	{
		CurrentDelta += DeltaValue;
		FadeDelay = 1f;
		Delta.color = DeltaColor;
		Delta.text = ((CurrentDelta > 0) ? "+" : "") + CurrentDelta;
		Delay = 1f;
	}

	private void Update()
	{
		if (CurrentDelta != 0)
		{
			if ((Delay -= Time.deltaTime) < 0f)
			{
				if (CurrentDelta > 0)
				{
					CurrentDelta--;
					CurrentCount++;
				}
				else if (CurrentDelta < 0)
				{
					CurrentDelta++;
					CurrentCount--;
				}
				Delay = 0.05f;
			}
		}
		else if ((FadeDelay -= Time.deltaTime) < 0f)
		{
			Delta.color = Color.Lerp(Delta.color, DeltaFade, 2f * Time.deltaTime);
		}
	}
}
