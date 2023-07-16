using DG.Tweening;
using I2.Loc;
using TMPro;
using UnityEngine;

public class UIEnemyRoundsHUD : BaseMonoBehaviour
{
	public static UIEnemyRoundsHUD Instance;

	[SerializeField]
	private TMP_Text roundsText;

	[SerializeField]
	private CanvasGroup cG;

	public static void Play(int currentRound, int totalRounds)
	{
		if (Instance == null)
		{
			Instance = Object.Instantiate(Resources.Load<UIEnemyRoundsHUD>("Prefabs/UI/UI Enemy Rounds HUD"), GameObject.FindWithTag("Canvas").transform);
		}
		Instance.OnRoundStart(currentRound, totalRounds);
		EnemyRoundsBase.OnRoundStart += Instance.OnRoundStart;
		HUD_Manager.Instance.XPBarTransitions.gameObject.SetActive(false);
	}

	public static void Hide()
	{
		if (Instance != null)
		{
			Object.Destroy(Instance.gameObject);
			Instance = null;
		}
		HUD_Manager instance = HUD_Manager.Instance;
		if ((object)instance != null)
		{
			UI_Transitions xPBarTransitions = instance.XPBarTransitions;
			if ((object)xPBarTransitions != null)
			{
				xPBarTransitions.gameObject.SetActive(true);
			}
		}
	}

	private void OnRoundStart(int round, int maxRounds)
	{
		cG.DOFade(1f, 1f);
		if (roundsText != null)
		{
			roundsText.text = string.Format(ScriptLocalization.Interactions.Round, round + 1, maxRounds);
		}
	}
}
