using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using KnuckleBones;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KBDiceTub : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	public Action OnDiceLost;

	public Action OnDiceMatched;

	[SerializeField]
	private RectTransform _rectTransform;

	[SerializeField]
	private KBDiceTub _opponentTub;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private TextMeshProUGUI _scoreText;

	[SerializeField]
	private List<RectTransform> _diceContainers = new List<RectTransform>();

	private int _score;

	private List<Dice> _dice = new List<Dice>();

	private RectTransform _highlightRectTransform;

	private Vector3 _highlightOriginScale;

	private Vector3 _positionOrigin;

	public int Score
	{
		get
		{
			return _score;
		}
	}

	public List<Dice> Dice
	{
		get
		{
			return _dice;
		}
	}

	public KBDiceTub OpponentTub
	{
		get
		{
			return _opponentTub;
		}
	}

	private void Start()
	{
		_highlightRectTransform = _highlight.GetComponent<RectTransform>();
		_highlightOriginScale = _highlightRectTransform.localScale;
		_highlight.enabled = false;
		_scoreText.text = "";
		_positionOrigin = _rectTransform.anchoredPosition;
	}

	public bool TrySelectTub()
	{
		if (_dice.Count < 3)
		{
			return true;
		}
		AudioManager.Instance.PlayOneShot("event:/ui/negative_feedback");
		_rectTransform.DOKill();
		_rectTransform.anchoredPosition = _positionOrigin;
		_rectTransform.DOShakeAnchorPos(0.75f, 5f).SetUpdate(true);
		return false;
	}

	public IEnumerator AddDice(Dice dice)
	{
		yield return dice.GoToLocationRoutine(GetNextPosition());
		_dice.Add(dice);
		UpdateScore(null);
		yield return _opponentTub.CheckDice(dice);
	}

	private IEnumerator CheckDice(Dice dice)
	{
		bool flag = false;
		int j = -1;
		while (true)
		{
			int num = j + 1;
			j = num;
			if (num >= Dice.Count)
			{
				break;
			}
			if (Dice[j].Num == dice.Num)
			{
				AudioManager.Instance.PlayOneShot("event:/material/stained_glass_impact");
				yield return Dice[j].StartCoroutine(Dice[j].ShakeRoutine());
				UnityEngine.Object.Destroy(Dice[j].gameObject);
				Dice.RemoveAt(j);
				flag = true;
				num = j - 1;
				j = num;
			}
		}
		UpdateScore(dice);
		if (!flag)
		{
			yield break;
		}
		Action onDiceLost = OnDiceLost;
		if (onDiceLost != null)
		{
			onDiceLost();
		}
		j = -1;
		while (true)
		{
			int num = j + 1;
			j = num;
			if (num < _dice.Count)
			{
				if (!_dice[j].transform.position.Equals(GetPosition(j)))
				{
					yield return _dice[j].GoToLocationRoutine(GetPosition(j));
				}
				continue;
			}
			break;
		}
	}

	private void UpdateScore(Dice dice)
	{
		_score = 0;
		foreach (Dice die in _dice)
		{
			int num = NumMatchingDice(die.Num);
			_score += die.Num * num;
			if (num > 1)
			{
				die.image.color = ((num == 2) ? new Color32(246, 235, 152, byte.MaxValue) : new Color32(117, 189, byte.MaxValue, byte.MaxValue));
				if (!die.matched)
				{
					Debug.Log("Match Dice");
					die.matched = true;
					Action onDiceMatched = OnDiceMatched;
					if (onDiceMatched != null)
					{
						onDiceMatched();
					}
					AudioManager.Instance.PlayOneShot("event:/ui/open_menu");
				}
				if (dice != null && die.Num == dice.Num)
				{
					die.Scale();
				}
			}
			else
			{
				die.image.color = Color.white;
			}
		}
		if (Score == 0)
		{
			_scoreText.text = "";
			return;
		}
		_scoreText.text = Score.ToString();
		_scoreText.transform.DOKill();
		_scoreText.transform.DOPunchPosition(new Vector3(5f, 0f), 1f).SetUpdate(true);
	}

	private void Highlight()
	{
		_highlight.enabled = true;
		_highlightRectTransform.localScale = _highlightOriginScale * 0.8f;
		_highlightRectTransform.DOKill();
		_highlightRectTransform.DOScale(_highlightOriginScale, 0.5f).SetEase(Ease.OutQuart).SetUpdate(true);
		_highlight.DOFade(1f, 0.5f).SetEase(Ease.OutQuart).SetUpdate(true);
		_highlight.color = ((_dice.Count < 3) ? Color.red : Color.black);
	}

	private void UnHighlight()
	{
		_highlight.enabled = false;
	}

	public void OnSelect(BaseEventData eventData)
	{
		Highlight();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		UnHighlight();
	}

	public int NumMatchingDice(int number)
	{
		int num = 0;
		foreach (Dice die in _dice)
		{
			if (die.Num == number)
			{
				num++;
			}
		}
		return num;
	}

	private Vector2 GetNextPosition()
	{
		return _diceContainers[_dice.Count].position;
	}

	private Vector2 GetPosition(int index)
	{
		return _diceContainers[index].position;
	}

	public void FinalizeScore()
	{
		int score = Score;
		int score2 = _opponentTub.Score;
	}
}
