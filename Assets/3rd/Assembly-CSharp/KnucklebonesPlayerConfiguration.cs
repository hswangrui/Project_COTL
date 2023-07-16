using FMODUnity;
using I2.Loc;
using Spine.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "KnucklebonesPlayerConfiguration", menuName = "COTL/Knucklebones/KnucklebonesPlayerConfiguration")]
public class KnucklebonesPlayerConfiguration : ScriptableObject
{
	[SerializeField]
	[TermsPopup("")]
	private string _opponentName;

	[SerializeField]
	private DataManager.Variables _variableToShow;

	[SerializeField]
	private DataManager.Variables _variableToChangeOnWin;

	[SerializeField]
	[EventRef]
	private string _soundToPlay;

	[SerializeField]
	[EventRef]
	private string _victoryAudio;

	[SerializeField]
	[EventRef]
	private string _defeatAudio;

	[SerializeField]
	private TarotCards.Card _reward;

	[SerializeField]
	private SkeletonDataAsset _spine;

	[SerializeField]
	private string _initialSkinName;

	[SerializeField]
	[Range(0f, 10f)]
	private int _difficulty;

	[SerializeField]
	private int _maxBet;

	[SerializeField]
	private Vector2 _scale;

	[SerializeField]
	private Vector2 _positionOffset;

	public string OpponentName
	{
		get
		{
			return _opponentName;
		}
	}

	public DataManager.Variables VariableToShow
	{
		get
		{
			return _variableToShow;
		}
	}

	public DataManager.Variables VariableToChangeOnWin
	{
		get
		{
			return _variableToChangeOnWin;
		}
	}

	public string SoundToPlay
	{
		get
		{
			return _soundToPlay;
		}
	}

	public string VictoryAudio
	{
		get
		{
			return _victoryAudio;
		}
	}

	public string DefeatAudio
	{
		get
		{
			return _defeatAudio;
		}
	}

	public TarotCards.Card Reward
	{
		get
		{
			return _reward;
		}
	}

	public SkeletonDataAsset Spine
	{
		get
		{
			return _spine;
		}
	}

	public string InitialSkinName
	{
		get
		{
			return _initialSkinName;
		}
	}

	public int Difficulty
	{
		get
		{
			return _difficulty;
		}
	}

	public int MaxBet
	{
		get
		{
			return _maxBet;
		}
	}

	public Vector2 Scale
	{
		get
		{
			return _scale;
		}
	}

	public Vector2 PositionOffset
	{
		get
		{
			return _positionOffset;
		}
	}

	public KBOpponentAI CreateAI()
	{
		return new KBOpponentAI(_difficulty);
	}
}
