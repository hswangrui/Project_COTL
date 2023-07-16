using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD_XP : BaseMonoBehaviour
{
	public Image _instantBar;

	public Image _lerpBar;

	public Image _flashBar;

	public UI_Transitions _transition;

	public TextMeshProUGUI _text;

	private int _cacheXP;

	private int _xpTmp;

	private float _previousXP;

	private int _tmpXPTarget;

	private Coroutine _getXPCoroutine;

	private Coroutine _lerpBarCoroutine;

	private Coroutine _flashBarCoroutine;

	private int XP
	{
		get
		{
			return DataManager.Instance.XP;
		}
	}

	private int _targetXP
	{
		get
		{
			return DataManager.GetTargetXP(Mathf.Min(DataManager.Instance.Level, Mathf.Max(DataManager.TargetXP.Count - 1, 0)));
		}
	}

	private void OnEnable()
	{
		if (!GameManager.HasUnlockAvailable() && !DataManager.Instance.DeathCatBeaten)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (XP > _targetXP)
		{
			_xpTmp = _targetXP;
		}
		else
		{
			_xpTmp = XP;
		}
		_tmpXPTarget = _targetXP;
		_text.text = _xpTmp + "/" + _tmpXPTarget;
		PlayerFarming.OnGetXP = (Action)Delegate.Combine(PlayerFarming.OnGetXP, new Action(OnGetXP));
		RectTransform rectTransform = _instantBar.rectTransform;
		Vector3 localScale = (_lerpBar.rectTransform.localScale = new Vector3(Mathf.Max(Mathf.Clamp((float)XP / (float)_targetXP, 0f, 1f), 0f), 1f));
		rectTransform.localScale = localScale;
		_flashBar.enabled = false;
		if (SceneManager.GetActiveScene().name != "Base Biome 1")
		{
			_transition.hideBar();
		}
		_cacheXP = XP;
		StartCoroutine(DungeonRoutine());
	}

	private void Start()
	{
		StartCoroutine(OnGetXPRoutine(true));
	}

	private IEnumerator DungeonRoutine()
	{
		_transition.hideBar();
		while (true)
		{
			if (GameManager.IsDungeon(PlayerFarming.Location))
			{
				if (XP != _cacheXP)
				{
					Debug.Log("XP != cache");
					if (_transition.Hidden)
					{
						_transition.StartCoroutine(_transition.MoveBarIn());
					}
					if (_transition.Hidden)
					{
						yield return new WaitForSeconds(0.5f);
					}
					if (XP > _targetXP)
					{
						_xpTmp = _targetXP;
					}
					else
					{
						_xpTmp = XP;
					}
					_tmpXPTarget = _targetXP;
					_cacheXP = XP;
					yield return new WaitForSeconds(3f);
					_transition.StartCoroutine(_transition.MoveBarOut());
					yield return null;
				}
				if (!_transition.Hidden)
				{
					_transition.hideBar();
				}
			}
			yield return null;
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		PlayerFarming.OnGetXP = (Action)Delegate.Remove(PlayerFarming.OnGetXP, new Action(OnGetXP));
	}

	private void OnGetXP()
	{
		if (_getXPCoroutine != null)
		{
			StopCoroutine(_getXPCoroutine);
		}
		_getXPCoroutine = StartCoroutine(OnGetXPRoutine(false));
	}

	private IEnumerator OnGetXPRoutine(bool forced)
	{
		while (!_transition.Revealed && !forced)
		{
			yield return null;
		}
		if (XP > _targetXP)
		{
			_xpTmp = _targetXP;
		}
		else
		{
			_xpTmp = XP;
		}
		_tmpXPTarget = _targetXP;
		if (XP > _targetXP)
		{
			RectTransform rectTransform = _lerpBar.rectTransform;
			Vector3 localScale = (_instantBar.rectTransform.localScale = Vector3.zero);
			rectTransform.localScale = localScale;
			if (_flashBarCoroutine != null)
			{
				StopCoroutine(_flashBarCoroutine);
			}
			StartCoroutine(FlashBarRoutine());
		}
		else
		{
			Vector3 localScale2 = new Vector3(Mathf.Clamp((float)XP / (float)_targetXP, 0f, 1f), 1f);
			if (float.IsNaN(localScale2.x))
			{
				localScale2.x = 0f;
			}
			_instantBar.rectTransform.localScale = localScale2;
			if (_lerpBarCoroutine != null)
			{
				StopCoroutine(_lerpBarCoroutine);
			}
			if (_instantBar.rectTransform.localScale.x > _lerpBar.rectTransform.localScale.x)
			{
				_lerpBarCoroutine = StartCoroutine(LerpBarRoutine());
			}
			else
			{
				_lerpBar.rectTransform.localScale = _instantBar.rectTransform.localScale;
			}
		}
		float xp = _previousXP;
		_previousXP = _xpTmp;
		if (forced || xp > (float)_xpTmp)
		{
			_text.text = _xpTmp + "/" + _tmpXPTarget;
			yield break;
		}
		float t = 0f;
		while (true)
		{
			float num;
			t = (num = t + Time.deltaTime * 2f);
			if (!(num < 1f))
			{
				break;
			}
			_text.text = (int)Mathf.Lerp(xp, _xpTmp, t) + "/" + _tmpXPTarget;
			yield return null;
		}
		_text.text = _xpTmp + "/" + _tmpXPTarget;
	}

	private IEnumerator LerpBarRoutine()
	{
		yield return new WaitForSeconds(0.2f);
		Vector3 startPosition = _lerpBar.rectTransform.localScale;
		float progress = 0f;
		float duration = 0.3f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime);
			if (!(num < duration))
			{
				break;
			}
			_lerpBar.rectTransform.localScale = Vector3.Lerp(startPosition, _instantBar.rectTransform.localScale, Mathf.SmoothStep(0f, 1f, progress / duration));
			yield return null;
		}
		_lerpBar.rectTransform.localScale = _instantBar.rectTransform.localScale;
	}

	private IEnumerator FlashBarRoutine()
	{
		_flashBar.enabled = true;
		_flashBar.color = Color.white;
		Color fadeColor = new Color(1f, 1f, 1f, 0f);
		yield return new WaitForSeconds(0.5f);
		float progress = 0f;
		float duration = 1f;
		while (true)
		{
			float num;
			progress = (num = progress + Time.deltaTime);
			if (!(num < duration))
			{
				break;
			}
			_flashBar.color = Color.Lerp(Color.white, fadeColor, Mathf.SmoothStep(0f, 1f, progress / duration));
			yield return null;
		}
		_flashBar.enabled = false;
	}
}
