using RewiredConsts;
using Steamworks;

public class InputManager : Singleton<InputManager>
{
	private GeneralInputSource _general;

	private RewiredUIInputSource _ui;

	private RewiredGameplayInputSource _gameplay;

	private PhotoModeInputSource _photoMode;

	public static GeneralInputSource General
	{
		get
		{
			return Singleton<InputManager>.Instance._general;
		}
	}

	public static RewiredUIInputSource UI
	{
		get
		{
			return Singleton<InputManager>.Instance._ui;
		}
	}

	public static RewiredGameplayInputSource Gameplay
	{
		get
		{
			return Singleton<InputManager>.Instance._gameplay;
		}
	}

	public static PhotoModeInputSource PhotoMode
	{
		get
		{
			return Singleton<InputManager>.Instance._photoMode;
		}
	}

	public InputManager()
	{
		//SteamInput.Init(false);
		_ui = new RewiredUIInputSource();
		_gameplay = new RewiredGameplayInputSource();
		_photoMode = new PhotoModeInputSource();
		_general = new GeneralInputSource();
	}
}
